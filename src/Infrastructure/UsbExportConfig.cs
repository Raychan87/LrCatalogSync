using LrCatalogSync.Core;

namespace LrCatalogSync.Infrastructure
{
    public sealed class UsbExportConfig
    {
        public string ExternalRoot { get; set; } = string.Empty;
        public string TargetPath { get; set; } = "LrBackup";
        public UsbExportDirection Direction { get; set; } = UsbExportDirection.ComputerToExternal;
        public List<string> Sources { get; set; } = new();
        public string ExcludePatterns { get; set; } = "Thumbs.db;*.lrcat.lock;*.lrcat-shm;*.lrcat-wal";
        public bool UseHashComparison { get; set; }
        public bool TransferMetadata { get; set; }

        public static UsbExportConfig Load(string path)
        {
            var config = new UsbExportConfig();
            if (!File.Exists(path))
                return config;

            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || !line.Contains('='))
                    continue;

                string[] parts = line.Split('=', 2);
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                switch (key)
                {
                    case "ExternalRoot":
                        config.ExternalRoot = value;
                        break;
                    case "TargetPath":
                        config.TargetPath = value;
                        break;
                    case "Direction":
                        if (Enum.TryParse(value, out UsbExportDirection direction))
                            config.Direction = direction;
                        break;
                    case "Sources":
                        config.Sources = value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                        break;
                    case "ExcludePatterns":
                        config.ExcludePatterns = value;
                        break;
                    case "UseHashComparison":
                        config.UseHashComparison = bool.TryParse(value, out bool useHashComparison) && useHashComparison;
                        break;
                    case "TransferMetadata":
                        config.TransferMetadata = bool.TryParse(value, out bool transferMetadata) && transferMetadata;
                        break;
                }
            }

            return config;
        }

        public void Save(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            string[] lines =
            {
                "ExternalRoot=" + ExternalRoot,
                "TargetPath=" + TargetPath,
                "Direction=" + Direction,
                "Sources=" + string.Join('|', Sources),
                "ExcludePatterns=" + ExcludePatterns,
                "UseHashComparison=" + UseHashComparison,
                "TransferMetadata=" + TransferMetadata
            };
            File.WriteAllLines(path, lines);
        }
    }
}
