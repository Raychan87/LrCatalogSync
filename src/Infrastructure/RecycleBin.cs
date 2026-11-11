using System.Runtime.InteropServices;

namespace LrCatalogSync.Infrastructure
{
    // Löscht Dateien und Verzeichnisse über die Windows-Shell, genau wie der Explorer:
    // Windows zeigt Bestätigung, Fortschritt und Fehlermeldungen selbst an und verschiebt in den Papierkorb.
    public static class RecycleBin
    {
        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_ALLOWUNDO = 0x0040;            // In den Papierkorb statt endgültig löschen
        private const ushort FOF_WANTNUKEWARNING = 0x4000;      // Warnen, wenn doch endgültig gelöscht würde (kein Papierkorb)

        // Löscht den Pfad über die Windows-Shell. Blockiert, bis die Dialoge von Windows geschlossen sind.
        // Rückgabe: 0 bei Erfolg, sonst der Fehlercode der Shell (auch bei Abbruch durch den Benutzer).
        // Ob wirklich gelöscht wurde, prüft der Aufrufer am Pfad: Windows meldet einen Abbruch nicht einheitlich.
        public static int Delete(IntPtr ownerHandle, string path)
        {
            // Die Shell erwartet eine Liste von Pfaden, die mit einem doppelten Nullzeichen endet
            string pathList = path + "\0\0";
            ushort flags = FOF_ALLOWUNDO | FOF_WANTNUKEWARNING;

            // Die Struktur ist unter 32 Bit ohne Füllbytes gepackt, unter 64 Bit normal ausgerichtet
            if (Environment.Is64BitProcess)
            {
                var operation = new SHFILEOPSTRUCT64
                {
                    hwnd = ownerHandle,
                    wFunc = FO_DELETE,
                    pFrom = pathList,
                    fFlags = flags
                };
                return SHFileOperation64(ref operation);
            }
            else
            {
                var operation = new SHFILEOPSTRUCT32
                {
                    hwnd = ownerHandle,
                    wFunc = FO_DELETE,
                    pFrom = pathList,
                    fFlags = flags
                };
                return SHFileOperation32(ref operation);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT64
        {
            public IntPtr hwnd;
            public uint wFunc;
            [MarshalAs(UnmanagedType.LPWStr)] public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)] public string? pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)] public string? lpszProgressTitle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct SHFILEOPSTRUCT32
        {
            public IntPtr hwnd;
            public uint wFunc;
            [MarshalAs(UnmanagedType.LPWStr)] public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)] public string? pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)] public string? lpszProgressTitle;
        }

        [DllImport("shell32.dll", EntryPoint = "SHFileOperationW", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation64(ref SHFILEOPSTRUCT64 operation);

        [DllImport("shell32.dll", EntryPoint = "SHFileOperationW", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation32(ref SHFILEOPSTRUCT32 operation);
    }
}
