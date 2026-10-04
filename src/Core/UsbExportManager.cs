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
        public IReadOnlyList<string> ExcludePatterns { get; init; } = Array.Empty<string>();
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
                    request.ExcludePatterns,
                    request.UseHashComparison,
                    request.TransferMetadata,
                    progress,
                    transferProgress,
                    cancellationToken);

                progress?.Report(result.Message);

                if (!result.Succeeded)
                    return result;
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
                progress?.Report($"Quelle: {source}");
                progress?.Report($"Ziel: {destination}");
                int exitCode = await RunRcloneCheckAsync(
                    appConfig.RclonePath,
                    source,
                    destination,
                    request.ExcludePatterns,
                    request.UseHashComparison,
                    progress,
                    transferProgress,
                    cancellationToken);
                if (exitCode != 0)
                    allEqual = false;
            }

            return allEqual
                ? new UsbExportResult(true, false, "Checksummen-Vergleich erfolgreich: Quelle und Ziel stimmen überein.")
                : new UsbExportResult(false, false, "Checksummen-Vergleich abgeschlossen: Es wurden Unterschiede gefunden.");
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
                if (clearingDriveRoot && (entryName.Equals("System Volume Information", StringComparison.OrdinalIgnoreCase)
                    || entryName.Equals("$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase)))
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

            progress?.Report("rclone sync wird gestartet. Das Ziel wird an die Quelle angepasst.");

            var startInfo = new ProcessStartInfo
            {
                FileName = rclonePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
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

                void ReportOutput(string line)
                {
                    if (TryParseRcloneProgress(line, out UsbExportProgress parsedProgress))
                        transferProgress?.Report(parsedProgress);

                    if (TryFormatRcloneOutput(line, out string formattedLine))
                        progress?.Report(formattedLine);
                }
                Task outputTask = ForwardOutputAsync(process.StandardOutput, ReportOutput, cancellationToken);
                Task errorTask = ForwardOutputAsync(process.StandardError, ReportOutput, cancellationToken);
                await process.WaitForExitAsync(cancellationToken);
                await Task.WhenAll(outputTask, errorTask);

                return process.ExitCode == 0
                    ? new UsbExportResult(true, false, "Quelle erfolgreich übertragen.")
                    : new UsbExportResult(false, false, $"rclone wurde mit Exitcode {process.ExitCode} beendet.");
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
                formattedLine = $"{matchingFilesMatch.Groups[1].Value} identische Daten";
                return true;
            }

            var errorMatch = System.Text.RegularExpressions.Regex.Match(line, @"\bERROR:\s*(.*)$");
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

                void ReportOutput(string line)
                {
                    if (TryParseRcloneProgress(line, out UsbExportProgress parsedProgress))
                    {
                        var checksMatch = System.Text.RegularExpressions.Regex.Match(line, @"Checks:\s*(\d+)\s*/\s*\d+");
                        if (checksMatch.Success && int.TryParse(checksMatch.Groups[1].Value, out int completedChecks))
                        {
                            double checksPerSecond = completedChecks / Math.Max(checkStopwatch.Elapsed.TotalSeconds, 0.1);
                            parsedProgress = new UsbExportProgress(parsedProgress.Percent, $"{checksPerSecond:0.#} Checks/s");
                        }

                        checkProgress?.Report(parsedProgress);
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
                return process.ExitCode;
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

        private static IEnumerable<string> GetExcludePatterns(IEnumerable<string> customPatterns)
        {
            yield return "Thumbs.db";

            foreach (string pattern in LightroomLockSuffixes.Select(suffix => $"*{suffix}"))
                yield return pattern;

            foreach (string pattern in customPatterns.Where(pattern => !string.IsNullOrWhiteSpace(pattern)))
            {
                string trimmedPattern = pattern.Trim();
                bool isLightroomLockPattern = LightroomLockSuffixes.Any(suffix =>
                    string.Equals(trimmedPattern, $"*{suffix}", StringComparison.OrdinalIgnoreCase));
                if (!isLightroomLockPattern && !string.Equals(trimmedPattern, "Thumbs.db", StringComparison.OrdinalIgnoreCase))
                    yield return trimmedPattern;
            }
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
