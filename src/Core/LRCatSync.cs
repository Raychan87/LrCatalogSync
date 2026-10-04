using LrCatalogSync.Infrastructure;    // ← für Log, AppConfig, GlobalData
using LrCatalogSync.Resources.Strings;
using LrCatalogSync.UI;                // ← für TrayManager
using Microsoft.Win32;

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
        private UsbExportForm? usbExportForm;                    // Bereits geöffnetes USB-Export-Fenster
        private System.Threading.Timer? MainCycleTimer;     // Timer für Sync-Zyklus (Backup + Katalog)
        private bool LrCatSyncEnabled = true;               // Sync aktiv (beim Start immer an)
        private bool syncEnabledBeforeUsbExport;               // Zustand vor dem Öffnen von USB-Export
        private ToolStripMenuItem? toggleItem;              // Menü-Eintrag für Sync ein/aus

        // ==================== KONSTRUKTOR - HAUPTEINSTIEGSPUNKT ====================
        // Initialisiert die Anwendung: Logs, Config, Tray und Menü
        public LrCatSyncInit()
        {
            SystemEvents.SessionEnding += OnSessionEnding;

            // ========== INITIALISIERUNG ==========
            // Logs im Verzeichnis data/logs erstellen
            Log.Initialize(GlobalData.BaseDir);
            Log.Info($"LrCatSync: {Strings.Get("Log_LrCatSync_Started")}");

            // Config aus Datei laden (falls vorhanden, sonst Standard-Einstellungen)
            config = AppConfig.LoadFromFile(GlobalData.LrCatSyncConfigPath, GlobalData.BaseDir);
            Log.SetLogLevel(config.LogLevel);
            Localization.Apply(config.Language);
            RcloneInstaller.EnsureManagedRclone(config.RclonePath);
            if (config.RcloneFolderWasMigrated && File.Exists(GlobalData.LrCatSyncConfigPath))
            {
                try
                {
                    config.Save(GlobalData.LrCatSyncConfigPath);
                }
                catch (Exception ex)
                {
                    Log.Error($"LrCatSync: {string.Format(Strings.Get("Log_LrCatSync_MigratedPathSaveFailed"), ex.Message)}");
                }
            }

            // Autorun aus Registry laden und in Config speichern (für Anzeige in SettingsForm)
            config.AutoRun = Autorun.IsEnabled();

            // ========== MANAGER INITIALISIEREN ==========
            // Erstelle TrayManager für UI-Verwaltung
            trayManager = new TrayManager();
            SMBConnectionManager.Instance.SetStatusCallback(trayManager.UpdateStatus);

            // ========== KONTEXTMENÜ AUFBAUEN ==========
            SetupContextMenu();
            
            // ========== CRASH-RECOVERY: Verwaiste Locks bereinigen ==========
            // Nur ausführen, wenn Config existiert (sonst keine SMB-Verbindung nötig)
            if (LockManager.CheckRecovery(config, trayManager))
                Log.Debug($"LrCatSync: {Strings.Get("Log_LrCatSync_CrashRecoveryDone")}");

            // ========== INITIALISIERE MAIN-CYCLE ==========
            InitMain();
        }

        // ==================== INITIALISIERE MAIN-CYCLE-TIMER ===================
        private void InitMain()
        {
            // Stoppe vorherigen Timer (falls vorhanden)
            MainCycleTimer?.Dispose();    
            Log.Debug($"LrCatSync: {string.Format(Strings.Get("Log_LrCatSync_MainCycleInitialized"), config.GlobalCycleInterval)}");
            // Timer führt alle GlobalCycleInterval Sekunden kompletten Zyklus aus (Backup → Katalog)     
            MainCycleTimer = new System.Threading.Timer(MainCycle, null, 0, config.GlobalCycleInterval * 1000);
        }

        // ==================== MAIN-CYCLE ====================
        // Ein Zyklus des Programms: Backup → Katalog-Sync
        private void MainCycle(object? state)
        {
            Localization.ApplyToCurrentThread();

            // Fehlende Haupt-Config immer anzeigen (auch bei geöffnetem USB-Export)
            if (!File.Exists(GlobalData.LrCatSyncConfigPath))
            {
                if (!Coordinator.IsCycleRunning)
                    trayManager.UpdateStatus("NoCfg");
                if (!LrCatSyncEnabled)
                    return;
            }

            // ========== PRÜFUNG: LrCatSync aktiviert? ==========
            if (!LrCatSyncEnabled)
            {
                Log.Debug($"LrCatSync: {Strings.Get("Log_LrCatSync_CoordinatorDisabled")}");
                trayManager.UpdateStatus("SyncDisabled");
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

            var previousMenu = trayManager.GetTrayIcon().ContextMenuStrip;
            // ========== MENÜ-EINTRAG: USB-EXPORT ==========
            var usbExportItem = new ToolStripMenuItem(Strings.Tray_Menu_UsbExport);
            usbExportItem.Click += (s, e) => OpenUsbExportForm();
            menu.Items.Add(usbExportItem);

            // ========== MENÜ-TRENNLINIE ==========
            menu.Items.Add(new ToolStripSeparator());

            // ========== MENÜ-EINTRAG: Sync ein/aus (über Einstellungen) ==========
            // Zeigt "Ausschalten" wenn Sync läuft, "Einschalten" wenn er aus ist
            toggleItem = new ToolStripMenuItem(GetToggleMenuText())
            {
                Enabled = usbExportForm is not { IsDisposed: false }
            };
            toggleItem.Click += (s, e) => OnOffCoordinator(toggleItem!);
            menu.Items.Add(toggleItem);

            // ========== MENÜ-TRENNLINIE ==========
            menu.Items.Add(new ToolStripSeparator());

            // ========== MENÜ-EINTRAG: Einstellungen öffnen ==========
            var settingsItem = new ToolStripMenuItem(Strings.Tray_Menu_Settings);
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
                        Localization.Apply(config.Language);
                        trayManager.RefreshText();
                        SetupContextMenu();
                        RcloneInstaller.EnsureManagedRclone(config.RclonePath);
                        InitMain();
                        Log.Info($"Config: {Strings.Get("Log_LrCatSync_SettingsUpdated")}");
                    }
                }

                settingsForm = null;
            };
            menu.Items.Add(settingsItem);

            // ========== MENÜ-TRENNLINIE ==========
            menu.Items.Add(new ToolStripSeparator());

            // ========== MENÜ-EINTRAG: Programm beenden ==========
            var exitItem = new ToolStripMenuItem(Strings.Tray_Menu_Exit);
            exitItem.Click += (s, e) => 
            { 
                trayManager.GetTrayIcon().Visible = false;
                Application.Exit(); 
            };
            menu.Items.Add(exitItem);

            // Binde Menü an Tray-Icon
            trayManager.GetTrayIcon().ContextMenuStrip = menu;
            previousMenu?.Dispose();
        }

        private string GetToggleMenuText()
        {
            if (usbExportForm is { IsDisposed: false })
                return Strings.Tray_Menu_UsbExportOpen;

            return LrCatSyncEnabled ? Strings.Tray_Menu_TurnOff : Strings.Tray_Menu_TurnOn;
        }

        // Öffnet das USB-Export-Fenster modeless und pausiert den normalen Sync.
        private void OpenUsbExportForm()
        {
            if (usbExportForm is { IsDisposed: false })
            {
                usbExportForm.Activate();
                return;
            }

            syncEnabledBeforeUsbExport = LrCatSyncEnabled;
            LrCatSyncEnabled = false;
            if (toggleItem != null)
            {
                toggleItem.Enabled = false;
                toggleItem.Text = Strings.Tray_Menu_UsbExportOpen;
            }

            usbExportForm = new UsbExportForm(config, () => Coordinator.IsCycleRunning);
            usbExportForm.FormClosed += UsbExportFormClosed;
            usbExportForm.Show();
        }

        private void UsbExportFormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is Form form)
            {
                form.FormClosed -= UsbExportFormClosed;
            }

            usbExportForm = null;
            LrCatSyncEnabled = syncEnabledBeforeUsbExport;
            if (toggleItem != null)
            {
                toggleItem.Enabled = true;
                toggleItem.Text = GetToggleMenuText();
            }

            Log.Info($"LrCatSync: {Strings.Get("Log_LrCatSync_UsbExportClosed")}");
        }

        // ==================== SYNC EIN/AUS SCHALTEN ====================
        // Schaltet den Sync-Zyklus ein oder aus
        private void OnOffCoordinator(ToolStripMenuItem toggleItem)
        {
            if (LrCatSyncEnabled)
            {
                // ========== AUSSCHALTEN ==========
                LrCatSyncEnabled = false;
                toggleItem.Text = Strings.Tray_Menu_TurnOn;
                trayManager.UpdateStatus("SyncDisabled");           
                Log.Info($"LrCatSync: {Strings.Get("Log_LrCatSync_ManuallyStopped")}");
            }
            else
            {
                // ========== EINSCHALTEN ==========
                LrCatSyncEnabled = true;
                toggleItem.Text = Strings.Tray_Menu_TurnOff;
                trayManager.UpdateStatus("Standby");
                Log.Info($"LrCatSync: {Strings.Get("Log_LrCatSync_ManuallyStarted")}");
            }
        }
        
        // ==================== BEREINIGUNG ====================
        // Cleanup: Beende Timer und gebe Ressourcen frei
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SystemEvents.SessionEnding -= OnSessionEnding;
                RcloneProcessManager.StopAll();

                usbExportForm?.Close();

                // Stoppe und dispose Timer
                if (MainCycleTimer != null)
                {
                    MainCycleTimer.Dispose();
                    Log.Debug($"LrCatSync: {Strings.Get("Log_LrCatSync_TimerStopped")}");
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

        private void OnSessionEnding(object? sender, SessionEndingEventArgs e)
        {
            Log.Info($"LrCatSync: {string.Format(Strings.Get("Log_LrCatSync_SessionEnding"), e.Reason)}");
            LrCatSyncEnabled = false;
            MainCycleTimer?.Dispose();
            RcloneProcessManager.BeginShutdown();
        }
    }
}
