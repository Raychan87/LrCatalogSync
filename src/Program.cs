using System.Diagnostics;

using LrCatalogSync.Core;    // ← für LrCatSync
using LrCatalogSync.Infrastructure;
using LrCatalogSync.UI;

namespace LrCatalogSync
{
    static class Program
    {
        // Der Einsprungpunkt des Programms.
        [STAThread]
        static void Main(string[] args)
        {
            // Aktiviert visuelle Windows-Styles
            Application.EnableVisualStyles();

            // Setzt Standard-Text-Rendering
            Application.SetCompatibleTextRenderingDefault(false);

            // Eigenständiger SizeViewer: wird nur für den Scan mit Administratorrechten gestartet.
            // In diesem Modus gibt es kein Tray-Icon und keinen Sync.
            if (args.Length > 0 && string.Equals(args[0], GlobalConst.SIZEVIEWER_ARG, StringComparison.OrdinalIgnoreCase))
            {
                // Aufruf: --sizeviewer <Prozess-ID von LrCatalogSync> [Verzeichnis]
                int parentProcessId = args.Length > 1 && int.TryParse(args[1], out int processId) ? processId : 0;
                RunStandaloneSizeViewer(parentProcessId, args.Length > 2 ? args[2] : null);
                return;
            }

            // Startet die Anwendung mit unserem TrayIcon
            Application.Run(new LrCatSyncInit());
        }

        private static void RunStandaloneSizeViewer(int parentProcessId, string? startupPath)
        {
            Log.Initialize(GlobalData.BaseDir);
            AppConfig config = AppConfig.LoadFromFile(GlobalData.LrCatSyncConfigPath, GlobalData.BaseDir);
            Log.SetLogLevel(config.LogLevel);
            Localization.Apply(config.Language);
            Theme.Apply(config.Theme);

            // Mit Administratorrechten auch Ordner lesen, die nur für SYSTEM freigegeben sind
            if (Environment.IsPrivilegedProcess)
            {
                int error = SizeScanner.EnableBackupPrivilege();
                if (error != 0)
                    Log.Error($"SizeViewer: {string.Format(Resources.Strings.Strings.Get("Log_SizeViewer_BackupPrivilegeFailed"), error)}");
            }

            var form = new SizeViewerForm(startupPath);
            using Process? parentProcess = FindProcess(parentProcessId);
            if (parentProcess != null)
            {
                // Wird LrCatalogSync beendet, schließt sich auch dieser SizeViewer.
                // Erst ab Load verdrahten: vorher hat das Fenster kein Handle für den Aufruf im UI-Thread.
                parentProcess.Exited += (s, e) =>
                {
                    if (!form.IsDisposed)
                        form.Close();
                };
                form.Load += (s, e) =>
                {
                    parentProcess.SynchronizingObject = form;
                    parentProcess.EnableRaisingEvents = true;
                };
            }

            Application.Run(form);
        }

        private static Process? FindProcess(int processId)
        {
            if (processId <= 0)
                return null;

            try
            {
                return Process.GetProcessById(processId);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                // LrCatalogSync läuft nicht mehr: SizeViewer ohne Überwachung öffnen
                return null;
            }
        }
    }
}
