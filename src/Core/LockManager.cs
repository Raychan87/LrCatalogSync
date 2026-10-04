using System.Diagnostics;
using System.Text;

using LrCatalogSync.Infrastructure;    // ← für Log, AppConfig, SMBConnectionManager
using LrCatalogSync.Resources.Strings;
using LrCatalogSync.UI;

namespace LrCatalogSync.Core
{
    // LockManager für atomare Lock-Akquise lokal + remote
    // Verwaltet LrCatSync.lock Datei für Synchronisation
    public class LockManager : IDisposable
    {
        // ==================== EIGENSCHAFTEN ====================
        // Eindeutige Sync-GUID für Tracking
        public string SyncGuid { get; private set; }

        // Lokale Lock-Datei
        private FileStream? _localLockStream;

        // Heartbeat Thread
        private Thread? _heartbeatThread;

        private CancellationTokenSource? _cts;

        // AppConfig für Lock-Pfade
        private AppConfig? _config;

        // Trackt ob vorher ein Remote Lock vorhanden war (für Cleanup-Logik)
        private static bool wasRemoteLockPresent = false;

        // ==================== KONSTRUKTOR ====================
        public LockManager(AppConfig config)
        {
            _config = config;
            SyncGuid = config.SyncGuid;
        }

        // ==================== CRASH-RECOVERY ====================
        // Prüft ob ein LrCatSync Crash vorlag 
        // Remote und Lokal Lock müssen vorhanden sein und SyncGuid muss übereinstimmen
        // Wenn ja, führt Crash-Recovery ein rclone Sync durch, so wie vor den letzten Crash
        public static bool CheckRecovery(AppConfig config, TrayManager trayManager)
        {
            try
            {
                // ========== Local Lock ERKENNUNG ==========
                //Wenn Local Lock vorhanden ist und lese SyncGuid aus
                if (File.Exists(config.SyncLocalLockFile) && IsCatalogLockOurs(config))
                {
                    // Prüfe lokalen Lock auf SyncGuid und speichere es
                    string localLockContent = File.ReadAllText(config.SyncLocalLockFile);
                    var localLines = localLockContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    string localSyncGuid = string.Empty;
                    foreach (var line in localLines)
                    {
                        if (line.StartsWith("SyncGuid="))
                        {
                            localSyncGuid = line.Substring("SyncGuid=".Length).Trim();
                            break;
                        }
                    }

                    // ========== Remote Lock ERKENNUNG ==========
                    // Stelle SMB-Verbindung her falls nicht vorhanden
                    if (SMBConnectionManager.Instance.EnsureConnected(config))
                    {
                        byte[]? remoteLockData = SMBConnectionManager.Instance.ReadFile(GlobalConst.LOCK_FILE);
                        if (remoteLockData != null)
                        {
                            // SyncGuid und Direction aus Remote Lock extrahieren
                            string remoteLockContent = Encoding.UTF8.GetString(remoteLockData);
                            CatalogManager.SyncDirection direction = ExtractDirection(remoteLockContent);
                            var remoteLines = remoteLockContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                            string remoteSyncGuid = string.Empty;
                            foreach (var line in remoteLines)
                            {
                                if (line.StartsWith("SyncGuid="))
                                {
                                    remoteSyncGuid = line.Substring("SyncGuid=".Length).Trim();
                                    break;
                                }
                            }
                            // Prüfe ob SyncGuids übereinstimmen
                            if (localSyncGuid != remoteSyncGuid)
                            {
                                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_SyncGuidMismatch"), localSyncGuid, remoteSyncGuid)}");
                                trayManager.UpdateStatus("Error");
                                return false;
                            }

                            trayManager.UpdateStatus("CrashRecovery");
                            Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_CrashRecoveryStarted"), direction)}");

                            // ========== RCLONE SYNC  ==========
                            bool recoveryOk = CatalogManager.RunRcloneSync(config, direction, false); // false = rclone copy überspringen beim Crash-Recovery

                            if (recoveryOk)
                            {
                                // Alle drei Locks nur bei Erfolg bereinigen
                                try { File.Delete(config.SyncLocalLockFile); } catch { }
                                CatalogManager.CleanupLightroomLocks(config);
                                SMBConnectionManager.Instance.DeleteFile(GlobalConst.LOCK_FILE);
                                Log.Debug($"LockManager: {Strings.Get("Log_Lock_CrashRecoveryCompleted")}");
                                trayManager.UpdateStatus("Standby");
                                return true;
                            }
                            else
                            {
                                Log.Error($"LockManager: {Strings.Get("Log_Lock_CrashRecoveryFailed")}");
                                trayManager.UpdateStatus("Error");
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_CrashRecoveryError"), ex.Message)}");
                trayManager.UpdateStatus("Error");
            }
            return false;
        }
  
        // ==================== ÖFFENTLICHE METHODEN ====================
        
        // Prüft ob ein Remote Lock von einem anderen Client aktiv ist
        // Rückgabewerte:
        // 0 = SMB-Fehler oder Fehler beim Lesen/Prüfen der Lock-Datei
        // 1 = Kein Remote Lock vorhanden (normaler Zustand)
        // 2 = Remote Lock vorhanden und aktuell (< 30 min) - andere Client arbeitet
        // 3 = Remote Lock vorhanden aber veraltet (> 30 min) oder Fehlerhaft - manuell prüfen erforderlich
        // 
        // Side-Effects:
        // - Wenn Remote Lock mit Upload: Erstellt Lightroom-Lock lokal
        // - Wenn Remote Lock mit Download: Erstellt KEIN Lightroom-Lock
        // - Wenn Remote Lock verschwindet (war vorher da): Löscht Lightroom-Lock
        public static int CheckLock(AppConfig config, TrayManager trayManager)
        {
            try
            {
                bool CreateLRLockFile = false;

                // ========== VALIDIERUNG: SMB-Config vollständig? ==========
                if (string.IsNullOrEmpty(config.RemoteIP) || string.IsNullOrEmpty(config.SambaUser))
                {
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_SmbConfigIncompleteCheck")}");
                    trayManager.UpdateStatus("NoSamba");
                    return 0; // SMB-Fehler
                }

                // ========== SMB-VERBINDUNG HERSTELLEN ==========
                if (!SMBConnectionManager.Instance.EnsureConnected(config))
                {
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_SmbConnectFailedCheck")}");
                    return 0; // SMB-Fehler
                }

                // ========== REMOTE LOCK PRÜFEN ==========
                // Versuche Lockfile direkt zu lesen
                byte[]? lockData = SMBConnectionManager.Instance.ReadFile(GlobalConst.LOCK_FILE);
                
                if (lockData == null)
                {
                    // ========== REMOTE LOCK VERSCHWUNDEN ==========
                    // Wenn es vorher da war → Cleanup durchführen
                    if (wasRemoteLockPresent)
                    {
                        Log.Debug($"LockManager: {Strings.Get("Log_Lock_RemoteLockDisappeared")}");
                        CatalogManager.CleanupLightroomLocks(config);
                        wasRemoteLockPresent = false;
                    }
                    
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_NoRemoteLock")}");
                    return 1; // Kein Lock vorhanden
                }

                // ========== LOCKFILE INHALT PRÜFEN ==========
                if (lockData == null)
                {
                    Log.Error($"LockManager: {Strings.Get("Log_Lock_RemoteLockReadFailed")}");
                    trayManager.UpdateStatus("NoSamba");
                    return 0; // SMB-Fehler
                }

                string lockContent = Encoding.UTF8.GetString(lockData);
                DateTime lastHeartbeat = ExtractLatestTimestamp(lockContent);
                string remoteSyncGuid = ExtractValue(lockContent, "SyncGuid");
                string lockType = ExtractValue(lockContent, "LockType");

                if (lastHeartbeat == DateTime.MinValue)
                {
                    Log.Error($"LockManager: {Strings.Get("Log_Lock_RemoteLockInvalidFormat")}");
                    trayManager.UpdateStatus("LockfileErr");
                    return 3; // Fehlerhaft
                }

                // ========== ALTER DES LOCKS PRÜFEN ==========
                TimeSpan lockAge = DateTime.UtcNow - lastHeartbeat;

                if (lockAge.TotalMinutes > GlobalConst.SYNC_LOCK_TIMEOUT_MIN)
                {
                    // Ein veralteter eigener Lightroom-Status kann sicher entfernt werden:
                    // Er gehört nicht zu einem laufenden Sync und Lightroom läuft lokal nicht mehr.
                    if (lockType == GlobalConst.LIGHTROOM_LOCK_TYPE && remoteSyncGuid == config.SyncGuid)
                    {
                        SMBConnectionManager.Instance.DeleteFile(GlobalConst.LOCK_FILE);
                        wasRemoteLockPresent = false;
                        Log.Debug($"LockManager: {Strings.Get("Log_Lock_StaleOwnLightroomStatusRemoved")}");
                        return 1;
                    }

                    // Lock ist älter als Timeout → Warnung ausgeben
                    Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_RemoteLockStale"), GlobalConst.SYNC_LOCK_TIMEOUT_MIN, lockAge.TotalMinutes)}");
                    trayManager.UpdateStatus("LockfileErr");
                    wasRemoteLockPresent = false; // Lock ist nicht mehr gültig
                    return 3; // Lock veraltet
                }

                // Der eigene Lightroom-Status ist kein Grund, den eigenen Client zu blockieren.
                // Ein eigener Upload-/Download-Lock darf dagegen nur während des laufenden Syncs existieren.
                if (lockType == GlobalConst.LIGHTROOM_LOCK_TYPE && remoteSyncGuid == config.SyncGuid)
                {
                    wasRemoteLockPresent = true;
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_OwnLightroomLockDetected")}");
                    return 1;
                }

                // ========== REMOTE LOCK IST AKTIV - PRÜFE DIRECTION ==========
                CatalogManager.SyncDirection direction = ExtractDirection(lockContent);
                
                if (direction == CatalogManager.SyncDirection.Download)
                {
                    // Ein Remote-Download verändert den lokalen Katalog nicht.
                    // Deshalb weder Lightroom-Lock erzeugen noch vorhandene Locks löschen.
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_RemoteDownloadNoLightroomLock")}");
                    trayManager.UpdateStatus("RemoteLockfileDown");
                }
                else if (lockType == GlobalConst.LIGHTROOM_LOCK_TYPE)
                {
                    // Ein anderer Client hat Lightroom geöffnet. Erzeuge deshalb
                    // lokal eine von LrCatalogSync markierte Lightroom-Lock-Datei,
                    // damit Lightroom den synchronisierten Katalog nicht öffnen kann.
                    
                    CreateLRLockFile = CatalogManager.CreateLightroomLock(config);
                    trayManager.UpdateStatus("RemoteLightroom");
                    if (CreateLRLockFile)
                    {
                        Log.Debug($"LockManager: {Strings.Get("Log_Lock_RemoteLightroomLockCreated")}");
                    }                    
                }
                else if (direction == CatalogManager.SyncDirection.Upload)
                {
                    // Remote Lock ist Upload → Erstelle Lightroom-Lock lokal                    
                    CreateLRLockFile = CatalogManager.CreateLightroomLock(config);
                    trayManager.UpdateStatus("RemoteLockfileUp");
                    if (CreateLRLockFile)
                    {
                        Log.Debug($"LockManager: {Strings.Get("Log_Lock_RemoteUploadLightroomLockCreated")}");
                    }
                    }
                Log.Debug($"LockManager: {string.Format(Strings.Get("Log_Lock_RemoteLockActive"), lockAge.TotalMinutes, direction)}");
                
                wasRemoteLockPresent = true; // Merken dass wir ein aktives Remote Lock haben
                return 2; // Lock aktiv und aktuell
            }
            catch (Exception ex)
            {
                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_RemoteLockCheckFailed"), ex.Message)}");
                trayManager.UpdateStatus("Error");
                return 0; // SMB-Fehler
            }
        }

        // Extrahiert den aktuellsten Timestamp aus Lock-Datei (parst alle Heartbeat- und Timestamp-Zeilen)
        private static DateTime ExtractLatestTimestamp(string lockContent)
        {
            try
            {
                DateTime latestTime = DateTime.MinValue;
                var lines = lockContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (line.StartsWith("Timestamp=") || line.StartsWith("Heartbeat="))
                    {
                        string dateStr = line.Substring(line.IndexOf('=') + 1).Trim();
                        if (DateTime.TryParse(dateStr, out DateTime parsedTime))
                        {
                            if (parsedTime > latestTime)
                                latestTime = parsedTime;
                        }
                    }
                }

                return latestTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        // Liest einen einzelnen Schlüssel aus dem zeilenbasierten Lockfile-Format.
        // Die Methode toleriert unbekannte oder fehlende Schlüssel, damit alte Lockfiles
        // weiterhin als normale Upload-/Download-Locks verarbeitet werden können.
        private static string ExtractValue(string lockContent, string key)
        {
            try
            {
                var lines = lockContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                string prefix = key + "=";
                foreach (var line in lines)
                {
                    if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        return line.Substring(prefix.Length).Trim();
                }
            }
            catch { }
            return string.Empty;
        }

        // Extrahiert die Direction aus Lock-Datei (Upload oder Download)
        private static CatalogManager.SyncDirection ExtractDirection(string lockContent)
        {
            try
            {
                var lines = lockContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Direction="))
                    {
                        string dirStr = line.Substring("Direction=".Length).Trim();
                        if (Enum.TryParse<CatalogManager.SyncDirection>(dirStr, out CatalogManager.SyncDirection direction))
                            return direction;
                    }
                }
            }
            catch { }
            return CatalogManager.SyncDirection.None;
        }
        
        // geprpüft!! 2026.07.07
        // Akquiriert atomar lokale und remote Locks
        // Gibt false zurück wenn Locks nicht akquiriert werden können
        // syncDirection: Upload oder Download wird in Lock-Datei gespeichert
        public bool AcquireLocks(AppConfig config, TrayManager trayManager, CatalogManager.SyncDirection syncDirection)
        {
            try
            {
                // ========== VALIDIERUNG: SMB-Config vollständig? ==========
                if (string.IsNullOrEmpty(config.RemoteIP) || string.IsNullOrEmpty(config.SambaUser))
                {
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_SmbConfigIncompleteAcquire")}");
                    return false;
                }

                // ========== REMOTE LOCK AKQUIRIEREN ==========
                // Stelle SMB-Verbindung her
               if (!SMBConnectionManager.Instance.EnsureConnected(config))
                {
                    Log.Error($"LockManager: {Strings.Get("Log_Lock_SmbConnectFailedAcquire")}");
                    return false;
                }

                int remoteLockStatus = CheckLock(config, trayManager);
                
                // Wenn Lockfile erkannt, Fehlerhaft oder veraltet ist, dann Zyklus überspringen und roten Status anzeigen
                if (remoteLockStatus != 1)
                {
                    return false;
                }

                // Erstelle remote Lock-Datei via SMB mit Direction Information
                string lockContentNew = $"SyncGuid={SyncGuid}\nDirection={syncDirection}\nTimestamp={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                byte[] lockBytes = Encoding.UTF8.GetBytes(lockContentNew);
                
                if (!SMBConnectionManager.Instance.WriteFile(GlobalConst.LOCK_FILE, lockBytes))
                {
                    Log.Error($"LockManager: {Strings.Get("Log_Lock_RemoteLockWriteFailed")}");
                    _localLockStream?.Close();
                    _localLockStream = null;
                    return false;
                }

                // ========== LOKALER LOCK AKQUIRIEREN ==========
                // Erstelle lokale Lock-Datei mit FileShare.None (exklusiver Zugriff)
                if (File.Exists(config.SyncLocalLockFile))
                {
                    // Prüfe ob Lock veraltet ist (älter als SYNC_LOCK_TIMEOUT_MIN Minuten)
                    FileInfo lockInfo = new FileInfo(config.SyncLocalLockFile);
                    if (lockInfo.LastWriteTimeUtc.AddMinutes(GlobalConst.SYNC_LOCK_TIMEOUT_MIN) < DateTime.UtcNow)
                    {
                        Log.Debug($"LockManager: {string.Format(Strings.Get("Log_Lock_StaleLocalLockOverwritten"), config.SyncLocalLockFile)}");
                        File.Delete(config.SyncLocalLockFile);
                    }
                    else
                    {
                        Log.Debug($"LockManager: {string.Format(Strings.Get("Log_Lock_LocalLockActive"), GlobalConst.SYNC_LOCK_TIMEOUT_MIN)}");
                        DeleteRemoteLockIfOwned(config);
                        return false;
                    }
                }
                
                // Erstelle lokale Lock-Datei mit exklusivem Zugriff
                _localLockStream = new FileStream(config.SyncLocalLockFile, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                
                // Schreibe Sync-GUID in Lock-Datei für Tracking (Direction nur im Remote Lock!)
                // Der Writer darf den exklusiven Lock-Stream beim Freigeben nicht schließen.
                var writer = new StreamWriter(_localLockStream, Encoding.UTF8, 1024, leaveOpen: true);
                writer.WriteLine($"SyncGuid={SyncGuid}");
                writer.WriteLine($"Timestamp={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                writer.Flush();
                writer.Dispose(); // Nur Writer disposed, NICHT den underlying Stream!
                
                Log.Debug($"LockManager: {string.Format(Strings.Get("Log_Lock_BothLocksAcquired"), SyncGuid)}");
                
                // Starte Heartbeat-Thread
                StartHeartbeat();
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_AcquireFailed"), ex.Message)}");
                ReleaseLocks(config);
                return false;
            }
        }

        // Veröffentlicht, dass Lightroom auf diesem Client aktiv ist.
        // Der Status nutzt dieselbe Remote-Datei und dieselben Timeout-Regeln wie ein Sync,
        // enthält aber keinen künstlichen Upload-/Download-Richtungseintrag.
        public bool AcquireLightroomLock(AppConfig config, TrayManager trayManager)
        {
            try
            {
                if (string.IsNullOrEmpty(config.RemoteIP) || string.IsNullOrEmpty(config.SambaUser) ||
                    !SMBConnectionManager.Instance.EnsureConnected(config))
                {
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_LightroomStatusSmbUnavailable")}");
                    return false;
                }

                // CheckLock verhindert, dass ein fremder aktiver Sync oder Lightroom-Status
                // durch diesen Client überschrieben wird. Der eigene Lightroom-Status ist erlaubt.
                if (CheckLock(config, trayManager) != 1)
                    return false;

                byte[]? existingData = SMBConnectionManager.Instance.ReadFile(GlobalConst.LOCK_FILE);
                if (existingData != null)
                {
                    string existingContent = Encoding.UTF8.GetString(existingData);
                    if (ExtractValue(existingContent, "SyncGuid") == SyncGuid &&
                        ExtractValue(existingContent, "LockType") == GlobalConst.LIGHTROOM_LOCK_TYPE)
                    {
                        StartHeartbeat();
                        return true;
                    }
                }

                byte[] lockBytes = Encoding.UTF8.GetBytes(
                    $"SyncGuid={SyncGuid}\nLockType={GlobalConst.LIGHTROOM_LOCK_TYPE}\nTimestamp={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                if (!SMBConnectionManager.Instance.WriteFile(GlobalConst.LOCK_FILE, lockBytes))
                {
                    Log.Error($"LockManager: {Strings.Get("Log_Lock_LightroomStatusWriteFailed")}");
                    return false;
                }

                StartHeartbeat();
                Log.Debug($"LockManager: {string.Format(Strings.Get("Log_Lock_LightroomStatusPublished"), SyncGuid)}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_LightroomStatusPublishFailed"), ex.Message)}");
                return false;
            }
        }
                
        // Prüft ob vorhandene Catalog-Lock-Datei von uns erstellt wurde
        // Vergleicht nur die SyncGuid in der Datei mit der Config-GUID
        private static bool IsCatalogLockOurs(AppConfig config)
        {
            try
            {
                if (!File.Exists(config.CatalogLockFile))
                    return false;
                string content = File.ReadAllText(config.CatalogLockFile);
                var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("SyncGuid="))
                    {
                        string fileSyncGuid = line.Substring("SyncGuid=".Length).Trim();
                        return fileSyncGuid == config.SyncGuid;
                    }
                }
                return false;
            }
            catch { return false; }
        }

        // Startet Heartbeat-Thread für regelmäßige Aktualisierung
        public void StartHeartbeat()
        {
            if (_cts != null)
                return; // Bereits gestartet
                
            _cts = new CancellationTokenSource();
            
            _heartbeatThread = new Thread(() =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        Localization.ApplyToCurrentThread();
                        UpdateLockTimestamps();
                        Thread.Sleep(GlobalConst.HEARTBEAT_INTERVAL_SEC * 1000);
                    }
                    catch (ThreadInterruptedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_HeartbeatError"), ex.Message)}");
                    }
                }
            })
            {
                IsBackground = true
            };
            
            _heartbeatThread.Start();
            Log.Debug($"LockManager: {string.Format(Strings.Get("Log_Lock_HeartbeatStarted"), GlobalConst.HEARTBEAT_INTERVAL_SEC)}");
        }
        
        // Aktualisiert Timestamps in Lock-Dateien (Heartbeat)
        private void UpdateLockTimestamps()
        {
            try
            {
                if (_localLockStream != null && _localLockStream.CanWrite)
                {
                    // Schreibe neuen Timestamp an das Ende der Datei
                    _localLockStream.Seek(0, SeekOrigin.End);
                    using (var writer = new StreamWriter(_localLockStream))
                    {
                        writer.WriteLine($"Heartbeat={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                    }
                    _localLockStream.Flush();
                }
                
                // Remote Heartbeat via SMB
                if (_config != null && SMBConnectionManager.Instance.IsConnected)
                {
                    try
                    {
                        byte[]? existingData = SMBConnectionManager.Instance.ReadFile(GlobalConst.LOCK_FILE);
                        
                        if (existingData != null)
                        {
                            string content = Encoding.UTF8.GetString(existingData);
                            // Vor jedem Update erneut den Besitzer prüfen. So bleibt ein Lock,
                            // der inzwischen von einem anderen Client übernommen wurde, unangetastet.
                            if (ExtractValue(content, "SyncGuid") == SyncGuid)
                            {
                                content += $"\nHeartbeat={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                                byte[] updatedData = Encoding.UTF8.GetBytes(content);
                                SMBConnectionManager.Instance.WriteFile(GlobalConst.LOCK_FILE, updatedData);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_RemoteHeartbeatFailed"), ex.Message)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_HeartbeatUpdateFailed"), ex.Message)}");
            }
        }

        // Gibt alle Locks wieder frei
        // MUSS IMMER im finally-Block aufgerufen werden!
        public void ReleaseLocks(AppConfig config)
        {
            try
            {
                // Stoppe Heartbeat
                StopHeartbeat();
                
                // Release lokaler Lock
                if (_localLockStream != null)
                {
                    try
                    {
                        _localLockStream.Close();
                        _localLockStream = null;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_LocalLockCloseFailed"), ex.Message)}");
                    }
                }
                
                // Lösche lokale Lock-Datei
                if (!string.IsNullOrEmpty(config.SyncLocalLockFile) && File.Exists(config.SyncLocalLockFile))
                {
                    try
                    {
                        File.Delete(config.SyncLocalLockFile);
                        Log.Debug($"LockManager: {string.Format(Strings.Get("Log_Lock_LocalLockDeleted"), config.SyncLocalLockFile)}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_LocalLockDeleteFailed"), ex.Message)}");
                    }
                }
                
                DeleteRemoteLockIfOwned(config);
            }
            catch (Exception ex)
            {
                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_ReleaseFailed"), ex.Message)}");
            }
        }

        // Entfernt ausschließlich den eigenen Remote-Lock.
        // Das ist besonders wichtig beim Aufräumen nach einem Fehler, damit kein
        // inzwischen von einem anderen Client gesetzter Lock gelöscht wird.
        public void ReleaseLightroomLock(AppConfig config)
        {
            StopHeartbeat();
            DeleteRemoteLockIfOwned(config);
        }

        private void DeleteRemoteLockIfOwned(AppConfig config)
        {
            try
            {
                if (!SMBConnectionManager.Instance.EnsureConnected(config))
                {
                    Log.Error($"LockManager: {Strings.Get("Log_Lock_RemoteLockDeleteSmbUnavailable")}");
                    return;
                }

                byte[]? lockData = SMBConnectionManager.Instance.ReadFile(GlobalConst.LOCK_FILE);
                if (lockData == null)
                    return;

                string lockContent = Encoding.UTF8.GetString(lockData);
                if (ExtractValue(lockContent, "SyncGuid") == SyncGuid &&
                    SMBConnectionManager.Instance.DeleteFile(GlobalConst.LOCK_FILE))
                {
                    Log.Debug($"LockManager: {Strings.Get("Log_Lock_OwnRemoteLockDeleted")}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"LockManager: {string.Format(Strings.Get("Log_Lock_OwnRemoteLockDeleteFailed"), ex.Message)}");
            }
        }
        
        // Stoppt Heartbeat-Thread
        private void StopHeartbeat()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            
            if (_heartbeatThread != null && _heartbeatThread.IsAlive)
            {
                _heartbeatThread.Interrupt();
                _heartbeatThread.Join(TimeSpan.FromSeconds(5));
                _heartbeatThread = null;
            }
            
            Log.Debug($"LockManager: {Strings.Get("Log_Lock_HeartbeatStopped")}");
        }
        
        // ==================== DISPOSE ====================
        public void Dispose()
        {
            if (_config != null)
            {
                ReleaseLocks(_config);
                _config = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
