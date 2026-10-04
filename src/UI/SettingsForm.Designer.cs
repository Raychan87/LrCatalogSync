namespace LrCatalogSync.UI
{
    partial class SettingsForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null!;

        /// <summary>
        /// Bereinigt alle verwendeten Ressourcen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Layout mit festen Location/Size-Werten (keine Layout-Panels),
        /// damit Designer und gebautes Programm identisch aussehen.
        /// </summary>
        // Hinweis: Die GroupBox-Überschriften sind fett. Da Child-Controls die Schrift
        // vom Parent erben, haben alle Controls in den Rahmen explizit die normale Schrift.
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.settingsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.generalGroup = new System.Windows.Forms.GroupBox();
            this.chkAutoRun = new System.Windows.Forms.CheckBox();
            this.rcloneFolderLabel = new System.Windows.Forms.Label();
            this.txtRcloneFolder = new System.Windows.Forms.TextBox();
            this.browseRcloneFolderButton = new System.Windows.Forms.Button();
            this.rcloneDownloadLabel = new System.Windows.Forms.Label();
            this.logLevelLabel = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.updateIntervalLabel = new System.Windows.Forms.Label();
            this.txtGlobalCycleInterval = new System.Windows.Forms.TextBox();
            this.secondsLabel = new System.Windows.Forms.Label();
            this.catalogGroup = new System.Windows.Forms.GroupBox();
            this.chkSyncPreviewData = new System.Windows.Forms.CheckBox();
            this.catalogLocalFileLabel = new System.Windows.Forms.Label();
            this.txtCatalogLocalFile = new System.Windows.Forms.TextBox();
            this.browseCatalogFileButton = new System.Windows.Forms.Button();
            this.catalogRemotePathLabel = new System.Windows.Forms.Label();
            this.txtCatalogRemotePath = new System.Windows.Forms.TextBox();
            this.chkEnableRcloneCopy = new System.Windows.Forms.CheckBox();
            this.rcloneCopyFolderNameLabel = new System.Windows.Forms.Label();
            this.txtRcloneCopyFolderName = new System.Windows.Forms.TextBox();
            this.backupGroup = new System.Windows.Forms.GroupBox();
            this.chkEnableBackups = new System.Windows.Forms.CheckBox();
            this.backupsLocalPathLabel = new System.Windows.Forms.Label();
            this.txtBackupsLocalPath = new System.Windows.Forms.TextBox();
            this.browseBackupsPathButton = new System.Windows.Forms.Button();
            this.backupsRemotePathLabel = new System.Windows.Forms.Label();
            this.txtBackupsRemotePath = new System.Windows.Forms.TextBox();
            this.sambaGroup = new System.Windows.Forms.GroupBox();
            this.remoteIpLabel = new System.Windows.Forms.Label();
            this.txtRemoteIP = new System.Windows.Forms.TextBox();
            this.sambaUserLabel = new System.Windows.Forms.Label();
            this.txtSambaUser = new System.Windows.Forms.TextBox();
            this.sambaPasswordLabel = new System.Windows.Forms.Label();
            this.txtSambaPassword = new System.Windows.Forms.TextBox();
            this.gitHubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.websiteLinkLabel = new System.Windows.Forms.LinkLabel();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.generalGroup.SuspendLayout();
            this.catalogGroup.SuspendLayout();
            this.backupGroup.SuspendLayout();
            this.sambaGroup.SuspendLayout();
            this.SuspendLayout();
            //
            // Allgemeine Einstellungen (Rahmen)
            //
            this.generalGroup.Controls.Add(this.chkAutoRun);
            this.generalGroup.Controls.Add(this.rcloneFolderLabel);
            this.generalGroup.Controls.Add(this.txtRcloneFolder);
            this.generalGroup.Controls.Add(this.browseRcloneFolderButton);
            this.generalGroup.Controls.Add(this.rcloneDownloadLabel);
            this.generalGroup.Controls.Add(this.logLevelLabel);
            this.generalGroup.Controls.Add(this.cmbLogLevel);
            this.generalGroup.Controls.Add(this.updateIntervalLabel);
            this.generalGroup.Controls.Add(this.txtGlobalCycleInterval);
            this.generalGroup.Controls.Add(this.secondsLabel);
            this.generalGroup.Location = new System.Drawing.Point(12, 12);
            this.generalGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.generalGroup.Name = "generalGroup";
            this.generalGroup.Size = new System.Drawing.Size(472, 145);
            this.generalGroup.TabIndex = 0;
            this.generalGroup.TabStop = false;
            this.generalGroup.Text = "Allgemeine Einstellungen";
            //
            // CheckBox für Autostart
            //
            this.chkAutoRun.AutoSize = false;
            this.chkAutoRun.Location = new System.Drawing.Point(140, 18);
            this.chkAutoRun.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAutoRun.Name = "chkAutoRun";
            this.chkAutoRun.Size = new System.Drawing.Size(300, 20);
            this.chkAutoRun.TabIndex = 0;
            this.chkAutoRun.Text = "Automatisch beim Systemstart ausführen";
            this.chkAutoRun.UseVisualStyleBackColor = true;
            this.settingsToolTip.SetToolTip(this.chkAutoRun, "Startet LrCatalogSync automatisch beim Windows-Systemstart.");
            //
            // Label für rclone-Verzeichnis
            //
            this.rcloneFolderLabel.AutoSize = false;
            this.rcloneFolderLabel.Location = new System.Drawing.Point(12, 42);
            this.rcloneFolderLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rcloneFolderLabel.Name = "rcloneFolderLabel";
            this.rcloneFolderLabel.Size = new System.Drawing.Size(125, 23);
            this.rcloneFolderLabel.TabIndex = 1;
            this.rcloneFolderLabel.Text = "Rclone Verzeichnispfad:";
            this.rcloneFolderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für rclone-Verzeichnis
            //
            this.txtRcloneFolder.Location = new System.Drawing.Point(140, 42);
            this.txtRcloneFolder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRcloneFolder.Name = "txtRcloneFolder";
            this.txtRcloneFolder.Size = new System.Drawing.Size(270, 23);
            this.txtRcloneFolder.TabIndex = 2;
            this.settingsToolTip.SetToolTip(this.txtRcloneFolder, "Pfad zur rclone-Installation oder zum rclone-Verzeichnis.");
            //
            // Auswahl-Button für rclone-Verzeichnis
            //
            this.browseRcloneFolderButton.Location = new System.Drawing.Point(414, 41);
            this.browseRcloneFolderButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.browseRcloneFolderButton.Name = "browseRcloneFolderButton";
            this.browseRcloneFolderButton.Size = new System.Drawing.Size(44, 25);
            this.browseRcloneFolderButton.TabIndex = 3;
            this.browseRcloneFolderButton.Tag = this.txtRcloneFolder;
            this.browseRcloneFolderButton.Text = "...";
            this.browseRcloneFolderButton.UseVisualStyleBackColor = true;
            this.browseRcloneFolderButton.Click += new System.EventHandler(this.BrowseButton_Click);
            //
            // Link zum rclone-Download
            //
            this.rcloneDownloadLabel.AutoSize = false;
            this.rcloneDownloadLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rcloneDownloadLabel.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.rcloneDownloadLabel.Location = new System.Drawing.Point(140, 67);
            this.rcloneDownloadLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rcloneDownloadLabel.Name = "rcloneDownloadLabel";
            this.rcloneDownloadLabel.Size = new System.Drawing.Size(318, 16);
            this.rcloneDownloadLabel.TabIndex = 4;
            this.rcloneDownloadLabel.Text = "Download von rclone (https://rclone.org/downloads)";
            this.rcloneDownloadLabel.Click += new System.EventHandler(this.RcloneDownloadLabel_Click);
            //
            // Label für Log-Level
            //
            this.logLevelLabel.AutoSize = false;
            this.logLevelLabel.Location = new System.Drawing.Point(12, 87);
            this.logLevelLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.logLevelLabel.Name = "logLevelLabel";
            this.logLevelLabel.Size = new System.Drawing.Size(125, 23);
            this.logLevelLabel.TabIndex = 5;
            this.logLevelLabel.Text = "Log-Level:";
            this.logLevelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // ComboBox für Log-Level
            //
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Items.AddRange(new object[] { "DEBUG", "INFO", "NOTICE", "ERROR" });
            this.cmbLogLevel.Location = new System.Drawing.Point(140, 87);
            this.cmbLogLevel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(100, 23);
            this.cmbLogLevel.TabIndex = 6;
            //
            // Label für Aktualisierungszeit
            //
            this.updateIntervalLabel.AutoSize = false;
            this.updateIntervalLabel.Location = new System.Drawing.Point(12, 114);
            this.updateIntervalLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.updateIntervalLabel.Name = "updateIntervalLabel";
            this.updateIntervalLabel.Size = new System.Drawing.Size(125, 23);
            this.updateIntervalLabel.TabIndex = 7;
            this.updateIntervalLabel.Text = "Aktualisierungszeit:";
            this.updateIntervalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Aktualisierungszeit
            //
            this.txtGlobalCycleInterval.Location = new System.Drawing.Point(140, 114);
            this.txtGlobalCycleInterval.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtGlobalCycleInterval.Name = "txtGlobalCycleInterval";
            this.txtGlobalCycleInterval.Size = new System.Drawing.Size(40, 23);
            this.txtGlobalCycleInterval.TabIndex = 8;
            this.txtGlobalCycleInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.settingsToolTip.SetToolTip(this.txtGlobalCycleInterval, "Zeit in Sekunden zwischen den automatischen Synchronisationszyklen (1 bis 999).");
            //
            // Label "Sekunden"
            //
            this.secondsLabel.AutoSize = false;
            this.secondsLabel.Location = new System.Drawing.Point(184, 114);
            this.secondsLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.secondsLabel.Name = "secondsLabel";
            this.secondsLabel.Size = new System.Drawing.Size(70, 23);
            this.secondsLabel.TabIndex = 9;
            this.secondsLabel.Text = "Sekunden";
            this.secondsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // Lightroom Katalog (Rahmen)
            //
            this.catalogGroup.Controls.Add(this.chkSyncPreviewData);
            this.catalogGroup.Controls.Add(this.catalogLocalFileLabel);
            this.catalogGroup.Controls.Add(this.txtCatalogLocalFile);
            this.catalogGroup.Controls.Add(this.browseCatalogFileButton);
            this.catalogGroup.Controls.Add(this.catalogRemotePathLabel);
            this.catalogGroup.Controls.Add(this.txtCatalogRemotePath);
            this.catalogGroup.Controls.Add(this.chkEnableRcloneCopy);
            this.catalogGroup.Controls.Add(this.rcloneCopyFolderNameLabel);
            this.catalogGroup.Controls.Add(this.txtRcloneCopyFolderName);
            this.catalogGroup.Location = new System.Drawing.Point(12, 163);
            this.catalogGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.catalogGroup.Name = "catalogGroup";
            this.catalogGroup.Size = new System.Drawing.Size(472, 151);
            this.catalogGroup.TabIndex = 1;
            this.catalogGroup.TabStop = false;
            this.catalogGroup.Text = "Lightroom Katalog";
            //
            // CheckBox für Previews
            //
            this.chkSyncPreviewData.AutoSize = false;
            this.chkSyncPreviewData.Location = new System.Drawing.Point(140, 18);
            this.chkSyncPreviewData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkSyncPreviewData.Name = "chkSyncPreviewData";
            this.chkSyncPreviewData.Size = new System.Drawing.Size(300, 20);
            this.chkSyncPreviewData.TabIndex = 0;
            this.chkSyncPreviewData.Text = "*Previews.lrdata synchronisieren?";
            this.chkSyncPreviewData.UseVisualStyleBackColor = true;
            this.settingsToolTip.SetToolTip(this.chkSyncPreviewData, "Wenn aktiv, wird zusätzlich der Ordner *Previews.lrdata des Lightroom-Katalogs synchronisiert.");
            //
            // Label für lokale Katalogdatei
            //
            this.catalogLocalFileLabel.AutoSize = false;
            this.catalogLocalFileLabel.Location = new System.Drawing.Point(12, 42);
            this.catalogLocalFileLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.catalogLocalFileLabel.Name = "catalogLocalFileLabel";
            this.catalogLocalFileLabel.Size = new System.Drawing.Size(125, 23);
            this.catalogLocalFileLabel.TabIndex = 1;
            this.catalogLocalFileLabel.Text = "Lokale Katalog Datei:";
            this.catalogLocalFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für lokale Katalogdatei
            //
            this.txtCatalogLocalFile.Location = new System.Drawing.Point(140, 42);
            this.txtCatalogLocalFile.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCatalogLocalFile.Name = "txtCatalogLocalFile";
            this.txtCatalogLocalFile.Size = new System.Drawing.Size(270, 23);
            this.txtCatalogLocalFile.TabIndex = 2;
            this.settingsToolTip.SetToolTip(this.txtCatalogLocalFile, "Lokaler Pfad zur Lightroom-Katalogdatei (.lrcat).");
            //
            // Auswahl-Button für lokale Katalogdatei
            //
            this.browseCatalogFileButton.Location = new System.Drawing.Point(414, 41);
            this.browseCatalogFileButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.browseCatalogFileButton.Name = "browseCatalogFileButton";
            this.browseCatalogFileButton.Size = new System.Drawing.Size(44, 25);
            this.browseCatalogFileButton.TabIndex = 3;
            this.browseCatalogFileButton.Tag = this.txtCatalogLocalFile;
            this.browseCatalogFileButton.Text = "...";
            this.browseCatalogFileButton.UseVisualStyleBackColor = true;
            this.browseCatalogFileButton.Click += new System.EventHandler(this.BrowseButton_Click);
            //
            // Label für Remote-Katalogpfad
            //
            this.catalogRemotePathLabel.AutoSize = false;
            this.catalogRemotePathLabel.Location = new System.Drawing.Point(12, 69);
            this.catalogRemotePathLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.catalogRemotePathLabel.Name = "catalogRemotePathLabel";
            this.catalogRemotePathLabel.Size = new System.Drawing.Size(125, 23);
            this.catalogRemotePathLabel.TabIndex = 4;
            this.catalogRemotePathLabel.Text = "Remote Katalog Pfad:";
            this.catalogRemotePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Remote-Katalogpfad
            //
            this.txtCatalogRemotePath.Location = new System.Drawing.Point(140, 69);
            this.txtCatalogRemotePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtCatalogRemotePath.Name = "txtCatalogRemotePath";
            this.txtCatalogRemotePath.Size = new System.Drawing.Size(318, 23);
            this.txtCatalogRemotePath.TabIndex = 5;
            this.settingsToolTip.SetToolTip(this.txtCatalogRemotePath, "Zielpfad auf dem Samba-Server z.B. //192.168.1.100/SambaOrdner/ -> /SambaOrdner/");
            //
            // CheckBox für letzten Katalog behalten
            //
            this.chkEnableRcloneCopy.AutoSize = false;
            this.chkEnableRcloneCopy.Location = new System.Drawing.Point(140, 96);
            this.chkEnableRcloneCopy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkEnableRcloneCopy.Name = "chkEnableRcloneCopy";
            this.chkEnableRcloneCopy.Size = new System.Drawing.Size(300, 20);
            this.chkEnableRcloneCopy.TabIndex = 6;
            this.chkEnableRcloneCopy.Text = "Letzten Katalog behalten?";
            this.chkEnableRcloneCopy.UseVisualStyleBackColor = true;
            this.chkEnableRcloneCopy.CheckedChanged += new System.EventHandler(this.ChkEnableRcloneCopy_CheckedChanged);
            this.settingsToolTip.SetToolTip(this.chkEnableRcloneCopy, "Behält nach der Synchronisation eine Kopie des letzten Katalogs.");
            //
            // Label für Ordnername
            //
            this.rcloneCopyFolderNameLabel.AutoSize = false;
            this.rcloneCopyFolderNameLabel.Location = new System.Drawing.Point(12, 120);
            this.rcloneCopyFolderNameLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rcloneCopyFolderNameLabel.Name = "rcloneCopyFolderNameLabel";
            this.rcloneCopyFolderNameLabel.Size = new System.Drawing.Size(125, 23);
            this.rcloneCopyFolderNameLabel.TabIndex = 7;
            this.rcloneCopyFolderNameLabel.Text = "Ordnername:";
            this.rcloneCopyFolderNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Ordnername
            //
            this.txtRcloneCopyFolderName.Location = new System.Drawing.Point(140, 120);
            this.txtRcloneCopyFolderName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRcloneCopyFolderName.Name = "txtRcloneCopyFolderName";
            this.txtRcloneCopyFolderName.Size = new System.Drawing.Size(318, 23);
            this.txtRcloneCopyFolderName.TabIndex = 8;
            this.settingsToolTip.SetToolTip(this.txtRcloneCopyFolderName, "Name des Ordners für die Kopie des letzten Lightroom-Katalogs.");
            //
            // Lightroom Katalog Sicherungsordner (Rahmen)
            //
            this.backupGroup.Controls.Add(this.chkEnableBackups);
            this.backupGroup.Controls.Add(this.backupsLocalPathLabel);
            this.backupGroup.Controls.Add(this.txtBackupsLocalPath);
            this.backupGroup.Controls.Add(this.browseBackupsPathButton);
            this.backupGroup.Controls.Add(this.backupsRemotePathLabel);
            this.backupGroup.Controls.Add(this.txtBackupsRemotePath);
            this.backupGroup.Location = new System.Drawing.Point(12, 320);
            this.backupGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.backupGroup.Name = "backupGroup";
            this.backupGroup.Size = new System.Drawing.Size(472, 100);
            this.backupGroup.TabIndex = 2;
            this.backupGroup.TabStop = false;
            this.backupGroup.Text = "Lightroom Katalog Sicherungsordner";
            //
            // CheckBox für Sicherungsordner
            //
            this.chkEnableBackups.AutoSize = false;
            this.chkEnableBackups.Location = new System.Drawing.Point(140, 18);
            this.chkEnableBackups.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkEnableBackups.Name = "chkEnableBackups";
            this.chkEnableBackups.Size = new System.Drawing.Size(300, 20);
            this.chkEnableBackups.TabIndex = 0;
            this.chkEnableBackups.Text = "Sicherungsordner synchronisieren?";
            this.chkEnableBackups.UseVisualStyleBackColor = true;
            this.chkEnableBackups.CheckedChanged += new System.EventHandler(this.ChkEnableBackups_CheckedChanged);
            this.settingsToolTip.SetToolTip(this.chkEnableBackups, "Aktiviert die Sicherung der Sicherungsordner die Lightroom Classic ablegt.");
            //
            // Label für lokalen Backup-Pfad
            //
            this.backupsLocalPathLabel.AutoSize = false;
            this.backupsLocalPathLabel.Location = new System.Drawing.Point(12, 42);
            this.backupsLocalPathLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.backupsLocalPathLabel.Name = "backupsLocalPathLabel";
            this.backupsLocalPathLabel.Size = new System.Drawing.Size(125, 23);
            this.backupsLocalPathLabel.TabIndex = 1;
            this.backupsLocalPathLabel.Text = "Lokaler Backup Pfad:";
            this.backupsLocalPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für lokalen Backup-Pfad
            //
            this.txtBackupsLocalPath.Location = new System.Drawing.Point(140, 42);
            this.txtBackupsLocalPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBackupsLocalPath.Name = "txtBackupsLocalPath";
            this.txtBackupsLocalPath.Size = new System.Drawing.Size(270, 23);
            this.txtBackupsLocalPath.TabIndex = 2;
            this.settingsToolTip.SetToolTip(this.txtBackupsLocalPath, "Lokaler Pfad zum Sicherungsordner von Lightroom Classic.");
            //
            // Auswahl-Button für lokalen Backup-Pfad
            //
            this.browseBackupsPathButton.Location = new System.Drawing.Point(414, 41);
            this.browseBackupsPathButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.browseBackupsPathButton.Name = "browseBackupsPathButton";
            this.browseBackupsPathButton.Size = new System.Drawing.Size(44, 25);
            this.browseBackupsPathButton.TabIndex = 3;
            this.browseBackupsPathButton.Tag = this.txtBackupsLocalPath;
            this.browseBackupsPathButton.Text = "...";
            this.browseBackupsPathButton.UseVisualStyleBackColor = true;
            this.browseBackupsPathButton.Click += new System.EventHandler(this.BrowseButton_Click);
            //
            // Label für Remote-Backup-Pfad
            //
            this.backupsRemotePathLabel.AutoSize = false;
            this.backupsRemotePathLabel.Location = new System.Drawing.Point(12, 69);
            this.backupsRemotePathLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.backupsRemotePathLabel.Name = "backupsRemotePathLabel";
            this.backupsRemotePathLabel.Size = new System.Drawing.Size(125, 23);
            this.backupsRemotePathLabel.TabIndex = 4;
            this.backupsRemotePathLabel.Text = "Remote Backup Pfad:";
            this.backupsRemotePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Remote-Backup-Pfad
            //
            this.txtBackupsRemotePath.Location = new System.Drawing.Point(140, 69);
            this.txtBackupsRemotePath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtBackupsRemotePath.Name = "txtBackupsRemotePath";
            this.txtBackupsRemotePath.Size = new System.Drawing.Size(318, 23);
            this.txtBackupsRemotePath.TabIndex = 5;
            this.settingsToolTip.SetToolTip(this.txtBackupsRemotePath, "Zielpfad auf dem entfernten Speicher für die Sicherungsordner von Lightroom Classic.");
            //
            // Samba Server Einstellungen (Rahmen)
            //
            this.sambaGroup.Controls.Add(this.remoteIpLabel);
            this.sambaGroup.Controls.Add(this.txtRemoteIP);
            this.sambaGroup.Controls.Add(this.sambaUserLabel);
            this.sambaGroup.Controls.Add(this.txtSambaUser);
            this.sambaGroup.Controls.Add(this.sambaPasswordLabel);
            this.sambaGroup.Controls.Add(this.txtSambaPassword);
            this.sambaGroup.Location = new System.Drawing.Point(12, 426);
            this.sambaGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.sambaGroup.Name = "sambaGroup";
            this.sambaGroup.Size = new System.Drawing.Size(472, 107);
            this.sambaGroup.TabIndex = 3;
            this.sambaGroup.TabStop = false;
            this.sambaGroup.Text = "Samba Server Einstellungen";
            //
            // Label für Server
            //
            this.remoteIpLabel.AutoSize = false;
            this.remoteIpLabel.Location = new System.Drawing.Point(12, 22);
            this.remoteIpLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.remoteIpLabel.Name = "remoteIpLabel";
            this.remoteIpLabel.Size = new System.Drawing.Size(125, 23);
            this.remoteIpLabel.TabIndex = 0;
            this.remoteIpLabel.Text = "Server IP/Name:";
            this.remoteIpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Server
            //
            this.txtRemoteIP.Location = new System.Drawing.Point(140, 22);
            this.txtRemoteIP.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtRemoteIP.Name = "txtRemoteIP";
            this.txtRemoteIP.Size = new System.Drawing.Size(318, 23);
            this.txtRemoteIP.TabIndex = 1;
            this.settingsToolTip.SetToolTip(this.txtRemoteIP, "IP-Adresse oder Hostname des Samba-Servers.");
            //
            // Label für Benutzername
            //
            this.sambaUserLabel.AutoSize = false;
            this.sambaUserLabel.Location = new System.Drawing.Point(12, 49);
            this.sambaUserLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.sambaUserLabel.Name = "sambaUserLabel";
            this.sambaUserLabel.Size = new System.Drawing.Size(125, 23);
            this.sambaUserLabel.TabIndex = 2;
            this.sambaUserLabel.Text = "Benutzername:";
            this.sambaUserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Benutzername
            //
            this.txtSambaUser.Location = new System.Drawing.Point(140, 49);
            this.txtSambaUser.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSambaUser.Name = "txtSambaUser";
            this.txtSambaUser.Size = new System.Drawing.Size(318, 23);
            this.txtSambaUser.TabIndex = 3;
            this.settingsToolTip.SetToolTip(this.txtSambaUser, "Benutzername für die Verbindung zum Samba-Server.");
            //
            // Label für Passwort
            //
            this.sambaPasswordLabel.AutoSize = false;
            this.sambaPasswordLabel.Location = new System.Drawing.Point(12, 76);
            this.sambaPasswordLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.sambaPasswordLabel.Name = "sambaPasswordLabel";
            this.sambaPasswordLabel.Size = new System.Drawing.Size(125, 23);
            this.sambaPasswordLabel.TabIndex = 4;
            this.sambaPasswordLabel.Text = "Passwort:";
            this.sambaPasswordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Passwort
            //
            this.txtSambaPassword.Location = new System.Drawing.Point(140, 76);
            this.txtSambaPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSambaPassword.Name = "txtSambaPassword";
            this.txtSambaPassword.Size = new System.Drawing.Size(318, 23);
            this.txtSambaPassword.TabIndex = 5;
            this.txtSambaPassword.UseSystemPasswordChar = true;
            this.settingsToolTip.SetToolTip(this.txtSambaPassword, "Passwort für die Verbindung zum Samba-Server.");
            //
            // Link zum GitHub-Projekt
            //
            this.gitHubLinkLabel.AutoSize = false;
            this.gitHubLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.gitHubLinkLabel.LinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.gitHubLinkLabel.Location = new System.Drawing.Point(14, 540);
            this.gitHubLinkLabel.Name = "gitHubLinkLabel";
            this.gitHubLinkLabel.Size = new System.Drawing.Size(230, 18);
            this.gitHubLinkLabel.TabIndex = 4;
            this.gitHubLinkLabel.TabStop = true;
            this.gitHubLinkLabel.Text = "GitHub Project";
            this.gitHubLinkLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.gitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GitHubLinkLabel_LinkClicked);
            //
            // Link zur Webseite
            //
            this.websiteLinkLabel.AutoSize = false;
            this.websiteLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.websiteLinkLabel.LinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.websiteLinkLabel.Location = new System.Drawing.Point(14, 560);
            this.websiteLinkLabel.Name = "websiteLinkLabel";
            this.websiteLinkLabel.Size = new System.Drawing.Size(230, 18);
            this.websiteLinkLabel.TabIndex = 5;
            this.websiteLinkLabel.TabStop = true;
            this.websiteLinkLabel.Text = "© Fototour und Technik";
            this.websiteLinkLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.websiteLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WebsiteLinkLabel_LinkClicked);
            //
            // Button zum Speichern
            //
            this.saveButton.Location = new System.Drawing.Point(276, 542);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(100, 35);
            this.saveButton.TabIndex = 6;
            this.saveButton.Text = "Speichern";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // Button zum Abbrechen
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(384, 542);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 35);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // SettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(497, 588);
            this.Controls.Add(this.generalGroup);
            this.Controls.Add(this.catalogGroup);
            this.Controls.Add(this.backupGroup);
            this.Controls.Add(this.sambaGroup);
            this.Controls.Add(this.gitHubLinkLabel);
            this.Controls.Add(this.websiteLinkLabel);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LrCatalogSync - Einstellungen";
            this.generalGroup.ResumeLayout(false);
            this.generalGroup.PerformLayout();
            this.catalogGroup.ResumeLayout(false);
            this.catalogGroup.PerformLayout();
            this.backupGroup.ResumeLayout(false);
            this.backupGroup.PerformLayout();
            this.sambaGroup.ResumeLayout(false);
            this.sambaGroup.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip settingsToolTip = null!;
        private System.Windows.Forms.GroupBox generalGroup = null!;
        private System.Windows.Forms.CheckBox chkAutoRun = null!;
        private System.Windows.Forms.Label rcloneFolderLabel = null!;
        private System.Windows.Forms.TextBox txtRcloneFolder = null!;
        private System.Windows.Forms.Button browseRcloneFolderButton = null!;
        private System.Windows.Forms.Label rcloneDownloadLabel = null!;
        private System.Windows.Forms.Label logLevelLabel = null!;
        private System.Windows.Forms.ComboBox cmbLogLevel = null!;
        private System.Windows.Forms.Label updateIntervalLabel = null!;
        private System.Windows.Forms.TextBox txtGlobalCycleInterval = null!;
        private System.Windows.Forms.Label secondsLabel = null!;
        private System.Windows.Forms.GroupBox catalogGroup = null!;
        private System.Windows.Forms.CheckBox chkSyncPreviewData = null!;
        private System.Windows.Forms.Label catalogLocalFileLabel = null!;
        private System.Windows.Forms.TextBox txtCatalogLocalFile = null!;
        private System.Windows.Forms.Button browseCatalogFileButton = null!;
        private System.Windows.Forms.Label catalogRemotePathLabel = null!;
        private System.Windows.Forms.TextBox txtCatalogRemotePath = null!;
        private System.Windows.Forms.CheckBox chkEnableRcloneCopy = null!;
        private System.Windows.Forms.Label rcloneCopyFolderNameLabel = null!;
        private System.Windows.Forms.TextBox txtRcloneCopyFolderName = null!;
        private System.Windows.Forms.GroupBox backupGroup = null!;
        private System.Windows.Forms.CheckBox chkEnableBackups = null!;
        private System.Windows.Forms.Label backupsLocalPathLabel = null!;
        private System.Windows.Forms.TextBox txtBackupsLocalPath = null!;
        private System.Windows.Forms.Button browseBackupsPathButton = null!;
        private System.Windows.Forms.Label backupsRemotePathLabel = null!;
        private System.Windows.Forms.TextBox txtBackupsRemotePath = null!;
        private System.Windows.Forms.GroupBox sambaGroup = null!;
        private System.Windows.Forms.Label remoteIpLabel = null!;
        private System.Windows.Forms.TextBox txtRemoteIP = null!;
        private System.Windows.Forms.Label sambaUserLabel = null!;
        private System.Windows.Forms.TextBox txtSambaUser = null!;
        private System.Windows.Forms.Label sambaPasswordLabel = null!;
        private System.Windows.Forms.TextBox txtSambaPassword = null!;
        private System.Windows.Forms.LinkLabel gitHubLinkLabel = null!;
        private System.Windows.Forms.LinkLabel websiteLinkLabel = null!;
        private System.Windows.Forms.Button saveButton = null!;
        private System.Windows.Forms.Button cancelButton = null!;
    }
}
