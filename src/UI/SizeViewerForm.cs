using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Reflection;

using LrCatalogSync.Core;
using LrCatalogSync.Infrastructure;
using LrCatalogSync.Resources.Strings;

namespace LrCatalogSync.UI
{
    // SizeViewer: zeigt die Größe von Verzeichnissen als Baumtabelle.
    // Verändert nichts, außer der Benutzer löscht einen Eintrag (Entf, über die Windows-Shell in den Papierkorb).
    public partial class SizeViewerForm : Form
    {
        // Spaltenindizes, entsprechen der Reihenfolge im Designer
        private const int ColumnName = 0;
        private const int ColumnSize = 1;
        private const int ColumnAllocated = 2;
        private const int ColumnFiles = 3;
        private const int ColumnFolders = 4;
        private const int ColumnPercent = 5;
        private const int ColumnModified = 6;

        // Maße der Baumdarstellung in Pixeln bei 96 dpi
        private const int IndentWidth = 16;
        private const int GlyphWidth = 16;
        private const int IconSize = 16;
        private const int CellPadding = 4;
        private const int SizeBoxWidth = 68;        // Feld mit Größe und Balken zwischen Symbol und Name
        private const int ColumnLineOffset = 2;     // Abstand der Spaltenlinie vom rechten Zellenrand (wie Windows)

        // Symbole der Schaltflächen aus der Windows-Symbolschrift
        private const string GlyphFontName = "Segoe MDL2 Assets";
        private const float GlyphFontSize = 11F;
        private const string GlyphSelectFolder = "\uE838";
        private const string GlyphCancel = "\uE711";
        private const string GlyphRefresh = "\uE72C";
        private static readonly Color GlyphSelectFolderColor = Color.FromArgb(210, 153, 34);

        // Windows-Fehlercode: Abfrage der Benutzerkontensteuerung abgelehnt (ERROR_CANCELLED)
        private const int ErrorCancelled = 1223;

        // Fehlercode der Windows-Shell: Löschen vom Benutzer abgebrochen (DE_OPCANCELLED)
        private const int ShellOperationCancelled = 0x75;

        private const string SortArrowAscending = " ▲";
        private const string SortArrowDescending = " ▼";

        private readonly List<SizeNode> visibleRows = new();    // Aktuell sichtbare Zeilen (aufgeklappter Baum)
        private readonly Stopwatch scanStopwatch = new();
        private readonly Bitmap folderImage;
        private readonly Bitmap fileImage;
        private readonly string[] columnTitles = new string[7];
        private readonly List<Image> buttonImages = new();
        private readonly SizeViewerConfig viewConfig = new();
        private readonly string? startupPath;                   // Nur beim Start mit Administratorrechten gesetzt
        private CancellationTokenSource? scanCancellationSource;
        private SizeScanProgress? scanProgress;
        private SizeNode? rootNode;
        private SizeScanResult? lastResult;                     // Letzter abgeschlossener Scan (für die Statuszeile)
        private string? scannedPath;
        private bool scanRunning;
        private int sortColumn = ColumnAllocated;
        private bool sortDescending = true;

        // Start mit vorgegebenem Verzeichnis (eigenständiger SizeViewer mit Administratorrechten)
        public SizeViewerForm(string? startupPath) : this()
        {
            this.startupPath = startupPath;
        }

        public SizeViewerForm()
        {
            InitializeComponent();

            // Doppelpufferung gegen Flackern der selbst gezeichneten Zeilen. Die Eigenschaft ist geschützt;
            // per Reflection gesetzt bleibt im Designer eine Standard-ListView statt einer eigenen Klasse.
            typeof(Control)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(resultListView, true);

            Icon = LoadIcon("LrCatalogSync.Resources.Icons.app_icon.ico");
            folderImage = LoadStockImage(StockIconId.Folder);
            fileImage = LoadStockImage(StockIconId.DocumentNoAssociation);
            statusTimer.Interval = GlobalConst.SIZEVIEWER_STATUS_INTERVAL_MS;

            SetButtonImage(selectFolderButton, CreateGlyphImage(GlyphSelectFolder, GlyphSelectFolderColor));
            SetButtonImage(cancelScanButton, CreateGlyphImage(GlyphCancel, Theme.Current.Error));
            SetButtonImage(refreshButton, CreateGlyphImage(GlyphRefresh, Theme.Current.Link));
            SetButtonImage(adminButton, LoadStockImage(StockIconId.Shield));

            // Mit Administratorrechten gibt es nichts mehr anzufordern
            adminButton.Visible = !Environment.IsPrivilegedProcess;

            try
            {
                viewConfig = SizeViewerConfig.Load(GlobalData.SizeViewerConfigPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Nicht lesbare Ansichtseinstellungen: Standardansicht verwenden
            }

            ApplyViewConfig();
            ApplyLocalization();
            Theme.ApplyTo(this);
            Theme.ApplyTo(rowContextMenu);
            UpdateCommandState();
        }

        // ==================== EVENT-HANDLER (im Designer verdrahtet) ====================

        private async void SelectFolderButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = Strings.Get("Size_Dialog_SelectFolder"),
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (scannedPath != null)
                dialog.InitialDirectory = scannedPath;

            if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                await StartScanAsync(dialog.SelectedPath);
        }

        private void CancelScanButton_Click(object? sender, EventArgs e)
        {
            scanCancellationSource?.Cancel();
        }

        private async void RefreshButton_Click(object? sender, EventArgs e)
        {
            if (scannedPath != null)
                await StartScanAsync(scannedPath);
        }

        // Startet das Programm mit Administratorrechten neu, damit auch geschützte Ordner gelesen werden
        private void AdminButton_Click(object? sender, EventArgs e)
        {
            string? executable = Environment.ProcessPath;
            if (scanRunning || string.IsNullOrEmpty(executable))
                return;

            SaveViewConfig();

            try
            {
                var startInfo = new ProcessStartInfo(executable)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                startInfo.ArgumentList.Add(GlobalConst.SIZEVIEWER_ARG);

                // Eigene Prozess-ID mitgeben: der neue SizeViewer beendet sich, sobald LrCatalogSync endet
                startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
                if (scannedPath != null)
                    startInfo.ArgumentList.Add(scannedPath);

                using Process? process = Process.Start(startInfo);
                Log.Info($"SizeViewer: {Strings.Get("Log_SizeViewer_AdminStarted")}");
                Close();
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
            {
                // Abfrage der Benutzerkontensteuerung wurde abgelehnt: Fenster bleibt unverändert offen
            }
            catch (Exception ex)
            {
                Log.Error($"SizeViewer: {string.Format(Strings.Get("Log_SizeViewer_AdminFailed"), ex.Message)}");
                MessageForm.Show(
                    this,
                    string.Format(Strings.Get("Size_Dialog_AdminFailed"), ex.Message),
                    Strings.Get("Size_Dialog_Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void GitHubLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink(GlobalConst.GITHUB_URL);
        }

        private void WebsiteLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink(GlobalConst.WEBSITE_URL);
        }

        private async void SizeViewerForm_Shown(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(startupPath))
                await StartScanAsync(startupPath);
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            if (scanRunning && scanProgress != null)
            {
                statusLabel.Text = string.Format(
                    Strings.Get("Size_Status_Scanning"),
                    scanProgress.FileCount.ToString("N0"),
                    scanProgress.CurrentDirectory);

                // Zwischenstand anzeigen: der Baum wächst, während der Scan läuft
                if (rootNode == null && scanProgress.Root != null)
                {
                    rootNode = scanProgress.Root;
                    rootNode.Expanded = true;
                }

                if (rootNode != null)
                    RefreshVisibleRows();
            }
        }

        private void SizeViewerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Laufenden Scan beenden, das Ergebnis wird nicht mehr angezeigt
            scanCancellationSource?.Cancel();
            SaveViewConfig();
        }

        private void SizeViewerForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            foreach (Image image in buttonImages)
                image.Dispose();

            folderImage.Dispose();
            fileImage.Dispose();
        }

        private void ResultListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e)
        {
            SizeNode? node = e.ItemIndex >= 0 && e.ItemIndex < visibleRows.Count ? visibleRows[e.ItemIndex] : null;

            var item = new ListViewItem(node != null ? GetCellText(node, ColumnName) : string.Empty);
            for (int column = 1; column < resultListView.Columns.Count; column++)
                item.SubItems.Add(node != null ? GetCellText(node, column) : string.Empty);

            e.Item = item;
        }

        private void ResultListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            // Im hellen Modus zeichnet Windows die Spaltenköpfe wie gewohnt
            if (!Theme.Current.IsDark)
            {
                e.DrawDefault = true;
                return;
            }

            using (var backBrush = new SolidBrush(Theme.Current.Raised))
                e.Graphics.FillRectangle(backBrush, e.Bounds);

            using (var borderPen = new Pen(Theme.Current.Border))
            {
                int lineX = e.Bounds.Right - ColumnLineOffset;
                e.Graphics.DrawLine(borderPen, lineX, e.Bounds.Top, lineX, e.Bounds.Bottom);
                e.Graphics.DrawLine(borderPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            TextFormatFlags alignment = e.Header?.TextAlign switch
            {
                HorizontalAlignment.Right => TextFormatFlags.Right,
                HorizontalAlignment.Center => TextFormatFlags.HorizontalCenter,
                _ => TextFormatFlags.Left
            };
            DrawCellText(e.Graphics, e.Header?.Text ?? string.Empty, e.Bounds, Theme.Current.Text, alignment);
        }

        private void ResultListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= visibleRows.Count)
                return;

            SizeNode node = visibleRows[e.ItemIndex];
            bool selected = resultListView.SelectedIndices.Contains(e.ItemIndex);

            // Für die erste Spalte liefert Windows die ganze Zeile: Position und Breite selbst bestimmen
            Rectangle bounds = e.ColumnIndex == ColumnName ? GetNameCellBounds(e.ItemIndex) : e.Bounds;

            ThemePalette palette = Theme.Current;
            Color backColor = palette.Surface;
            Color foreColor = node.AccessDenied ? palette.Muted : palette.Text;
            if (selected)
            {
                backColor = resultListView.Focused ? palette.Selection : palette.InactiveSelection;
                foreColor = resultListView.Focused ? palette.OnSelection : palette.Text;
            }

            using (var backBrush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(backBrush, bounds);

            switch (e.ColumnIndex)
            {
                case ColumnName:
                    DrawNameCell(e.Graphics, node, bounds, foreColor);
                    break;
                case ColumnPercent:
                    DrawPercentCell(e.Graphics, node, bounds);
                    break;
                case ColumnModified:
                    DrawCellText(e.Graphics, GetCellText(node, e.ColumnIndex), bounds, foreColor, TextFormatFlags.HorizontalCenter);
                    break;
                default:
                    DrawCellText(e.Graphics, GetCellText(node, e.ColumnIndex), bounds, foreColor, TextFormatFlags.Right);
                    break;
            }

            // Spaltenlinie am rechten Zellenrand: setzt die Linien fort, die Windows im dunklen Modus
            // in den leeren Bereich unter den Zeilen zeichnet
            if (palette.IsDark && !selected)
            {
                using var linePen = new Pen(palette.ColumnLine);
                int lineX = bounds.Right - ColumnLineOffset;
                e.Graphics.DrawLine(linePen, lineX, bounds.Top, lineX, bounds.Bottom - 1);
            }
        }

        private void ResultListView_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (e.Column == sortColumn)
            {
                sortDescending = !sortDescending;
            }
            else
            {
                // Namen zuerst aufsteigend, Zahlen und Datum zuerst absteigend (größte/neueste oben)
                sortColumn = e.Column;
                sortDescending = e.Column != ColumnName;
            }

            UpdateColumnHeaders();
            RebuildVisibleRows(GetSelectedNode());
        }

        private void ResultListView_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            int index = GetRowIndexAt(e.Location);
            if (index >= 0 && IsOnGlyph(index, e.X))
                ToggleNode(index);
        }

        private void ResultListView_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // Auf dem Pfeil hat bereits MouseDown umgeschaltet
            int index = GetRowIndexAt(e.Location);
            if (index >= 0 && !IsOnGlyph(index, e.X))
                ToggleNode(index);
        }

        private void ResultListView_KeyDown(object? sender, KeyEventArgs e)
        {
            int index = GetSelectedIndex();
            if (index < 0)
                return;

            SizeNode node = visibleRows[index];
            if (e.KeyCode == Keys.Right)
            {
                if (node.HasChildren && !node.Expanded)
                    ToggleNode(index);

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (node.HasChildren && node.Expanded)
                    ToggleNode(index);
                else if (node.Parent != null)
                    SelectRow(visibleRows.IndexOf(node.Parent));

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedNode();
                e.Handled = true;
            }
        }

        private void RowContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SizeNode? node = GetSelectedNode();
            if (node == null)
            {
                e.Cancel = true;
                return;
            }

            deleteMenuItem.Enabled = CanDelete(node);
        }

        private void DeleteMenuItem_Click(object? sender, EventArgs e)
        {
            DeleteSelectedNode();
        }

        private void OpenInExplorerMenuItem_Click(object? sender, EventArgs e)
        {
            SizeNode? node = GetSelectedNode();
            if (node == null)
                return;

            try
            {
                // Dateien werden im Explorer markiert, Verzeichnisse direkt geöffnet
                string path = GetFullPath(node);
                string arguments = node.IsDirectory || node.IsFileGroup ? $"\"{path}\"" : $"/select,\"{path}\"";
                using Process? process = Process.Start(new ProcessStartInfo("explorer.exe", arguments));
            }
            catch (Exception ex)
            {
                Log.Error($"SizeViewer: {string.Format(Strings.Get("Log_SizeViewer_ExplorerFailed"), ex.Message)}");
                MessageForm.Show(
                    this,
                    Strings.Get("Size_Dialog_ExplorerFailed"),
                    Strings.Get("Size_Dialog_Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==================== LÖSCHEN ====================

        // Der Wurzelordner des Scans und die Sammelzeile "[n Dateien]" (kein echter Ordner) sind nicht löschbar.
        // Während eines Scans wird der Baum noch aufgebaut und darf nicht verändert werden.
        private bool CanDelete(SizeNode node)
        {
            return !scanRunning && node.Parent != null && !node.IsFileGroup;
        }

        // Löscht die markierte Zeile wie der Explorer: Windows fragt nach, verschiebt in den Papierkorb
        // und zeigt Fortschritt und Fehler selbst an.
        private void DeleteSelectedNode()
        {
            SizeNode? node = GetSelectedNode();
            if (node == null || !CanDelete(node))
                return;

            string path = GetFullPath(node);

            try
            {
                int result = RecycleBin.Delete(Handle, path);

                // Maßgeblich ist, ob der Eintrag noch existiert: bei "Nein" oder Abbruch bleibt alles unverändert
                bool stillExists = node.IsDirectory ? Directory.Exists(path) : File.Exists(path);
                if (stillExists)
                {
                    if (result != 0 && result != ErrorCancelled && result != ShellOperationCancelled)
                        ReportDeleteFailed(path, string.Format(Strings.Get("Size_Dialog_DeleteErrorCode"), result));

                    return;
                }

                Log.Info($"SizeViewer: {string.Format(Strings.Get("Log_SizeViewer_Deleted"), path)}");
                RemoveNode(node);
            }
            catch (Exception ex)
            {
                ReportDeleteFailed(path, ex.Message);
            }
        }

        private void ReportDeleteFailed(string path, string reason)
        {
            Log.Error($"SizeViewer: {string.Format(Strings.Get("Log_SizeViewer_DeleteFailed"), path, reason)}");
            MessageForm.Show(
                this,
                string.Format(Strings.Get("Size_Dialog_DeleteFailed"), path, reason),
                Strings.Get("Size_Dialog_Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        // Entfernt einen gelöschten Eintrag aus dem Baum und zieht seine Werte bis zur Wurzel ab (kein neuer Scan)
        private void RemoveNode(SizeNode node)
        {
            SizeNode? parent = node.Parent;
            if (parent?.Children == null)
                return;

            int index = visibleRows.IndexOf(node);
            parent.Children.Remove(node);

            long directoryCount = node.IsDirectory ? node.DirCount + 1 : 0;
            for (SizeNode? current = parent; current != null; current = current.Parent)
            {
                current.Size -= node.Size;
                current.Allocated -= node.Allocated;
                current.FileCount -= node.FileCount;
                current.DirCount -= directoryCount;
            }

            // Eine leere Sammelzeile "[n Dateien]" verschwindet mit ihrer letzten Datei
            if (parent.IsFileGroup && parent.Children.Count == 0)
                parent.Parent?.Children?.Remove(parent);

            RefreshVisibleRows();

            // Die Zeile an derselben Stelle markieren, damit sich mit Entf mehrere Einträge nacheinander löschen lassen
            SelectRow(Math.Min(index, visibleRows.Count - 1));
            UpdateResultStatus();
        }

        // ==================== SCAN ====================

        private async Task StartScanAsync(string path)
        {
            if (scanRunning)
                return;

            scanRunning = true;
            scannedPath = path;
            rootNode = null;
            lastResult = null;
            RebuildVisibleRows(null);

            var cancellationSource = new CancellationTokenSource();
            var progress = new SizeScanProgress();
            scanCancellationSource = cancellationSource;
            scanProgress = progress;

            UpdateCommandState();
            scanStopwatch.Restart();
            statusTimer.Start();
            Log.Info($"SizeViewer: {string.Format(Strings.Get("Log_SizeViewer_ScanStarted"), path)}");

            try
            {
                CancellationToken token = cancellationSource.Token;
                SizeScanResult result = await Task.Run(() => SizeScanner.Scan(path, progress, token));
                scanStopwatch.Stop();

                Log.Info($"SizeViewer: {string.Format(Strings.Get(result.Cancelled ? "Log_SizeViewer_ScanCancelled" : "Log_SizeViewer_ScanFinished"), path, result.Root.FileCount, scanStopwatch.Elapsed.TotalSeconds.ToString("N1"))}");

                if (!IsDisposed)
                    ShowResult(result);
            }
            catch (Exception ex)
            {
                Log.Error($"SizeViewer: {string.Format(Strings.Get("Log_SizeViewer_ScanFailed"), path, ex.Message)}");
                if (!IsDisposed)
                {
                    statusLabel.Text = Strings.Get("Size_Status_Empty");
                    MessageForm.Show(
                        this,
                        string.Format(Strings.Get("Size_Dialog_ScanFailed"), ex.Message),
                        Strings.Get("Size_Dialog_Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                scanCancellationSource = null;
                scanProgress = null;
                scanRunning = false;
                cancellationSource.Dispose();

                if (!IsDisposed)
                {
                    statusTimer.Stop();
                    UpdateCommandState();
                }
            }
        }

        private void ShowResult(SizeScanResult result)
        {
            // Derselbe Baum wie in der Live-Anzeige: Auswahl und aufgeklappte Ordner bleiben erhalten
            if (rootNode != result.Root)
            {
                rootNode = result.Root;
                rootNode.Expanded = true;
            }

            RefreshVisibleRows();

            lastResult = result;
            UpdateResultStatus();
        }

        // Statuszeile mit den Summen des letzten Scans. Nach dem Löschen eines Eintrags neu berechnet.
        private void UpdateResultStatus()
        {
            SizeScanResult? result = lastResult;
            if (result == null)
                return;

            string status = string.Format(
                Strings.Get(result.Cancelled ? "Size_Status_Cancelled" : "Size_Status_Done"),
                result.Root.FileCount.ToString("N0"),
                result.Root.DirCount.ToString("N0"),
                FormatAllocatedOfDrive(result.Root),
                scanStopwatch.Elapsed.TotalSeconds.ToString("N1"));

            if (result.InaccessibleCount > 0)
            {
                status += string.Format(Strings.Get("Size_Status_Inaccessible"), result.InaccessibleCount.ToString("N0"));
                if (!Environment.IsPrivilegedProcess)
                    status += Strings.Get("Size_Status_AdminHint");
            }

            statusLabel.Text = status;
        }

        // Belegter Platz des Scans im Verhältnis zur Gesamtgröße des Laufwerks, z. B. "158,7 GB belegt von 199,8 GB"
        private static string FormatAllocatedOfDrive(SizeNode root)
        {
            try
            {
                string? driveRoot = Path.GetPathRoot(Path.GetFullPath(root.Name));
                if (!string.IsNullOrEmpty(driveRoot) && !driveRoot.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    var drive = new DriveInfo(driveRoot);
                    if (drive.IsReady)
                    {
                        return string.Format(
                            Strings.Get("Size_Status_AllocatedOf"),
                            FormatBytes(root.Allocated),
                            FormatBytes(drive.TotalSize));
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
            {
                // Laufwerksgröße nicht ermittelbar (z. B. UNC-Pfad): nur den belegten Platz anzeigen
            }

            return string.Format(Strings.Get("Size_Status_Allocated"), FormatBytes(root.Allocated));
        }

        private void UpdateCommandState()
        {
            selectFolderButton.Enabled = !scanRunning;
            cancelScanButton.Enabled = scanRunning;
            refreshButton.Enabled = !scanRunning && scannedPath != null;
            adminButton.Enabled = !scanRunning;
        }

        // ==================== GESPEICHERTE ANSICHT ====================

        // Übernimmt Spaltenreihenfolge, Spaltenbreiten und Sortierung aus der Config
        private void ApplyViewConfig()
        {
            int columnCount = resultListView.Columns.Count;

            if (viewConfig.ColumnWidths.Length == columnCount)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    if (viewConfig.ColumnWidths[column] >= GlobalConst.SIZEVIEWER_MIN_COLUMN_WIDTH)
                        resultListView.Columns[column].Width = ScaleToDpi(viewConfig.ColumnWidths[column]);
                }
            }

            // Nur übernehmen, wenn jede Anzeigeposition genau einmal vorkommt
            if (viewConfig.ColumnOrder.Length == columnCount &&
                viewConfig.ColumnOrder.OrderBy(position => position).SequenceEqual(Enumerable.Range(0, columnCount)))
            {
                for (int position = 0; position < columnCount; position++)
                    resultListView.Columns[Array.IndexOf(viewConfig.ColumnOrder, position)].DisplayIndex = position;
            }

            if (viewConfig.SortColumn >= 0 && viewConfig.SortColumn < columnCount)
            {
                sortColumn = viewConfig.SortColumn;
                sortDescending = viewConfig.SortDescending;
            }
        }

        private void SaveViewConfig()
        {
            try
            {
                viewConfig.ColumnOrder = resultListView.Columns.Cast<ColumnHeader>().Select(column => column.DisplayIndex).ToArray();
                viewConfig.ColumnWidths = resultListView.Columns.Cast<ColumnHeader>().Select(column => column.Width * 96 / DeviceDpi).ToArray();
                viewConfig.SortColumn = sortColumn;
                viewConfig.SortDescending = sortDescending;
                viewConfig.Save(GlobalData.SizeViewerConfigPath);
            }
            catch (Exception ex)
            {
                Log.Error($"SizeViewer: {string.Format(Strings.Get("Log_SizeViewer_ConfigSaveFailed"), ex.Message)}");
            }
        }

        // ==================== BAUM / SICHTBARE ZEILEN ====================

        // Baut die Zeilenliste aus dem Baum neu auf (nach Scan oder geänderter Sortierung)
        private void RebuildVisibleRows(SizeNode? nodeToSelect)
        {
            visibleRows.Clear();
            if (rootNode != null)
                AppendNode(rootNode, 0, visibleRows);

            resultListView.SelectedIndices.Clear();
            resultListView.VirtualListSize = visibleRows.Count;

            int selectIndex = nodeToSelect != null ? visibleRows.IndexOf(nodeToSelect) : -1;
            if (selectIndex >= 0)
                SelectRow(selectIndex);

            resultListView.Invalidate();
        }

        // Aktualisiert die Zeilenliste, ohne Auswahl und Scrollposition zu verlieren (Live-Anzeige beim Scan)
        private void RefreshVisibleRows()
        {
            SizeNode? selectedNode = GetSelectedNode();
            int topIndex = resultListView.TopItem?.Index ?? 0;

            visibleRows.Clear();
            if (rootNode != null)
                AppendNode(rootNode, 0, visibleRows);

            resultListView.SelectedIndices.Clear();
            resultListView.VirtualListSize = visibleRows.Count;

            if (topIndex > 0 && topIndex < visibleRows.Count)
                resultListView.TopItem = resultListView.Items[topIndex];

            int selectIndex = selectedNode != null ? visibleRows.IndexOf(selectedNode) : -1;
            if (selectIndex >= 0)
            {
                resultListView.SelectedIndices.Add(selectIndex);
                resultListView.FocusedItem = resultListView.Items[selectIndex];
            }

            resultListView.Invalidate();
        }

        private void AppendNode(SizeNode node, int depth, List<SizeNode> target)
        {
            node.Depth = depth;
            target.Add(node);

            if (node.Expanded && node.Children != null)
                AppendChildren(node, target);
        }

        private void AppendChildren(SizeNode node, List<SizeNode> target)
        {
            if (node.Children == null)
                return;

            SortChildren(node.Children);
            foreach (SizeNode child in node.Children)
                AppendNode(child, node.Depth + 1, target);
        }

        // Sortiert mit einer Momentaufnahme der Werte: während eines Scans ändern sich die Größen
        // laufend, und ein Vergleich mit wechselnden Werten würde die Sortierung abbrechen lassen.
        private void SortChildren(List<SizeNode> children)
        {
            var snapshot = new (SizeNode Node, long Key)[children.Count];
            for (int i = 0; i < snapshot.Length; i++)
                snapshot[i] = (children[i], GetSortKey(children[i]));

            Array.Sort(snapshot, CompareSnapshots);
            for (int i = 0; i < snapshot.Length; i++)
                children[i] = snapshot[i].Node;
        }

        private long GetSortKey(SizeNode node)
        {
            return sortColumn switch
            {
                ColumnName => 0,
                ColumnSize => node.Size,
                ColumnFiles => node.FileCount,
                ColumnFolders => node.DirCount,
                ColumnModified => node.LastModified.Ticks,
                // Der Prozentanteil bezieht sich auf den belegten Platz: gleiche Reihenfolge
                _ => node.Allocated
            };
        }

        // Klappt eine Zeile auf oder zu und passt die Zeilenliste an
        private void ToggleNode(int index)
        {
            SizeNode node = visibleRows[index];
            if (!node.HasChildren)
                return;

            int topIndex = resultListView.TopItem?.Index ?? 0;

            if (node.Expanded)
            {
                node.Expanded = false;

                int count = 0;
                while (index + 1 + count < visibleRows.Count && visibleRows[index + 1 + count].Depth > node.Depth)
                    count++;

                visibleRows.RemoveRange(index + 1, count);
            }
            else
            {
                node.Expanded = true;

                var inserted = new List<SizeNode>();
                AppendChildren(node, inserted);
                visibleRows.InsertRange(index + 1, inserted);
            }

            resultListView.SelectedIndices.Clear();
            resultListView.VirtualListSize = visibleRows.Count;

            // Scrollposition halten, damit die Liste beim Auf-/Zuklappen nicht springt
            if (topIndex > 0 && topIndex < visibleRows.Count)
                resultListView.TopItem = resultListView.Items[topIndex];

            SelectRow(index);
            resultListView.Invalidate();
        }

        private int CompareSnapshots((SizeNode Node, long Key) first, (SizeNode Node, long Key) second)
        {
            int result = sortColumn == ColumnName
                ? string.Compare(GetDisplayName(first.Node), GetDisplayName(second.Node), StringComparison.CurrentCultureIgnoreCase)
                : first.Key.CompareTo(second.Key);

            if (sortDescending)
                result = -result;

            // Bei Gleichstand immer nach Name, damit die Reihenfolge stabil bleibt
            return result != 0
                ? result
                : string.Compare(first.Node.Name, second.Node.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        private int GetSelectedIndex()
        {
            if (resultListView.SelectedIndices.Count == 0)
                return -1;

            int index = resultListView.SelectedIndices[0];
            return index < visibleRows.Count ? index : -1;
        }

        private SizeNode? GetSelectedNode()
        {
            int index = GetSelectedIndex();
            return index >= 0 ? visibleRows[index] : null;
        }

        private void SelectRow(int index)
        {
            if (index < 0 || index >= visibleRows.Count)
                return;

            resultListView.SelectedIndices.Clear();
            resultListView.SelectedIndices.Add(index);
            resultListView.FocusedItem = resultListView.Items[index];
            resultListView.EnsureVisible(index);
        }

        private int GetRowIndexAt(Point location)
        {
            ListViewItem? item = resultListView.GetItemAt(location.X, location.Y);
            return item != null && item.Index < visibleRows.Count ? item.Index : -1;
        }

        // Prüft, ob die X-Position auf dem Auf-/Zuklapp-Pfeil der Zeile liegt
        private bool IsOnGlyph(int index, int x)
        {
            SizeNode node = visibleRows[index];
            if (!node.HasChildren)
                return false;

            int glyphLeft = GetNameCellBounds(index).X + ScaleToDpi(CellPadding + node.Depth * IndentWidth);
            return x >= glyphLeft && x < glyphLeft + ScaleToDpi(GlyphWidth);
        }

        // Zelle der Namensspalte. Berücksichtigt verschobene Spalten und horizontales Scrollen.
        private Rectangle GetNameCellBounds(int index)
        {
            Rectangle rowBounds = resultListView.GetItemRect(index);
            int left = rowBounds.X;
            foreach (ColumnHeader column in resultListView.Columns)
            {
                if (column.DisplayIndex < nameColumn.DisplayIndex)
                    left += column.Width;
            }

            return new Rectangle(left, rowBounds.Y, nameColumn.Width, rowBounds.Height);
        }

        // Vollständiger Pfad eines Knotens. Die Sammelzeile "[n Dateien]" ist kein echter Ordner.
        private static string GetFullPath(SizeNode node)
        {
            var parts = new Stack<string>();
            for (SizeNode? current = node; current != null; current = current.Parent)
            {
                if (!current.IsFileGroup)
                    parts.Push(current.Name);
            }

            string path = parts.Pop();
            while (parts.Count > 0)
                path = Path.Combine(path, parts.Pop());

            return path;
        }

        // ==================== ZEICHNEN ====================

        private void DrawNameCell(Graphics graphics, SizeNode node, Rectangle bounds, Color foreColor)
        {
            Region? previousClip = graphics.Clip;
            graphics.SetClip(bounds, CombineMode.Intersect);

            int x = bounds.X + ScaleToDpi(CellPadding + node.Depth * IndentWidth);
            int glyphWidth = ScaleToDpi(GlyphWidth);
            int iconSize = ScaleToDpi(IconSize);
            int centerY = bounds.Y + bounds.Height / 2;

            if (node.HasChildren)
                DrawGlyph(graphics, x + glyphWidth / 2, centerY, node.Expanded, foreColor);

            x += glyphWidth;

            Bitmap image = node.IsDirectory ? folderImage : fileImage;
            graphics.DrawImage(image, new Rectangle(x, centerY - iconSize / 2, iconSize, iconSize));
            x += iconSize + ScaleToDpi(CellPadding);

            // Belegter Platz mit Balken (Anteil am übergeordneten Ordner) vor dem Namen
            var sizeBounds = new Rectangle(x, bounds.Y + 1, ScaleToDpi(SizeBoxWidth), bounds.Height - 2);
            int barWidth = (int)Math.Round(sizeBounds.Width * Math.Clamp(GetPercent(node), 0, 100) / 100);
            if (barWidth > 0)
            {
                using var barBrush = new SolidBrush(Theme.Current.Bar);
                graphics.FillRectangle(barBrush, sizeBounds.X, sizeBounds.Y, barWidth, sizeBounds.Height);
            }

            TextRenderer.DrawText(
                graphics,
                FormatBytes(node.Allocated),
                resultListView.Font,
                sizeBounds,
                foreColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix |
                TextFormatFlags.SingleLine | TextFormatFlags.PreserveGraphicsClipping);
            x += sizeBounds.Width + ScaleToDpi(CellPadding * 2);

            string text = GetDisplayName(node);
            if (node.AccessDenied)
                text += " " + Strings.Get("Size_NoAccess");

            var textBounds = new Rectangle(x, bounds.Y, Math.Max(0, bounds.Right - x - ScaleToDpi(CellPadding)), bounds.Height);
            TextRenderer.DrawText(
                graphics,
                text,
                resultListView.Font,
                textBounds,
                foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.PreserveGraphicsClipping);

            graphics.Clip = previousClip;
            previousClip.Dispose();
        }

        // Zeichnet den Pfeil zum Auf-/Zuklappen (">" zugeklappt, "v" aufgeklappt)
        private void DrawGlyph(Graphics graphics, int centerX, int centerY, bool expanded, Color color)
        {
            int half = ScaleToDpi(4);
            int quarter = ScaleToDpi(2);

            Point[] points = expanded
                ? new[] { new Point(centerX - half, centerY - quarter), new Point(centerX, centerY + quarter), new Point(centerX + half, centerY - quarter) }
                : new[] { new Point(centerX - quarter, centerY - half), new Point(centerX + quarter, centerY), new Point(centerX - quarter, centerY + half) };

            SmoothingMode previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, ScaleToDpi(1) + 0.5f))
                graphics.DrawLines(pen, points);

            graphics.SmoothingMode = previousMode;
        }

        private void DrawPercentCell(Graphics graphics, SizeNode node, Rectangle bounds)
        {
            double percent = GetPercent(node);
            var barBounds = Rectangle.Inflate(bounds, -ScaleToDpi(3), -ScaleToDpi(2));
            if (barBounds.Width <= 2 || barBounds.Height <= 2)
                return;

            using (var backBrush = new SolidBrush(Theme.Current.Surface))
                graphics.FillRectangle(backBrush, barBounds);

            int fillWidth = (int)Math.Round((barBounds.Width - 1) * Math.Clamp(percent, 0, 100) / 100);
            if (fillWidth > 0)
            {
                using var fillBrush = new SolidBrush(Theme.Current.Bar);
                graphics.FillRectangle(fillBrush, barBounds.X, barBounds.Y, fillWidth, barBounds.Height);
            }

            using (var borderPen = new Pen(Theme.Current.Border))
                graphics.DrawRectangle(borderPen, barBounds.X, barBounds.Y, barBounds.Width - 1, barBounds.Height - 1);

            TextRenderer.DrawText(
                graphics,
                FormatPercent(percent),
                resultListView.Font,
                barBounds,
                Theme.Current.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }

        private void DrawCellText(Graphics graphics, string text, Rectangle bounds, Color foreColor, TextFormatFlags alignment)
        {
            var textBounds = Rectangle.Inflate(bounds, -ScaleToDpi(CellPadding), 0);
            TextRenderer.DrawText(
                graphics,
                text,
                resultListView.Font,
                textBounds,
                foreColor,
                alignment | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }

        private int ScaleToDpi(int value) => value * DeviceDpi / 96;

        // ==================== TEXTE / FORMATIERUNG ====================

        private void ApplyLocalization()
        {
            Text = string.Format(
                Strings.Get(Environment.IsPrivilegedProcess ? "Size_Title_Admin" : "Size_Title"),
                GetApplicationVersion());
            adminButton.Text = Strings.Get("Size_Button_Admin");
            exitButton.Text = Strings.Get("Size_Button_Exit");
            resultGroup.Text = Strings.Get("Size_Group_Result");
            selectFolderButton.Text = Strings.Get("Size_Button_SelectFolder");
            cancelScanButton.Text = Strings.Get("Size_Button_Cancel");
            refreshButton.Text = Strings.Get("Size_Button_Refresh");
            openInExplorerMenuItem.Text = Strings.Get("Size_Menu_OpenInExplorer");
            deleteMenuItem.Text = Strings.Get("Size_Menu_Delete");
            deleteMenuItem.ShortcutKeyDisplayString = Strings.Get("Size_Menu_DeleteShortcut");
            statusLabel.Text = Strings.Get("Size_Status_Empty");

            columnTitles[ColumnName] = Strings.Get("Size_Column_Name");
            columnTitles[ColumnSize] = Strings.Get("Size_Column_Size");
            columnTitles[ColumnAllocated] = Strings.Get("Size_Column_Allocated");
            columnTitles[ColumnFiles] = Strings.Get("Size_Column_Files");
            columnTitles[ColumnFolders] = Strings.Get("Size_Column_Folders");
            columnTitles[ColumnPercent] = Strings.Get("Size_Column_Percent");
            columnTitles[ColumnModified] = Strings.Get("Size_Column_Modified");
            UpdateColumnHeaders();
        }

        // Setzt die Spaltentitel und markiert die Sortierspalte mit einem Pfeil
        private void UpdateColumnHeaders()
        {
            for (int column = 0; column < resultListView.Columns.Count; column++)
            {
                string arrow = column == sortColumn
                    ? (sortDescending ? SortArrowDescending : SortArrowAscending)
                    : string.Empty;

                resultListView.Columns[column].Text = columnTitles[column] + arrow;
            }
        }

        private static string GetDisplayName(SizeNode node)
        {
            return node.IsFileGroup
                ? string.Format(Strings.Get("Size_FileGroup"), node.FileCount.ToString("N0"))
                : node.Name;
        }

        private static string GetCellText(SizeNode node, int column)
        {
            bool isContainer = node.IsDirectory || node.IsFileGroup;

            return column switch
            {
                ColumnName => GetDisplayName(node),
                ColumnSize => FormatBytes(node.Size),
                ColumnAllocated => FormatBytes(node.Allocated),
                ColumnFiles => isContainer ? node.FileCount.ToString("N0") : string.Empty,
                ColumnFolders => node.IsDirectory ? node.DirCount.ToString("N0") : string.Empty,
                ColumnPercent => FormatPercent(GetPercent(node)),
                ColumnModified => node.LastModified > DateTime.MinValue ? node.LastModified.ToString("d") : string.Empty,
                _ => string.Empty
            };
        }

        // Anteil am belegten Platz des übergeordneten Verzeichnisses (Wurzel = 100 %)
        private static double GetPercent(SizeNode node)
        {
            SizeNode? container = node.Parent;
            if (container is { IsFileGroup: true })
                container = container.Parent;

            if (container == null)
                return 100;

            return container.Allocated > 0 ? node.Allocated * 100.0 / container.Allocated : 0;
        }

        private static string FormatPercent(double percent) => $"{percent:N1} %";

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes:N0} {Strings.Get("Size_Unit_Bytes")}";

            string[] units = { "KB", "MB", "GB", "TB", "PB" };
            double value = bytes;
            int unit = -1;
            do
            {
                value /= 1024;
                unit++;
            }
            while (value >= 1024 && unit < units.Length - 1);

            return $"{value:N1} {units[unit]}";
        }

        private static string GetApplicationVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?";
        }

        private static Icon LoadIcon(string resourceName)
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            return stream != null ? new Icon(stream) : SystemIcons.Application;
        }

        private void OpenLink(string url)
        {
            try
            {
                using Process? process = Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch
            {
                MessageForm.Show(this, Strings.Get("Usb_Dialog_LinkError"), Strings.Get("Size_Dialog_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetButtonImage(Button button, Image image)
        {
            buttonImages.Add(image);
            button.Image = image;
        }

        // Zeichnet ein Zeichen der Windows-Symbolschrift als Bild für eine Schaltfläche
        private Bitmap CreateGlyphImage(string glyph, Color color)
        {
            int size = ScaleToDpi(IconSize);
            var image = new Bitmap(size, size);

            using Graphics graphics = Graphics.FromImage(image);
            using var font = new Font(GlyphFontName, GlyphFontSize * DeviceDpi / 96, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(color);
            using var format = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            graphics.DrawString(glyph, font, brush, new RectangleF(0, 0, size, size), format);
            return image;
        }

        private static Bitmap LoadStockImage(StockIconId iconId)
        {
            using Icon icon = SystemIcons.GetStockIcon(iconId, StockIconOptions.SmallIcon);
            return icon.ToBitmap();
        }
    }
}
