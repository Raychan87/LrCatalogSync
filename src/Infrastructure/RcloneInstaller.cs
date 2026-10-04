using System.IO.Compression;
using System.Diagnostics;
using System.Text.RegularExpressions;
using LrCatalogSync.Resources.Strings;

namespace LrCatalogSync.Infrastructure
{
    public static class RcloneInstaller
    {
        private const string VersionUrl = "https://downloads.rclone.org/version.txt";
        private const string DownloadUrl = "https://downloads.rclone.org/rclone-current-windows-amd64.zip";
        private const long MaximumExecutableSizeBytes = 200L * 1024 * 1024;
        private static readonly Regex versionPattern = new("^rclone (?<version>v\\d+\\.\\d+\\.\\d+)$", RegexOptions.CultureInvariant);
        private static readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromMinutes(5) };

        // Liest die aktuelle stabile Release-Version von der offiziellen rclone-Downloadseite.
        // Das strikte Format verhindert, dass eine Fehlerseite oder Beta-Version als Release verwendet wird.
        public static async Task<string> GetLatestVersionAsync(CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(VersionUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            string versionText = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            Match match = versionPattern.Match(versionText);
            if (!match.Success)
                throw new InvalidDataException("Die rclone-Versionsdatei enthält kein gültiges stabiles Release.");

            return match.Groups["version"].Value;
        }

        // Startet die vorhandene EXE nur mit dem Argument "version" und liest deren erste Ausgabezeile.
        // Bei Timeout oder ungültiger Ausgabe wird null zurückgegeben, damit ein Update versucht werden kann.
        public static async Task<string?> GetInstalledVersionAsync(string executablePath, CancellationToken cancellationToken = default)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("version");

            using Process process = new() { StartInfo = startInfo };
            if (!process.Start())
                return null;

            // Ausgabe parallel lesen, damit der Prozess nicht an einem vollen Pipe-Puffer hängen bleibt.
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                await process.WaitForExitAsync(timeoutSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Bei Timeout den Prozess beenden; bei echtem Abbruch des Aufrufers danach abbrechen.
                try
                {
                    if (!process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                }

                await process.WaitForExitAsync();
                await outputTask;
                cancellationToken.ThrowIfCancellationRequested();
                return null;
            }

            string output = await outputTask;
            if (process.ExitCode != 0)
                return null;

            string firstLine = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?.Trim() ?? string.Empty;
            Match match = versionPattern.Match(firstLine);
            return match.Success ? match.Groups["version"].Value : null;
        }

        // Prüft und aktualisiert ausschließlich die verwaltete Installation unter data/rclone.
        // Benutzerdefinierte Pfade bleiben vollständig unter Kontrolle des Benutzers.
        public static async Task EnsureManagedRcloneAsync(string configuredExecutablePath, CancellationToken cancellationToken = default)
        {
            string managedExecutablePath = Path.Combine(GlobalData.RcloneFolderPath, "rclone.exe");
            // Manuell konfigurierte Installationspfade werden nicht automatisch verändert.
            if (!PathsEqual(configuredExecutablePath, managedExecutablePath))
                return;

            bool executableExists = File.Exists(managedExecutablePath);
            if (executableExists)
            {
                string latestVersion;
                try
                {
                    latestVersion = await GetLatestVersionAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Eine vorhandene Version bleibt auch ohne Verbindung zur Downloadseite nutzbar.
                    Log.Notice($"RcloneInstaller: {string.Format(Strings.Get("Log_RcloneInstaller_UpdateCheckUnavailable"), ex.Message)}");
                    return;
                }

                string? installedVersion;
                try
                {
                    installedVersion = await GetInstalledVersionAsync(managedExecutablePath, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Notice($"RcloneInstaller: {string.Format(Strings.Get("Log_RcloneInstaller_LocalVersionUnreadable"), ex.Message)}");
                    installedVersion = null;
                }

                if (string.Equals(installedVersion, latestVersion, StringComparison.OrdinalIgnoreCase))
                {
                    // Die lokale Version ist bereits aktuell; ein erneuter Download ist nicht nötig.
                    Log.Debug($"RcloneInstaller: {string.Format(Strings.Get("Log_RcloneInstaller_VersionCurrent"), installedVersion)}");
                    return;
                }
            }

            // Bei fehlender EXE installieren, bei veralteter oder nicht lesbarer EXE aktualisieren.
            try
            {
                string installedVersion = await InstallLatestAsync(GlobalData.RcloneFolderPath, cancellationToken);
                Log.Info($"RcloneInstaller: {string.Format(Strings.Get("Log_RcloneInstaller_VersionReady"), installedVersion)}");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Bei einem fehlgeschlagenen Update bleibt die alte EXE erhalten; beim Erststart
                // protokolliert der Coordinator später den gewohnten Status "RcloneExe".
                if (executableExists)
                    Log.Notice($"RcloneInstaller: {string.Format(Strings.Get("Log_RcloneInstaller_UpdateFailed"), ex.Message)}");
                else
                    Log.Error($"RcloneInstaller: {string.Format(Strings.Get("Log_RcloneInstaller_DownloadFailed"), ex.Message)}");
            }
        }

        // Synchroner Einstieg für den WinForms-Start, der vor dem nächsten Init-Schritt warten muss.
        public static void EnsureManagedRclone(string configuredExecutablePath)
        {
            // Der Aufrufer wartet synchron; Task.Run verhindert einen Deadlock im WinForms-UI-Kontext.
            Task.Run(() => EnsureManagedRcloneAsync(configuredExecutablePath)).GetAwaiter().GetResult();
        }

        // Lädt das aktuelle ZIP herunter, extrahiert nur rclone.exe und ersetzt die Installation.
        public static async Task<string> InstallLatestAsync(string targetDirectory, CancellationToken cancellationToken = default)
        {
            string version = await GetLatestVersionAsync(cancellationToken);
            string installDirectory = Path.GetFullPath(targetDirectory);
            Directory.CreateDirectory(installDirectory);

            string temporaryDirectory = Path.Combine(installDirectory, $".rclone-download-{Guid.NewGuid():N}");
            Directory.CreateDirectory(temporaryDirectory);

            try
            {
                // Der temporäre Ordner liegt neben dem Ziel, damit der abschließende Austausch
                // auf demselben Laufwerk stattfindet und die bisherige EXE bis dahin erhalten bleibt.
                string archivePath = Path.Combine(temporaryDirectory, "rclone.zip");
                using (HttpResponseMessage response = await httpClient.GetAsync(
                    DownloadUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    await using Stream downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    await using FileStream archiveOutput = new(
                        archivePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        useAsync: true);
                    // Antwort streamen, statt das komplette ZIP im Arbeitsspeicher zwischenzuspeichern.
                    await downloadStream.CopyToAsync(archiveOutput, cancellationToken);
                }

                string temporaryExecutablePath = Path.Combine(temporaryDirectory, "rclone.exe");
                // Das ZIP enthält die EXE in einem nach der Release-Version benannten Unterordner.
                string expectedEntryPath = $"rclone-{version}-windows-amd64/rclone.exe";
                long expectedExecutableLength;
                using (FileStream archiveFile = new(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (ZipArchive archive = new(archiveFile, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry entry = archive.GetEntry(expectedEntryPath)
                        ?? throw new InvalidDataException($"Die rclone-EXE fehlt im Release-Archiv: {expectedEntryPath}");

                    if (entry.Length <= 0 || entry.Length > MaximumExecutableSizeBytes)
                        throw new InvalidDataException("Die rclone-EXE im Release-Archiv hat eine ungültige Größe.");
                    expectedExecutableLength = entry.Length;

                    using Stream executableInput = entry.Open();
                    await using FileStream executableOutput = new(
                        temporaryExecutablePath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        useAsync: true);
                    await executableInput.CopyToAsync(executableOutput, cancellationToken);
                }

                // Prüft, dass die Extraktion vollständig war, bevor eine vorhandene EXE ersetzt wird.
                if (new FileInfo(temporaryExecutablePath).Length != expectedExecutableLength)
                    throw new InvalidDataException("Die rclone-EXE konnte nicht vollständig extrahiert werden.");

                // Erst die geprüfte Datei ersetzt die bisherige Installation.
                File.Move(temporaryExecutablePath, Path.Combine(installDirectory, "rclone.exe"), overwrite: true);
                Log.Info($"RcloneInstaller: {string.Format(Strings.Get("Log_RcloneInstaller_Installed"), version)}");
                return version;
            }
            finally
            {
                try
                {
                    // Entfernt ZIP und temporäre EXE auch nach Download- oder Extraktionsfehlern.
                    Directory.Delete(temporaryDirectory, recursive: true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }

        // Normalisiert relative und absolute Schreibweisen für den Vergleich des Installationspfads.
        private static bool PathsEqual(string firstPath, string secondPath)
        {
            string normalizedFirstPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(firstPath));
            string normalizedSecondPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(secondPath));
            return string.Equals(normalizedFirstPath, normalizedSecondPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}