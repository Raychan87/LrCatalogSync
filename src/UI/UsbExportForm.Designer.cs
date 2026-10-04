namespace LrCatalogSync.UI
{
    partial class UsbExportForm
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
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.targetGroup = new System.Windows.Forms.GroupBox();
            this.targetPathLabel = new System.Windows.Forms.Label();
            this.targetPathTextBox = new System.Windows.Forms.TextBox();
            this.browseTargetButton = new System.Windows.Forms.Button();
            this.removeTargetButton = new System.Windows.Forms.Button();
            this.driveInfoLabel = new System.Windows.Forms.Label();
            this.driveDetailsLabel = new System.Windows.Forms.RichTextBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.metadataCheckBox = new System.Windows.Forms.CheckBox();
            this.hashComparisonCheckBox = new System.Windows.Forms.CheckBox();
            this.sourceGroup = new System.Windows.Forms.GroupBox();
            this.sourceListBox = new System.Windows.Forms.ListBox();
            this.addSourceButton = new System.Windows.Forms.Button();
            this.removeSourceButton = new System.Windows.Forms.Button();
            this.excludeLabel = new System.Windows.Forms.Label();
            this.excludePatternsTextBox = new System.Windows.Forms.TextBox();
            this.logGroup = new System.Windows.Forms.GroupBox();
            this.statusLabel = new System.Windows.Forms.RichTextBox();
            this.transferProgressBar = new System.Windows.Forms.ProgressBar();
            this.sectionProgressLabel = new System.Windows.Forms.Label();
            this.transferRateLabel = new System.Windows.Forms.Label();
            this.transferProgressLabel = new System.Windows.Forms.Label();
            this.elapsedTimeLabel = new System.Windows.Forms.Label();
            this.transferToExternalButton = new System.Windows.Forms.Button();
            this.compareButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.availabilityTimer = new System.Windows.Forms.Timer(this.components);
            this.gitHubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.websiteLinkLabel = new System.Windows.Forms.LinkLabel();
            this.targetGroup.SuspendLayout();
            this.sourceGroup.SuspendLayout();
            this.logGroup.SuspendLayout();
            this.SuspendLayout();
            //
            // Ziel Informationen (Rahmen)
            //
            this.targetGroup.Controls.Add(this.targetPathTextBox);
            this.targetGroup.Controls.Add(this.targetPathLabel);
            this.targetGroup.Controls.Add(this.browseTargetButton);
            this.targetGroup.Controls.Add(this.removeTargetButton);
            this.targetGroup.Controls.Add(this.driveInfoLabel);
            this.targetGroup.Controls.Add(this.driveDetailsLabel);
            this.targetGroup.Controls.Add(this.deleteButton);
            this.targetGroup.Controls.Add(this.metadataCheckBox);
            this.targetGroup.Controls.Add(this.hashComparisonCheckBox);
            this.targetGroup.Location = new System.Drawing.Point(12, 12);
            this.targetGroup.Name = "targetGroup";
            this.targetGroup.Size = new System.Drawing.Size(614, 143);
            this.targetGroup.TabIndex = 0;
            this.targetGroup.TabStop = false;
            this.targetGroup.Text = "Ziel Informationen";
            this.targetGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.targetPathTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.targetPathLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.browseTargetButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.removeTargetButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.driveInfoLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.driveDetailsLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.deleteButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.metadataCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.hashComparisonCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            //
            // Label für Zielpfad
            //
            this.targetPathLabel.AutoSize = false;
            this.targetPathLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.targetPathLabel.Location = new System.Drawing.Point(12, 24);
            this.targetPathLabel.Name = "targetPathLabel";
            this.targetPathLabel.Size = new System.Drawing.Size(67, 25);
            this.targetPathLabel.TabIndex = 0;
            this.targetPathLabel.Text = "Zielordner:";
            this.targetPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // TextBox für Zielpfad
            //
            this.targetPathTextBox.Location = new System.Drawing.Point(84, 25);
            this.targetPathTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.targetPathTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.targetPathTextBox.Name = "targetPathTextBox";
            this.targetPathTextBox.Size = new System.Drawing.Size(329, 20);
            this.targetPathTextBox.TabIndex = 1;
            this.targetPathTextBox.TextChanged += new System.EventHandler(this.TargetPathTextBox_TextChanged);
            //
            // Auswahl-Button für Zielpfad
            //
            this.browseTargetButton.Location = new System.Drawing.Point(421, 24);
            this.browseTargetButton.Name = "browseTargetButton";
            this.browseTargetButton.Size = new System.Drawing.Size(85, 25);
            this.browseTargetButton.TabIndex = 2;
            this.browseTargetButton.Text = "Auswählen";
            this.browseTargetButton.UseVisualStyleBackColor = true;
            this.browseTargetButton.Click += new System.EventHandler(this.BrowseTargetButton_Click);
            //
            // Entfernen-Button für Zielpfad
            //
            this.removeTargetButton.Location = new System.Drawing.Point(513, 24);
            this.removeTargetButton.Name = "removeTargetButton";
            this.removeTargetButton.Size = new System.Drawing.Size(85, 25);
            this.removeTargetButton.TabIndex = 3;
            this.removeTargetButton.Text = "Entfernen";
            this.removeTargetButton.UseVisualStyleBackColor = true;
            this.removeTargetButton.Click += new System.EventHandler(this.RemoveTargetButton_Click);
            //
            // Label für Geräteinformationen
            //
            this.driveInfoLabel.AutoSize = false;
            this.driveInfoLabel.Location = new System.Drawing.Point(12, 55);
            this.driveInfoLabel.Name = "driveInfoLabel";
            this.driveInfoLabel.Size = new System.Drawing.Size(69, 25);
            this.driveInfoLabel.TabIndex = 4;
            this.driveInfoLabel.Text = "Geräteinfo.:";
            this.driveInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // Geräteinformationen
            //
            this.driveDetailsLabel.BackColor = System.Drawing.SystemColors.Control;
            this.driveDetailsLabel.AutoSize = true;
            this.driveDetailsLabel.Dock = System.Windows.Forms.DockStyle.None;
            this.driveDetailsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.driveDetailsLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.driveDetailsLabel.DetectUrls = false;
            this.driveDetailsLabel.Location = new System.Drawing.Point(85, 58);
            this.driveDetailsLabel.Multiline = false;
            this.driveDetailsLabel.Name = "driveDetailsLabel";
            this.driveDetailsLabel.ReadOnly = true;
            this.driveDetailsLabel.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.driveDetailsLabel.Size = new System.Drawing.Size(327, 15);
            this.driveDetailsLabel.TabIndex = 5;
            this.driveDetailsLabel.TabStop = false;
            this.driveDetailsLabel.Text = "Kein Laufwerk ausgewählt";
            this.driveDetailsLabel.WordWrap = false;
            //
            // Button zum Löschen des externen Speichers
            //
            this.deleteButton.Enabled = false;
            this.deleteButton.Location = new System.Drawing.Point(421, 55);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(177, 25);
            this.deleteButton.TabIndex = 6;
            this.deleteButton.Text = "Inhalt des Zielordners löschen";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            //
            // CheckBox für Metadaten
            //
            this.metadataCheckBox.AutoSize = false;
            this.metadataCheckBox.Location = new System.Drawing.Point(87, 87);
            this.metadataCheckBox.Name = "metadataCheckBox";
            this.metadataCheckBox.Size = new System.Drawing.Size(300, 20);
            this.metadataCheckBox.TabIndex = 7;
            this.metadataCheckBox.Text = "Metadaten übertragen";
            this.metadataCheckBox.UseVisualStyleBackColor = true;
            this.metadataCheckBox.CheckedChanged += new System.EventHandler(this.MetadataCheckBox_CheckedChanged);
            //
            // CheckBox für Hash-Vergleich
            //
            this.hashComparisonCheckBox.AutoSize = false;
            this.hashComparisonCheckBox.Location = new System.Drawing.Point(87, 111);
            this.hashComparisonCheckBox.Name = "hashComparisonCheckBox";
            this.hashComparisonCheckBox.Size = new System.Drawing.Size(330, 20);
            this.hashComparisonCheckBox.TabIndex = 8;
            this.hashComparisonCheckBox.Text = "Hash-Vergleich verwenden und nachprüfen";
            this.hashComparisonCheckBox.UseVisualStyleBackColor = true;
            this.hashComparisonCheckBox.CheckedChanged += new System.EventHandler(this.HashComparisonCheckBox_CheckedChanged);
            //
            // Datenquellen (Rahmen)
            //
            this.sourceGroup.Controls.Add(this.sourceListBox);
            this.sourceGroup.Controls.Add(this.addSourceButton);
            this.sourceGroup.Controls.Add(this.removeSourceButton);
            this.sourceGroup.Controls.Add(this.excludeLabel);
            this.sourceGroup.Controls.Add(this.excludePatternsTextBox);
            this.sourceGroup.Location = new System.Drawing.Point(12, 160);
            this.sourceGroup.Name = "sourceGroup";
            this.sourceGroup.Size = new System.Drawing.Size(614, 215);
            this.sourceGroup.TabIndex = 1;
            this.sourceGroup.TabStop = false;
            this.sourceGroup.Text = "Datenquellen";
            this.sourceGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.sourceListBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.addSourceButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.removeSourceButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.excludeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.excludePatternsTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            //
            // Liste der Datenquellen
            //
            this.sourceListBox.IntegralHeight = false;
            this.sourceListBox.Location = new System.Drawing.Point(12, 24);
            this.sourceListBox.Name = "sourceListBox";
            this.sourceListBox.Size = new System.Drawing.Size(586, 136);
            this.sourceListBox.TabIndex = 0;
            //
            // Hinzufügen-Button für Datenquellen
            //
            this.addSourceButton.Location = new System.Drawing.Point(12, 174);
            this.addSourceButton.Name = "addSourceButton";
            this.addSourceButton.Size = new System.Drawing.Size(85, 25);
            this.addSourceButton.TabIndex = 1;
            this.addSourceButton.Text = "Hinzufügen";
            this.addSourceButton.UseVisualStyleBackColor = true;
            this.addSourceButton.Click += new System.EventHandler(this.AddSourceButton_Click);
            //
            // Entfernen-Button für Datenquellen
            //
            this.removeSourceButton.Location = new System.Drawing.Point(101, 174);
            this.removeSourceButton.Name = "removeSourceButton";
            this.removeSourceButton.Size = new System.Drawing.Size(85, 25);
            this.removeSourceButton.TabIndex = 2;
            this.removeSourceButton.Text = "Entfernen";
            this.removeSourceButton.UseVisualStyleBackColor = true;
            this.removeSourceButton.Click += new System.EventHandler(this.RemoveSourceButton_Click);
            //
            // Ausschließen-Label
            //
            this.excludeLabel.AutoSize = false;
            this.excludeLabel.Location = new System.Drawing.Point(192, 176);
            this.excludeLabel.Name = "excludeLabel";
            this.excludeLabel.Size = new System.Drawing.Size(81, 25);
            this.excludeLabel.TabIndex = 3;
            this.excludeLabel.Text = "Ausschließen:";
            this.excludeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // Ausschließen-TextBox
            //
            this.excludePatternsTextBox.Location = new System.Drawing.Point(271, 178);
            this.excludePatternsTextBox.Name = "excludePatternsTextBox";
            this.excludePatternsTextBox.PlaceholderText = "Beispiel: *.tmp;*.partial;*Previews.lrdata";
            this.excludePatternsTextBox.Size = new System.Drawing.Size(327, 20);
            this.excludePatternsTextBox.TabIndex = 4;
            this.excludePatternsTextBox.TextChanged += new System.EventHandler(this.ExcludePatternsTextBox_TextChanged);
            //
            // Status-Log (Rahmen)
            //
            this.logGroup.Controls.Add(this.statusLabel);
            this.logGroup.Controls.Add(this.transferProgressBar);
            this.logGroup.Controls.Add(this.sectionProgressLabel);
            this.logGroup.Controls.Add(this.transferRateLabel);
            this.logGroup.Controls.Add(this.transferProgressLabel);
            this.logGroup.Controls.Add(this.elapsedTimeLabel);
            this.logGroup.Controls.Add(this.transferToExternalButton);
            this.logGroup.Controls.Add(this.compareButton);
            this.logGroup.Controls.Add(this.cancelButton);
            this.logGroup.Controls.Add(this.exitButton);
            this.logGroup.Controls.SetChildIndex(this.transferProgressLabel, 0);
            this.logGroup.Location = new System.Drawing.Point(12, 380);
            this.logGroup.Name = "logGroup";
            this.logGroup.Size = new System.Drawing.Size(614, 252);
            this.logGroup.TabIndex = 2;
            this.logGroup.TabStop = false;
            this.logGroup.Text = "Status-Log";
            this.logGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.transferProgressBar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.sectionProgressLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.transferRateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.transferProgressLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.elapsedTimeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.transferToExternalButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.compareButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cancelButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.exitButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            //
            // Status-Log Textbox
            //
            this.statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.statusLabel.Location = new System.Drawing.Point(12, 22);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.ReadOnly = true;
            this.statusLabel.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.statusLabel.Size = new System.Drawing.Size(586, 136);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "";
            this.statusLabel.WordWrap = false;
                        //
            // Fortschritt in Prozent
            //
            this.transferProgressLabel.AutoSize = false;
            this.transferProgressLabel.Text = "0%";
            this.transferProgressLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.transferProgressLabel.BackColor = System.Drawing.Color.Transparent;
            this.transferProgressLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.transferProgressLabel.Location = new System.Drawing.Point(13, 173);
            this.transferProgressLabel.Name = "transferProgressLabel";
            this.transferProgressLabel.Size = new System.Drawing.Size(37, 25);
            this.transferProgressLabel.TabIndex = 4;
            this.transferProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // Fortschrittsbalken
            //
            this.transferProgressBar.Location = new System.Drawing.Point(51, 173);
            this.transferProgressBar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.transferProgressBar.Maximum = 100;
            this.transferProgressBar.Minimum = 0;
            this.transferProgressBar.Name = "transferProgressBar";
            this.transferProgressBar.Size = new System.Drawing.Size(402, 25);
            this.transferProgressBar.TabIndex = 1;
            //
            // Abschnitt (z.B. 1/2)
            //
            this.sectionProgressLabel.AutoSize = false;
            this.sectionProgressLabel.BackColor = System.Drawing.Color.Transparent;
            this.sectionProgressLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.sectionProgressLabel.Location = new System.Drawing.Point(453, 173);
            this.sectionProgressLabel.Name = "sectionProgressLabel";
            this.sectionProgressLabel.Size = new System.Drawing.Size(33, 25);
            this.sectionProgressLabel.TabIndex = 2;
            this.sectionProgressLabel.Text = "0/0";
            this.sectionProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // Übertragungsrate
            //
            this.transferRateLabel.AutoSize = false;
            this.transferRateLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.transferRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.transferRateLabel.BackColor = System.Drawing.Color.Transparent;
            this.transferRateLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.transferRateLabel.Location = new System.Drawing.Point(486, 173);
            this.transferRateLabel.Name = "transferRateLabel";
            this.transferRateLabel.Size = new System.Drawing.Size(74, 25);
            this.transferRateLabel.TabIndex = 3;

            //
            // Verstrichene Zeit
            //
            this.elapsedTimeLabel.AutoSize = false;
            this.elapsedTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.elapsedTimeLabel.BackColor = System.Drawing.Color.Transparent;
            this.elapsedTimeLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.elapsedTimeLabel.Location = new System.Drawing.Point(560, 173);
            this.elapsedTimeLabel.Name = "elapsedTimeLabel";
            this.elapsedTimeLabel.Size = new System.Drawing.Size(36, 25);
            this.elapsedTimeLabel.TabIndex = 5;
            this.elapsedTimeLabel.Text = "00:00";
            this.elapsedTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // Button für Übertragung zu externem Speicher
            //
            this.transferToExternalButton.Enabled = false;
            this.transferToExternalButton.Location = new System.Drawing.Point(12, 212);
            this.transferToExternalButton.Name = "transferToExternalButton";
            this.transferToExternalButton.Size = new System.Drawing.Size(140, 25);
            this.transferToExternalButton.TabIndex = 6;
            this.transferToExternalButton.Text = "Übertragen zu Extern";
            this.transferToExternalButton.UseVisualStyleBackColor = true;
            this.transferToExternalButton.Click += new System.EventHandler(this.ExportToExternalButton_Click);
            //
            // Button für Checksummen-Vergleich
            //
            this.compareButton.Enabled = false;
            this.compareButton.Location = new System.Drawing.Point(158, 212);
            this.compareButton.Name = "compareButton";
            this.compareButton.Size = new System.Drawing.Size(163, 25);
            this.compareButton.TabIndex = 7;
            this.compareButton.Text = "Checksummen-Vergleich";
            this.compareButton.UseVisualStyleBackColor = true;
            this.compareButton.Click += new System.EventHandler(this.CompareButton_Click);
            //
            // Button für Abbrechen
            //
            this.cancelButton.Enabled = false;
            this.cancelButton.Location = new System.Drawing.Point(330, 212);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(85, 25);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            //
            // Button zum Beenden
            //
            this.exitButton.Location = new System.Drawing.Point(514, 212);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(85, 25);
            this.exitButton.TabIndex = 9;
            this.exitButton.Text = "Beenden";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
            //
            // Timer für Verfügbarkeitsprüfung
            //
            this.availabilityTimer.Interval = 500;
            this.availabilityTimer.Tick += new System.EventHandler(this.AvailabilityTimer_Tick);
            //
            // Link zum GitHub-Projekt
            //
            this.gitHubLinkLabel.AutoSize = false;
            this.gitHubLinkLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.gitHubLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.gitHubLinkLabel.LinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.gitHubLinkLabel.Location = new System.Drawing.Point(14, 638);
            this.gitHubLinkLabel.Name = "gitHubLinkLabel";
            this.gitHubLinkLabel.Size = new System.Drawing.Size(150, 18);
            this.gitHubLinkLabel.TabIndex = 3;
            this.gitHubLinkLabel.TabStop = true;
            this.gitHubLinkLabel.Text = "GitHub Project";
            this.gitHubLinkLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.gitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GitHubLinkLabel_LinkClicked);
            //
            // Link zur Webseite
            //
            this.websiteLinkLabel.AutoSize = false;
            this.websiteLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.websiteLinkLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.websiteLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.websiteLinkLabel.LinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.websiteLinkLabel.Location = new System.Drawing.Point(424, 638);
            this.websiteLinkLabel.Name = "websiteLinkLabel";
            this.websiteLinkLabel.Size = new System.Drawing.Size(200, 18);
            this.websiteLinkLabel.TabIndex = 4;
            this.websiteLinkLabel.TabStop = true;
            this.websiteLinkLabel.Text = "© Fototour und Technik";
            this.websiteLinkLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.websiteLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WebsiteLinkLabel_LinkClicked);
            //
            // UsbExportForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 667);
            this.Controls.Add(this.targetGroup);
            this.Controls.Add(this.sourceGroup);
            this.Controls.Add(this.logGroup);
            this.Controls.Add(this.gitHubLinkLabel);
            this.Controls.Add(this.websiteLinkLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "UsbExportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "USB-Export";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UsbExportForm_FormClosing);
            this.targetGroup.ResumeLayout(false);
            this.targetGroup.PerformLayout();
            this.sourceGroup.ResumeLayout(false);
            this.sourceGroup.PerformLayout();
            this.logGroup.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox targetGroup;
        private System.Windows.Forms.Label targetPathLabel;
        private System.Windows.Forms.TextBox targetPathTextBox;
        private System.Windows.Forms.Button browseTargetButton;
        private System.Windows.Forms.Button removeTargetButton;
        private System.Windows.Forms.Label driveInfoLabel;
        private System.Windows.Forms.RichTextBox driveDetailsLabel;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.CheckBox metadataCheckBox;
        private System.Windows.Forms.CheckBox hashComparisonCheckBox;
        private System.Windows.Forms.GroupBox sourceGroup;
        private System.Windows.Forms.ListBox sourceListBox;
        private System.Windows.Forms.Button addSourceButton;
        private System.Windows.Forms.Button removeSourceButton;
        private System.Windows.Forms.Label excludeLabel;
        private System.Windows.Forms.TextBox excludePatternsTextBox;
        private System.Windows.Forms.GroupBox logGroup;
        private System.Windows.Forms.LinkLabel gitHubLinkLabel;
        private System.Windows.Forms.LinkLabel websiteLinkLabel;
        private System.Windows.Forms.RichTextBox statusLabel;
        private System.Windows.Forms.ProgressBar transferProgressBar;
        private System.Windows.Forms.Label sectionProgressLabel;
        private System.Windows.Forms.Label transferRateLabel;
        private System.Windows.Forms.Label transferProgressLabel;
        private System.Windows.Forms.Label elapsedTimeLabel;
        private System.Windows.Forms.Button transferToExternalButton;
        private System.Windows.Forms.Button compareButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Timer availabilityTimer;
    }
}