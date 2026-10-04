using System.Diagnostics;
using System.Reflection;

using LrCatalogSync.Infrastructure;
using LrCatalogSync.Resources.Strings;

namespace LrCatalogSync.UI
{
    public partial class SettingsForm : Form
    {
        private readonly string appVersion = GetApplicationVersion();
        private AppConfig config = null!;
        private string originalPasswordRclone = string.Empty;
        private string originalPasswordAes = string.Empty;
        public SettingsForm()
        {
            InitializeComponent();
            ApplyLocalization();
        }

        public SettingsForm(AppConfig cfg)
        {
            InitializeComponent();
            config = cfg;
            originalPasswordRclone = cfg.SambaPasswordRclone;
            originalPasswordAes = cfg.SambaPasswordAes;

            cmbLanguage.DataSource = Localization.Languages.ToArray();
            cmbLanguage.DisplayMember = nameof(LanguageOption.DisplayName);
            cmbLanguage.ValueMember = nameof(LanguageOption.Code);
            cmbLanguage.SelectedValue = cfg.Language;
            ApplyLocalization();

            Icon = LoadIcon("LrCatalogSync.Resources.Icons.app_icon.ico");
            txtRcloneFolder.Text = cfg.RcloneFolder;
            txtGlobalCycleInterval.Text = cfg.GlobalCycleInterval.ToString();
            txtCatalogLocalFile.Text = cfg.CatalogLocalFile;
            txtCatalogRemotePath.Text = cfg.CatalogRemotePath;
            txtRcloneCopyFolderName.Text = cfg.RcloneCopyFolderName;
            txtBackupsLocalPath.Text = cfg.BackupsLocalPath;
            txtBackupsRemotePath.Text = cfg.BackupsRemotePath;
            txtRemoteIP.Text = cfg.RemoteIP;
            txtSambaUser.Text = cfg.SambaUser;
            cmbLogLevel.SelectedItem = cfg.LogLevel;
            chkAutoRun.Checked = cfg.AutoRun;
            chkSyncPreviewData.Checked = cfg.SyncPreviewData;
            chkEnableRcloneCopy.Checked = cfg.EnableRcloneCopy;
            chkEnableBackups.Checked = cfg.EnableBackups;
            LoadSettings();
            UpdateBackupControls();
            UpdateRcloneCopyControls();
        }

        private void ChkEnableBackups_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateBackupControls();
        }

        private void ChkEnableRcloneCopy_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateRcloneCopyControls();
        }

        // Die Checkbox selbst bleibt bedienbar, daher werden nur die Felder darunter deaktiviert
        private void UpdateBackupControls()
        {
            bool enabled = chkEnableBackups.Checked;
            backupsLocalPathLabel.Enabled = enabled;
            txtBackupsLocalPath.Enabled = enabled;
            browseBackupsPathButton.Enabled = enabled;
            backupsRemotePathLabel.Enabled = enabled;
            txtBackupsRemotePath.Enabled = enabled;
        }

        private void UpdateRcloneCopyControls()
        {
            bool enabled = chkEnableRcloneCopy.Checked;
            rcloneCopyFolderNameLabel.Enabled = enabled;
            txtRcloneCopyFolderName.Enabled = enabled;
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button button || button.Tag is not TextBox textBox)
                return;

            string path = textBox == txtCatalogLocalFile
                ? BrowseFile(Strings.Settings_Filter_Catalog)
                : BrowseFolder();

            if (!string.IsNullOrEmpty(path))
                textBox.Text = path;
        }

        private void RcloneDownloadLabel_Click(object? sender, EventArgs e)
        {
            OpenLink("https://rclone.org/downloads/");
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
                MessageBox.Show(Strings.Settings_Dialog_LinkError, Strings.Settings_Title_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string BrowseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = Strings.Settings_Dialog_SelectFolder;
                if (dialog.ShowDialog() == DialogResult.OK)
                    return dialog.SelectedPath ?? string.Empty;
            }
            return string.Empty;
        }

        private string BrowseFile(string filter)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = Strings.Settings_Dialog_SelectFile;
                dialog.Filter = filter;
                if (dialog.ShowDialog() == DialogResult.OK)
                    return dialog.FileName ?? string.Empty;
            }
            return string.Empty;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                config.RcloneFolder = GetControlValue("txtRcloneFolder");
                config.CatalogLocalFile = GetControlValue("txtCatalogLocalFile");
                config.BackupsLocalPath = GetControlValue("txtBackupsLocalPath");
                config.BackupsRemotePath = GetControlValue("txtBackupsRemotePath");
                config.EnableBackups = GetCheckBoxValue("chkEnableBackups");
                config.EnableRcloneCopy = GetCheckBoxValue("chkEnableRcloneCopy");
                config.RcloneCopyFolderName = GetControlValue("txtRcloneCopyFolderName");
                config.SyncPreviewData = GetCheckBoxValue("chkSyncPreviewData");
                config.RemoteIP = GetControlValue("txtRemoteIP");
                config.CatalogRemotePath = GetControlValue("txtCatalogRemotePath");
                config.SambaUser = GetControlValue("txtSambaUser");
                config.LogLevel = GetControlValue("cmbLogLevel");
                config.Language = cmbLanguage.SelectedValue?.ToString() ?? Localization.SystemCode;
                config.AutoRun = GetCheckBoxValue("chkAutoRun");

                if (!ValidateRemotePath(ref config.CatalogRemotePath, "Remote Katalog Pfad") ||
                    !ValidateRemotePath(ref config.BackupsRemotePath, "Remote Backup Pfad"))
                    return;

                if (!int.TryParse(GetControlValue("txtGlobalCycleInterval"), out int globalCycleInterval) || globalCycleInterval <= 0)
                {
                    MessageBox.Show(
                        Strings.Settings_Dialog_InvalidInterval,
                        Strings.Settings_Dialog_InvalidIntervalTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (globalCycleInterval > 999)
                {
                    MessageBox.Show(
                        Strings.Settings_Dialog_IntervalRange,
                        Strings.Settings_Dialog_IntervalRangeTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                config.GlobalCycleInterval = globalCycleInterval;

                string rcloneFolder = config.RcloneFolder;
                string absoluteRcloneFolder = Path.IsPathRooted(rcloneFolder)
                    ? rcloneFolder
                    : Path.GetFullPath(Path.Combine(GlobalData.BaseDir, rcloneFolder));
                string absoluteRclonePath = Path.Combine(absoluteRcloneFolder, "rclone.exe");

                if (!File.Exists(absoluteRclonePath))
                {
                    MessageBox.Show(
                        string.Format(Strings.Settings_Dialog_RcloneMissing, absoluteRclonePath),
                        Strings.Settings_Dialog_RcloneMissingTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(config.CatalogLocalFile))
                {
                    MessageBox.Show(Strings.Settings_Dialog_CatalogRequired, Strings.Settings_Dialog_CatalogRequiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(config.CatalogLocalFile))
                {
                    MessageBox.Show(
                        string.Format(Strings.Settings_Dialog_CatalogMissing, config.CatalogLocalFile),
                        Strings.Settings_Dialog_CatalogMissingTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (!config.CatalogLocalFile.EndsWith(".lrcat", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        string.Format(Strings.Settings_Dialog_CatalogInvalid, config.CatalogLocalFile),
                        Strings.Settings_Dialog_CatalogInvalidTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (config.EnableBackups)
                {
                    if (string.IsNullOrEmpty(config.BackupsLocalPath))
                    {
                        MessageBox.Show(Strings.Settings_Dialog_BackupLocalRequired, Strings.Settings_Dialog_BackupLocalRequiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!Directory.Exists(config.BackupsLocalPath))
                    {
                        MessageBox.Show(
                            string.Format(Strings.Settings_Dialog_BackupLocalMissing, config.BackupsLocalPath),
                            Strings.Settings_Dialog_BackupLocalMissingTitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(config.BackupsRemotePath))
                    {
                        MessageBox.Show(Strings.Settings_Dialog_BackupRemoteRequired, Strings.Settings_Dialog_BackupRemoteRequiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (config.EnableRcloneCopy && string.IsNullOrEmpty(config.RcloneCopyFolderName))
                {
                    MessageBox.Show(Strings.Settings_Dialog_CopyFolderRequired, Strings.Settings_Dialog_CopyFolderRequiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string passwordInput = GetControlValue("txtSambaPassword");
                if (string.IsNullOrEmpty(passwordInput) || passwordInput == "****")
                {
                    config.SambaPasswordRclone = originalPasswordRclone;
                    config.SambaPasswordAes = originalPasswordAes;
                }
                else
                {
                    config.SambaPasswordRclone = ObscurePassword(passwordInput, absoluteRclonePath);
                    config.SambaPasswordAes = Cryptor.Encrypt(passwordInput);
                }

                string configDir = Path.Combine(GlobalData.BaseDir, "data", "config");
                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);

                config.Save(GlobalData.LrCatSyncConfigPath);
                SaveRcloneConfig();
                Localization.Apply(config.Language);

                if (config.AutoRun)
                    Autorun.Enable(Application.ExecutablePath);
                else
                    Autorun.Disable();

                MessageBox.Show(Strings.Settings_Dialog_SaveSuccess, Strings.Settings_Title_Success, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.Settings_Dialog_SaveFailure, ex.Message), Strings.Settings_Title_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string GetControlValue(string controlName)
        {
            var control = this.Controls.Find(controlName, true);
            if (control.Length > 0)
            {
                if (control[0] is TextBox tb)
                    return tb.Text;
                if (control[0] is ComboBox cb)
                    return cb.SelectedItem?.ToString() ?? "";
            }
            return "";
        }

        private bool GetCheckBoxValue(string controlName)
        {
            var control = this.Controls.Find(controlName, true);
            if (control.Length > 0 && control[0] is CheckBox cb)
                return cb.Checked;
            return false;
        }

        private bool ValidateRemotePath(ref string remotePath, string fieldName)
        {
            string trimmedRemotePath = remotePath.Trim().Replace('\\', '/');

            if (trimmedRemotePath.Length >= 2 &&
                char.IsLetter(trimmedRemotePath[0]) &&
                trimmedRemotePath[1] == ':')
            {
                MessageBox.Show(
                    string.Format(Strings.Settings_Dialog_RemotePathInvalid, fieldName),
                    string.Format(Strings.Settings_Dialog_RemotePathInvalidTitle, fieldName),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            remotePath = trimmedRemotePath.StartsWith('/')
                ? trimmedRemotePath
                : $"/{trimmedRemotePath}";

            return true;
        }

        private void ApplyLocalization()
        {
            Text = string.Format(Strings.Settings_Title, appVersion);
            generalGroup.Text = Strings.Settings_Group_General;
            chkAutoRun.Text = Strings.Settings_AutoRun;
            rcloneFolderLabel.Text = Strings.Settings_RcloneFolder;
            rcloneDownloadLabel.Text = Strings.Settings_RcloneDownload;
            logLevelLabel.Text = Strings.Settings_LogLevel;
            languageLabel.Text = Strings.Settings_Language;
            updateIntervalLabel.Text = Strings.Settings_UpdateInterval;
            secondsLabel.Text = Strings.Settings_Seconds;
            catalogGroup.Text = Strings.Settings_Group_Catalog;
            chkSyncPreviewData.Text = Strings.Settings_SyncPreviews;
            catalogLocalFileLabel.Text = Strings.Settings_CatalogLocal;
            catalogRemotePathLabel.Text = Strings.Settings_CatalogRemote;
            chkEnableRcloneCopy.Text = Strings.Settings_KeepCatalog;
            rcloneCopyFolderNameLabel.Text = Strings.Settings_FolderName;
            backupGroup.Text = Strings.Settings_Group_Backups;
            chkEnableBackups.Text = Strings.Settings_SyncBackups;
            backupsLocalPathLabel.Text = Strings.Settings_BackupLocal;
            backupsRemotePathLabel.Text = Strings.Settings_BackupRemote;
            sambaGroup.Text = Strings.Settings_Group_Samba;
            remoteIpLabel.Text = Strings.Settings_Server;
            sambaUserLabel.Text = Strings.Settings_Username;
            sambaPasswordLabel.Text = Strings.Settings_Password;
            saveButton.Text = Strings.Settings_Save;
            cancelButton.Text = Strings.Settings_Cancel;

            settingsToolTip.SetToolTip(chkAutoRun, Strings.Settings_Tip_AutoRun);
            settingsToolTip.SetToolTip(txtRcloneFolder, Strings.Settings_Tip_RcloneFolder);
            settingsToolTip.SetToolTip(txtGlobalCycleInterval, Strings.Settings_Tip_Interval);
            settingsToolTip.SetToolTip(chkSyncPreviewData, Strings.Settings_Tip_SyncPreviews);
            settingsToolTip.SetToolTip(txtCatalogLocalFile, Strings.Settings_Tip_CatalogLocal);
            settingsToolTip.SetToolTip(txtCatalogRemotePath, Strings.Settings_Tip_CatalogRemote);
            settingsToolTip.SetToolTip(chkEnableRcloneCopy, Strings.Settings_Tip_KeepCatalog);
            settingsToolTip.SetToolTip(txtRcloneCopyFolderName, Strings.Settings_Tip_FolderName);
            settingsToolTip.SetToolTip(chkEnableBackups, Strings.Settings_Tip_SyncBackups);
            settingsToolTip.SetToolTip(txtBackupsLocalPath, Strings.Settings_Tip_BackupLocal);
            settingsToolTip.SetToolTip(txtBackupsRemotePath, Strings.Settings_Tip_BackupRemote);
            settingsToolTip.SetToolTip(txtRemoteIP, Strings.Settings_Tip_Server);
            settingsToolTip.SetToolTip(txtSambaUser, Strings.Settings_Tip_Username);
            settingsToolTip.SetToolTip(txtSambaPassword, Strings.Settings_Tip_Password);

#if DEBUG
            CheckLocalizedControlWidths(this);
#endif
        }

#if DEBUG
        private static void CheckLocalizedControlWidths(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (!string.IsNullOrEmpty(control.Text))
                {
                    int textWidth = TextRenderer.MeasureText(
                        control.Text,
                        control.Font,
                        new Size(int.MaxValue, control.ClientSize.Height),
                        TextFormatFlags.NoPadding).Width;
                    if (textWidth > control.ClientSize.Width)
                        Log.Debug(string.Format(Strings.Settings_Debug_TextTooWide, control.Name, textWidth, control.ClientSize.Width));
                }

                CheckLocalizedControlWidths(control);
            }
        }
#endif

        private string ObscurePassword(string? password, string rcloneExePath)
        {
            try
            {
                string passwordArg = password ?? string.Empty;
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = rcloneExePath,
                    Arguments = $"obscure \"{passwordArg}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process p = RcloneProcessManager.Start(psi)!)
                {
                    string result = p.StandardOutput.ReadToEnd().Trim();
                    p.WaitForExit();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SettingsForm: {string.Format(Strings.Get("Log_Settings_PasswordObscureFailed"), ex.Message)}");
                throw;
            }
        }

        private void SaveRcloneConfig()
        {
            string[] lines = new string[]
            {
                $"[{GlobalConst.REMOTE_NAME}]",
                "type = smb",
                $"host = {config.RemoteIP}",
                $"user = {config.SambaUser}",
                $"pass = {config.SambaPasswordRclone}"
            };

            File.WriteAllLines(GlobalData.LrCatSyncRcloneConfigPath, lines);
            Log.Debug($"Config: {Strings.Get("Log_Settings_RcloneConfigCreated")}");
        }

        // ==================== HILFSMETHODEN FÜR VERSION UND EINSTELLUNGEN ====================
        private static string GetApplicationVersion() //Aus Assembly-Informationen auslesen
        {
            // Lese die Version direkt aus der Assembly Information
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttr = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>();
            if (versionAttr?.InformationalVersion != null)
            {
                // Entferne den Hash-Teil nach dem "+"
                string version = versionAttr.InformationalVersion;
                int plusIndex = version.IndexOf('+');
                if (plusIndex > 0)
                {
                    version = version.Substring(0, plusIndex);
                }
                return version;
            }
            
            // Fallback auf FileVersion
            return FileVersionInfo.GetVersionInfo(AppContext.BaseDirectory + "LRCatalogSync.exe").ProductVersion ?? "0.0.0.0";
        }

        private static Icon LoadIcon(string resourceName)
        {
            using var stream = typeof(SettingsForm).Assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Icon-Ressource nicht gefunden: {resourceName}");
            using var icon = new Icon(stream);
            return new Icon(icon, icon.Width, icon.Height);
        }
        
        private void LoadSettings()
        {
            var passwordControl = this.Controls.Find("txtSambaPassword", true);
            if (passwordControl.Length > 0 && (!string.IsNullOrEmpty(originalPasswordRclone) || !string.IsNullOrEmpty(originalPasswordAes)))
            {
                ((TextBox)passwordControl[0]).Text = "****";
            }
            
            // Setze Standardwerte für rclone copy, falls noch nicht gesetzt
            var chkEnableRcloneCopy = this.Controls.Find("chkEnableRcloneCopy", true);
            if (chkEnableRcloneCopy.Length > 0)
            {
                ((CheckBox)chkEnableRcloneCopy[0]).Checked = config.EnableRcloneCopy;
            }
            
            var txtRcloneCopyFolderName = this.Controls.Find("txtRcloneCopyFolderName", true);
            if (txtRcloneCopyFolderName.Length > 0)
            {
                ((TextBox)txtRcloneCopyFolderName[0]).Text = config.RcloneCopyFolderName;
            }

            // Setze Autorun Checkbox
            var chkAutoRun = this.Controls.Find("chkAutoRun", true);
            if (chkAutoRun.Length > 0)
            {
                ((CheckBox)chkAutoRun[0]).Checked = config.AutoRun;
            }
        }
    }
}