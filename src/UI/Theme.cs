using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

using LrCatalogSync.Infrastructure;
using LrCatalogSync.Resources.Strings;
using Microsoft.Win32;

namespace LrCatalogSync.UI
{
    public sealed record ThemeOption(string Code, string DisplayName);

    // Alle Farben eines Farbmodus. Fenster lesen ihre Farben ausschließlich hier.
    public sealed record ThemePalette(
        bool IsDark,
        Color Window,               // Fensterhintergrund
        Color Surface,              // Listen und Eingabefelder
        Color Raised,               // Schaltflächen, Spaltenköpfe
        Color Border,
        Color Text,
        Color Muted,                // Nebentext, gesperrte Einträge
        Color Accent,               // Hervorgehobene Schaltfläche
        Color OnAccent,
        Color Selection,            // Markierte Zeile
        Color OnSelection,
        Color InactiveSelection,    // Markierte Zeile ohne Fokus
        Color Bar,                  // Größen- und Prozentbalken
        Color Link,
        Color Error,
        Color Success,
        Color Glow = default,       // Leuchtfarbe für Schaltflächen unter der Maus (leer = kein Leuchten)
        Color HoverBorder = default,    // Rahmenfarbe einer Schaltfläche unter der Maus
        Color ColumnLine = default);    // Senkrechte Spaltenlinien in Listen (nur dunkler Modus)

    // Farbmodus der Anwendung: Hell, Dunkel (Lila), Dunkel (Windows-Akzentfarbe) oder automatisch nach Windows.
    public static class Theme
    {
        public const string SystemCode = "System";
        public const string LightCode = "Light";
        public const string DarkPurpleCode = "DarkPurple";
        public const string DarkAccentCode = "DarkAccent";

        // ========== FESTE FARBEN ==========
        // Dunkles Grundgerüst (GitHub-Grau)
        private static readonly Color DarkWindow = Color.FromArgb(13, 17, 23);
        private static readonly Color DarkSurface = Color.FromArgb(22, 27, 34);
        private static readonly Color DarkRaised = Color.FromArgb(33, 38, 45);
        private static readonly Color DarkHover = Color.FromArgb(48, 54, 61);
        private static readonly Color DarkBorder = Color.FromArgb(48, 54, 61);
        private static readonly Color DarkColumnLine = Color.FromArgb(57, 61, 67);    // Entspricht den von Windows gezeichneten Linien
        private static readonly Color DarkText = Color.FromArgb(230, 237, 243);
        private static readonly Color DarkMuted = Color.FromArgb(139, 148, 158);
        private static readonly Color DarkError = Color.FromArgb(248, 81, 73);
        private static readonly Color DarkSuccess = Color.FromArgb(63, 185, 80);

        // Lila Akzent
        private static readonly Color Purple = Color.FromArgb(90, 26, 130);
        private static readonly Color PurpleSelection = Color.FromArgb(75, 19, 110);
        private static readonly Color PurpleLink = Color.FromArgb(184, 138, 224);

        // Leuchten der Schaltflächen unter der Maus im Modus "Dunkel Lila"
        private static readonly Color GoldGlow = Color.FromArgb(240, 160, 60);
        private const double GlowBackAmount = 0.14;                         // Goldanteil der Fläche unter der Maus
        private static readonly int[] GlowRingAlpha = { 150, 90, 46, 18 };  // Deckkraft der Ringe von innen nach außen

        // Form der von Windows gezeichneten Schaltflächen in Pixeln bei 96 dpi: der Rahmen unter der Maus
        // wird genau über deren abgerundeten Rahmen gelegt.
        private const int DarkButtonBorderInset = 1;
        private const int DarkButtonCornerRadius = 3;
        private const int LightButtonBorderInset = 1;
        private const int DarkButtonBorderOverhang = 1;     // Im dunklen Modus reicht der Rahmen rechts und unten bis an de1n Rand
        private const int LightButtonCornerRadius = 4;
        private const double DarkHoverLighten = 0.3;    // Aufhellung der Akzentfarbe für den Rahmen auf dunklem Grund

        // Gold für Balken (halbtransparent, damit der Text darüber lesbar bleibt)
        private static readonly Color GoldBarDark = Color.FromArgb(128, 210, 153, 34);
        private static readonly Color GoldBarLight = Color.FromArgb(128, 227, 164, 26);

        // Heller Modus
        private static readonly Color LightLink = Color.FromArgb(0, 120, 215);
        private static readonly Color LightText = Color.FromArgb(35, 35, 35);
        private static readonly Color LightProgressBack = Color.FromArgb(230, 230, 230);

        // Ersatz, wenn Windows keine Akzentfarbe liefert (Windows-Standardblau)
        private static readonly Color FallbackAccent = Color.FromArgb(0, 120, 212);

        private const string DwmKeyPath = @"Software\Microsoft\Windows\DWM";
        private const string DwmAccentValue = "AccentColor";
        private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightThemeValue = "AppsUseLightTheme";

        public static ThemePalette Current { get; private set; } = CreateLightPalette();

        public static IReadOnlyList<ThemeOption> Options => new[]
        {
            new ThemeOption(SystemCode, Strings.Get("Theme_System")),
            new ThemeOption(LightCode, Strings.Get("Theme_Light")),
            new ThemeOption(DarkPurpleCode, Strings.Get("Theme_DarkPurple")),
            new ThemeOption(DarkAccentCode, Strings.Get("Theme_DarkAccent"))
        };

        public static bool IsValidCode(string code) =>
            code is SystemCode or LightCode or DarkPurpleCode or DarkAccentCode;

        // Aktiviert einen Farbmodus. Gilt für alle Fenster, die danach geöffnet werden.
        public static void Apply(string code)
        {
            // "Automatisch": Windows entscheidet zwischen Hell und Dunkel (dunkel mit Windows-Akzentfarbe)
            if (code == SystemCode || !IsValidCode(code))
                code = IsWindowsDarkMode() ? DarkAccentCode : LightCode;

            bool dark = code != LightCode;
            try
            {
                Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            }
            catch (Exception ex)
            {
                Log.Error($"Theme: {string.Format(Strings.Get("Log_Theme_ColorModeFailed"), ex.Message)}");
            }

            // Erst nach dem Umschalten aufbauen: die hellen Farben stammen aus den Windows-Systemfarben
            Current = code switch
            {
                DarkPurpleCode => CreateDarkPalette(Purple, PurpleSelection, PurpleLink, GoldGlow),
                DarkAccentCode => CreateDarkAccentPalette(),
                _ => CreateLightPalette()
            };
        }

        // Färbt ein Fenster samt aller Controls im aktuellen Farbmodus ein.
        // Im hellen Modus bleibt das Windows-Standardaussehen unverändert.
        public static void ApplyTo(Form form)
        {
            if (Current.IsDark)
            {
                form.BackColor = Current.Window;
                form.ForeColor = Current.Text;
                ApplyToChildren(form);

                // Windows markiert den Text des ersten Eingabefelds beim Öffnen. Die Markierungsfarbe
                // von Eingabefeldern (Hellblau) lässt sich nicht ändern, deshalb wird die Markierung aufgehoben.
                form.Shown += Form_ShownClearSelection;
            }

            // Rahmen unter der Maus und Fortschrittsbalken in der Akzentfarbe gelten in jedem Farbmodus
            AddCommonStyles(form);
        }

        private static void Form_ShownClearSelection(object? sender, EventArgs e)
        {
            if (sender is Form { ActiveControl: TextBoxBase textBox })
                textBox.Select(textBox.TextLength, 0);
        }

        private static void AddCommonStyles(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Button button)
                {
                    AddHoverBorder(button);
                }
                else if (control is ProgressBar progressBar)
                {
                    progressBar.HandleCreated += (sender, e) => new ProgressBarPainter(progressBar);
                    if (progressBar.IsHandleCreated)
                        new ProgressBarPainter(progressBar);
                }
                else
                {
                    AddCommonStyles(control);
                }
            }
        }

        // Färbt ein Menü (Tray-Menü, Kontextmenü) im aktuellen Farbmodus ein, einschließlich der Markierung
        public static void ApplyTo(ToolStrip menu)
        {
            menu.BackColor = Current.Surface;
            menu.ForeColor = Current.Text;
            menu.Renderer = new ThemeMenuRenderer();
        }

        // Hebt die Hauptschaltfläche eines Fensters in der Akzentfarbe hervor (nur im dunklen Modus)
        public static void StyleAccentButton(Button button)
        {
            if (!Current.IsDark)
                return;

            button.BackColor = Current.Accent;
            button.ForeColor = Current.OnAccent;
            button.FlatAppearance.BorderColor = Current.Accent;
            button.FlatAppearance.MouseOverBackColor = Current.Glow.IsEmpty
                ? Blend(Current.Accent, Color.White, 0.15)
                : Blend(Current.Accent, Current.Glow, GlowBackAmount);
            button.FlatAppearance.MouseDownBackColor = Blend(Current.Accent, Color.Black, 0.15);
        }

        private static void ApplyToChildren(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                switch (control)
                {
                    case Button button:
                        button.FlatStyle = FlatStyle.Flat;
                        button.UseVisualStyleBackColor = false;
                        button.BackColor = Current.Raised;
                        button.ForeColor = Current.Text;
                        button.FlatAppearance.BorderSize = 1;
                        button.FlatAppearance.BorderColor = Current.Border;
                        button.FlatAppearance.MouseOverBackColor = DarkHover;
                        button.FlatAppearance.MouseDownBackColor = Current.Selection;
                        if (!Current.Glow.IsEmpty)
                            button.FlatAppearance.MouseOverBackColor = Blend(Current.Raised, Current.Glow, GlowBackAmount);
                        break;

                    case LinkLabel link:
                        link.LinkColor = Current.Link;
                        link.VisitedLinkColor = Current.Link;
                        link.ActiveLinkColor = Current.Text;
                        break;

                    case Label label:
                        // Blau eingefärbte Labels sind klickbare Links (z. B. rclone-Download)
                        label.ForeColor = label.ForeColor.ToArgb() == LightLink.ToArgb() ? Current.Link : Current.Text;
                        if (label.BorderStyle != BorderStyle.None)
                        {
                            label.BorderStyle = BorderStyle.None;
                            AddBorder(label);
                        }
                        label.Paint += DisabledLabel_Paint;
                        break;

                    case GroupBox group:
                        group.BackColor = Current.Window;
                        group.ForeColor = Current.Text;
                        group.Paint += GroupBox_Paint;
                        break;

                    case RichTextBox richText:
                        // Rahmenlose Textfelder dienen als mehrfarbige Labels und liegen auf dem Fensterhintergrund
                        richText.BackColor = richText.BorderStyle == BorderStyle.None ? Current.Window : Current.Surface;
                        richText.ForeColor = Current.Text;
                        if (richText.BorderStyle != BorderStyle.None)
                        {
                            richText.BorderStyle = BorderStyle.None;
                            AddBorder(richText);
                        }
                        break;

                    case TextBox textBox:
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        textBox.BackColor = Current.Surface;
                        textBox.ForeColor = Current.Text;
                        break;

                    case ComboBox comboBox:
                        comboBox.FlatStyle = FlatStyle.Flat;
                        comboBox.BackColor = Current.Surface;
                        comboBox.ForeColor = Current.Text;
                        break;

                    case ListBox listBox:
                        listBox.BackColor = Current.Surface;
                        listBox.ForeColor = Current.Text;
                        listBox.BorderStyle = BorderStyle.None;
                        AddBorder(listBox);
                        break;

                    case ListView listView:
                        listView.BackColor = Current.Surface;
                        listView.ForeColor = Current.Text;
                        listView.BorderStyle = BorderStyle.None;
                        AddBorder(listView);

                        // Dunkle Bildlaufleisten; die senkrechten Spaltenlinien zeichnet Windows im dunklen Modus selbst
                        listView.HandleCreated += ListView_HandleCreated;
                        if (listView.IsHandleCreated)
                            ApplyDarkListTheme(listView);
                        break;

                    case CheckBox or RadioButton:
                        control.ForeColor = Current.Text;
                        control.Paint += Check_Paint;
                        break;

                    case ProgressBar:
                        AddBorder(control);
                        break;

                    default:
                        control.BackColor = Current.Window;
                        control.ForeColor = Current.Text;
                        break;
                }

                ApplyToChildren(control);
            }
        }

        // Schaltfläche, über der gerade die Maus steht
        private static Button? hoveredButton;

        // Hebt eine Schaltfläche unter der Maus mit einem Rahmen in der Hover-Farbe hervor. Der Rahmen folgt
        // den abgerundeten Ecken. Hat der Farbmodus eine Leuchtfarbe, kommt ein weicher Schein darum dazu;
        // dessen äußerer Teil liegt außerhalb der Schaltfläche und wird vom übergeordneten Control gezeichnet.
        private static void AddHoverBorder(Button button)
        {
            Control? parent = button.Parent;
            if (parent == null)
                return;

            void SetHover(bool active)
            {
                if (active == (hoveredButton == button))
                    return;

                hoveredButton = active ? button : null;
                button.Invalidate();
                parent.Invalidate(Rectangle.Inflate(button.Bounds, GlowRingAlpha.Length + 1, GlowRingAlpha.Length + 1));
            }

            button.MouseEnter += (sender, e) => SetHover(button.Enabled);
            button.MouseLeave += (sender, e) => SetHover(false);
            button.EnabledChanged += (sender, e) => SetHover(false);
            button.VisibleChanged += (sender, e) => SetHover(false);
            button.Disposed += (sender, e) =>
            {
                if (hoveredButton == button)
                    hoveredButton = null;
            };

            // Windows zeichnet die Schaltfläche selbst: der Rahmen wird direkt danach darübergelegt
            button.HandleCreated += (sender, e) => new ButtonHoverPainter(button);
            if (button.IsHandleCreated)
                new ButtonHoverPainter(button);

            parent.Paint += (sender, e) =>
            {
                if (hoveredButton == button && button.Visible)
                    DrawHoverBorder(e.Graphics, button, button.Location, drawBorder: false);
            };
        }

        // Zeichnet Rahmen und Schein einer Schaltfläche. "origin" ist die linke obere Ecke der Schaltfläche
        // in den Koordinaten der Zeichenfläche (Schaltfläche selbst oder übergeordnetes Control).
        private static void DrawHoverBorder(Graphics graphics, Button button, Point origin, bool drawBorder)
        {
            int inset = (Current.IsDark ? DarkButtonBorderInset : LightButtonBorderInset) * button.DeviceDpi / 96;
            int radius = (Current.IsDark ? DarkButtonCornerRadius : LightButtonCornerRadius) * button.DeviceDpi / 96;
            int overhang = Current.IsDark ? DarkButtonBorderOverhang : 0;
            var border = new Rectangle(
                origin.X + inset,
                origin.Y + inset,
                button.Width - 2 * inset - 1 + overhang,
                button.Height - 2 * inset - 1 + overhang);

            SmoothingMode previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (!Current.Glow.IsEmpty)
            {
                for (int ring = 0; ring < GlowRingAlpha.Length; ring++)
                {
                    using var ringPen = new Pen(Color.FromArgb(GlowRingAlpha[ring], Current.Glow));
                    using GraphicsPath ringPath = CreateRoundedPath(Rectangle.Inflate(border, ring + 1, ring + 1), radius + ring + 1);
                    graphics.DrawPath(ringPen, ringPath);
                }
            }

            if (drawBorder)
            {
                using var borderPen = new Pen(Current.HoverBorder);
                using GraphicsPath borderPath = CreateRoundedPath(border, radius);
                graphics.DrawPath(borderPen, borderPath);
            }

            graphics.SmoothingMode = previousMode;
        }

        // Zeichnet den Fortschrittsbalken in der Akzentfarbe des Farbmodus (Windows-Akzentfarbe bzw. Lila)
        // statt im festen Grün/Blau von Windows. Laufbalken ohne festen Wert bleiben unverändert.
        private sealed class ProgressBarPainter : NativeWindow
        {
            private readonly ProgressBar progressBar;

            public ProgressBarPainter(ProgressBar progressBar)
            {
                this.progressBar = progressBar;
                AssignHandle(progressBar.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg != WM_PAINT || progressBar.Style == ProgressBarStyle.Marquee)
                    return;

                Rectangle bounds = progressBar.ClientRectangle;
                int range = progressBar.Maximum - progressBar.Minimum;
                double fraction = range > 0 ? (double)(progressBar.Value - progressBar.Minimum) / range : 0;
                int fillWidth = (int)Math.Round(bounds.Width * Math.Clamp(fraction, 0, 1));

                using Graphics graphics = Graphics.FromHwnd(progressBar.Handle);
                using (var backBrush = new SolidBrush(Current.IsDark ? Current.Surface : LightProgressBack))
                    graphics.FillRectangle(backBrush, bounds);

                if (fillWidth > 0)
                {
                    using var fillBrush = new SolidBrush(Current.Accent);
                    graphics.FillRectangle(fillBrush, bounds.X, bounds.Y, fillWidth, bounds.Height);
                }

                // Im dunklen Modus zeichnet das übergeordnete Control den Rahmen (AddBorder)
                if (!Current.IsDark)
                {
                    using var borderPen = new Pen(Current.Border);
                    graphics.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }
        }

        private sealed class ButtonHoverPainter : NativeWindow
        {
            private readonly Button button;

            public ButtonHoverPainter(Button button)
            {
                this.button = button;
                AssignHandle(button.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg != WM_PAINT || hoveredButton != button)
                    return;

                using Graphics graphics = Graphics.FromHwnd(button.Handle);
                DrawHoverBorder(graphics, button, Point.Empty, drawBorder: true);
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            int diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height)));
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Windows zeichnet gesperrten Text im dunklen Modus fast schwarz. Die beiden folgenden Methoden
        // zeichnen den Text gesperrter Labels und Kontrollkästchen in der gedämpften Textfarbe neu.
        private static void DisabledLabel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Label label || label.Enabled)
                return;

            using (var backBrush = new SolidBrush(label.BackColor))
                e.Graphics.FillRectangle(backBrush, label.ClientRectangle);

            TextFormatFlags flags = GetTextFlags(label.TextAlign);
            if (!label.UseMnemonic)
                flags |= TextFormatFlags.NoPrefix;

            TextRenderer.DrawText(e.Graphics, label.Text, label.Font, label.ClientRectangle, Current.Muted, flags);
        }

        // Kontrollkästchen im dunklen Modus: Windows zeichnet das Häkchen-Feld in festem Hellblau.
        // Es wird hier in der Akzentfarbe des Farbmodus neu gezeichnet, bei gesperrten Kästchen auch der Text.
        private static void Check_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not ButtonBase check)
                return;

            int glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal).Width;

            if (check is CheckBox checkBox)
                DrawCheckGlyph(e.Graphics, checkBox, glyphSize);

            if (check.Enabled)
                return;

            int textLeft = glyphSize + CheckTextGap * check.DeviceDpi / 96;
            var textBounds = new Rectangle(textLeft, 0, Math.Max(0, check.Width - textLeft), check.Height);

            using (var backBrush = new SolidBrush(check.BackColor))
                e.Graphics.FillRectangle(backBrush, textBounds);

            TextRenderer.DrawText(
                e.Graphics,
                check.Text,
                check.Font,
                textBounds,
                Current.Muted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private static void DrawCheckGlyph(Graphics graphics, CheckBox checkBox, int glyphSize)
        {
            // Das von Windows gezeichnete Feld vollständig überdecken
            using (var backBrush = new SolidBrush(checkBox.BackColor))
                graphics.FillRectangle(backBrush, 0, 0, glyphSize + 1, checkBox.Height);

            var box = new Rectangle(0, (checkBox.Height - glyphSize) / 2, glyphSize - 1, glyphSize - 1);
            bool isChecked = checkBox.CheckState != CheckState.Unchecked;

            Color fill = isChecked ? Current.Accent : Current.Surface;
            Color border = isChecked ? Blend(Current.Accent, Color.White, CheckBorderLighten) : Current.Muted;
            Color mark = Current.OnAccent;
            if (!checkBox.Enabled)
            {
                fill = Blend(fill, Current.Window, DisabledCheckFade);
                border = Blend(border, Current.Window, DisabledCheckFade);
                mark = Blend(mark, Current.Window, DisabledCheckFade);
            }

            SmoothingMode previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath boxPath = CreateRoundedPath(box, CheckCornerRadius * checkBox.DeviceDpi / 96))
            {
                using var fillBrush = new SolidBrush(fill);
                using var borderPen = new Pen(border);
                graphics.FillPath(fillBrush, boxPath);
                graphics.DrawPath(borderPen, boxPath);
            }

            if (isChecked)
            {
                // Häkchen relativ zur Feldgröße
                float unit = box.Width / 12f;
                using var markPen = new Pen(mark, 1.6f * unit) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
                graphics.DrawLines(markPen, new[]
                {
                    new PointF(box.X + 3f * unit, box.Y + 6.2f * unit),
                    new PointF(box.X + 5.2f * unit, box.Y + 8.5f * unit),
                    new PointF(box.X + 9.2f * unit, box.Y + 3.8f * unit)
                });
            }

            graphics.SmoothingMode = previousMode;
        }

        private static TextFormatFlags GetTextFlags(ContentAlignment alignment)
        {
            TextFormatFlags horizontal = alignment switch
            {
                ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => TextFormatFlags.HorizontalCenter,
                ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
                _ => TextFormatFlags.Left
            };

            TextFormatFlags vertical = alignment switch
            {
                ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter,
                ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight => TextFormatFlags.Bottom,
                _ => TextFormatFlags.Top
            };

            return horizontal | vertical;
        }

        // Ersetzt den hellen Windows-Rahmen eines Controls durch eine Linie in der Rahmenfarbe.
        // Der Rahmen wird vom übergeordneten Control direkt um das Control gezeichnet.
        private static void AddBorder(Control control)
        {
            Control? parent = control.Parent;
            if (parent == null)
                return;

            // Platz für die Rahmenlinie schaffen, ohne die äußere Größe zu ändern
            AnchorStyles anchor = control.Anchor;
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            control.SetBounds(control.Left + 1, control.Top + 1, control.Width - 2, control.Height - 2);
            control.Anchor = anchor;

            parent.Paint += (sender, e) =>
            {
                if (!control.Visible)
                    return;

                using var borderPen = new Pen(Current.Border);
                e.Graphics.DrawRectangle(borderPen, control.Left - 1, control.Top - 1, control.Width + 1, control.Height + 1);
            };
            control.Resize += (sender, e) => parent.Invalidate();
            control.Move += (sender, e) => parent.Invalidate();
        }

        private static void ListView_HandleCreated(object? sender, EventArgs e)
        {
            if (sender is ListView listView)
                ApplyDarkListTheme(listView);
        }

        private static void ApplyDarkListTheme(ListView listView)
        {
            SetWindowTheme(listView.Handle, DarkListTheme, null);
        }

        // Zeichnet Rahmen und Titel eines GroupBox-Rahmens in den Farben des Farbmodus
        private static void GroupBox_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not GroupBox group)
                return;

            e.Graphics.Clear(group.BackColor);

            Size textSize = TextRenderer.MeasureText(e.Graphics, group.Text, group.Font);
            int top = textSize.Height / 2;
            using (var borderPen = new Pen(Current.Border))
                e.Graphics.DrawRectangle(borderPen, 0, top, group.Width - 1, group.Height - top - 1);

            if (string.IsNullOrEmpty(group.Text))
                return;

            int textLeft = group.Font.Height / 2;
            var textBounds = new Rectangle(textLeft, 0, textSize.Width, textSize.Height);
            using (var backBrush = new SolidBrush(group.BackColor))
                e.Graphics.FillRectangle(backBrush, textBounds);

            TextRenderer.DrawText(e.Graphics, group.Text, group.Font, textBounds, Current.Text, TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }

        // Zeichnet Menüs in den Farben des Farbmodus: Markierung in der Auswahlfarbe
        private sealed class ThemeMenuRenderer : ToolStripProfessionalRenderer
        {
            public ThemeMenuRenderer() : base(new ThemeMenuColors())
            {
                RoundedEdges = false;
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                bool highlighted = e.Item.Selected && e.Item.Enabled;
                using var backBrush = new SolidBrush(highlighted ? Current.Selection : Current.Surface);
                e.Graphics.FillRectangle(backBrush, new Rectangle(Point.Empty, e.Item.Size));
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = GetItemColor(e.Item);
                base.OnRenderItemText(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                if (e.Item != null)
                    e.ArrowColor = GetItemColor(e.Item);

                base.OnRenderArrow(e);
            }

            private static Color GetItemColor(ToolStripItem item)
            {
                if (!item.Enabled)
                    return Current.Muted;

                return item.Selected ? Current.OnSelection : Current.Text;
            }
        }

        private sealed class ThemeMenuColors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground => Current.Surface;
            public override Color ImageMarginGradientBegin => Current.Surface;
            public override Color ImageMarginGradientMiddle => Current.Surface;
            public override Color ImageMarginGradientEnd => Current.Surface;
            public override Color MenuBorder => Current.Border;
            public override Color MenuItemBorder => Current.Selection;
            public override Color SeparatorDark => Current.Border;
            public override Color SeparatorLight => Current.Surface;
            public override Color CheckBackground => Current.Raised;
            public override Color CheckSelectedBackground => Current.Raised;
            public override Color CheckPressedBackground => Current.Raised;
        }

        // Abstand zwischen Kontrollkästchen und Beschriftung in Pixeln bei 96 dpi
        private const int CheckTextGap = 3;
        private const int CheckCornerRadius = 2;
        private const double CheckBorderLighten = 0.25;     // Aufhellung des Rahmens eines angehakten Kästchens
        private const double DisabledCheckFade = 0.5;       // Abdunkelung gesperrter Kästchen zum Fensterhintergrund

        // Windows-Darstellung für Listen im dunklen Modus
        private const string DarkListTheme = "DarkMode_CFD";

        private const int WM_PAINT = 0x000F;
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string? subAppName, string? subIdList);

        private static ThemePalette CreateLightPalette()
        {
            Color accent = GetWindowsAccentColor();
            return new ThemePalette(
                IsDark: false,
                Window: SystemColors.Control,
                Surface: SystemColors.Window,
                Raised: SystemColors.Control,
                Border: SystemColors.ControlDark,
                Text: LightText,
                Muted: SystemColors.GrayText,
                Accent: accent,
                OnAccent: GetReadableTextColor(accent),
                Selection: accent,
                OnSelection: GetReadableTextColor(accent),
                InactiveSelection: SystemColors.Control,
                Bar: GoldBarLight,
                Link: LightLink,
                Error: Color.Firebrick,
                Success: Color.ForestGreen,
                HoverBorder: accent);
        }

        private static ThemePalette CreateDarkAccentPalette()
        {
            Color accent = GetWindowsAccentColor();

            // Helle Variante der Akzentfarbe, damit Links auf dem dunklen Hintergrund lesbar sind
            return CreateDarkPalette(accent, accent, Blend(accent, Color.White, 0.45));
        }

        private static ThemePalette CreateDarkPalette(Color accent, Color selection, Color link, Color glow = default)
        {
            return new ThemePalette(
                IsDark: true,
                Window: DarkWindow,
                Surface: DarkSurface,
                Raised: DarkRaised,
                Border: DarkBorder,
                Text: DarkText,
                Muted: DarkMuted,
                Accent: accent,
                OnAccent: GetReadableTextColor(accent),
                Selection: selection,
                OnSelection: GetReadableTextColor(selection),
                InactiveSelection: DarkRaised,
                Bar: GoldBarDark,
                Link: link,
                Error: DarkError,
                Success: DarkSuccess,
                Glow: glow,
                HoverBorder: glow.IsEmpty ? Blend(accent, Color.White, DarkHoverLighten) : glow,
                ColumnLine: DarkColumnLine);
        }

        // Akzentfarbe aus den Windows-Einstellungen (Personalisierung → Farben)
        private static Color GetWindowsAccentColor()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(DwmKeyPath);
                if (key?.GetValue(DwmAccentValue) is int value)
                {
                    // Windows speichert die Farbe als ABGR
                    return Color.FromArgb(value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF);
                }
            }
            catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
            {
                // Registry nicht lesbar: Standardfarbe verwenden
            }

            return FallbackAccent;
        }

        private static bool IsWindowsDarkMode()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);
                return key?.GetValue(AppsUseLightThemeValue) is int value && value == 0;
            }
            catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
            {
                return false;
            }
        }

        // Schwarz oder Weiß, je nachdem was auf der Farbe besser lesbar ist
        private static Color GetReadableTextColor(Color background)
        {
            double luminance = (0.299 * background.R + 0.587 * background.G + 0.114 * background.B) / 255;
            return luminance > 0.6 ? Color.Black : Color.White;
        }

        private static Color Blend(Color color, Color target, double amount)
        {
            return Color.FromArgb(
                (int)Math.Round(color.R + (target.R - color.R) * amount),
                (int)Math.Round(color.G + (target.G - color.G) * amount),
                (int)Math.Round(color.B + (target.B - color.B) * amount));
        }
    }
}
