namespace LrCatalogSync.UI
{
    partial class MessageForm
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
        /// Layout mit festen Location/Size-Werten. Die endgültige Fenstergröße richtet sich
        /// nach der Textlänge und wird im Konstruktor gesetzt.
        /// </summary>
        private void InitializeComponent()
        {
            this.iconBox = new System.Windows.Forms.PictureBox();
            this.messageLabel = new System.Windows.Forms.Label();
            this.primaryButton = new System.Windows.Forms.Button();
            this.secondaryButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
            this.SuspendLayout();
            //
            // iconBox
            //
            this.iconBox.Location = new System.Drawing.Point(20, 20);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new System.Drawing.Size(32, 32);
            this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconBox.TabIndex = 0;
            this.iconBox.TabStop = false;
            //
            // messageLabel
            //
            this.messageLabel.AutoSize = true;
            this.messageLabel.Location = new System.Drawing.Point(68, 22);
            this.messageLabel.MaximumSize = new System.Drawing.Size(420, 0);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(52, 15);
            this.messageLabel.TabIndex = 1;
            this.messageLabel.Text = "Meldung";
            this.messageLabel.UseMnemonic = false;
            //
            // primaryButton
            //
            this.primaryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryButton.Location = new System.Drawing.Point(126, 76);
            this.primaryButton.Name = "primaryButton";
            this.primaryButton.Size = new System.Drawing.Size(90, 28);
            this.primaryButton.TabIndex = 2;
            this.primaryButton.Text = "OK";
            this.primaryButton.UseVisualStyleBackColor = true;
            //
            // secondaryButton
            //
            this.secondaryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.secondaryButton.Location = new System.Drawing.Point(226, 76);
            this.secondaryButton.Name = "secondaryButton";
            this.secondaryButton.Size = new System.Drawing.Size(90, 28);
            this.secondaryButton.TabIndex = 3;
            this.secondaryButton.Text = "Abbrechen";
            this.secondaryButton.UseVisualStyleBackColor = true;
            //
            // MessageForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(332, 120);
            this.Controls.Add(this.iconBox);
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.primaryButton);
            this.Controls.Add(this.secondaryButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LrCatalogSync";
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox iconBox;
        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.Button primaryButton;
        private System.Windows.Forms.Button secondaryButton;
    }
}
