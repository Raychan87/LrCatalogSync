namespace LrCatalogSync.UI
{
    partial class UsbExportForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.targetGroup = new System.Windows.Forms.GroupBox();
            this.targetPanel = new System.Windows.Forms.TableLayoutPanel();
            this.targetPathLabel = new System.Windows.Forms.Label();
            this.targetPathTextBox = new System.Windows.Forms.TextBox();
            this.targetButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.browseTargetButton = new System.Windows.Forms.Button();
            this.removeTargetButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.driveInfoLabel = new System.Windows.Forms.Label();
            this.driveDetailsLabel = new System.Windows.Forms.RichTextBox();
            this.hashComparisonCheckBox = new System.Windows.Forms.CheckBox();
            this.metadataCheckBox = new System.Windows.Forms.CheckBox();
            this.sourceGroup = new System.Windows.Forms.GroupBox();
            this.sourcePanel = new System.Windows.Forms.TableLayoutPanel();
            this.sourceListBox = new System.Windows.Forms.ListBox();
            this.sourceFooter = new System.Windows.Forms.FlowLayoutPanel();
            this.addSourceButton = new System.Windows.Forms.Button();
            this.removeSourceButton = new System.Windows.Forms.Button();
            this.excludeLabel = new System.Windows.Forms.Label();
            this.excludePatternsTextBox = new System.Windows.Forms.TextBox();
            this.logGroup = new System.Windows.Forms.GroupBox();
            this.logPanel = new System.Windows.Forms.TableLayoutPanel();
            this.statusLabel = new System.Windows.Forms.RichTextBox();
            this.progressPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.transferProgressBar = new System.Windows.Forms.ProgressBar();
            this.sectionProgressLabel = new System.Windows.Forms.Label();
            this.transferProgressLabel = new System.Windows.Forms.Label();
            this.transferRateLabel = new System.Windows.Forms.Label();
            this.elapsedTimeLabel = new System.Windows.Forms.Label();
            this.actionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.transferToExternalButton = new System.Windows.Forms.Button();
            this.compareButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.availabilityTimer = new System.Windows.Forms.Timer(this.components);
            this.layout.SuspendLayout();
            this.targetGroup.SuspendLayout();
            this.targetPanel.SuspendLayout();
            this.targetButtons.SuspendLayout();
            this.sourceGroup.SuspendLayout();
            this.sourcePanel.SuspendLayout();
            this.sourceFooter.SuspendLayout();
            this.logGroup.SuspendLayout();
            this.logPanel.SuspendLayout();
            this.progressPanel.SuspendLayout();
            this.actionPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // Fensterrahmen von UsbExportForm
            //
            this.layout.ColumnCount = 1;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.Controls.Add(this.targetGroup, 0, 0);
            this.layout.Controls.Add(this.sourceGroup, 0, 1);
            this.layout.Controls.Add(this.logGroup, 0, 2);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Location = new System.Drawing.Point(0, 0);
            this.layout.Name = "layout";
            this.layout.Padding = new System.Windows.Forms.Padding(12);
            this.layout.RowCount = 3;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 156F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 226F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.Size = new System.Drawing.Size(744, 743);
            this.layout.TabIndex = 0;
            //
            // Zielinformationen Rahmen
            //
            this.targetGroup.Controls.Add(this.targetPanel);
            this.targetGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.targetGroup.Location = new System.Drawing.Point(15, 15);
            this.targetGroup.Name = "targetGroup";
            this.targetGroup.Padding = new System.Windows.Forms.Padding(12);
            this.targetGroup.Size = new System.Drawing.Size(714, 150);
            this.targetGroup.TabIndex = 0;
            this.targetGroup.TabStop = false;
            this.targetGroup.Text = "Ziel Informationen";
            //
            // Zielinformationen Panel
            //
            this.targetPanel.ColumnCount = 3;
            this.targetPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.targetPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.targetPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.targetPanel.Controls.Add(this.targetPathLabel, 0, 0);
            this.targetPanel.Controls.Add(this.targetPathTextBox, 1, 0);
            this.targetPanel.Controls.Add(this.targetButtons, 2, 0);
            this.targetPanel.Controls.Add(this.driveInfoLabel, 0, 1);
            this.targetPanel.Controls.Add(this.driveDetailsLabel, 1, 1);
            this.targetPanel.Controls.Add(this.deleteButton, 2, 1);
            this.targetPanel.Controls.Add(this.metadataCheckBox, 1, 2);
            this.targetPanel.Controls.Add(this.hashComparisonCheckBox, 1, 3);
            this.targetPanel.Location = new System.Drawing.Point(15, 15);
            this.targetPanel.Name = "targetPanel";
            this.targetPanel.RowCount = 4;
            this.targetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.targetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.targetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.targetPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.targetPanel.Size = new System.Drawing.Size(690, 132);
            this.targetPanel.TabIndex = 0;
            this.targetPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.targetPanel.AutoSize = false;
            this.targetPanel.AllowDrop = false;
            this.targetPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            //
            // Label für Zielpfad
            //
            this.targetPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.targetPathLabel.AutoSize = true;
            this.targetPathLabel.Location = new System.Drawing.Point(3, 0);
            this.targetPathLabel.Name = "targetPathLabel";
            this.targetPathLabel.Size = new System.Drawing.Size(62, 15);
            this.targetPathLabel.TabIndex = 0;
            this.targetPathLabel.Text = "Zielordner:";
            //
            // TextBox für Zielpfad
            //
            this.targetPathTextBox.Location = new System.Drawing.Point(143, 8);
            this.targetPathTextBox.Name = "targetPathTextBox";
            this.targetPathTextBox.Size = new System.Drawing.Size(344, 30);
            this.targetPathTextBox.TabIndex = 1;
            this.targetPathTextBox.TextChanged += new System.EventHandler(this.TargetPathTextBox_TextChanged);
            this.targetPathTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.targetPathTextBox.CausesValidation = true;
            //
            // Panel für Auswahl- und Entfernen-Buttons des Zielpfads
            //
            this.targetButtons.AutoSize = true;
            this.targetButtons.Controls.Add(this.browseTargetButton);
            this.targetButtons.Controls.Add(this.removeTargetButton);
            this.targetButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.targetButtons.Location = new System.Drawing.Point(506, 3);
            this.targetButtons.Name = "targetButtons";
            this.targetButtons.Size = new System.Drawing.Size(181, 26);
            this.targetButtons.TabIndex = 2;
            this.targetButtons.WrapContents = false;
            this.targetButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            //
            // Auswahl-Button für Zielpfad
            //
            this.browseTargetButton.AutoSize = false;
            this.browseTargetButton.Location = new System.Drawing.Point(30, 4);
            this.browseTargetButton.Name = "browseTargetButton";
            this.browseTargetButton.Size = new System.Drawing.Size(86, 23);
            this.browseTargetButton.TabIndex = 0;
            this.browseTargetButton.Text = "Auswählen";
            this.browseTargetButton.UseVisualStyleBackColor = true;
            this.browseTargetButton.Click += new System.EventHandler(this.BrowseTargetButton_Click);
            this.browseTargetButton.Margin = new System.Windows.Forms.Padding(7, 3, 2, 3);
            this.browseTargetButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            //
            // Entfernen-Button für Zielpfad
            //
            this.removeTargetButton.AutoSize = true;
            this.removeTargetButton.Location = new System.Drawing.Point(95, 3);
            this.removeTargetButton.Name = "removeTargetButton";
            this.removeTargetButton.Size = new System.Drawing.Size(82, 23);
            this.removeTargetButton.TabIndex = 1;
            this.removeTargetButton.Text = "Entfernen";
            this.removeTargetButton.UseVisualStyleBackColor = true;
            this.removeTargetButton.Click += new System.EventHandler(this.RemoveTargetButton_Click);
            this.removeTargetButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.removeTargetButton.Margin = new System.Windows.Forms.Padding(2, 3, 8, 3);
            //
            // Label für Geräteinformationen
            //
            this.driveInfoLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.driveInfoLabel.AutoSize = true;
            this.driveInfoLabel.Location = new System.Drawing.Point(3, 42);
            this.driveInfoLabel.Name = "driveInfoLabel";
            this.driveInfoLabel.Size = new System.Drawing.Size(113, 15);
            this.driveInfoLabel.TabIndex = 3;
            this.driveInfoLabel.Text = "Geräteinformationen:";
            this.driveInfoLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            //
            // Geräteinformationen
            //
            this.driveDetailsLabel.BackColor = System.Drawing.SystemColors.Control;
            this.driveDetailsLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.driveDetailsLabel.DetectUrls = false;
            this.driveDetailsLabel.Location = new System.Drawing.Point(153, 42);
            this.driveDetailsLabel.Name = "driveDetailsLabel";
            this.driveDetailsLabel.ReadOnly = true;
            this.driveDetailsLabel.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.driveDetailsLabel.Size = new System.Drawing.Size(344, 15);
            this.driveDetailsLabel.TabIndex = 4;
            this.driveDetailsLabel.Text = "Kein Laufwerk ausgewählt";
            this.driveDetailsLabel.TabStop = false;
            this.driveDetailsLabel.WordWrap = true;
            this.driveDetailsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            //
            // CheckBox für Hash-Vergleich
            //
            this.hashComparisonCheckBox.AutoSize = true;
            this.hashComparisonCheckBox.Location = new System.Drawing.Point(3, 97);
            this.hashComparisonCheckBox.Name = "hashComparisonCheckBox";
            this.hashComparisonCheckBox.Size = new System.Drawing.Size(226, 19);
            this.hashComparisonCheckBox.TabIndex = 5;
            this.hashComparisonCheckBox.Text = "Hash-Vergleich verwenden und nachprüfen";
            this.hashComparisonCheckBox.UseVisualStyleBackColor = true;
            this.hashComparisonCheckBox.CheckedChanged += new System.EventHandler(this.HashComparisonCheckBox_CheckedChanged);
            this.hashComparisonCheckBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            //
            // CheckBox für Metadaten
            //
            this.metadataCheckBox.AutoSize = true;
            this.metadataCheckBox.Location = new System.Drawing.Point(506, 84);
            this.metadataCheckBox.Name = "metadataCheckBox";
            this.metadataCheckBox.Size = new System.Drawing.Size(196, 19);
            this.metadataCheckBox.TabIndex = 6;
            this.metadataCheckBox.Text = "Metadaten übertragen";
            this.metadataCheckBox.UseVisualStyleBackColor = true;
            this.metadataCheckBox.CheckedChanged += new System.EventHandler(this.MetadataCheckBox_CheckedChanged);
            this.metadataCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.metadataCheckBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            //
            // Datenquellen Rahmen
            //
            this.sourceGroup.Controls.Add(this.sourcePanel);
            this.sourceGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceGroup.Location = new System.Drawing.Point(15, 171);
            this.sourceGroup.Name = "sourceGroup";
            this.sourceGroup.Padding = new System.Windows.Forms.Padding(12);
            this.sourceGroup.Size = new System.Drawing.Size(714, 220);
            this.sourceGroup.TabIndex = 1;
            this.sourceGroup.TabStop = false;
            this.sourceGroup.Text = "Datenquellen";
            //
            // Datenquellen Panel
            //
            this.sourcePanel.ColumnCount = 1;
            this.sourcePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.sourcePanel.Controls.Add(this.sourceListBox, 0, 0);
            this.sourcePanel.Controls.Add(this.sourceFooter, 0, 1);
            this.sourcePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourcePanel.Location = new System.Drawing.Point(12, 19);
            this.sourcePanel.Name = "sourcePanel";
            this.sourcePanel.RowCount = 2;
            this.sourcePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.sourcePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.sourcePanel.Size = new System.Drawing.Size(690, 189);
            this.sourcePanel.TabIndex = 0;
            //
            // TextBox für Datenquellen
            //
            this.sourceListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceListBox.IntegralHeight = false;
            this.sourceListBox.Location = new System.Drawing.Point(3, 3);
            this.sourceListBox.Name = "sourceListBox";
            this.sourceListBox.Size = new System.Drawing.Size(684, 135);
            this.sourceListBox.TabIndex = 0;
            //
            // Footer Panel für Datenquellen
            //
            this.sourceFooter.Controls.Add(this.addSourceButton);
            this.sourceFooter.Controls.Add(this.removeSourceButton);
            this.sourceFooter.Controls.Add(this.excludeLabel);
            this.sourceFooter.Controls.Add(this.excludePatternsTextBox);
            this.sourceFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceFooter.Location = new System.Drawing.Point(3, 144);
            this.sourceFooter.Name = "sourceFooter";
            this.sourceFooter.Size = new System.Drawing.Size(684, 42);
            this.sourceFooter.TabIndex = 1;
            this.sourceFooter.WrapContents = false;
            //
            // Hinzufügen-Button für Datenquellen
            //
            this.addSourceButton.AutoSize = true;
            this.addSourceButton.Location = new System.Drawing.Point(3, 15);
            this.addSourceButton.Name = "addSourceButton";
            this.addSourceButton.Size = new System.Drawing.Size(88, 23);
            this.addSourceButton.TabIndex = 0;
            this.addSourceButton.Text = "Hinzufügen";
            this.addSourceButton.UseVisualStyleBackColor = true;
            this.addSourceButton.Click += new System.EventHandler(this.AddSourceButton_Click);
            this.addSourceButton.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            //
            // Entfernen-Button für Datenquellen
            //
            this.removeSourceButton.AutoSize = true;
            this.removeSourceButton.Location = new System.Drawing.Point(97, 15);
            this.removeSourceButton.Name = "removeSourceButton";
            this.removeSourceButton.Size = new System.Drawing.Size(82, 23);
            this.removeSourceButton.TabIndex = 1;
            this.removeSourceButton.Text = "Entfernen";
            this.removeSourceButton.UseVisualStyleBackColor = true;
            this.removeSourceButton.Click += new System.EventHandler(this.RemoveSourceButton_Click);
            this.removeSourceButton.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            //
            // Ausschließen-Label
            //
            this.excludeLabel.AutoSize = false;
            this.excludeLabel.Location = new System.Drawing.Point(191, 22);
            this.excludeLabel.Name = "excludeLabel";
            this.excludeLabel.Padding = new System.Windows.Forms.Padding(0);
            this.excludeLabel.Size = new System.Drawing.Size(80, 25);
            this.excludeLabel.TabIndex = 2;
            this.excludeLabel.Text = "Ausschließen:";
            this.excludeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.excludeLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.excludeLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.excludeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // Ausschließen-TextBox
            //
            this.excludePatternsTextBox.Location = new System.Drawing.Point(185, 3);
            this.excludePatternsTextBox.Name = "excludePatternsTextBox";
            this.excludePatternsTextBox.PlaceholderText = "Beispiel: *.tmp;*.partial;*Previews.lrdata";
            this.excludePatternsTextBox.Size = new System.Drawing.Size(408, 25);
            this.excludePatternsTextBox.TabIndex = 3;
            this.excludePatternsTextBox.TextChanged += new System.EventHandler(this.ExcludePatternsTextBox_TextChanged);
            this.excludePatternsTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.excludePatternsTextBox.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            //
            // Log- und Aktionen Rahmen
            //
            this.logGroup.Controls.Add(this.logPanel);
            this.logGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logGroup.Location = new System.Drawing.Point(15, 397);
            this.logGroup.Name = "logGroup";
            this.logGroup.Padding = new System.Windows.Forms.Padding(12);
            this.logGroup.Size = new System.Drawing.Size(714, 333);
            this.logGroup.TabIndex = 2;
            this.logGroup.TabStop = false;
            this.logGroup.Text = "Status-Log";
            //
            // Log- und Aktionen Panel
            //
            this.logPanel.ColumnCount = 1;
            this.logPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.logPanel.Controls.Add(this.statusLabel, 0, 0);
            this.logPanel.Controls.Add(this.progressPanel, 0, 1);
            this.logPanel.Controls.Add(this.actionPanel, 0, 2);
            this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logPanel.Location = new System.Drawing.Point(12, 19);
            this.logPanel.Name = "logPanel";
            this.logPanel.RowCount = 3;
            this.logPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.logPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.logPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.logPanel.Size = new System.Drawing.Size(690, 302);
            this.logPanel.TabIndex = 0;
            //
            //  Status-Log Textbox
            //
            this.statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusLabel.Location = new System.Drawing.Point(3, 3);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.ReadOnly = true;
            this.statusLabel.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.statusLabel.Size = new System.Drawing.Size(684, 250);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "";
            this.statusLabel.WordWrap = false;
            //
            // Transfer-Fortschritt
            //
            this.progressPanel.AutoScroll = false;
            this.progressPanel.Controls.Add(this.transferProgressBar);
            this.progressPanel.Controls.Add(this.sectionProgressLabel);
            this.progressPanel.Controls.Add(this.transferRateLabel);
            this.progressPanel.Controls.Add(this.transferProgressLabel);
            this.progressPanel.Controls.Add(this.elapsedTimeLabel);
            this.progressPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.progressPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressPanel.TabIndex = 2;
            this.progressPanel.WrapContents = false;
            this.transferProgressBar.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.transferProgressBar.Name = "transferProgressBar";
            this.transferProgressBar.Minimum = 0;
            this.transferProgressBar.Maximum = 100;
            this.transferProgressBar.TabIndex = 0;
            this.transferProgressBar.Size = new System.Drawing.Size(490, 18);
            this.transferProgressBar.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.sectionProgressLabel.AutoSize = false;
            this.sectionProgressLabel.Margin = new System.Windows.Forms.Padding(0);
            this.sectionProgressLabel.Size = new System.Drawing.Size(35, 24);
            this.sectionProgressLabel.Name = "sectionProgressLabel";
            this.sectionProgressLabel.Text = "0/0";
            this.sectionProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.sectionProgressLabel.TabIndex = 1;
            this.sectionProgressLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.transferProgressLabel.AutoSize = false;
            this.transferProgressLabel.Margin = new System.Windows.Forms.Padding(0);
            this.transferProgressLabel.Size = new System.Drawing.Size(40, 24);
            this.transferProgressLabel.Name = "transferProgressLabel";
            this.transferProgressLabel.Text = "0%";
            this.transferProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.transferProgressLabel.TabIndex = 1;
            this.transferProgressLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.transferRateLabel.AutoSize = false;
            this.transferRateLabel.Margin = new System.Windows.Forms.Padding(0);
            this.transferRateLabel.Size = new System.Drawing.Size(80, 24);
            this.transferRateLabel.Name = "transferRateLabel";
            this.transferRateLabel.Text = "";
            this.transferRateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.transferRateLabel.TabIndex = 2;
            this.transferRateLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.elapsedTimeLabel.AutoSize = false;
            this.elapsedTimeLabel.Margin = new System.Windows.Forms.Padding(0);
            this.elapsedTimeLabel.Size = new System.Drawing.Size(35, 24);
            this.elapsedTimeLabel.Name = "elapsedTimeLabel";
            this.elapsedTimeLabel.Text = "00:00";
            this.elapsedTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.elapsedTimeLabel.TabIndex = 3;
            this.elapsedTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.elapsedTimeLabel.Location = new System.Drawing.Point(590, 0);
            //
            // Button-Panel für Aktionen
            //
            this.actionPanel.AutoScroll = true;
            this.actionPanel.Controls.Add(this.transferToExternalButton);
            this.actionPanel.Controls.Add(this.compareButton);
            this.actionPanel.Controls.Add(this.cancelButton);
            this.actionPanel.Controls.Add(this.exitButton);
            this.actionPanel.Location = new System.Drawing.Point(3, 259);
            this.actionPanel.Name = "actionPanel";
            this.actionPanel.Size = new System.Drawing.Size(684, 40);
            this.actionPanel.TabIndex = 1;
            this.actionPanel.WrapContents = false;
            this.actionPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            //
            // Button für Übertragung zu externem Speicher
            //
            this.transferToExternalButton.AutoSize = false;
            this.transferToExternalButton.Enabled = false;
            this.transferToExternalButton.Location = new System.Drawing.Point(3, 9);
            this.transferToExternalButton.Name = "transferToExternalButton";
            this.transferToExternalButton.Size = new System.Drawing.Size(131, 25);
            this.transferToExternalButton.TabIndex = 0;
            this.transferToExternalButton.Text = "Übertragen zu Extern";
            this.transferToExternalButton.UseVisualStyleBackColor = true;
            this.transferToExternalButton.Click += new System.EventHandler(this.ExportToExternalButton_Click);
            this.transferToExternalButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.transferToExternalButton.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            //
            // Button für Vergleich
            //
            this.compareButton.AutoSize = false;
            this.compareButton.Enabled = false;
            this.compareButton.Location = new System.Drawing.Point(140, 9);
            this.compareButton.Name = "compareButton";
            this.compareButton.Size = new System.Drawing.Size(150, 25);
            this.compareButton.TabIndex = 1;
            this.compareButton.Text = "Checksummen-Vergleich";
            this.compareButton.UseVisualStyleBackColor = true;
            this.compareButton.Click += new System.EventHandler(this.CompareButton_Click);
            this.compareButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.compareButton.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            //
            // Button für Löschen
            //
            this.deleteButton.AutoSize = true;
            this.deleteButton.Enabled = false;
            this.deleteButton.Location = new System.Drawing.Point(3, 32);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(174, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Externen Speicher löschen";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            this.deleteButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            //
            // Button für Abbrechen
            //
            this.cancelButton.Enabled = false;
            this.cancelButton.Location = new System.Drawing.Point(279, 9);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(84, 25);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Abbrechen";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            // Button zum Beenden
            //
            this.exitButton.Location = new System.Drawing.Point(366, 9);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(84, 25);
            this.exitButton.TabIndex = 4;
            this.exitButton.Text = "Beenden";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
            this.exitButton.Margin = new System.Windows.Forms.Padding(209, 7, 3, 3);
            this.exitButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            //
            // Timer für Verfügbarkeitsprüfung
            //
            this.availabilityTimer.Interval = 500;
            this.availabilityTimer.Tick += new System.EventHandler(this.AvailabilityTimer_Tick);
            //
            // UsbExportForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 743);
            this.Controls.Add(this.layout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "UsbExportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "USB-Export";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UsbExportForm_FormClosing);
            this.layout.ResumeLayout(false);
            this.layout.PerformLayout();
            this.targetGroup.ResumeLayout(false);
            this.targetPanel.ResumeLayout(false);
            this.targetPanel.PerformLayout();
            this.targetButtons.ResumeLayout(false);
            this.targetButtons.PerformLayout();
            this.sourceGroup.ResumeLayout(false);
            this.sourcePanel.ResumeLayout(false);
            this.sourceFooter.ResumeLayout(false);
            this.sourceFooter.PerformLayout();
            this.logGroup.ResumeLayout(false);
            this.logPanel.ResumeLayout(false);
            this.progressPanel.ResumeLayout(false);
            this.actionPanel.ResumeLayout(false);
            this.actionPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.GroupBox targetGroup;
        private System.Windows.Forms.TableLayoutPanel targetPanel;
        private System.Windows.Forms.Label targetPathLabel;
        private System.Windows.Forms.TextBox targetPathTextBox;
        private System.Windows.Forms.FlowLayoutPanel targetButtons;
        private System.Windows.Forms.Button browseTargetButton;
        private System.Windows.Forms.Button removeTargetButton;
        private System.Windows.Forms.Label driveInfoLabel;
        private System.Windows.Forms.RichTextBox driveDetailsLabel;
        private System.Windows.Forms.CheckBox hashComparisonCheckBox;
        private System.Windows.Forms.CheckBox metadataCheckBox;
        private System.Windows.Forms.GroupBox sourceGroup;
        private System.Windows.Forms.TableLayoutPanel sourcePanel;
        private System.Windows.Forms.ListBox sourceListBox;
        private System.Windows.Forms.FlowLayoutPanel sourceFooter;
        private System.Windows.Forms.Button addSourceButton;
        private System.Windows.Forms.Button removeSourceButton;
        private System.Windows.Forms.Label excludeLabel;
        private System.Windows.Forms.TextBox excludePatternsTextBox;
        private System.Windows.Forms.GroupBox logGroup;
        private System.Windows.Forms.TableLayoutPanel logPanel;
        private System.Windows.Forms.RichTextBox statusLabel;
        private System.Windows.Forms.FlowLayoutPanel progressPanel;
        private System.Windows.Forms.ProgressBar transferProgressBar;
        private System.Windows.Forms.Label sectionProgressLabel;
        private System.Windows.Forms.Label transferProgressLabel;
        private System.Windows.Forms.Label transferRateLabel;
        private System.Windows.Forms.Label elapsedTimeLabel;
        private System.Windows.Forms.FlowLayoutPanel actionPanel;
        private System.Windows.Forms.Button transferToExternalButton;
        private System.Windows.Forms.Button compareButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Timer availabilityTimer;
    }
}
