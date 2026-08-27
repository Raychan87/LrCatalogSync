using System.ComponentModel;
using System.Diagnostics;

using LrCatalogSync.Infrastructure;    // ← für Log, AppConfig, GlobalData
using LrCatalogSync.UI;                // ← für TrayManager

namespace LrCatalogSync.Core
{
    // Manager für alle Backup-Operationen (Check, Sync, Statistiken)
    public static class BackupManager
    {
        // ==================== ÖFFENTLICHE FUNKTIONEN ====================
        // Führt Backup-Sync durch (rclone bisync)
        // config: App-Konfiguration
        // remoteFullPath: Remote-Pfad
        public static bool SyncBackups(AppConfig config, string remoteFullPath)
        {
            try
            {
                // ========== LOG-EINTRAG: START ==========
                Log.Debug($"BackupManager: gestartet {config.BackupsLocalPath} -> {remoteFullPath}");

                // ========== LOG-DATEI VORBEREITEN ==========
                string tempLog = Path.Combine(GlobalData.BaseDir, "data", "logs", "rclone_backup_sync.log");
                string logsDir = Path.Combine(GlobalData.BaseDir, "data", "logs");
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);

                if (File.Exists(tempLog))
                    File.Delete(tempLog);

                // ========== RCLONE PROZESS STARTEN ==========
                // Starte rclone bisync (Synchronisation)
                var psi = new ProcessStartInfo
                {
                    FileName = config.RclonePath,
                    Arguments = $"--config \"{GlobalData.RcloneConfigPath}\" bisync \"{config.BackupsLocalPath}\" {remoteFullPath} --compare modtime,size --metadata --max-delete -1 --log-file \"{tempLog}\" --log-level {config.LogLevel} {GlobalConst.RCLONE_TIMEOUT_ARGS}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                // Starte rclone und warte bis Prozess beendet ist
                using (var p = Process.Start(psi))
                {
                    if (p == null)
                        return false;

                    p.WaitForExit(); // Warte bis Prozess beendet ist

                    // ========== FEHLERBEHANDLUNG FÜR BISYNC ==========
                    // Prüfe ob Fehler "cannot find prior Path1 or Path2 listings" auftrat
                    if (p.ExitCode != 0)
                    {
                        // Lese Logdatei für Fehleranalyse
                        var lines = ReadLogFileWithRetry(tempLog, 5, 200);
                        if (lines != null)
                        {
                            string logContent = string.Join("\n", lines);

                            // Ein abgebrochener rclone-Prozess kann ein verwaistes Lockfile hinterlassen.
                            if (logContent.Contains("prior lock file found", StringComparison.OrdinalIgnoreCase))
                            {
                                if (CleanupRcloneLockFile(logContent))
                                {
                                    Log.Debug("BackupManager: Verwaistes Lockfile gelöscht, starte Bisync erneut");

                                    var retryPsi = new ProcessStartInfo
                                    {
                                        FileName = config.RclonePath,
                                        Arguments = $"--config \"{GlobalData.RcloneConfigPath}\" bisync \"{config.BackupsLocalPath}\" {remoteFullPath} --compare modtime,size --metadata --max-delete -1 --log-file \"{tempLog}\" --log-level {config.LogLevel} {GlobalConst.RCLONE_TIMEOUT_ARGS}",
                                        UseShellExecute = false,
                                        RedirectStandardOutput = true,
                                        RedirectStandardError = true,
                                        CreateNoWindow = true
                                    };

                                    using (var retryProc = Process.Start(retryPsi))
                                    {
                                        if (retryProc == null)
                                            return false;

                                        retryProc.WaitForExit();
                                        if (retryProc.ExitCode == 0)
                                        {
                                            WriteRcloneStats(tempLog);
                                            Log.Debug("BackupManager: Bisync nach Lockfile-Bereinigung erfolgreich");
                                            return true;
                                        }

                                        Log.Error($"BackupManager: Bisync nach Lockfile-Bereinigung fehlgeschlagen (ExitCode: {retryProc.ExitCode})");
                                        return false;
                                    }
                                }

                                Log.Error("BackupManager: Das von rclone gemeldete Lockfile konnte nicht bereinigt werden");
                                return false;
                            }

                            // Prüfe auf spezifischen Fehler
                            if (logContent.Contains("cannot find prior Path1 or Path2 listings"))
                            {
                                Log.Debug("BackupManager: Bisync-Fehler erkannt, starte mit --resync neu");

                                // Erstelle neuen ProcessStartInfo mit --resync
                                var resyncPsi = new ProcessStartInfo
                                {
                                    FileName = config.RclonePath,
                                    Arguments = $"--config \"{GlobalData.RcloneConfigPath}\" bisync \"{config.BackupsLocalPath}\" {remoteFullPath} --compare modtime,size --metadata --max-delete -1 --log-file \"{tempLog}\" --log-level {config.LogLevel} {GlobalConst.RCLONE_TIMEOUT_ARGS} --resync",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true
                                };

                                // Führe resync aus
                                using (var resyncProc = Process.Start(resyncPsi))
                                {
                                    if (resyncProc == null)
                                        return false;

                                    resyncProc.WaitForExit();

                                    // Logge Ergebnis
                                    if (resyncProc.ExitCode == 0)
                                    {
                                        Log.Debug("BackupManager: Bisync mit --resync erfolgreich");
                                        return true;
                                    }

                                    Log.Error($"BackupManager: Bisync mit --resync fehlgeschlagen (ExitCode: {resyncProc.ExitCode})");
                                    return false;
                                }
                            }
                        }

                        Log.Error($"BackupManager: Bisync fehlgeschlagen (ExitCode: {p.ExitCode})");
                        return false;
                    }
                }

                // ========== LOG-STATISTIKEN AUSGEBEN ==========
                // Lese rclone Logdatei und gebe Statistiken aus (Copied, Deleted, etc.)
                WriteRcloneStats(tempLog);

                // ========== LOG-EINTRAG: ENDE ==========
                Log.Debug("BackupManager: abgeschlossen");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"BackupManager: {ex.Message}");
                return false;
            }
        }

        // ==================== HAUPTMETHODE: BACKUP-PROZESS ====================
        /// Führt kompletten Backup-Prozess aus: Validiert Konfiguration und startet SyncBackups().
        /// Aktualisiert Tray-Status.
        /// config: App-Konfiguration mit Pfaden und Einstellungen
        /// trayManager: TrayManager für Status-Updates (Syncing/Standby/Error)
        public static bool RunBackupProcess(AppConfig config, TrayManager trayManager)
        {
            try
            {
                // Zusammenstellung des Remote-Pfads (z.B. "synology:/Lightroom/Backups")
                string remoteFullPath = GlobalConst.REMOTE_NAME;
                if (!string.IsNullOrEmpty(config.BackupsRemotePath))
                    remoteFullPath += ":" + config.BackupsRemotePath;

                // Setze Tray auf "Syncing" und starte Sync
                trayManager.UpdateStatus("BSyncing");

                // Führe Sync durch und verwende das Ergebnis für den Erfolg/Fehler
                bool syncSucceeded = SyncBackups(config, remoteFullPath);

                if (syncSucceeded)
                {
                    trayManager.UpdateStatus("Standby");
                    return true;
                }else
                {
                    trayManager.UpdateStatus("Error");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"BackupManager: {ex.Message}");
                trayManager.UpdateStatus("Error");
                return false;
            }
        }

        // ==================== PRIVATE HILFSFUNKTIONEN ====================
        // Liest rclone Logdatei und gibt wichtige Statistiken aus (Copied, Deleted, etc.)
        // logFile: Pfad zur rclone Logdatei
        private static void WriteRcloneStats(string logFile)
        {
            try
            {
                // Prüfe ob Logdatei existiert
                if (string.IsNullOrEmpty(logFile) || !File.Exists(logFile)) 
                    return;

                // ========== LOG-DATEI LESEN ==========
                var lines = ReadLogFileWithRetry(logFile, 5, 200);
                if (lines == null || lines.Length == 0) 
                    return;

                // ========== RELEVANTE ZEILEN FILTERN UND LOGGEN ==========
                // Suche nach Statistik-Zeilen in rclone Logdatei
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    // Gebe nur wichtige Zeilen aus (Copied, Deleted, Transferred, Elapsed)
                    if (trimmed.Contains("Copied") || 
                        trimmed.Contains("Deleted") || 
                        (trimmed.Contains("Transferred:") && !trimmed.Contains("0 B / 0 B")) || 
                        trimmed.Contains("Elapsed time:"))
                    {
                        // Ausgabe ins Logfile mit rclone-Präfix
                        Log.Debug("BackupManager: - rclone: " + trimmed);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"BackupManager: - rclone: {ex.Message}");
            }
        }

        private static bool CleanupRcloneLockFile(string logContent)
        {
            const string lockFileMarker = "prior lock file found:";
            int markerIndex = logContent.IndexOf(lockFileMarker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
                return false;

            int pathStart = markerIndex + lockFileMarker.Length;
            int pathEnd = logContent.IndexOf(".lck", pathStart, StringComparison.OrdinalIgnoreCase);
            if (pathEnd < 0)
                return false;

            string lockFilePath = logContent[pathStart..(pathEnd + ".lck".Length)].Trim().Trim('"');
            string bisyncDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "rclone",
                "bisync");

            if (!Path.GetFullPath(lockFilePath).StartsWith(
                    Path.GetFullPath(bisyncDirectory) + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
            {
                Log.Error($"BackupManager: Unsicherer Lockfile-Pfad abgelehnt: {lockFilePath}");
                return false;
            }

            if (!File.Exists(lockFilePath))
                return false;

            try
            {
                File.Delete(lockFilePath);
                return !File.Exists(lockFilePath);
            }
            catch (IOException ex)
            {
                Log.Error($"BackupManager: Lockfile konnte nicht gelöscht werden: {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error($"BackupManager: Zugriff auf Lockfile verweigert: {ex.Message}");
                return false;
            }
        }

        /// Liest Datei mit Retry-Logik (falls Datei noch gesperrt ist)
        /// filePath: Pfad zur Datei
        /// maxRetries: Max. Anzahl Versuche
        /// delayMs: Wartezeit zwischen Versuchen in ms
        /// returns: Array von Zeilen oder leeres Array wenn Fehler
        private static string[] ReadLogFileWithRetry(string filePath, int maxRetries = 5, int delayMs = 200)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (!File.Exists(filePath))
                        return Array.Empty<string>();

                    // Versuche Datei zu lesen
                    return File.ReadAllLines(filePath);
                }
                catch (IOException)
                {
                    // Datei ist noch gesperrt, warte und versuche erneut
                    if (i < maxRetries - 1)
                        Thread.Sleep(delayMs);
                    else
                        return Array.Empty<string>(); // Nach max. Versuchen: leeres Array zurückgeben
                }
            }
            return Array.Empty<string>();
        }
    }
}
