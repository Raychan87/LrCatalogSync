
using LrCatalogSync.Infrastructure;
using LrCatalogSync.Resources.Strings;

namespace LrCatalogSync.UI
{
    // Manager für Tray-Icon Verwaltung und Status-Updates
    public class TrayManager
    {
        // ==================== EIGENSCHAFTEN ====================
        private NotifyIcon trayIcon;                            // Tray-Icon in der Taskleiste
        private Icon iconGreen;                                 // Status: Standby
        private Icon iconRed;                                   // Status: Fehler
        private Icon iconOrange;                                // Status: Syncing
        private Icon iconYellow;                                // Status: Syncing
        private Icon iconBlue;                                  // Status: Lockfile erkannt
        private Icon iconWhite;                                 // Status: Keine Samba-Verbindung
        private Icon iconLightBlue;                             // Status: Crash-Recovery aktiv
        private Icon iconViolet;                                // Status: Remote Lockfile aktiv
        private Icon iconApp;                                   // Sync deaktiviert (Programm-Icon)
        private readonly SynchronizationContext? uiContext = null!;      // Für Thread-sichere UI-Updates
        private readonly List<Stream> iconStreams = new();
        private string? lastStatusKey;
        // ==================== KONSTRUKTOR ====================
        // Initialisiert TrayManager mit Icons und Tray-Icon
        public TrayManager()
        {
            // Speichere UI-Kontext für Thread-sichere Updates
            uiContext = SynchronizationContext.Current;

            // ========== ICONS LADEN ==========
            iconGreen = LoadIcon("tray_green.ico");         // Standby
            iconRed = LoadIcon("tray_red.ico");             // Fehler
            iconOrange = LoadIcon("tray_orange.ico");       // Syncing
            iconYellow = LoadIcon("tray_yellow.ico");       // Syncing
            iconLightBlue = LoadIcon("tray_lightBlue.ico"); // Lockfile erkannt
            iconWhite = LoadIcon("tray_white.ico");         // Keine Samba-Verbindung
            iconViolet = LoadIcon("tray_violet.ico");       // Remote Lockfile aktiv (Katalog-Sync)
            iconBlue = LoadIcon("tray_blue.ico");           // Crash-Recovery aktiv
            iconApp = LoadIcon("app_icon.ico");             // Sync deaktiviert (Programm-Icon)

            // ========== TRAY-ICON EINRICHTEN ==========
            trayIcon = new NotifyIcon()
            {
                Icon = iconGreen,
                Visible = true
            };
            SetTrayTooltip(Strings.Tray_Tip_Start);
        }

        // ==================== ÖFFENTLICHE FUNKTIONEN ====================
        // Gibt das NotifyIcon zurück (für ContextMenuStrip Zuweisung)
        // returns: Das verwaltete Tray-Icon
        public NotifyIcon GetTrayIcon()
        {
            return trayIcon;
        }

        public void RefreshText()
        {
            if (lastStatusKey is { } statusKey)
            {
                UpdateStatus(statusKey);
                return;
            }

            if (uiContext != null && SynchronizationContext.Current != uiContext)
                uiContext.Post(_ => SetTrayTooltip(Strings.Tray_Tip_Start), null);
            else
                SetTrayTooltip(Strings.Tray_Tip_Start);
        }

        // Aktualisiert den Status im Tray-Icon (mit Thread-Safety)
        // state: Neuer Status (Standby, Syncing, rclone, Error)
        public void UpdateStatus(string state)
        {
            // Wenn kein UI-Kontext vorhanden, direkt setzen
            if (uiContext == null)
            {
                SetTrayText(state);
                return;
            }

            // Prüfe ob bereits im UI-Thread
            if (SynchronizationContext.Current == uiContext)
            {
                // Ja: direkt setzen
                SetTrayText(state);
            }
            else
            {
                // Nein: Post in UI-Thread zum Setzen
                uiContext.Post(_ => SetTrayText(state), null);
            }
        }

        // ==================== PRIVATE HILFSFUNKTIONEN ====================
        // Setzt Icon und Text des Tray-Icons basierend auf Status
        // state: Status (Standby, Syncing, rclone, Error, Lockfile, NoSamba)
        private void SetTrayText(string state)
        {
            string text;
            switch (state)
            {
                case "NoCfg":
                    trayIcon.Icon = iconWhite;
                    text = Strings.Tray_Tip_NoCfg;
                    break;
                case "Standby":
                    trayIcon.Icon = iconGreen;
                    text = Strings.Tray_Tip_Standby;
                    break;
                case "BSyncing":
                    trayIcon.Icon = iconOrange;
                    text = Strings.Tray_Tip_BSyncing;
                    break;
                case "LSyncing":
                    trayIcon.Icon = iconYellow;
                    text = Strings.Tray_Tip_LSyncing;
                    break;
                case "RcloneCfg":
                    trayIcon.Icon = iconRed;
                    text = Strings.Tray_Tip_RcloneCfg;
                    break;
                case "RcloneExe":
                    trayIcon.Icon = iconRed;
                    text = Strings.Tray_Tip_RcloneExe;
                    break;
                case "Error":
                    trayIcon.Icon = iconRed;
                    text = Strings.Tray_Tip_Error;
                    break;
                case "Lockfile":
                    trayIcon.Icon = iconLightBlue;
                    text = Strings.Tray_Tip_Lockfile;
                    break;
                case "NoSamba":
                    trayIcon.Icon = iconRed;
                    text = Strings.Tray_Tip_NoSamba;
                    break;
                case "RemoteLockfileDown":
                    trayIcon.Icon = iconViolet;
                    text = Strings.Tray_Tip_RemoteLockfileDown;
                    break;
                case "RemoteLockfileUp":
                    trayIcon.Icon = iconViolet;
                    text = Strings.Tray_Tip_RemoteLockfileUp;
                    break;
                case "RemoteLightroom":
                    trayIcon.Icon = iconViolet;
                    text = Strings.Tray_Tip_RemoteLightroom;
                    break;
                case "LockfileErr":
                    trayIcon.Icon = iconRed;
                    text = Strings.Tray_Tip_LockfileErr;
                    break;
                case "CrashRecovery":
                    trayIcon.Icon = iconBlue;
                    text = Strings.Tray_Tip_CrashRecovery;
                    break;
                case "SyncDisabled":
                    trayIcon.Icon = iconApp;
                    text = Strings.Tray_Tip_SyncDisabled;
                    break;
                default:
                    Log.Error($"TrayManager: {string.Format(Strings.Get("Log_Tray_UnknownStatus"), state)}");
                    return;
            }

            lastStatusKey = state;
            SetTrayTooltip(text);
        }

        private void SetTrayTooltip(string text)
        {
            const int maxTooltipLength = 127;
            if (text.Length > maxTooltipLength)
            {
                Log.Debug($"TrayManager: {string.Format(Strings.Get("Log_Tray_TooltipTruncated"), text.Length, maxTooltipLength)}");
                text = text[..maxTooltipLength];
            }

            trayIcon.Text = text;
        }

        private Icon LoadIcon(string fileName)
        {
            var resourceName = $"LrCatalogSync.Resources.Icons.{fileName}";
            var stream = typeof(TrayManager).Assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Icon-Ressource nicht gefunden: {resourceName}");
            iconStreams.Add(stream);
            return new Icon(stream);
        }

        // Gibt alle verwalteten Ressourcen frei
        public void Dispose()
        {
            // Icons freigeben (GDI+ Ressourcen)
            iconGreen?.Dispose();
            iconRed?.Dispose();
            iconOrange?.Dispose();
            iconYellow?.Dispose();
            iconLightBlue?.Dispose();
            iconWhite?.Dispose();
            iconViolet?.Dispose();
            iconBlue?.Dispose();
            foreach (var stream in iconStreams)
                stream.Dispose();
            iconStreams.Clear();

            // Tray-Icon entfernen und freigeben
            trayIcon?.Dispose();
        }
    }
}
