using System.Collections.Concurrent;
using System.Diagnostics;

namespace LrCatalogSync.Infrastructure
{
    public static class RcloneProcessManager
    {
        private static readonly ConcurrentDictionary<int, Process> activeProcesses = new();
        private static int shutdownStarted;
        private const int SM_SHUTTINGDOWN = 0x2000;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        // Auch true, wenn Windows bereits herunterfährt, ohne dass SessionEnding ankam.
        public static bool IsShuttingDown =>
            Volatile.Read(ref shutdownStarted) != 0 || GetSystemMetrics(SM_SHUTTINGDOWN) != 0;

        public static Process? Start(ProcessStartInfo startInfo)
        {
            // Beim Herunterfahren keine neuen Prozesse starten (sonst Fehler 0xc0000142).
            if (IsShuttingDown)
                return null;

            Process? process = Process.Start(startInfo);
            if (process != null)
                Register(process);

            return process;
        }

        public static void Register(Process process)
        {
            // Id sofort erfassen: nach Dispose ist process.Id nicht mehr lesbar.
            int id;
            try
            {
                id = process.Id;
            }
            catch (InvalidOperationException)
            {
                // Prozess wurde bereits beendet/disposed - nichts zu registrieren.
                return;
            }

            activeProcesses[id] = process;
            process.EnableRaisingEvents = true;
            // Über die feste Id entfernen, nicht über process.Id (nach Dispose nicht verfügbar).
            process.Exited += (_, _) => activeProcesses.TryRemove(id, out _);

            if (Volatile.Read(ref shutdownStarted) != 0)
            {
                TryStop(process);
                activeProcesses.TryRemove(id, out _);
            }
        }

        public static void Unregister(Process process)
        {
            // process.Id kann nach einem Dispose eine InvalidOperationException werfen.
            int id;
            try
            {
                id = process.Id;
            }
            catch (InvalidOperationException)
            {
                return;
            }

            activeProcesses.TryRemove(id, out _);
        }

        public static void BeginShutdown()
        {
            Interlocked.Exchange(ref shutdownStarted, 1);
            StopAll();
        }

        public static void StopAll()
        {
            foreach (Process process in activeProcesses.Values.ToArray())
            {
                TryStop(process);
                Unregister(process);
            }
        }

        private static void TryStop(Process process)
        {
            try
            {
                if (process.HasExited)
                    return;

                process.CloseMainWindow();
                if (!process.WaitForExit(3000))
                    process.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Beenden von rclone: {ex.Message}");
            }
        }
    }
}