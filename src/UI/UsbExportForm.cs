using System.Reflection;
using System.Diagnostics;

using LrCatalogSync.Core;
using LrCatalogSync.Infrastructure;
using LrCatalogSync.Resources.Strings;

namespace LrCatalogSync.UI
{
    public partial class UsbExportForm : Form
    {
        private const int StatusLogTrimThreshold = 1000;
        private const int StatusLogTrimCount = 500;

        // "null!" = zur Laufzeit immer über den Haupt-Konstruktor gesetzt, der parameterlose
        // Konstruktor wird nur vom WinForms-Designer zur Designzeit verwendet
        private readonly Func<bool> isCoordinatorRunning = null!;
        private readonly UsbExportConfig usbExportConfig = null!;
        private readonly AppConfig appConfig = null!;
        private readonly Stopwatch operationStopwatch = new();
        private CancellationTokenSource? exportCancellationSource;
        private bool operationRunning;
        private bool initializingControls;
        private bool hasLogEntries;
        private int displayedLogEntryCount;
        private string? displayedStatusText;

        // Parameterloser Konstruktor – wird nur vom WinForms-Designer zur Designzeit verwendet
        public UsbExportForm()
        {
            InitializeComponent();
            ApplyLocalization();
        }

        public UsbExportForm(AppConfig config, Func<bool> coordinatorRunning)
        {
            InitializeComponent();
            ApplyLocalization();

            isCoordinatorRunning = coordinatorRunning;
            appConfig = config;
            usbExportConfig = UsbExportConfig.Load(GlobalData.UsbExportConfigPath);
            UsbExportLog.Initialize(GlobalData.BaseDir);
            Text = string.Format(Strings.Get("Usb_Title"), GetApplicationVersion());
            Icon = LoadIcon("LrCatalogSync.Resources.Icons.app_icon.ico");

            // Werte aus der gespeicherten Konfiguration in die Controls übernehmen
            initializingControls = true;
            try
            {
                targetPathTextBox.Text = GetInitialTargetPath();
                LoadSources();
                hashComparisonCheckBox.Checked = usbExportConfig.UseHashComparison;
                metadataCheckBox.Checked = usbExportConfig.TransferMetadata;
                excludePatternsTextBox.Text = usbExportConfig.UserExcludePatterns;
                UpdateDriveDetails();
                UpdateAvailability();
            }
            finally
            {
                initializingControls = false;
            }

            // Statusanzeige und Laufwerksinfos regelmäßig aktualisieren, damit z.B. das Ende
            // des normalen Sync-Zyklus ohne erneutes Öffnen des Fensters erkannt wird.
            availabilityTimer.Start();
        }

        // Wird vom availabilityTimer zyklisch aufgerufen
        private void AvailabilityTimer_Tick(object? sender, EventArgs e)
        {
            RefreshDrives();
            UpdateElapsedTime();
        }

        // ==================== EVENT-HANDLER (im Designer verdrahtet) ====================

        private void TargetPathTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateDriveDetails();
            UpdateAvailability();
        }

        private void BrowseTargetButton_Click(object? sender, EventArgs e)
        {
            SelectTargetFolder();
        }

        private void RemoveTargetButton_Click(object? sender, EventArgs e)
        {
            targetPathTextBox.Clear();
            SaveConfiguration();
        }

        private void HashComparisonCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            SaveConfiguration();
        }

        private void MetadataCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            SaveConfiguration();
        }

        private void AddSourceButton_Click(object? sender, EventArgs e)
        {
            AddSource();
        }

        private void RemoveSourceButton_Click(object? sender, EventArgs e)
        {
            RemoveSelectedSource();
        }

        private void ExcludePatternsTextBox_TextChanged(object? sender, EventArgs e)
        {
            SaveConfiguration();
        }

        private async void ExportToExternalButton_Click(object? sender, EventArgs e)
        {
            await StartExportAsync(UsbExportDirection.ComputerToExternal);
        }

        private async void CompareButton_Click(object? sender, EventArgs e)
        {
            await StartCompareAsync();
        }

        private async void DeleteButton_Click(object? sender, EventArgs e)
        {
            await DeleteTargetAsync();
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            exportCancellationSource?.Cancel();
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void GitHubLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink("https://github.com/Raychan87/LrCatalogSync");
        }

        private void WebsiteLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink("https://Fototour-und-Technik.de");
        }

        private static void OpenLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch
            {
                MessageBox.Show(Strings.Get("Usb_Dialog_LinkError"), Strings.Get("Usb_Dialog_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectTargetFolder()
        {
            string initialPath = targetPathTextBox.Text;
            using var dialog = new FolderBrowserDialog
            {
                Description = Strings.Get("Usb_Dialog_SelectTarget"),
                SelectedPath = Directory.Exists(initialPath) ? initialPath : string.Empty
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            targetPathTextBox.Text = dialog.SelectedPath;

            SaveConfiguration();
            UpdateAvailability();
        }

        private string GetInitialTargetPath()
        {
            if (Path.IsPathRooted(usbExportConfig.TargetPath))
                return usbExportConfig.TargetPath;
            if (Path.IsPathRooted(usbExportConfig.ExternalRoot))
                return Path.Combine(usbExportConfig.ExternalRoot, usbExportConfig.TargetPath);
            return string.Empty;
        }

        private void LoadSources()
        {
            foreach (string source in usbExportConfig.Sources)
                sourceListBox.Items.Add(source);
        }

        private void AddSource()
        {
            using var dialog = new FolderBrowserDialog { Description = Strings.Get("Usb_Dialog_SelectSource") };
            if (dialog.ShowDialog(this) == DialogResult.OK && !sourceListBox.Items.Contains(dialog.SelectedPath))
                sourceListBox.Items.Add(dialog.SelectedPath);
            SaveConfiguration();
            UpdateAvailability();
        }

        private void RemoveSelectedSource()
        {
            if (sourceListBox.SelectedIndex >= 0)
                sourceListBox.Items.RemoveAt(sourceListBox.SelectedIndex);
            SaveConfiguration();
            UpdateAvailability();
        }

        private UsbExportRequest BuildRequest()
        {
            return new UsbExportRequest
            {
                ExternalRoot = Path.GetPathRoot(targetPathTextBox.Text) ?? string.Empty,
                TargetPath = targetPathTextBox.Text,
                Direction = UsbExportDirection.ComputerToExternal,
                Sources = sourceListBox.Items.Cast<string>().ToArray(),
                UserExcludePatterns = UsbExportManager.RemoveBuiltInPatterns(excludePatternsTextBox.Text)
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                UseHashComparison = hashComparisonCheckBox.Checked,
                TransferMetadata = metadataCheckBox.Checked && SupportsMetadataTarget()
            };
        }

        private async Task StartExportAsync(UsbExportDirection direction)
        {
            if (operationRunning || isCoordinatorRunning())
            {
                AddLogEntry(new UsbLogEntry(UsbLogSeverity.Error, Strings.Get("Usb_Log_CycleBusy")));
                return;
            }

            BeginLogAction(Strings.Get(direction == UsbExportDirection.ComputerToExternal ? "Usb_Action_Export" : "Usb_Action_Download"));
            UsbExportRequest request = BuildRequest();
            UsbExportValidationResult validation = UsbExportManager.ValidateRequest(request);
            if (!validation.IsValid)
            {
                AddLogEntry(new UsbLogEntry(UsbLogSeverity.Error, string.Format(Strings.Get("Usb_Log_ValidationError"), validation.Message)));
                if (validation.LockFiles.Count > 0)
                {
                    foreach (string lockFile in validation.LockFiles)
                        AddLogEntry(new UsbLogEntry(UsbLogSeverity.Error, string.Format(Strings.Get("Usb_Log_LockFile"), lockFile)));
                }
                return;
            }

            SaveConfiguration();
            operationRunning = true;
            exportCancellationSource = new CancellationTokenSource();
            StartOperationTimer();
            UpdateAvailability();
            AddLogEntry(Strings.Get("Usb_Log_TransferRunning"));
            ResetTransferProgress();

            var progress = new Progress<UsbLogEntry>(AddLogEntry);
            var transferProgress = new Progress<UsbExportProgress>(UpdateTransferProgress);
            var sectionProgress = new Progress<UsbExportSectionProgress>(UpdateSectionProgress);
            UsbExportResult result;
            try
            {
                result = await UsbExportManager.ExportAsync(
                    appConfig,
                    request,
                    progress,
                    transferProgress,
                    sectionProgress,
                    exportCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                result = new UsbExportResult(false, true, Strings.Get("Usb_Log_TransferCancelled"), UsbLogSeverity.Error);
            }

            exportCancellationSource.Dispose();
            exportCancellationSource = null;
            StopOperationTimer();
            operationRunning = false;
            if (result.Succeeded)
                SetTransferProgress(100);
            ((IProgress<UsbLogEntry>)progress).Report(new UsbLogEntry(GetResultSeverity(result), result.Message));
            UpdateAvailability(updateStatus: false);
        }

        private void SetTransferProgress(int percent)
        {
            int boundedPercent = Math.Clamp(percent, 0, 100);
            transferProgressBar.Value = boundedPercent;
            transferProgressLabel.Text = $"{boundedPercent}%";
        }

        private void ResetTransferProgress()
        {
            transferRateLabel.Text = string.Empty;
            sectionProgressLabel.Text = string.Empty;
            SetTransferProgress(0);
        }

        private void UpdateSectionProgress(UsbExportSectionProgress progress)
        {
            sectionProgressLabel.Text = $"{progress.Current}/{progress.Total}";
        }

        private void StartOperationTimer()
        {
            operationStopwatch.Restart();
            UpdateElapsedTime();
        }

        private void StopOperationTimer()
        {
            operationStopwatch.Stop();
            UpdateElapsedTime();
        }

        private void UpdateElapsedTime()
        {
            TimeSpan elapsed = operationStopwatch.Elapsed;
            elapsedTimeLabel.Text = $"{(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}";
        }

        private void UpdateTransferProgress(UsbExportProgress progress)
        {
            transferRateLabel.Text = progress.Speed;
            SetTransferProgress(progress.Percent);
        }

        private async Task StartCompareAsync()
        {
            if (operationRunning || isCoordinatorRunning())
            {
                AddLogEntry(new UsbLogEntry(UsbLogSeverity.Error, Strings.Get("Usb_Log_CycleBusy")));
                return;
            }

            BeginLogAction(Strings.Get("Usb_Action_Compare"));
            UsbExportRequest request = BuildRequest();
            UsbExportValidationResult validation = UsbExportManager.ValidateRequest(request);
            if (!validation.IsValid)
            {
                AddLogEntry(new UsbLogEntry(UsbLogSeverity.Error, string.Format(Strings.Get("Usb_Log_ValidationError"), validation.Message)));
                return;
            }

            operationRunning = true;
            exportCancellationSource = new CancellationTokenSource();
            StartOperationTimer();
            UpdateAvailability();
            AddLogEntry(Strings.Get("Usb_Log_CompareRunning"));
            ResetTransferProgress();
            var progress = new Progress<UsbLogEntry>(AddLogEntry);
            var checkProgress = new Progress<UsbExportProgress>(UpdateTransferProgress);
            var sectionProgress = new Progress<UsbExportSectionProgress>(UpdateSectionProgress);
            UsbExportResult result;
            try
            {
                result = await UsbExportManager.CompareAsync(appConfig, request, progress, checkProgress, sectionProgress, exportCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                result = new UsbExportResult(false, true, Strings.Get("Usb_Log_CompareCancelled"), UsbLogSeverity.Error);
            }
            exportCancellationSource.Dispose();
            exportCancellationSource = null;
            StopOperationTimer();
            operationRunning = false;
            if (result.Succeeded)
                SetTransferProgress(100);
            ((IProgress<UsbLogEntry>)progress).Report(new UsbLogEntry(GetResultSeverity(result), result.Message));
            UpdateAvailability(updateStatus: false);
        }

        private async Task DeleteTargetAsync()
        {
            if (operationRunning || isCoordinatorRunning())
                return;

            BeginLogAction(Strings.Get("Usb_Action_Delete"));

            string targetPath;
            try
            {
                targetPath = Path.GetFullPath(targetPathTextBox.Text);
            }
            catch (Exception)
            {
                AddLogEntry(new UsbLogEntry(UsbLogSeverity.Error, Strings.Get("Usb_Log_TargetInvalid")));
                return;
            }

            string? driveRoot = Path.GetPathRoot(targetPath);
            bool isDriveRoot = !string.IsNullOrWhiteSpace(driveRoot)
                && string.Equals(targetPath, Path.GetFullPath(driveRoot), StringComparison.OrdinalIgnoreCase);
            int entryCount = Directory.Exists(targetPath)
                ? Directory.EnumerateFileSystemEntries(targetPath).Count(entry =>
                    !isDriveRoot
                    || !UsbExportManager.IsSystemExcludeName(Path.GetFileName(entry)))
                : 0;
            string prompt = isDriveRoot
                ? Strings.Get("Usb_Dialog_DeleteDriveConfirm")
                : Strings.Get("Usb_Dialog_DeleteFolderConfirm");
            string confirmationMessage = string.Format(Strings.Get("Usb_Dialog_DeleteBody"), prompt, targetPath, entryCount);
            DialogResult confirmation = MessageBox.Show(
                this,
                confirmationMessage,
                Strings.Get("Usb_Dialog_DeleteTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirmation != DialogResult.Yes)
                return;

            operationRunning = true;
            exportCancellationSource = new CancellationTokenSource();
            StartOperationTimer();
            UpdateAvailability();
            AddLogEntry(string.Format(Strings.Get("Usb_Log_DeleteStarted"), targetPath, entryCount));
            UsbExportResult result;
            try
            {
                result = await UsbExportManager.ClearExternalTargetAsync(BuildRequest(), new Progress<UsbLogEntry>(AddLogEntry), exportCancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                result = new UsbExportResult(false, true, Strings.Get("Usb_Log_DeleteCancelled"), UsbLogSeverity.Error);
            }
            exportCancellationSource.Dispose();
            exportCancellationSource = null;
            StopOperationTimer();
            operationRunning = false;
            AddLogEntry(new UsbLogEntry(result.Succeeded ? UsbLogSeverity.Info : UsbLogSeverity.Error, result.Message));
            UpdateAvailability(updateStatus: false);
        }

        private void AddLogEntry(string message)
        {
            AddLogEntry(new UsbLogEntry(UsbLogSeverity.Info, message));
        }

        private void AddLogEntry(UsbLogEntry logEntry)
        {
            IReadOnlyList<string> entries = UsbExportLog.Add(logEntry.Text);
            if (!hasLogEntries)
            {
                statusLabel.Clear();
                displayedLogEntryCount = 0;
                hasLogEntries = true;
            }

            AppendStatusLogEntry(entries[^1], logEntry.Severity);
        }

        private void BeginLogAction(string action)
        {
            string actionStarted = string.Format(Strings.Get("Usb_Log_ActionStarted"), action);
            UsbExportLog.BeginAction(actionStarted);
            statusLabel.Clear();
            displayedStatusText = string.Empty;
            displayedLogEntryCount = 0;
            hasLogEntries = false;
            AppendStatusLogEntry(UsbExportLog.GetLastEntries()[^1], UsbLogSeverity.Info);
        }

        private void AppendStatusLogEntry(string entry, UsbLogSeverity severity)
        {
            statusLabel.SelectionStart = statusLabel.TextLength;
            statusLabel.SelectionLength = 0;
            statusLabel.SelectionColor = severity == UsbLogSeverity.Error ? Color.Firebrick
                : severity == UsbLogSeverity.Success ? Color.ForestGreen
                : Color.FromArgb(35, 35, 35);
            statusLabel.AppendText(entry + Environment.NewLine);
            statusLabel.SelectionColor = Color.FromArgb(35, 35, 35);
            displayedLogEntryCount++;
            hasLogEntries = true;
            displayedStatusText = string.Empty;

            if (displayedLogEntryCount > StatusLogTrimThreshold)
            {
                int trimLength = statusLabel.GetFirstCharIndexFromLine(StatusLogTrimCount);
                if (trimLength > 0)
                {
                    statusLabel.Select(0, trimLength);
                    statusLabel.SelectedText = string.Empty;
                    displayedLogEntryCount -= StatusLogTrimCount;
                }
            }

            statusLabel.SelectionStart = statusLabel.TextLength;
            statusLabel.SelectionLength = 0;
            statusLabel.ScrollToCaret();
        }

        private static UsbLogSeverity GetResultSeverity(UsbExportResult result)
        {
            return result.Severity;
        }

        private void SaveConfiguration()
        {
            if (initializingControls)
                return;

            usbExportConfig.ExternalRoot = Path.GetPathRoot(targetPathTextBox.Text) ?? string.Empty;
            usbExportConfig.TargetPath = targetPathTextBox.Text;
            usbExportConfig.Direction = UsbExportDirection.ComputerToExternal;
            usbExportConfig.Sources = sourceListBox.Items.Cast<string>().ToList();
            usbExportConfig.UserExcludePatterns = UsbExportManager.RemoveBuiltInPatterns(excludePatternsTextBox.Text);
            usbExportConfig.UseHashComparison = hashComparisonCheckBox.Checked;
            usbExportConfig.TransferMetadata = metadataCheckBox.Checked;
            usbExportConfig.Save(GlobalData.UsbExportConfigPath);
        }

        private void RefreshDrives()
        {
            UpdateDriveDetails();
            UpdateAvailability();
        }

        private void UpdateDriveDetails()
        {
            string? root = Path.GetPathRoot(targetPathTextBox.Text);
            if (string.IsNullOrWhiteSpace(root))
            {
                SetDriveDetailsText(Strings.Get("Usb_Drive_NotSelected"));
                return;
            }

            try
            {
                var drive = new DriveInfo(root);
                long freeSpace = drive.AvailableFreeSpace;
                long usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                driveDetailsLabel.Clear();
                driveDetailsLabel.SelectionColor = SystemColors.ControlText;
                driveDetailsLabel.AppendText(string.Format(Strings.Get("Usb_Drive_DetailsHeader"), drive.Name.TrimEnd('\\'), drive.DriveFormat, FormatBytes(drive.TotalSize)));
                driveDetailsLabel.SelectionColor = Color.Firebrick;
                driveDetailsLabel.AppendText(string.Format(Strings.Get("Usb_Drive_Used"), FormatBytes(usedSpace)));
                driveDetailsLabel.SelectionColor = SystemColors.ControlText;
                driveDetailsLabel.AppendText(", ");
                driveDetailsLabel.SelectionColor = Color.ForestGreen;
                driveDetailsLabel.AppendText(string.Format(Strings.Get("Usb_Drive_Free"), FormatBytes(freeSpace)));
                driveDetailsLabel.SelectionStart = 0;
                driveDetailsLabel.SelectionLength = 0;
                driveDetailsLabel.SelectionColor = SystemColors.ControlText;
            }
            catch (Exception)
            {
                SetDriveDetailsText(string.Format(Strings.Get("Usb_Drive_Unreachable"), root));
            }
        }

        private void SetDriveDetailsText(string text)
        {
            driveDetailsLabel.Clear();
            driveDetailsLabel.SelectionColor = SystemColors.ControlText;
            driveDetailsLabel.AppendText(text);
            driveDetailsLabel.SelectionStart = 0;
            driveDetailsLabel.SelectionLength = 0;
        }

        private void UpdateAvailability(bool updateStatus = true)
        {
            bool deviceSelected = !string.IsNullOrWhiteSpace(Path.GetPathRoot(targetPathTextBox.Text));
            bool coordinatorBusy = isCoordinatorRunning();
            bool configurationEnabled = !coordinatorBusy && !operationRunning;
            bool actionEnabled = deviceSelected && configurationEnabled;
            bool metadataSupported = SupportsMetadataTarget();

            targetPathTextBox.Enabled = configurationEnabled;
            browseTargetButton.Enabled = configurationEnabled;
            removeTargetButton.Enabled = configurationEnabled;
            hashComparisonCheckBox.Enabled = configurationEnabled;
            metadataCheckBox.Enabled = configurationEnabled && metadataSupported;
            sourceListBox.Enabled = configurationEnabled;
            addSourceButton.Enabled = configurationEnabled;
            removeSourceButton.Enabled = configurationEnabled;
            excludeLabel.Enabled = configurationEnabled;
            excludePatternsTextBox.Enabled = configurationEnabled;
            transferToExternalButton.Enabled = actionEnabled;
            compareButton.Enabled = actionEnabled;
            deleteButton.Enabled = actionEnabled;
            cancelButton.Enabled = operationRunning;
            exitButton.Enabled = !operationRunning;
            if (updateStatus && !operationRunning && !hasLogEntries)
            {
                string statusText = coordinatorBusy
                    ? Strings.Get("Usb_Status_CycleBusy")
                    : deviceSelected ? Strings.Get("Usb_Status_ReadyWithDrive") : Strings.Get("Usb_Status_ReadyNoDrive");

                if (!string.Equals(displayedStatusText, statusText, StringComparison.Ordinal))
                {
                    statusLabel.Text = statusText;
                    displayedStatusText = statusText;
                }
            }
        }

        private bool SupportsMetadataTarget()
        {
            string? root = Path.GetPathRoot(targetPathTextBox.Text);
            if (string.IsNullOrWhiteSpace(root))
                return false;

            try
            {
                var drive = new DriveInfo(root);
                return drive.IsReady && string.Equals(drive.DriveFormat, "NTFS", StringComparison.OrdinalIgnoreCase);
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private void UsbExportForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (operationRunning)
            {
                DialogResult result = MessageBox.Show(this, Strings.Get("Usb_Dialog_ConfirmCancel"), Strings.Get("Usb_Dialog_Title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    exportCancellationSource?.Cancel();
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = true;
                }
                return;
            }

            if (isCoordinatorRunning())
            {
                e.Cancel = true;
                MessageBox.Show(this, Strings.Get("Usb_Dialog_CloseWhileSync"), Strings.Get("Usb_Dialog_Title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveConfiguration();
        }

        private void ApplyLocalization()
        {
            Text = string.Format(Strings.Get("Usb_Title"), GetApplicationVersion());
            targetGroup.Text = Strings.Get("Usb_Group_Target");
            targetPathLabel.Text = Strings.Get("Usb_Label_TargetPath");
            browseTargetButton.Text = Strings.Get("Usb_Button_Browse");
            removeTargetButton.Text = Strings.Get("Usb_Button_Remove");
            driveInfoLabel.Text = Strings.Get("Usb_Label_Drive");
            driveDetailsLabel.Text = Strings.Get("Usb_Drive_NotSelected");
            deleteButton.Text = Strings.Get("Usb_Button_ClearTarget");
            metadataCheckBox.Text = Strings.Get("Usb_Check_Metadata");
            hashComparisonCheckBox.Text = Strings.Get("Usb_Check_Hash");
            sourceGroup.Text = Strings.Get("Usb_Group_Sources");
            addSourceButton.Text = Strings.Get("Usb_Button_Add");
            removeSourceButton.Text = Strings.Get("Usb_Button_Remove");
            excludeLabel.Text = Strings.Get("Usb_Label_Exclude");
            excludePatternsTextBox.PlaceholderText = Strings.Get("Usb_Placeholder_Exclude");
            logGroup.Text = Strings.Get("Usb_Group_StatusLog");
            transferToExternalButton.Text = Strings.Get("Usb_Button_Transfer");
            compareButton.Text = Strings.Get("Usb_Button_Compare");
            cancelButton.Text = Strings.Get("Usb_Button_Cancel");
            exitButton.Text = Strings.Get("Usb_Button_Exit");
        }

        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int unit = 0;
            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }
            return $"{value:0.##} {units[unit]}";
        }

        private static string GetApplicationVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "unbekannt";
        }

        private static Icon LoadIcon(string resourceName)
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            return stream != null ? new Icon(stream) : SystemIcons.Application;
        }
    }
}
