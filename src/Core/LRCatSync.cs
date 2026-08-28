using LrCatalogSync.Infrastructure;    // ← für Log, AppConfig, GlobalData
using LrCatalogSync.UI;                // ← für TrayManager

namespace LrCatalogSync.Core
{
    // Hauptklasse: Startet und verwaltet die Anwendung
    // Delegiert Backup-Logik an BackupManager und UI an TrayManager
    // Startet automatischen Backup-Zyklus
    public class LrCatSyncInit : ApplicationContext
    {
        // ==================== EIGENSCHAFTEN ====================
        private AppConfig config;                           // Konfigurationsdaten laden/speichern
        private TrayManager trayManager;                    // Manager für Tray-Icon und Status
        private SettingsForm? settingsForm;                 // Bereits geöffnetes Einstellungsfenster
        private System.Threading.Timer? MainCycleTimer;     // Timer für Sync-Zyklus (Backup + Katalog)
        private bool LrCatSyncEnabled = true;               // Sync aktiv (beim Start immer an)
        private ToolStripMenuItem? toggleItem;              // Menü-Eintrag für Sync ein/aus

        // ==================== KONSTRUKTOR - HAUPTEINSTIEGSPUNKT ====================
        // Initialisiert die Anwendung: Logs, Config, Tray und Menü
        public LrCatSyncInit()
        {
            // ========== INITIALISIERUNG ==========
            // Logs im Verzeichnis data/logs erstellen
            Log.Initialize(GlobalData.BaseDir);
            Log.Info("LrCatalog Sync gestartet");

            // Config aus Datei laden (falls vorhanden, sonst Standard-Einstellungen)
            config = AppConfig.LoadFromFile(GlobalData.LrCatSyncConfigPath, GlobalData.BaseDir);
            Log.SetLogLevel(config.LogLevel);

            // Autorun aus Registry laden und in Config speichern (für Anzeige in SettingsForm)
            config.AutoRun = Autorun.IsEnabled();

            // ========== MANAGER INITIALISIEREN ==========
            // Erstelle TrayManager für UI-Verwaltung
            trayManager = new TrayManager();
            SMBConnectionManager.Instance.SetStatusCallback(trayManager.UpdateStatus);

            // ========== KONTEXTMENÜ AUFBAUEN ==========
            SetupContextMenu();

            // ========== PRÜFUNG: Config-Datei vorhanden? ==========
            // Wenn Config-Datei fehlt, zeige weißen Status an und BEENDHE Konstruktor
            // Damit ist das Tray-Menü sofort bedienbar - der Nutzer kann eine Config anlegen
            if (!File.Exists(GlobalData.LrCatSyncConfigPath))
            {
                Log.Error("LrCatSync: Konfigurationsdatei fehlt - erstelle Standard-Konfiguration");
                trayManager.UpdateStatus("NoCfg");
                return;
            }

            // ========== CRASH-RECOVERY: Verwaiste Locks bereinigen ==========
            // Nur ausführen, wenn Config existiert (sonst keine SMB-Verbindung nötig)
            if (LockManager.CheckRecovery(config, trayManager))
                Log.Debug("LrCatSync: Crash-Recovery abgeschlossen - nächster Zyklus startet Sync neu");

            // ========== INITIALISIERE MAIN-CYCLE-TIMER ==========          
            InitMain();
        }

        // ==================== INITIALISIERE MAIN-CYCLE-TIMER ===================
        private void InitMain()
        {
            // Stoppe vorherigen Timer (falls vorhanden)
            MainCycleTimer?.Dispose();    
            Log.Debug($"LrCatSync: Initialisiere Main-Zyklus mit ({config.GlobalCycleInterval}sec Intervall)");   
            // Timer führt alle GlobalCycleInterval Sekunden kompletten Zyklus aus (Backup → Katalog)            
            MainCycleTimer = new System.Threading.Timer(MainCycle, null, 0, config.GlobalCycleInterval * 1000);
        }

        // ==================== MAIN-CYCLE ====================
        // Ein Zyklus des Programms: Backup → Katalog-Sync
        private void MainCycle(object? state)
        {
            // ========== PRÜFUNG: LrCatSync aktiviert? ==========
            if (!LrCatSyncEnabled)
            {
                Log.Debug("LrCatSync: Coordinator ist deaktiviert - Zyklus übersprungen");
                return;
            }

            // Coordinator übernimmt die sequenzielle Ausführung
            Coordinator.RunCoordinator(config, trayManager);
        }

        // ==================== MENÜ-SETUP ====================
        // Erstellt das Kontextmenü für das Tray-Icon
        private void SetupContextMenu()
        {
            var menu = new ContextMenuStrip();

            // ========== MENÜ-EINTRAG: Status (nur anzeigen) ==========
//            var statusItem = new ToolStripMenuItem("Status: Standby") 
//            { 
//                Enabled = false, 
//                Name = "statusItem" 
//            };
//            menu.Items.Add(statusItem);
//            menu.Items.Add(new ToolStripSeparator());

            // ========== MENÜ-EINTRAG: Sync ein/aus (über Einstellungen) ==========
            // Zeigt "Ausschalten" wenn Sync läuft, "Einschalten" wenn er aus ist
            toggleItem = new ToolStripMenuItem("Ausschalten");
            toggleItem.Click += (s, e) => OnOffCoordinator(toggleItem!);
            menu.Items.Add(toggleItem);

            // ========== MENÜ-EINTRAG: Einstellungen öffnen ==========
            var settingsItem = new ToolStripMenuItem("Einstellungen");
            settingsItem.Click += (s, e) =>
            {
                if (settingsForm is { IsDisposed: false })
                {
                    settingsForm.Activate();
                    return;
                }

                // Zeige Einstellungs-Dialog
                using (settingsForm = new SettingsForm(config))
                {
                    if (settingsForm.ShowDialog() == DialogResult.OK)
                    {
                        // Config neu laden (wenn in SettingsForm gespeichert wurde)
                        config = AppConfig.LoadFromFile(GlobalData.LrCatSyncConfigPath, GlobalData.BaseDir);
                        Log.SetLogLevel(config.LogLevel);
                        InitMain();
                        Log.Info("Config: Einstellungen aktualisiert");
                    }
                }

                settingsForm = null;
            };
            menu.Items.Add(settingsItem);

            // ========== MENÜ-TRENNLINIE ==========
            menu.Items.Add(new ToolStripSeparator());

            // ========== MENÜ-EINTRAG: Programm beenden ==========
            var exitItem = new ToolStripMenuItem("Beenden");
            exitItem.Click += (s, e) => 
            { 
                trayManager.GetTrayIcon().Visible = false;
                Application.Exit(); 
            };
            menu.Items.Add(exitItem);

            // Binde Menü an Tray-Icon
            trayManager.GetTrayIcon().ContextMenuStrip = menu;
        }

        // ==================== SYNC EIN/AUS SCHALTEN ====================
        // Schaltet den Sync-Zyklus ein oder aus
        private void OnOffCoordinator(ToolStripMenuItem toggleItem)
        {
            if (LrCatSyncEnabled)
            {
                // ========== AUSSCHALTEN ==========
                LrCatSyncEnabled = false;
                toggleItem.Text = "Einschalten";
                trayManager.UpdateStatus("SyncDisabled");
                Log.Info("LrCatSync: manuell gestoppt");
            }
            else
            {
                // ========== EINSCHALTEN ==========
                LrCatSyncEnabled = true;
                toggleItem.Text = "Ausschalten";
                trayManager.UpdateStatus("standby");
                Log.Info("LrCatSync: manuell gestartet");
            }
        }

        // ==================== BEREINIGUNG ====================
        // Cleanup: Beende Timer und gebe Ressourcen frei
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Stoppe und dispose Timer
                if (MainCycleTimer != null)
                {
                    MainCycleTimer.Dispose();
                    Log.Debug("LrCatSync:Zyklus Timer beendet");
                }

                // Verstecke Tray-Icon und gebe Ressourcen frei
                if (trayManager != null)
                {
                    trayManager.GetTrayIcon().Visible = false;
                    trayManager.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
