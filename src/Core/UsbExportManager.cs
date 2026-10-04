using LrCatalogSync.Infrastructure;
using System.Diagnostics;
using System.Globalization;

namespace LrCatalogSync.Core
{
    public enum UsbExportDirection
    {
        ComputerToExternal,
        ExternalToComputer
    }

    public sealed class UsbExportRequest
    {
        public string ExternalRoot { get; init; } = string.Empty;
        public string TargetPath { get; init; } = string.Empty;
        public UsbExportDirection Direction { get; init; }
        public IReadOnlyList<string> Sources { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> UserExcludePatterns { get; init; } = Array.Empty<string>();
        public bool UseHashComparison { get; init; }
        public bool TransferMetadata { get; init; }
    }

    public sealed record UsbExportValidationResult(bool IsValid, string Message, IReadOnlyList<string> LockFiles)
    {
        public static UsbExportValidationResult Valid() => new(true, "Die USB-Export-Konfiguration ist gültig.", Array.Empty<string>());
    }

    public sealed record UsbExportResult(bool Succeeded, bool Cancelled, string Message);

    public readonly record struct UsbExportProgress(int Percent, string Speed);
    public readonly record struct UsbExportSectionProgress(int Current, int Total);

    public static class UsbExportManager
    {
        private static readonly string[] LightroomLockSuffixes =
        {
            ".lrcat.lock",
            ".lrcat-shm",
            ".lrcat-wal"
        };

        // Programmspezifisch: werden immer ausgeschlossen und dem Nutzer nicht angezeigt
        private static readonly string[] ProgramExcludePatterns =
        {
            "*.lrcat.lock",
            "*.lrcat-shm",
            "*.lrcat-wal",
            "*.lock",
            "Thumbs.db"
        };

        // Windows-Systemordner im Wurzelverzeichnis: weder kopieren noch löschen
        private static readonly string[] SystemExcludeNames =
        {
            "System Volume Information",
            "$RECYCLE.BIN",
            "Recovery"
        };

        public static bool IsSystemExcludeName(string name) =>
            SystemExcludeNames.Any(systemName => systemName.Equals(name, StringComparison.OrdinalIgnoreCase));

        // Entfernt Einträge, die intern ohnehin gelten, aus der Nutzerliste
        public static string RemoveBuiltInPatterns(string patterns)
        {
            return string.Join(';', patterns
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(pattern => !ProgramExcludePatterns.Contains(pattern, StringComparer.OrdinalIgnoreCase)
                    && !IsSystemExcludeName(pattern.Trim('/', '\\'))));
        }

        public static UsbExportValidationResult ValidateRequest(UsbExportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ExternalRoot))
                return Invalid("Kein externes Laufwerk ausgewählt.");

            string externalRoot;
            try
            {
                externalRoot = NormalizeDirectory(request.ExternalRoot);
            }
            catch (Exception)
            {
                return Invalid("Das externe Laufwerk ist ungültig.");
            }

            if (!Directory.Exists(externalRoot))
                return Invalid("Das ausgewählte externe Laufwerk ist nicht erreichbar.");

            if (request.Sources.Count == 0)
                return Invalid("Mindestens eine Datenquelle muss ausgewählt werden.");

            foreach (string source in request.Sources)
            {
                if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
                    return Invalid($"Die Datenquelle ist nicht erreichbar: {source}");

                if (request.Direction == UsbExportDirection.ExternalToComputer)
                {
                    string normalizedSource = NormalizeDirectory(source);
                    if (!IsWithinDirectory(normalizedSource, externalRoot) &&
                        !string.Equals(normalizedSource, externalRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        return Invalid("Beim Download müssen alle Datenquellen innerhalb des ausgewählten externen Laufwerks liegen.");
                    }
                }
            }

            string targetPath;
            try
            {
                targetPath = request.Direction == UsbExportDirection.ComputerToExternal
                    ? ResolveExternalTarget(externalRoot, request.TargetPath)
                    : NormalizeDirectory(request.TargetPath);
            }
            catch (ArgumentException ex)
            {
                return Invalid(ex.Message);
            }

            if (request.Direction == UsbExportDirection.ExternalToComputer && !Directory.Exists(targetPath))
                return Invalid($"Das lokale Zielverzeichnis ist nicht erreichbar: {targetPath}");

            IReadOnlyList<string> lockFiles = FindLightroomLocks(request.Sources);
            if (lockFiles.Count > 0)
            {
                return new UsbExportValidationResult(
                    false,
                    "Ein Lightroom-Katalog ist geöffnet. Der Vorgang wurde aus Sicherheitsgründen blockiert.",
                    lockFiles);
            }

            return UsbExportValidationResult.Valid();
        }

        public static IReadOnlyList<string> FindLightroomLocks(IEnumerable<string> sources)
        {
            var lockFiles = new List<string>();
            foreach (string source in sources)
            {
                if (!Directory.Exists(source))
                    continue;

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        if (IsLightroomLock(file))
                            lockFiles.Add(file);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Geschützte Ordner wie "System Volume Information" werden übersprungen.
                }
                catch (DirectoryNotFoundException)
                {
                    // Das Laufwerk oder ein Unterordner wurde während der Prüfung entfernt.
                }
                catch (IOException)
                {
                    // Nicht lesbare Verzeichnisse dürfen die Lock-Prüfung nicht abbrechen.
                }
            }
            return lockFiles;
        }

        public static bool IsLightroomLock(string filePath)
        {
            return LightroomLockSuffixes.Any(suffix => filePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<UsbExportResult> ExportAsync(
            AppConfig appConfig,
            UsbExportRequest request,
            IProgress<string>? progress,
            IProgress<UsbExportProgress>? transferProgress,
            IProgress<UsbExportSectionProgress>? sectionProgress,
            CancellationToken cancellationToken)
        {
            UsbExportResult? configResult = EnsureUsbExportRcloneConfig();
            if (configResult != null)
                return configResult;

            UsbExportResult? prerequisiteResult = ValidatePrerequisites(appConfig);
            if (prerequisiteResult != null)
                return prerequisiteResult;

            UsbExportValidationResult validation = ValidateRequest(request);
            if (!validation.IsValid)
                return new UsbExportResult(false, false, validation.Message);

            string externalRoot = NormalizeDirectory(request.ExternalRoot);
            string targetRoot = request.Direction == UsbExportDirection.ComputerToExternal
                ? ResolveExternalTarget(externalRoot, request.TargetPath)
                : NormalizeDirectory(request.TargetPath);

            Directory.CreateDirectory(targetRoot);
            progress?.Report($"Zielordner: {targetRoot}");
            var usedDestinationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int totalSections = request.Sources.Count * (request.UseHashComparison ? 2 : 1);

            for (int sourceIndex = 0; sourceIndex < request.Sources.Count; sourceIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string source = request.Sources[sourceIndex];
                transferProgress?.Report(new UsbExportProgress(0, string.Empty));
                sectionProgress?.Report(new UsbExportSectionProgress(sourceIndex + 1, totalSections));
                string destination = CreateSourceDestination(targetRoot, source, usedDestinationNames);
                Directory.CreateDirectory(destination);
                progress?.Report($"Quelle: {source}");
                progress?.Report($"Ziel: {destination}");

                UsbExportResult result = await RunRcloneSyncAsync(
                    appConfig.RclonePath,
                    source,
                    destination,
                    request.UserExcludePatterns,
                    request.UseHashComparison,
                    request.TransferMetadata,
                    progress,
                    transferProgress,
                    cancellationToken);

                if (!result.Succeeded)
                    return result;

                progress?.Report(result.Message);
            }

            if (request.UseHashComparison)
            {
                progress?.Report("Vollständiger Hash-Nachcheck läuft...");
                UsbExportResult verification = await CompareAsync(
                    appConfig,
                    request,
                    progress,
                    transferProgress,
                    sectionProgress,
                    cancellationToken,
                    request.Sources.Count,
                    totalSections);
                if (!verification.Succeeded)
                {
                    return new UsbExportResult(
                        false,
                        verification.Cancelled,
                        $"Übertragung abgeschlossen, Hash-Nachcheck fehlgeschlagen: {verification.Message}");
                }
            }

            return new UsbExportResult(true, false, "Übertragung erfolgreich abgeschlossen.");
        }

        public static async Task<UsbExportResult> CompareAsync(
            AppConfig appConfig,
            UsbExportRequest request,
            IProgress<string>? progress,
            IProgress<UsbExportProgress>? transferProgress,
            IProgress<UsbExportSectionProgress>? sectionProgress,
            CancellationToken cancellationToken,
            int sectionOffset = 0,
            int? totalSectionsOverride = null)
        {
            UsbExportResult? configResult = EnsureUsbExportRcloneConfig();
            if (configResult != null)
                return configResult;

            UsbExportResult? prerequisiteResult = ValidatePrerequisites(appConfig);
            if (prerequisiteResult != null)
                return prerequisiteResult;

            UsbExportValidationResult validation = ValidateRequest(request);
            if (!validation.IsValid)
                return new UsbExportResult(false, false, validation.Message);

            string externalRoot = NormalizeDirectory(request.ExternalRoot);
            string targetRoot = request.Direction == UsbExportDirection.ComputerToExternal
                ? ResolveExternalTarget(externalRoot, request.TargetPath)
                : NormalizeDirectory(request.TargetPath);
            bool allEqual = true;
            var usedDestinationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int totalSections = totalSectionsOverride ?? request.Sources.Count;

            for (int sourceIndex = 0; sourceIndex < request.Sources.Count; sourceIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string source = request.Sources[sourceIndex];
                transferProgress?.Report(new UsbExportProgress(0, string.Empty));
                sectionProgress?.Report(new UsbExportSectionProgress(sectionOffset + sourceIndex + 1, totalSections));
                string destination = CreateSourceDestination(targetRoot, source, usedDestinationNames);
                progress?.Report($"Quelle: {source} -> Ziel: {destination}");
                int exitCode = await RunRcloneCheckAsync(
                    appConfig.RclonePath,
                    source,
                    destination,
                    request.UserExcludePatterns,
                    request.UseHashComparison,
                    progress,
                    transferProgress,
                    cancellationToken);
                if (exitCode != 0)
                    allEqual = false;
            }

            return allEqual
                ? new UsbExportResult(true, false, "Checksummen-Vergleich erfolgreich abgeschlossen.")
                : new UsbExportResult(false, false, "Checksummen-Vergleich fehlerhaft abgeschlossen!");
        }

        private static UsbExportResult? ValidatePrerequisites(AppConfig appConfig)
        {
            if (string.IsNullOrWhiteSpace(appConfig.RclonePath) || !File.Exists(appConfig.RclonePath))
                return new UsbExportResult(false, false, $"rclone wurde nicht gefunden: {appConfig.RclonePath}");

            return null;
        }

        private static UsbExportResult? EnsureUsbExportRcloneConfig()
        {
            try
            {
                string? directory = Path.GetDirectoryName(GlobalData.UsbExportRcloneConfigPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                if (!File.Exists(GlobalData.UsbExportRcloneConfigPath))
                    File.WriteAllText(GlobalData.UsbExportRcloneConfigPath, string.Empty);

                return null;
            }
            catch (Exception ex)
            {
                return new UsbExportResult(false, false, $"Die USB-Export-rclone-Konfiguration konnte nicht angelegt werden: {ex.Message}");
            }
        }

        private static string CreateSourceDestination(
            string targetRoot,
            string source,
            ISet<string> usedDestinationNames)
        {
            var sourceDirectory = new DirectoryInfo(source);
            string sourceName = sourceDirectory.Name;
            string candidate = sourceName;

            if (usedDestinationNames.Contains(candidate))
            {
                string parentName = sourceDirectory.Parent?.Name ?? "Quelle";
                candidate = $"{parentName}_{sourceName}";
            }

            if (usedDestinationNames.Contains(candidate))
            {
                string driveName = (Path.GetPathRoot(source) ?? string.Empty)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, ':')
                    .Replace(":", string.Empty);
                if (string.IsNullOrWhiteSpace(driveName))
                    driveName = "Quelle";
                string parentName = sourceDirectory.Parent?.Name ?? "Quelle";
                candidate = $"{driveName}_{parentName}_{sourceName}";
            }

            int suffix = 2;
            string uniqueCandidate = candidate;
            while (!usedDestinationNames.Add(uniqueCandidate))
                uniqueCandidate = $"{candidate}_{suffix++}";

            return Path.Combine(targetRoot, uniqueCandidate);
        }

        public static async Task<UsbExportResult> ClearExternalTargetAsync(
            UsbExportRequest request,
            IProgress<string>? progress,
            CancellationToken cancellationToken)
        {
            if (request.Direction != UsbExportDirection.ComputerToExternal)
                return new UsbExportResult(false, false, "Löschen ist nur für ein externes Ziel verfügbar.");

            string externalRoot = NormalizeDirectory(request.ExternalRoot);
            if (!Directory.Exists(externalRoot))
                return new UsbExportResult(false, false, "Das ausgewählte externe Laufwerk ist nicht erreichbar.");

            string targetPath;
            try
            {
                targetPath = ResolveExternalTarget(externalRoot, request.TargetPath, allowRoot: true);
            }
            catch (ArgumentException ex)
            {
                return new UsbExportResult(false, false, ex.Message);
            }
            if (!Directory.Exists(targetPath))
                return new UsbExportResult(true, false, "Der Zielordner existiert nicht und ist bereits leer.");

            bool clearingDriveRoot = string.Equals(targetPath, externalRoot, StringComparison.OrdinalIgnoreCase);
            int deletedCount = 0;
            int errorCount = 0;
            foreach (string entry in Directory.EnumerateFileSystemEntries(targetPath))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string entryName = Path.GetFileName(entry.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (clearingDriveRoot && IsSystemExcludeName(entryName))
                    continue;

                try
                {
                    FileAttributes attributes = File.GetAttributes(entry);
                    if ((attributes & FileAttributes.ReparsePoint) != 0 || File.Exists(entry))
                        File.Delete(entry);
                    else
                        Directory.Delete(entry, recursive: true);

                    deletedCount++;
                    progress?.Report($"Gelöscht: {entry}");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    progress?.Report($"Fehler beim Löschen: {entry} - {ex.Message}");
                }
            }

            return errorCount == 0
                ? new UsbExportResult(true, false, $"Der ausgewählte externe Zielordner wurde geleert. Gelöscht: {deletedCount} Einträge.")
                : new UsbExportResult(false, false, $"Löschen mit Fehlern abgeschlossen. Gelöscht: {deletedCount}, Fehler: {errorCount}.");
        }

        private static async Task<UsbExportResult> RunRcloneSyncAsync(
            string rclonePath,
            string source,
            string destination,
            IReadOnlyList<string> excludePatterns,
            bool useHashComparison,
            bool transferMetadata,
            IProgress<string>? progress,
            IProgress<UsbExportProgress>? transferProgress,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(rclonePath))
                return new UsbExportResult(false, false, $"rclone wurde nicht gefunden: {rclonePath}");

            var startInfo = new ProcessStartInfo
            {
                FileName = rclonePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("--config");
            startInfo.ArgumentList.Add(GlobalData.UsbExportRcloneConfigPath);
            startInfo.ArgumentList.Add("sync");
            startInfo.ArgumentList.Add(source);
            startInfo.ArgumentList.Add(destination);
            startInfo.ArgumentList.Add("--stats-one-line");
            startInfo.ArgumentList.Add("--stats");
            startInfo.ArgumentList.Add("1s");
            startInfo.ArgumentList.Add("--stats-log-level");
            startInfo.ArgumentList.Add("NOTICE");
            if (useHashComparison)
                startInfo.ArgumentList.Add("--checksum");
            if (transferMetadata)
                startInfo.ArgumentList.Add("--metadata");
            foreach (string pattern in GetExcludePatterns(excludePatterns))
            {
                startInfo.ArgumentList.Add("--exclude");
                startInfo.ArgumentList.Add(pattern);
            }

            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            try
            {
                if (!process.Start())
                    return new UsbExportResult(false, false, "rclone konnte nicht gestartet werden.");

                RcloneProcessManager.Register(process);

                string? lastRcloneError = null;
                bool diskFull = false;

                void ReportOutput(string line)
                {
                    if (diskFull)
                        return;

                    var rcloneError = System.Text.RegularExpressions.Regex.Match(line, @"\bERROR\s*:\s*(.*)$");
                    if (rcloneError.Success)
                    {
                        lastRcloneError = rcloneError.Groups[1].Value.Trim();
                        if (IsDiskFullMessage(line))
                        {
                            diskFull = true;
                            TryKillProcess(process);
                            return;
                        }
                    }

                    if (TryParseRcloneProgress(line, out UsbExportProgress parsedProgress))
                        transferProgress?.Report(parsedProgress);

                    if (TryFormatRcloneOutput(line, out string formattedLine))
                        progress?.Report(formattedLine);
                }
                Task outputTask = ForwardOutputAsync(process.StandardOutput, ReportOutput, cancellationToken);
                Task errorTask = ForwardOutputAsync(process.StandardError, ReportOutput, cancellationToken);
                await process.WaitForExitAsync(cancellationToken);
                await Task.WhenAll(outputTask, errorTask);

                if (process.ExitCode == 0)
                    return new UsbExportResult(true, false, "Quelle erfolgreich übertragen.");

                if (diskFull)
                {
                    string missingInfo = await GetMissingSpaceInfoAsync(rclonePath, source, destination, excludePatterns);
                    return new UsbExportResult(false, false, $"Abbruch: Es ist nicht genügend Speicherplatz vorhanden.{missingInfo}");
                }

                return new UsbExportResult(false, false, string.IsNullOrEmpty(lastRcloneError)
                    ? $"rclone wurde mit Exitcode {process.ExitCode} beendet."
                    : $"rclone wurde mit Exitcode {process.ExitCode} beendet. Letzter Fehler: {lastRcloneError}");
            }
            catch (OperationCanceledException)
            {
                TryKillProcess(process);
                return new UsbExportResult(false, true, "Übertragung abgebrochen.");
            }
            catch (Exception ex)
            {
                TryKillProcess(process);
                return new UsbExportResult(false, false, $"Fehler beim Kopieren: {ex.Message}");
            }
            finally
            {
                RcloneProcessManager.Unregister(process);
            }
        }

        // Fehlmenge = (Quellgröße - bereits vorhandene Zielgröße) - freier Zielspeicher
        private static async Task<string> GetMissingSpaceInfoAsync(string rclonePath, string source, string destination, IReadOnlyList<string> excludePatterns)
        {
            try
            {
                string? root = Path.GetPathRoot(Path.GetFullPath(destination));
                if (string.IsNullOrEmpty(root))
                    return string.Empty;

                long free = new DriveInfo(root).AvailableFreeSpace;
                long sourceBytes = await GetRcloneSizeAsync(rclonePath, source, excludePatterns, "bytes");
                long destinationBytes = await GetRcloneSizeAsync(rclonePath, destination, excludePatterns, "bytes");
                long missing = sourceBytes - destinationBytes - free;
                if (missing <= 0)
                    return string.Empty;

                return $" Fehlt: {(missing / 1024d / 1024d / 1024d).ToString("0.00", CultureInfo.CurrentCulture)} GB";
            }
            catch
            {
                return string.Empty;
            }
        }

        private static async Task<long> GetRcloneSizeAsync(string rclonePath, string path, IReadOnlyList<string> excludePatterns, string property)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = rclonePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("--config");
            startInfo.ArgumentList.Add(GlobalData.UsbExportRcloneConfigPath);
            startInfo.ArgumentList.Add("size");
            startInfo.ArgumentList.Add(path);
            startInfo.ArgumentList.Add("--json");
            foreach (string pattern in GetExcludePatterns(excludePatterns))
            {
                startInfo.ArgumentList.Add("--exclude");
                startInfo.ArgumentList.Add(pattern);
            }

            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("rclone size konnte nicht gestartet werden.");
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            string output = await outputTask;
            await errorTask;

            if (process.ExitCode != 0)
                return 0;

            using var json = System.Text.Json.JsonDocument.Parse(output);
            return json.RootElement.GetProperty(property).GetInt64();
        }

        private static bool IsDiskFullMessage(string line) =>
            line.Contains("nicht genug Speicherplatz", StringComparison.OrdinalIgnoreCase)
            || line.Contains("not enough space on the disk", StringComparison.OrdinalIgnoreCase)
            || line.Contains("no space left on device", StringComparison.OrdinalIgnoreCase)
            || line.Contains("disk is full", StringComparison.OrdinalIgnoreCase)
            || line.Contains("ERROR_DISK_FULL", StringComparison.OrdinalIgnoreCase)
            || line.Contains("ERROR_HANDLE_DISK_FULL", StringComparison.OrdinalIgnoreCase);

        private static bool TryFormatRcloneOutput(string line, out string formattedLine)
        {
            var differencesMatch = System.Text.RegularExpressions.Regex.Match(line, @":\s*(\d+)\s+differences found\b");
            if (differencesMatch.Success)
            {
                formattedLine = $"{differencesMatch.Groups[1].Value} unterschiedliche Daten";
                return true;
            }

            var matchingFilesMatch = System.Text.RegularExpressions.Regex.Match(line, @":\s*(\d+)\s+matching files\b");
            if (matchingFilesMatch.Success)
            {
                formattedLine = $"{matchingFilesMatch.Groups[1].Value} identische Dateien";
                return true;
            }

            var errorMatch = System.Text.RegularExpressions.Regex.Match(line, @"\bERROR\s*:\s*(.*)$");
            if (errorMatch.Success)
            {
                formattedLine = $"rclone-Fehler: {errorMatch.Groups[1].Value}";
                return true;
            }

            formattedLine = string.Empty;
            return false;
        }

        private static bool TryParseRcloneProgress(string line, out UsbExportProgress progress)
        {
            var checksMatch = System.Text.RegularExpressions.Regex.Match(line, @"Checks:\s*(\d+)\s*/\s*(\d+)(?:,\s*(\d{1,3})%)?");
            if (checksMatch.Success
                && int.TryParse(checksMatch.Groups[2].Value, out int totalChecks)
                && totalChecks > 0
                && int.TryParse(checksMatch.Groups[1].Value, out int completedChecks))
            {
                int checkPercent = checksMatch.Groups[3].Success && int.TryParse(checksMatch.Groups[3].Value, out int reportedPercent)
                    ? reportedPercent
                    : (int)(completedChecks * 100d / totalChecks);
                progress = new UsbExportProgress(Math.Clamp(checkPercent, 0, 100), string.Empty);
                return true;
            }

            var percentMatch = System.Text.RegularExpressions.Regex.Match(line, @"/\s*[^,]+,\s*(\d{1,3})%");
            if (percentMatch.Success && int.TryParse(percentMatch.Groups[1].Value, out int percent))
            {
                var speedMatch = System.Text.RegularExpressions.Regex.Match(line, @",\s*([\d.]+)\s*([A-Za-z]+/s)(?:,|$)");
                string speed = string.Empty;
                if (speedMatch.Success
                    && decimal.TryParse(speedMatch.Groups[1].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal transferRate))
                {
                    speed = $"{transferRate.ToString("0.0", CultureInfo.InvariantCulture)} {speedMatch.Groups[2].Value}";
                }
                progress = new UsbExportProgress(Math.Clamp(percent, 0, 100), speed);
                return true;
            }

            var checkedFilesMatch = System.Text.RegularExpressions.Regex.Match(line, @"\(chk#(\d+)\s*/\s*(\d+)\)");
            if (checkedFilesMatch.Success
                && int.TryParse(checkedFilesMatch.Groups[1].Value, out int completedFiles)
                && int.TryParse(checkedFilesMatch.Groups[2].Value, out int totalFiles)
                && totalFiles > 0)
            {
                int checkPercent = (int)(completedFiles * 100d / totalFiles);
                progress = new UsbExportProgress(Math.Clamp(checkPercent, 0, 100), string.Empty);
                return true;
            }

            progress = default;
            return false;
        }

        private static async Task<int> RunRcloneCheckAsync(
            string rclonePath,
            string source,
            string destination,
            IReadOnlyList<string> excludePatterns,
            bool useHashComparison,
            IProgress<string>? progress,
            IProgress<UsbExportProgress>? checkProgress,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(rclonePath))
                return -1;

            var startInfo = new ProcessStartInfo
            {
                FileName = rclonePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("--config");
            startInfo.ArgumentList.Add(GlobalData.UsbExportRcloneConfigPath);
            startInfo.ArgumentList.Add("check");
            startInfo.ArgumentList.Add(source);
            startInfo.ArgumentList.Add(destination);
            startInfo.ArgumentList.Add("--stats");
            startInfo.ArgumentList.Add("1s");
            startInfo.ArgumentList.Add("--stats-log-level");
            startInfo.ArgumentList.Add("NOTICE");
            string combinedFile = Path.Combine(Path.GetTempPath(), $"LrCatSync_check_{Guid.NewGuid():N}.txt");
            startInfo.ArgumentList.Add("--combined");
            startInfo.ArgumentList.Add(combinedFile);
            if (useHashComparison)
                startInfo.ArgumentList.Add("--checksum");
            foreach (string pattern in GetExcludePatterns(excludePatterns))
            {
                startInfo.ArgumentList.Add("--exclude");
                startInfo.ArgumentList.Add(pattern);
            }

            using var process = new Process { StartInfo = startInfo };
            try
            {
                if (!process.Start())
                    return -1;

                RcloneProcessManager.Register(process);
                Stopwatch checkStopwatch = Stopwatch.StartNew();

                // rclone kennt die Gesamtzahl erst nach dem Auflisten; daher vorab die Quell-Dateianzahl nutzen
                long totalFiles = 0;
                try { totalFiles = await GetRcloneSizeAsync(rclonePath, source, excludePatterns, "count"); }
                catch (Exception ex) when (ex is not OperationCanceledException) { }

                void ReportOutput(string line)
                {
                    if (line.Contains("differences found", StringComparison.OrdinalIgnoreCase))
                        return;

                    var checkError = System.Text.RegularExpressions.Regex.Match(line, @"\bERROR\s*:\s*(.*)$");
                    if (checkError.Success)
                    {
                        string message = checkError.Groups[1].Value;
                        if (message.Contains(": file not in ", StringComparison.OrdinalIgnoreCase)
                            || message.Contains(" differ", StringComparison.OrdinalIgnoreCase))
                            return;
                    }

                    var checksMatch = System.Text.RegularExpressions.Regex.Match(line, @"Checks:\s*(\d+)\s*/\s*(\d+)");
                    if (checksMatch.Success
                        && int.TryParse(checksMatch.Groups[1].Value, out int completedChecks)
                        && long.TryParse(checksMatch.Groups[2].Value, out long reportedTotal))
                    {
                        long total = Math.Max(totalFiles, reportedTotal);
                        int percent = total > 0 ? (int)Math.Min(100, completedChecks * 100L / total) : 0;
                        double checksPerSecond = completedChecks / Math.Max(checkStopwatch.Elapsed.TotalSeconds, 0.1);
                        checkProgress?.Report(new UsbExportProgress(percent, $"{checksPerSecond:0} Chk/s"));
                    }

                    if (line.Equals("Checking:", StringComparison.OrdinalIgnoreCase)
                        || line.EndsWith(": checking", StringComparison.OrdinalIgnoreCase))
                        return;

                    if (TryFormatRcloneOutput(line, out string formattedLine))
                        progress?.Report(formattedLine);
                }

                Task outputTask = ForwardOutputAsync(process.StandardOutput, ReportOutput, cancellationToken);
                Task errorTask = ForwardOutputAsync(process.StandardError, ReportOutput, cancellationToken);
                await process.WaitForExitAsync(cancellationToken);
                await Task.WhenAll(outputTask, errorTask);

                var missingPaths = new List<string>();
                var differingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var errorPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(combinedFile))
                {
                    foreach (string combinedLine in File.ReadLines(combinedFile))
                    {
                        if (combinedLine.Length < 3)
                            continue;

                        // rclone --combined: + fehlt im Ziel, * unterschiedlich, ! Fehler; - (nur im Ziel) wird ignoriert
                        string path = combinedLine[2..];
                        switch (combinedLine[0])
                        {
                            case '+': missingPaths.Add(path); break;
                            case '*': differingPaths.Add(path); break;
                            case '!': errorPaths.Add(path); break;
                        }
                    }
                }

                // Dateien, die auch als unterschiedlich/fehlerhaft gemeldet werden, nicht doppelt als fehlend zählen
                int missingOnDestination = missingPaths.Count(p => !differingPaths.Contains(p) && !errorPaths.Contains(p));
                int differing = differingPaths.Count;
                int readErrors = errorPaths.Count;

                if (missingOnDestination + differing + readErrors == 0)
                {
                    progress?.Report("Keine Unterschiede gefunden");
                    return 0;
                }

                if (missingOnDestination > 0)
                    progress?.Report($"{missingOnDestination} fehlende Dateien");
                if (differing > 0)
                    progress?.Report($"{differing} unterschiedliche Dateien");
                if (readErrors > 0)
                    progress?.Report($"{readErrors} Dateien konnten nicht gelesen werden");
                progress?.Report("Unterschiede gefunden!");

                return process.ExitCode == 0 ? 1 : process.ExitCode;
            }
            catch (OperationCanceledException)
            {
                TryKillProcess(process);
                throw;
            }
            catch
            {
                TryKillProcess(process);
                return -1;
            }
            finally
            {
                RcloneProcessManager.Unregister(process);
                try { File.Delete(combinedFile); } catch { }
            }
        }

        private static async Task ForwardOutputAsync(
            StreamReader reader,
            Action<string> report,
            CancellationToken cancellationToken)
        {
            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    report(line.Trim());
            }
        }

        private static void TryKillProcess(Process process)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static string ResolveExternalTarget(string externalRoot, string targetPath, bool allowRoot = true)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
                throw new ArgumentException("Der externe Zielordner muss angegeben werden.");

            string normalizedTarget = Path.IsPathRooted(targetPath)
                ? targetPath.Trim()
                : Path.Combine(externalRoot, targetPath.Trim().TrimStart('\\', '/'));
            if (normalizedTarget.Length == 0 || normalizedTarget == ".")
                throw new ArgumentException("Das Laufwerksstammverzeichnis darf nicht als Zielordner verwendet werden.");

            string fullPath = NormalizeDirectory(normalizedTarget);
            bool isExternalRoot = string.Equals(fullPath, externalRoot, StringComparison.OrdinalIgnoreCase);
            if (isExternalRoot && !allowRoot)
                throw new ArgumentException("Das Laufwerksstammverzeichnis darf nicht als Zielordner verwendet werden.");
            if (!isExternalRoot && !IsWithinDirectory(fullPath, externalRoot))
                throw new ArgumentException("Der externe Zielordner muss innerhalb des ausgewählten Laufwerks liegen.");

            return fullPath;
        }

        private static IEnumerable<string> GetExcludePatterns(IEnumerable<string> userPatterns)
        {
            foreach (string pattern in ProgramExcludePatterns)
                yield return pattern;

            foreach (string systemName in SystemExcludeNames)
                yield return $"/{systemName}/**";

            foreach (string pattern in RemoveBuiltInPatterns(string.Join(';', userPatterns))
                .Split(';', StringSplitOptions.RemoveEmptyEntries))
                yield return pattern;
        }

        private static bool IsWithinDirectory(string path, string directory)
        {
            string prefix = directory.EndsWith(Path.DirectorySeparatorChar) ? directory : directory + Path.DirectorySeparatorChar;
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDirectory(string path)
        {
            string fullPath = Path.GetFullPath(path);
            string? root = Path.GetPathRoot(fullPath);
            return string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase)
                ? fullPath
                : fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static UsbExportValidationResult Invalid(string message)
        {
            return new UsbExportValidationResult(false, message, Array.Empty<string>());
        }
    }
}
