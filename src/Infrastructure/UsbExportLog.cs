namespace LrCatalogSync.Infrastructure
{
    public static class UsbExportLog
    {
        private const int MaxEntries = 500;
        private static readonly object SyncRoot = new();
        private static readonly Queue<string> Entries = new();
        private static string logFilePath = string.Empty;

        public static void Initialize(string baseDir)
        {
            string logsDirectory = Path.Combine(baseDir, "data", "logs");
            Directory.CreateDirectory(logsDirectory);
            logFilePath = Path.Combine(logsDirectory, "USBExport.log");
            lock (SyncRoot)
            {
                Entries.Clear();
                if (File.Exists(logFilePath))
                {
                    foreach (string entry in File.ReadAllLines(logFilePath).TakeLast(MaxEntries))
                        Entries.Enqueue(entry);
                }
            }
        }

        public static void BeginAction(string action)
        {
            Clear();
            Add($"Aktion gestartet: {action}");
        }

        public static IReadOnlyList<string> Add(string message)
        {
            lock (SyncRoot)
            {
                EnsureInitialized();
                string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
                if (Entries.Count >= MaxEntries)
                    Entries.Dequeue();
                Entries.Enqueue(entry);
                File.AppendAllText(logFilePath, entry + Environment.NewLine);
                return Entries.ToArray();
            }
        }

        public static void Clear()
        {
            lock (SyncRoot)
            {
                EnsureInitialized();
                Entries.Clear();
                File.WriteAllText(logFilePath, string.Empty);
            }
        }

        public static IReadOnlyList<string> GetLastEntries()
        {
            lock (SyncRoot)
            {
                EnsureInitialized();
                return Entries.ToArray();
            }
        }

        private static void EnsureInitialized()
        {
            if (!string.IsNullOrWhiteSpace(logFilePath))
                return;

            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs");
            Directory.CreateDirectory(logsDirectory);
            logFilePath = Path.Combine(logsDirectory, "USBExport.log");
        }
    }
}
