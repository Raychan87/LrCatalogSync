using System.Diagnostics;
using System.Reflection;

using LrCatalogSync.Infrastructure;

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
        }

        public SettingsForm(AppConfig cfg)
        {
            InitializeComponent();
            config = cfg;
            originalPasswordRclone = cfg.SambaPasswordRclone;
            originalPasswordAes = cfg.SambaPasswordAes;

            Text = $"LrCatalogSync v{appVersion} - Fototour-und-Technik.de";
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
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button button || button.Tag is not TextBox textBox)
                return;

            string path = textBox == txtCatalogLocalFile
                ? BrowseFile("Lightroom Katalog-Datei (*.lrcat)|*.lrcat|Alle Dateien (*.*)|*.*")
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
                MessageBox.Show("Link konnte nicht geöffnet werden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string BrowseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Ordner auswählen";
                if (dialog.ShowDialog() == DialogResult.OK)
                    return dialog.SelectedPath ?? string.Empty;
            }
            return string.Empty;
        }

        private string BrowseFile(string filter = "Alle Dateien (*.*)|*.*")
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Datei auswählen";
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
                config.AutoRun = GetCheckBoxValue("chkAutoRun");

                if (!ValidateRemotePath(ref config.CatalogRemotePath, "Remote Katalog Pfad") ||
                    !ValidateRemotePath(ref config.BackupsRemotePath, "Remote Backup Pfad"))
                    return;

                if (!int.TryParse(GetControlValue("txtGlobalCycleInterval"), out int globalCycleInterval) || globalCycleInterval <= 0)
                {
                    MessageBox.Show(
                        "Fehler: Die Aktualisierungszeit muss eine positive Zahl in Sekunden sein!",
                        "Ungültige Aktualisierungszeit",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (globalCycleInterval > 999)
                {
                    MessageBox.Show(
                        "Fehler: Die Aktualisierungszeit muss zwischen 1 und 999 Sekunden liegen!",
                        "Ungültiger Wertebereich",
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
                        $"Fehler: rclone.exe nicht gefunden!\n\nPfad: {absoluteRclonePath}\n\nBitte überprüfen Sie den Pfad.",
                        "rclone.exe nicht gefunden",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(config.CatalogLocalFile))
                {
                    MessageBox.Show("Fehler: Die Katalog-Datei ist erforderlich!", "Katalog-Datei fehlt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(config.CatalogLocalFile))
                {
                    MessageBox.Show(
                        $"Fehler: Die Katalog-Datei existiert nicht!\n\nPfad: {config.CatalogLocalFile}",
                        "Katalog-Datei existiert nicht",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (!config.CatalogLocalFile.EndsWith(".lrcat", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        $"Fehler: Die ausgewählte Datei ist keine Lightroom Katalog-Datei!\n\nDatei: {config.CatalogLocalFile}\n\nBitte wählen Sie eine *.lrcat Datei.",
                        "Keine .lrcat Datei",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (config.EnableBackups)
                {
                    if (string.IsNullOrEmpty(config.BackupsLocalPath))
                    {
                        MessageBox.Show("Fehler: Der lokale Backup Pfad ist erforderlich wenn Backups aktiviert sind!", "Lokaler Backup Pfad fehlt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!Directory.Exists(config.BackupsLocalPath))
                    {
                        MessageBox.Show(
                            $"Fehler: Der lokale Backup Pfad existiert nicht!\n\nPfad: {config.BackupsLocalPath}",
                            "Lokaler Backup Pfad existiert nicht",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(config.BackupsRemotePath))
                    {
                        MessageBox.Show("Fehler: Der Remote Backup Pfad ist erforderlich wenn Backups aktiviert sind!", "Remote Backup Pfad fehlt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (config.EnableRcloneCopy && string.IsNullOrEmpty(config.RcloneCopyFolderName))
                {
                    MessageBox.Show("Fehler: Der rclone copy Ordnername darf nicht leer sein!", "Ordnername fehlt", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                if (config.AutoRun)
                    Autorun.Enable(Application.ExecutablePath);
                else
                    Autorun.Disable();

                MessageBox.Show("Einstellungen erfolgreich gespeichert!", "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    $"Das Feld \"{fieldName}\" enthält einen Windows-Laufwerksbuchstaben (z.B. X:, D:, F:), der hier nicht erlaubt ist.\n\n" +
                    "Tragen Sie hier den Ordnerpfad innerhalb Ihrer Samba-Freigabe ein, z.B.:\n" +
                    "  /SambaOrdner/\n" +
                    "  /SambaOrdner/Lightroom/\n\n" +
                    "Der Samba-Server (IP oder Hostname) wird separat im Feld \"Server IP/Name\" eingetragen.\n\n" +
                    $"Aus den beiden Feldern wird der vollständige Netzwerkpfad zusammengesetzt:\n" +
                    $"  \\\\{{Server IP/Name}}{{{fieldName}}}\n" +
                    $"  Beispiel: \\\\192.168.1.100/SambaOrdner/",
                    $"Ungültiger Pfad in {fieldName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            remotePath = trimmedRemotePath.StartsWith('/')
                ? trimmedRemotePath
                : $"/{trimmedRemotePath}";

            return true;
        }

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
                Log.Error($"SettingsForm: Verschlüsseln des Passworts: {ex.Message}");
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
            Log.Debug("Config: LrCatSyncRclone.conf erfolgreich erstellt");
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