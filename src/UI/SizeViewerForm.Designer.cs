namespace LrCatalogSync.UI
{
    partial class SizeViewerForm
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
        /// Beim Vergrößern des Fensters wachsen Rahmen und Liste über Anchor mit.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.selectFolderButton = new System.Windows.Forms.Button();
            this.cancelScanButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.adminButton = new System.Windows.Forms.Button();
            this.resultGroup = new System.Windows.Forms.GroupBox();
            this.resultListView = new System.Windows.Forms.ListView();
            this.nameColumn = new System.Windows.Forms.ColumnHeader();
            this.sizeColumn = new System.Windows.Forms.ColumnHeader();
            this.allocatedColumn = new System.Windows.Forms.ColumnHeader();
            this.filesColumn = new System.Windows.Forms.ColumnHeader();
            this.foldersColumn = new System.Windows.Forms.ColumnHeader();
            this.percentColumn = new System.Windows.Forms.ColumnHeader();
            this.modifiedColumn = new System.Windows.Forms.ColumnHeader();
            this.rowContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openInExplorerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusLabel = new System.Windows.Forms.Label();
            this.exitButton = new System.Windows.Forms.Button();
            this.gitHubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.websiteLinkLabel = new System.Windows.Forms.LinkLabel();
            this.statusTimer = new System.Windows.Forms.Timer(this.components);
            this.resultGroup.SuspendLayout();
            this.rowContextMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // Button: Verzeichnis auswählen
            //
            this.selectFolderButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.selectFolderButton.Location = new System.Drawing.Point(12, 12);
            this.selectFolderButton.Name = "selectFolderButton";
            this.selectFolderButton.Size = new System.Drawing.Size(160, 28);
            this.selectFolderButton.TabIndex = 0;
            this.selectFolderButton.Text = "Verzeichnis auswählen";
            this.selectFolderButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.selectFolderButton.UseVisualStyleBackColor = true;
            this.selectFolderButton.Click += new System.EventHandler(this.SelectFolderButton_Click);
            //
            // Button: Scan abbrechen
            //
            this.cancelScanButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cancelScanButton.Location = new System.Drawing.Point(178, 12);
            this.cancelScanButton.Name = "cancelScanButton";
            this.cancelScanButton.Size = new System.Drawing.Size(130, 28);
            this.cancelScanButton.TabIndex = 1;
            this.cancelScanButton.Text = "Scan abbrechen";
            this.cancelScanButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cancelScanButton.UseVisualStyleBackColor = true;
            this.cancelScanButton.Click += new System.EventHandler(this.CancelScanButton_Click);
            //
            // Button: Aktualisieren
            //
            this.refreshButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.refreshButton.Location = new System.Drawing.Point(314, 12);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(115, 28);
            this.refreshButton.TabIndex = 2;
            this.refreshButton.Text = "Aktualisieren";
            this.refreshButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            //
            // Button: Als Administrator scannen
            //
            this.adminButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.adminButton.Location = new System.Drawing.Point(435, 12);
            this.adminButton.Name = "adminButton";
            this.adminButton.Size = new System.Drawing.Size(190, 28);
            this.adminButton.TabIndex = 3;
            this.adminButton.Text = "Als Administrator scannen";
            this.adminButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.adminButton.UseVisualStyleBackColor = true;
            this.adminButton.Click += new System.EventHandler(this.AdminButton_Click);
            //
            // Speicherbelegung (Rahmen)
            //
            this.resultGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.resultGroup.Controls.Add(this.resultListView);
            this.resultGroup.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.resultGroup.Location = new System.Drawing.Point(12, 48);
            this.resultGroup.Name = "resultGroup";
            this.resultGroup.Size = new System.Drawing.Size(960, 434);
            this.resultGroup.TabIndex = 4;
            this.resultGroup.TabStop = false;
            this.resultGroup.Text = "Speicherbelegung";
            //
            // Liste mit dem Größenbaum
            //
            this.resultListView.AllowColumnReorder = true;
            this.resultListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.resultListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.sizeColumn,
            this.allocatedColumn,
            this.filesColumn,
            this.foldersColumn,
            this.percentColumn,
            this.modifiedColumn});
            this.resultListView.ContextMenuStrip = this.rowContextMenu;
            this.resultListView.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.resultListView.FullRowSelect = true;
            this.resultListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
            this.resultListView.Location = new System.Drawing.Point(10, 22);
            this.resultListView.MultiSelect = false;
            this.resultListView.Name = "resultListView";
            this.resultListView.OwnerDraw = true;
            this.resultListView.Size = new System.Drawing.Size(940, 402);
            this.resultListView.TabIndex = 0;
            this.resultListView.UseCompatibleStateImageBehavior = false;
            this.resultListView.View = System.Windows.Forms.View.Details;
            this.resultListView.VirtualMode = true;
            this.resultListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ResultListView_ColumnClick);
            this.resultListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.ResultListView_DrawColumnHeader);
            this.resultListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.ResultListView_DrawSubItem);
            this.resultListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ResultListView_RetrieveVirtualItem);
            this.resultListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ResultListView_KeyDown);
            this.resultListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ResultListView_MouseDoubleClick);
            this.resultListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ResultListView_MouseDown);
            //
            // Spalte: Name
            //
            this.nameColumn.Text = "Name";
            this.nameColumn.Width = 370;
            //
            // Spalte: Größe
            //
            this.sizeColumn.Text = "Größe";
            this.sizeColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.sizeColumn.Width = 90;
            //
            // Spalte: Belegt
            //
            this.allocatedColumn.Text = "Belegt";
            this.allocatedColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.allocatedColumn.Width = 90;
            //
            // Spalte: Dateien
            //
            this.filesColumn.Text = "Dateien";
            this.filesColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.filesColumn.Width = 80;
            //
            // Spalte: Ordner
            //
            this.foldersColumn.Text = "Ordner";
            this.foldersColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.foldersColumn.Width = 80;
            //
            // Spalte: % Anteil
            //
            this.percentColumn.Text = "% Anteil";
            this.percentColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.percentColumn.Width = 90;
            //
            // Spalte: Letzte Änderung
            //
            this.modifiedColumn.Text = "Letzte Änderung";
            this.modifiedColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.modifiedColumn.Width = 105;
            //
            // Kontextmenü der Zeilen
            //
            this.rowContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInExplorerMenuItem});
            this.rowContextMenu.Name = "rowContextMenu";
            this.rowContextMenu.Size = new System.Drawing.Size(180, 26);
            this.rowContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.RowContextMenu_Opening);
            //
            // Kontextmenü: Im Explorer öffnen
            //
            this.openInExplorerMenuItem.Name = "openInExplorerMenuItem";
            this.openInExplorerMenuItem.Size = new System.Drawing.Size(179, 22);
            this.openInExplorerMenuItem.Text = "Im Explorer öffnen";
            this.openInExplorerMenuItem.Click += new System.EventHandler(this.OpenInExplorerMenuItem_Click);
            //
            // Statuszeile (einzeilig, mittig zum Beenden-Button)
            //
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.AutoEllipsis = true;
            this.statusLabel.AutoSize = false;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.statusLabel.Location = new System.Drawing.Point(12, 494);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(865, 17);
            this.statusLabel.TabIndex = 5;
            this.statusLabel.Text = "Kein Verzeichnis ausgewählt.";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // Button zum Beenden
            //
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.exitButton.Location = new System.Drawing.Point(887, 488);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(85, 28);
            this.exitButton.TabIndex = 6;
            this.exitButton.Text = "Beenden";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
            //
            // Link zum GitHub-Projekt
            //
            this.gitHubLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gitHubLinkLabel.AutoSize = false;
            this.gitHubLinkLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.gitHubLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.gitHubLinkLabel.LinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.gitHubLinkLabel.Location = new System.Drawing.Point(14, 532);
            this.gitHubLinkLabel.Name = "gitHubLinkLabel";
            this.gitHubLinkLabel.Size = new System.Drawing.Size(150, 18);
            this.gitHubLinkLabel.TabIndex = 7;
            this.gitHubLinkLabel.TabStop = true;
            this.gitHubLinkLabel.Text = "GitHub Project";
            this.gitHubLinkLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.gitHubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GitHubLinkLabel_LinkClicked);
            //
            // Link zur Webseite
            //
            this.websiteLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.websiteLinkLabel.AutoSize = false;
            this.websiteLinkLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.websiteLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.websiteLinkLabel.LinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.websiteLinkLabel.Location = new System.Drawing.Point(770, 532);
            this.websiteLinkLabel.Name = "websiteLinkLabel";
            this.websiteLinkLabel.Size = new System.Drawing.Size(200, 18);
            this.websiteLinkLabel.TabIndex = 8;
            this.websiteLinkLabel.TabStop = true;
            this.websiteLinkLabel.Text = "© Fototour und Technik";
            this.websiteLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.websiteLinkLabel.VisitedLinkColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.websiteLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WebsiteLinkLabel_LinkClicked);
            //
            // Timer für die Statuszeile während des Scans
            //
            this.statusTimer.Interval = 250;
            this.statusTimer.Tick += new System.EventHandler(this.StatusTimer_Tick);
            //
            // SizeViewerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.selectFolderButton);
            this.Controls.Add(this.cancelScanButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.adminButton);
            this.Controls.Add(this.resultGroup);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.gitHubLinkLabel);
            this.Controls.Add(this.websiteLinkLabel);
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "SizeViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SizeViewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SizeViewerForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SizeViewerForm_FormClosed);
            this.Shown += new System.EventHandler(this.SizeViewerForm_Shown);
            this.resultGroup.ResumeLayout(false);
            this.rowContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button selectFolderButton;
        private System.Windows.Forms.Button cancelScanButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button adminButton;
        private System.Windows.Forms.GroupBox resultGroup;
        private System.Windows.Forms.ListView resultListView;
        private System.Windows.Forms.ColumnHeader nameColumn;
        private System.Windows.Forms.ColumnHeader sizeColumn;
        private System.Windows.Forms.ColumnHeader allocatedColumn;
        private System.Windows.Forms.ColumnHeader filesColumn;
        private System.Windows.Forms.ColumnHeader foldersColumn;
        private System.Windows.Forms.ColumnHeader percentColumn;
        private System.Windows.Forms.ColumnHeader modifiedColumn;
        private System.Windows.Forms.ContextMenuStrip rowContextMenu;
        private System.Windows.Forms.ToolStripMenuItem openInExplorerMenuItem;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.LinkLabel gitHubLinkLabel;
        private System.Windows.Forms.LinkLabel websiteLinkLabel;
        private System.Windows.Forms.Timer statusTimer;
    }
}
