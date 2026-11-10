namespace LrCatalogSync.Infrastructure
{
    // Gespeicherte Ansicht des SizeViewers: Spaltenreihenfolge, Spaltenbreiten und Sortierung
    public sealed class SizeViewerConfig
    {
        public int[] ColumnOrder { get; set; } = Array.Empty<int>();     // Anzeigeposition je Spalte
        public int[] ColumnWidths { get; set; } = Array.Empty<int>();    // Breite je Spalte in Pixeln
        public int SortColumn { get; set; } = GlobalConst.SIZEVIEWER_DEFAULT_SORT_COLUMN;
        public bool SortDescending { get; set; } = true;

        public static SizeViewerConfig Load(string path)
        {
            var config = new SizeViewerConfig();
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
                    case "ColumnOrder":
                        config.ColumnOrder = ParseNumbers(value);
                        break;
                    case "ColumnWidths":
                        config.ColumnWidths = ParseNumbers(value);
                        break;
                    case "SortColumn":
                        if (int.TryParse(value, out int sortColumn))
                            config.SortColumn = sortColumn;
                        break;
                    case "SortDescending":
                        config.SortDescending = !bool.TryParse(value, out bool sortDescending) || sortDescending;
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
                "ColumnOrder=" + string.Join('|', ColumnOrder),
                "ColumnWidths=" + string.Join('|', ColumnWidths),
                "SortColumn=" + SortColumn,
                "SortDescending=" + SortDescending
            };
            File.WriteAllLines(path, lines);
        }

        // Ungültige Einträge verwerfen die ganze Liste, dann gelten die Standardwerte
        private static int[] ParseNumbers(string value)
        {
            string[] parts = value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var numbers = new int[parts.Length];
            for (int index = 0; index < parts.Length; index++)
            {
                if (!int.TryParse(parts[index], out numbers[index]))
                    return Array.Empty<int>();
            }

            return numbers;
        }
    }
}
