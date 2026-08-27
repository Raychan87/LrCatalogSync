namespace LrCatalogSync
{
    public static class GlobalData
    {
        public static string BaseDir { get; private set; } = AppDomain.CurrentDomain.BaseDirectory;
        public static string LrCatSyncConfigPath { get; private set; } = Path.Combine(GlobalData.BaseDir, "data", "config", "LrCatSync.conf");
        public static string RcloneConfigPath { get; private set; } = Path.Combine(GlobalData.BaseDir, "data", "config", "rclone.conf");
    }

    public static class GlobalConst
    {
        public const string REMOTE_NAME = "LrCatalogSync";
        
        // Sync-Lock Timeout - wann ein Lock als "Veraltet" gilt (30 Minuten)
        public const int SYNC_LOCK_TIMEOUT_MIN = 30;
        
        // Heartbeat-Intervall für Lock-Aktualisierung (2,5 Minuten)
        public const int HEARTBEAT_INTERVAL_SEC = 150;
        
        // Rclone-Verbindungs-Timeout (Go-Duration-Format, z.B. "30s")
        public const string RCLONE_CONNECT_TIMEOUT = "20s";

        // Rclone-Retry-Anzahl (max. Wiederholungen bei Verbindungsfehlern)
        public const string RCLONE_RECONNECT = "2";

        // Kombinierte Timeout-/Retry-Parameter für alle rclone-Aufrufe
        // Verhindert, dass rclone nach Verbindungsabbruch endlos weiterläuft
        public const string RCLONE_TIMEOUT_ARGS = $"--contimeout {RCLONE_CONNECT_TIMEOUT} --low-level-retries {RCLONE_RECONNECT} --retries {RCLONE_RECONNECT}";
        
        // Lock-Dateinamen für Synchronisation
        public const string LOCK_FILE = "LrCatSync.lock";
    }    
}

