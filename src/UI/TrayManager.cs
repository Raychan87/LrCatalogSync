
namespace LrCatalogSync.UI
{
    // Manager für Tray-Icon Verwaltung und Status-Updates
    public class TrayManager
    {
        // ==================== EIGENSCHAFTEN ====================
        private NotifyIcon trayIcon;                           // Tray-Icon in der Taskleiste
        private Icon iconGreen;                                 // Status: Standby
        private Icon iconRed;                                   // Status: Fehler
        private Icon iconOrange;                               // Status: Syncing
        private Icon iconYellow;                                // Status: Syncing
        private Icon iconBlue;                                  // Status: Lockfile erkannt
        private Icon iconWhite;                                 // Status: Keine Samba-Verbindung
        private Icon iconMagenta;                               // Status: Remote Lockfile aktiv
        private Icon iconLightBlue;                                  // Status: Crash-Recovery aktiv
        private Icon iconApp;                                     // Sync deaktiviert (Programm-Icon)
        private readonly SynchronizationContext? uiContext = null!;      // Für Thread-sichere UI-Updates
        private readonly List<Stream> iconStreams = new();
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
            iconMagenta = LoadIcon("tray_violet.ico");      // Remote Lockfile aktiv
            iconBlue = LoadIcon("tray_blue.ico");           // Crash-Recovery aktiv
            iconApp = LoadIcon("app_icon.ico");             // Sync deaktiviert (Programm-Icon)

            // ========== TRAY-ICON EINRICHTEN ==========
            trayIcon = new NotifyIcon()
            {
                Icon = iconGreen,
                Text = "LR Catalog Sync - Standby",
                Visible = true
            };
        }

        // ==================== ÖFFENTLICHE FUNKTIONEN ====================
        // Gibt das NotifyIcon zurück (für ContextMenuStrip Zuweisung)
        // returns: Das verwaltete Tray-Icon
        public NotifyIcon GetTrayIcon()
        {
            return trayIcon;
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
            switch (state)
            {
                case "NoCfg":
                    trayIcon.Icon = iconWhite;
                    trayIcon.Text = "LrCatSync: Konfigurationsdateien fehlen!";
                    break;
                case "Standby":
                    trayIcon.Icon = iconGreen;
                    trayIcon.Text = "LrCatSync: wartet auf Änderungen...";
                    break;
                case "BSyncing":
                    trayIcon.Icon = iconOrange;
                    trayIcon.Text = "LrCatSync: synchronisiere Lightroom Sicherungsordner.";
                    break;
                case "LSyncing":
                    trayIcon.Icon = iconYellow;
                    trayIcon.Text = "LrCatSync: synchronisiere Lightroom Katalog.";
                    break;
                case "RcloneCfg":
                    trayIcon.Icon = iconRed;
                    trayIcon.Text = "LrCatSync: rclone Konfigurationsdatei fehlt!";
                    break;
                case "RcloneExe":
                    trayIcon.Icon = iconRed;
                    trayIcon.Text = "LrCatSync: rclone.exe fehlt!";
                    break;
                case "Error":
                    trayIcon.Icon = iconRed;
                    trayIcon.Text = "LrCatSync: Interner Programm fehler, bitte Log überprüfen!";
                    break;
                case "Lockfile":
                    trayIcon.Icon = iconLightBlue;
                    trayIcon.Text = "LrCatSync: Lightroom Classic ist aktiv.";
                    break;
                case "NoSamba":
                    trayIcon.Icon = iconRed;
                    trayIcon.Text = "LrCatSync: Keine Verbindung zum Samba Server!";
                    break;
                case "RemoteLockfile":
                    trayIcon.Icon = iconMagenta;
                    trayIcon.Text = "LrCatSync: Remote Sync ist aktiv.";
                    break;
                case "LockfileErr":
                    trayIcon.Icon = iconRed;
                    trayIcon.Text = "LrCatSync: Veralteter Remote Sync Prozess erkannt, bitte Remotepfad prüfen!";
                    break;
                case "CrashRecovery":
                    trayIcon.Icon = iconBlue;
                    trayIcon.Text = "LrCatSync: Crash-Recovery läuft...";
                    break;
                case "SyncDisabled":
                    trayIcon.Icon = iconApp;
                    trayIcon.Text = "LrCatSync: ist Ausgeschaltet.";
                    break;
            }
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
            iconMagenta?.Dispose();
            iconBlue?.Dispose();
            foreach (var stream in iconStreams)
                stream.Dispose();
            iconStreams.Clear();

            // Tray-Icon entfernen und freigeben
            trayIcon?.Dispose();
        }
    }
}
