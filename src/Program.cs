using LrCatalogSync.Core;    // ← für LrCatSync

namespace LrCatalogSync
{
    static class Program
    {
        // Der Einsprungpunkt des Programms.
        [STAThread]
        static void Main()
        {
            // Aktiviert visuelle Windows-Styles
            Application.EnableVisualStyles();

            // Setzt Standard-Text-Rendering
            Application.SetCompatibleTextRenderingDefault(false);

            // Startet die Anwendung mit unserem TrayIcon
            Application.Run(new LrCatSync());
        }
    }
}

