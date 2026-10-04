using LrCatalogSync.Infrastructure;
using LrCatalogSync.Resources.Strings;
using LrCatalogSync.UI;

namespace LrCatalogSync.Core
{
    // Koordinator für sequenzielle Ausführung von Backup und Katalog-Sync
    // Stellt sicher dass BackupManager und CatalogManager NACHEINANDER laufen
    public static class Coordinator
    {
        // Lock gegen parallele Ausführung
        private static readonly object cycleLock = new object();
        private static bool isCycleRunning = false;

        // Liefert den Laufstatus für manuelle Arbeitsbereiche wie USB-Export.
        public static bool IsCycleRunning
        {
            get
            {
                lock (cycleLock)
                {
                    return isCycleRunning;
                }
            }
        }

        private static bool hasError = false;
        private static bool backupSyncSucceeded = false;
        private static bool catalogSyncSucceeded = false;
        private static bool cfgFileLost = false;
        // Bleibt über mehrere Prüfzyklen bestehen, damit der Lightroom-Status
        // durchgehend per Heartbeat auf dem Remote-System gehalten wird.
        private static LockManager? lightroomLockManager;

        // Führt kompletten Sync-Zyklus aus: Backup → Katalog-Sync
        // Wird vom Timer in LrCatSync aufgerufen
        public static void RunCoordinator(AppConfig config, TrayManager trayManager)
        {
            // Verhindere parallele Ausführung
            lock (cycleLock)
            {
                if (isCycleRunning)
                {
                    return;
                }                
                isCycleRunning = true;
            }
            try
            {
                // ========== VALIDIERUNGEN ==========
                // Prüfe zuerst ob Config-Datei existiert (wichtig für ersten Start)
                if (!File.Exists(GlobalData.LrCatSyncConfigPath))
                {
                    Log.Error($"Coordinator: {Strings.Get("Log_Coordinator_ConfigMissing")}");
                    trayManager.UpdateStatus("NoCfg");
                    cfgFileLost = true;
                    return;
                }
                // Prüfe ob LrCatSyncRclone.conf existiert
                if (!File.Exists(GlobalData.LrCatSyncRcloneConfigPath))
                {
                    Log.Error($"Coordinator: {Strings.Get("Log_Coordinator_RcloneConfigMissing")}");
                    trayManager.UpdateStatus("RcloneCfg");
                    cfgFileLost = true;
                    return;
                }
                // Lade Config neu (falls in SettingsForm gespeichert wurde)
                if (cfgFileLost)
                {
                    Log.Info($"Coordinator: {Strings.Get("Log_Coordinator_ConfigRestored")}");
                    config = AppConfig.LoadFromFile(GlobalData.LrCatSyncConfigPath, GlobalData.BaseDir);
                    Log.SetLogLevel(config.LogLevel);
                    Localization.Apply(config.Language);
                    cfgFileLost = false;
                }
                // Prüfe ob rclone.exe existiert    
                if (!File.Exists(config.RclonePath))
                {
                    Log.Error($"Coordinator: {Strings.Get("Log_Coordinator_RcloneMissing")}");
                    trayManager.UpdateStatus("RcloneExe");
                    return;
                }

                // ========== PRÜFUNG: Ob ein anderer LrCatalogSync läuft (Anderer Rechner) ==========
                // Remote Lockfile vom Samba-Server prüfen
                // Rückgabewerte: 0=Fehler, 1=Kein Lock, 2=Lock aktiv, 3=Lock veraltet
                int remoteLockStatus = LockManager.CheckLock(config, trayManager);
                
                // Wenn Lockfile erkannt, Fehlerhaft oder veraltet ist, dann Zyklus überspringen und roten Status anzeigen
                if (remoteLockStatus != 1)
                {
                    return;
                }   

                // ========== PRÜFUNG: LIGHTROOM LÄUFT? ==========
                // Prüfe ob Lightroom geöffnet ist (Lock-Dateien erkennen)
                // Wenn ja überspringe Backup und Katalog-Sync, zeige roten Status an
                if (IsLightroomRunning(config))
                {
                    // Erst jetzt wird der lokale Zustand auch für andere Clients sichtbar.
                    // Ein vorhandener eigener Status wird von CheckLock bewusst akzeptiert.
                    lightroomLockManager ??= new LockManager(config);
                    if (!lightroomLockManager.AcquireLightroomLock(config, trayManager))
                    {
                        Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_LightroomStatusFailed")}");
                        trayManager.UpdateStatus("NoSamba");
                        return;
                    }

                    Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_LightroomActiveSkipped")}");
                    trayManager.UpdateStatus("Lockfile");
                    return;
                }

                // Lightroom wurde seit dem letzten Prüfzyklus geschlossen. Der eigene
                // Remote-Status darf jetzt entfernt werden; fremde Locks bleiben geschützt.
                if (lightroomLockManager != null)
                {
                    lightroomLockManager.ReleaseLightroomLock(config);
                    lightroomLockManager = null;
                    Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_LightroomClosed")}");
                }

                // ========== PRÜFUNG: BACKUP AKTIV? ==========
                if (!config.EnableBackups)
                {
                    Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_BackupDisabled")}");
                }
                else
                {
                    // ========== SCHRITT 1: BackupManager ausführen ==========
                    // BackupManager synchronisiert BackupsLocalPath → NAS
                    Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_BackupStarted")}");
                    try
                    {
                        backupSyncSucceeded = BackupManager.RunBackupProcess(config, trayManager);                        
                    }
                    catch (Exception ex)
                    {
                        hasError = true;
                        Log.Error($"Coordinator: {string.Format(Strings.Get("Log_Coordinator_BackupFailed"), ex.Message)}");
                        trayManager.UpdateStatus("Error");
                        return;
                    }
                    finally
                    {
                        if (!hasError && backupSyncSucceeded)
                        {
                            Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_BackupFinished")}");
                        }else
                        {
                            Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_BackupCancelled")}");
                        }
                    }
                }

                // ========== SCHRITT 2: KATALOG-SYNC ==========
                // CatalogManager synchronisiert CatalogLocalPath → NAS (oder umgekehrt)
                Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_CatalogStarted")}");
                
                try
                {
                    catalogSyncSucceeded = CatalogManager.RunCatalogSync(config, trayManager);
                    if (catalogSyncSucceeded)
                    {
                        Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_CatalogFinished")}");
                    }
                    else
                    {
                        Log.Error($"Coordinator: {Strings.Get("Log_Coordinator_CatalogFailed")}");
                        trayManager.UpdateStatus("Error");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Coordinator: {string.Format(Strings.Get("Log_Coordinator_CatalogFailedWithMessage"), ex.Message)}");
                    trayManager.UpdateStatus("Error");
                }

                // ========== ZYKLUS ABGESCHLOSSEN ==========
                if (!hasError && (backupSyncSucceeded || !config.EnableBackups) && catalogSyncSucceeded)
                {
                    Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_CycleSucceeded")}");
                    trayManager.UpdateStatus("Standby");
                }
                else
                {
                    Log.Debug($"Coordinator: {Strings.Get("Log_Coordinator_CycleFinishedWithErrors")}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Coordinator: {string.Format(Strings.Get("Log_Coordinator_CycleFailed"), ex.Message)}");
                trayManager.UpdateStatus("Error");
            }
            finally
            {
                // Lock freigeben für nächsten Zyklus
                lock (cycleLock)
                {
                    isCycleRunning = false;
                }
            }
        }        

        // Prüft ob Lightroom läuft (sucht nach Lock-Dateien)
        private static bool IsLightroomRunning(AppConfig config)
        {
            try
            {
                string[] lockFiles = {
                    $"{config.CatalogName}.lrcat.lock",
                    $"{config.CatalogName}.lrcat-shm",
                    $"{config.CatalogName}.lrcat-wal"
                };
                
                foreach (string lockFile in lockFiles)
                {
                    string fullPath = Path.Combine(config.CatalogLocalPath, lockFile);
                    if (File.Exists(fullPath))
                    {
                        Log.Debug($"Coordinator: {string.Format(Strings.Get("Log_Coordinator_LightroomLockFound"), fullPath)}");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Coordinator: {string.Format(Strings.Get("Log_Coordinator_LightroomCheckFailed"), ex.Message)}");
                return false;
            }
        }
    }
}
