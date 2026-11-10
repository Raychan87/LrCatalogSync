using System.Media;

using LrCatalogSync.Resources.Strings;

namespace LrCatalogSync.UI
{
    // Meldungsfenster im Farbmodus der Anwendung. Ersetzt die Windows-MessageBox,
    // die sich nicht einfärben lässt. Aufruf wie MessageBox.Show(...).
    public partial class MessageForm : Form
    {
        // Abstände in Pixeln bei 96 dpi
        private const int Margin96 = 20;
        private const int ButtonGap96 = 10;
        private const int MinimumTextWidth96 = 220;

        private readonly Bitmap? iconImage;

        // Parameterloser Konstruktor für den Designer
        public MessageForm()
        {
            InitializeComponent();
        }

        private MessageForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            InitializeComponent();

            Text = caption;
            messageLabel.Text = text;

            iconImage = LoadIconImage(icon);
            iconBox.Image = iconImage;

            // ========== SCHALTFLÄCHEN ==========
            switch (buttons)
            {
                case MessageBoxButtons.YesNo:
                    SetButton(primaryButton, "Dialog_Button_Yes", DialogResult.Yes);
                    SetButton(secondaryButton, "Dialog_Button_No", DialogResult.No);
                    break;
                case MessageBoxButtons.OKCancel:
                    SetButton(primaryButton, "Dialog_Button_Ok", DialogResult.OK);
                    SetButton(secondaryButton, "Dialog_Button_Cancel", DialogResult.Cancel);
                    break;
                default:
                    SetButton(primaryButton, "Dialog_Button_Ok", DialogResult.OK);
                    secondaryButton.Visible = false;
                    break;
            }

            // Nicht secondaryButton.Visible abfragen: das ist vor dem Anzeigen des Fensters immer false
            bool hasSecondButton = buttons is MessageBoxButtons.YesNo or MessageBoxButtons.OKCancel;
            Button acceptButton = hasSecondButton && defaultButton == MessageBoxDefaultButton.Button2 ? secondaryButton : primaryButton;
            AcceptButton = acceptButton;

            // Escape und das Schließen-Kreuz entsprechen der ablehnenden Schaltfläche
            CancelButton = hasSecondButton ? secondaryButton : primaryButton;

            // ========== GRÖSSE NACH TEXTLÄNGE ==========
            int margin = ScaleToDpi(Margin96);
            int textRight = messageLabel.Left + Math.Max(messageLabel.Width, ScaleToDpi(MinimumTextWidth96));
            int contentBottom = Math.Max(messageLabel.Bottom, iconBox.Bottom);
            ClientSize = new Size(textRight + margin, contentBottom + margin + primaryButton.Height + margin);

            // Schaltflächen rechts unten ausrichten
            int buttonTop = ClientSize.Height - margin - primaryButton.Height;
            int right = ClientSize.Width - margin;
            if (hasSecondButton)
            {
                secondaryButton.Location = new Point(right - secondaryButton.Width, buttonTop);
                right = secondaryButton.Left - ScaleToDpi(ButtonGap96);
            }

            primaryButton.Location = new Point(right - primaryButton.Width, buttonTop);

            Theme.ApplyTo(this);
            Theme.StyleAccentButton(acceptButton);
            ActiveControl = acceptButton;

            Shown += (sender, e) => GetSound(icon)?.Play();
            FormClosed += (sender, e) => iconImage?.Dispose();
        }

        // ==================== AUFRUFE (wie MessageBox.Show) ====================

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        public static DialogResult Show(IWin32Window? owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return Show(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        public static DialogResult Show(
            IWin32Window? owner,
            string text,
            string caption,
            MessageBoxButtons buttons,
            MessageBoxIcon icon,
            MessageBoxDefaultButton defaultButton)
        {
            using var form = new MessageForm(text, caption, buttons, icon, defaultButton);

            // Ohne übergeordnetes Fenster in der Bildschirmmitte anzeigen
            owner ??= Form.ActiveForm;
            if (owner == null)
                form.StartPosition = FormStartPosition.CenterScreen;

            return owner != null ? form.ShowDialog(owner) : form.ShowDialog();
        }

        // ==================== HILFSMETHODEN ====================

        private static void SetButton(Button button, string textKey, DialogResult result)
        {
            button.Text = Strings.Get(textKey);
            button.DialogResult = result;
        }

        private static Bitmap? LoadIconImage(MessageBoxIcon icon)
        {
            StockIconId? iconId = icon switch
            {
                MessageBoxIcon.Error => StockIconId.Error,
                MessageBoxIcon.Warning => StockIconId.Warning,
                MessageBoxIcon.Information => StockIconId.Info,
                MessageBoxIcon.Question => StockIconId.Help,
                _ => null
            };

            if (iconId == null)
                return null;

            using Icon stockIcon = SystemIcons.GetStockIcon(iconId.Value);
            return stockIcon.ToBitmap();
        }

        private static SystemSound? GetSound(MessageBoxIcon icon)
        {
            return icon switch
            {
                MessageBoxIcon.Error => SystemSounds.Hand,
                MessageBoxIcon.Warning => SystemSounds.Exclamation,
                MessageBoxIcon.Information => SystemSounds.Asterisk,
                MessageBoxIcon.Question => SystemSounds.Question,
                _ => null
            };
        }

        private int ScaleToDpi(int value) => value * DeviceDpi / 96;
    }
}
