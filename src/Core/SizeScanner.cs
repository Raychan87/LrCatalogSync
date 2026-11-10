using System.IO.Enumeration;
using System.Runtime.InteropServices;

namespace LrCatalogSync.Core
{
    // Ein Eintrag im Größenbaum: Verzeichnis, Datei oder die Sammelzeile "[n Dateien]"
    public sealed class SizeNode
    {
        public string Name { get; init; } = string.Empty;   // Beim Wurzelknoten der vollständige Pfad
        public SizeNode? Parent { get; set; }
        public List<SizeNode>? Children { get; set; }
        public bool IsDirectory { get; init; }
        public bool IsFileGroup { get; init; }               // Sammelzeile für die Dateien eines Verzeichnisses
        public bool AccessDenied { get; set; }               // Verzeichnis konnte nicht gelesen werden
        public long Size { get; set; }                       // Logische Größe in Bytes
        public long Allocated { get; set; }                  // Belegter Platz auf dem Datenträger in Bytes
        public long FileCount { get; set; }
        public long DirCount { get; set; }
        public DateTime LastModified { get; set; }

        // Anzeigezustand, wird ausschließlich vom SizeViewer-Fenster gesetzt
        public bool Expanded { get; set; }
        public int Depth { get; set; }

        public bool HasChildren => Children is { Count: > 0 };
    }

    // Fortschritt eines laufenden Scans. Wird vom Scan-Thread geschrieben und vom UI-Timer gelesen.
    public sealed class SizeScanProgress
    {
        private long fileCount;
        private int inaccessibleCount;
        private volatile string currentDirectory = string.Empty;

        public long FileCount => Interlocked.Read(ref fileCount);
        public int InaccessibleCount => Volatile.Read(ref inaccessibleCount);
        public string CurrentDirectory => currentDirectory;

        internal void AddFiles(int count) => Interlocked.Add(ref fileCount, count);
        internal void AddInaccessible() => Interlocked.Increment(ref inaccessibleCount);
        internal void SetCurrentDirectory(string path) => currentDirectory = path;
    }

    public sealed record SizeScanResult(SizeNode Root, bool Cancelled, int InaccessibleCount);

    // Liest ein Verzeichnis rekursiv ein und berechnet Größe, belegten Platz und Anzahl der Einträge.
    // Rein lesend: es wird nichts verändert, verschoben oder gelöscht.
    public static class SizeScanner
    {
        // Attribute, bei denen der belegte Platz von der Dateigröße abweichen kann
        // (komprimiert, Sparse, Offline sowie Cloud-Platzhalter wie OneDrive)
        private const FileAttributes AllocationLookupAttributes =
            FileAttributes.Compressed |
            FileAttributes.SparseFile |
            FileAttributes.Offline |
            FileAttributes.ReparsePoint |
            (FileAttributes)0x00040000 |    // FILE_ATTRIBUTE_RECALL_ON_OPEN
            (FileAttributes)0x00400000;     // FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS

        private const uint INVALID_FILE_SIZE = 0xFFFFFFFF;
        private const int MAX_PATH = 260;

        private static readonly EnumerationOptions enumerationOptions = new()
        {
            AttributesToSkip = 0,               // Auch versteckte Dateien und Systemdateien zählen
            IgnoreInaccessible = false,         // Zugriffsfehler sollen als "kein Zugriff" sichtbar werden
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false
        };

        private readonly record struct Entry(string Name, FileAttributes Attributes, long Length, DateTime LastWrite);

        public static SizeScanResult Scan(string rootPath, SizeScanProgress progress, CancellationToken cancellationToken)
        {
            long clusterSize = GetClusterSize(rootPath);
            var root = new SizeNode { Name = rootPath, IsDirectory = true };

            try
            {
                root.LastModified = Directory.GetLastWriteTime(rootPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Zeitstempel ist nur Zusatzinfo, der eigentliche Zugriffsfehler wird in ScanDirectory behandelt
            }

            ScanDirectory(root, rootPath, clusterSize, !IsNetworkPath(rootPath), progress, cancellationToken);
            return new SizeScanResult(root, cancellationToken.IsCancellationRequested, progress.InaccessibleCount);
        }

        private static void ScanDirectory(
            SizeNode node,
            string path,
            long clusterSize,
            bool queryAllocation,
            SizeScanProgress progress,
            CancellationToken cancellationToken)
        {
            progress.SetCurrentDirectory(path);

            var directories = new List<SizeNode>();
            var files = new List<SizeNode>();
            DateTime newestChange = DateTime.MinValue;

            try
            {
                var entries = new FileSystemEnumerable<Entry>(
                    path,
                    (ref FileSystemEntry entry) => new Entry(
                        entry.FileName.ToString(),
                        entry.Attributes,
                        entry.Length,
                        entry.LastWriteTimeUtc.LocalDateTime),
                    enumerationOptions);

                foreach (Entry entry in entries)
                {
                    // Bei Abbruch bleibt das bisher gelesene Teilergebnis erhalten
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if ((entry.Attributes & FileAttributes.Directory) != 0)
                    {
                        var child = new SizeNode
                        {
                            Name = entry.Name,
                            IsDirectory = true,
                            Parent = node,
                            LastModified = entry.LastWrite
                        };

                        // Verknüpfungen (Junctions, Symlinks) nicht verfolgen, sonst würde doppelt gezählt
                        if ((entry.Attributes & FileAttributes.ReparsePoint) == 0)
                            ScanDirectory(child, Path.Combine(path, entry.Name), clusterSize, queryAllocation, progress, cancellationToken);

                        directories.Add(child);
                    }
                    else
                    {
                        files.Add(new SizeNode
                        {
                            Name = entry.Name,
                            Size = entry.Length,
                            Allocated = GetAllocatedSize(path, entry, clusterSize, queryAllocation),
                            FileCount = 1,
                            LastModified = entry.LastWrite
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
            {
                node.AccessDenied = true;
                progress.AddInaccessible();
            }

            progress.AddFiles(files.Count);

            // ========== SUMMEN BILDEN ==========
            long fileSize = 0;
            long fileAllocated = 0;
            DateTime newestFile = DateTime.MinValue;
            foreach (SizeNode file in files)
            {
                fileSize += file.Size;
                fileAllocated += file.Allocated;
                if (file.LastModified > newestFile)
                    newestFile = file.LastModified;
            }

            node.Size = fileSize;
            node.Allocated = fileAllocated;
            node.FileCount = files.Count;
            node.DirCount = directories.Count;
            newestChange = newestFile;

            foreach (SizeNode directory in directories)
            {
                node.Size += directory.Size;
                node.Allocated += directory.Allocated;
                node.FileCount += directory.FileCount;
                node.DirCount += directory.DirCount;
                if (directory.LastModified > newestChange)
                    newestChange = directory.LastModified;
            }

            if (newestChange > DateTime.MinValue)
                node.LastModified = newestChange;

            // ========== KINDKNOTEN AUFBAUEN ==========
            // Gibt es Unterverzeichnisse, werden die Dateien in einer Sammelzeile zusammengefasst
            if (directories.Count > 0 && files.Count > 0)
            {
                var fileGroup = new SizeNode
                {
                    IsFileGroup = true,
                    Parent = node,
                    Children = files,
                    Size = fileSize,
                    Allocated = fileAllocated,
                    FileCount = files.Count,
                    LastModified = newestFile
                };

                foreach (SizeNode file in files)
                    file.Parent = fileGroup;

                directories.Add(fileGroup);
                node.Children = directories;
            }
            else if (directories.Count > 0)
            {
                node.Children = directories;
            }
            else if (files.Count > 0)
            {
                foreach (SizeNode file in files)
                    file.Parent = node;

                node.Children = files;
            }
        }

        // Belegter Platz einer Datei: normalerweise die auf Clustergröße aufgerundete Dateigröße.
        // Nur bei komprimierten, Sparse- und Cloud-Dateien wird Windows direkt gefragt (langsamer).
        // Auf Netzlaufwerken entfällt diese Abfrage: sie kostet pro Datei einen Netzwerkzugriff.
        private static long GetAllocatedSize(string directory, Entry entry, long clusterSize, bool queryAllocation)
        {
            if (queryAllocation && (entry.Attributes & AllocationLookupAttributes) != 0)
            {
                uint low = GetCompressedFileSizeW(ToExtendedPath(Path.Combine(directory, entry.Name)), out uint high);
                if (low != INVALID_FILE_SIZE || Marshal.GetLastWin32Error() == 0)
                    return ((long)high << 32) | low;
            }

            if (entry.Length <= 0)
                return 0;

            return (entry.Length + clusterSize - 1) / clusterSize * clusterSize;
        }

        // Aktiviert das Windows-Sicherungsrecht (SeBackupPrivilege) für diesen Prozess.
        // Damit lassen sich mit Administratorrechten auch Ordner lesen, die nur für SYSTEM
        // freigegeben sind (z. B. "System Volume Information"). Das Recht erlaubt nur Lesen.
        // Rückgabe: 0 bei Erfolg, sonst der Windows-Fehlercode.
        public static int EnableBackupPrivilege()
        {
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr token))
                return Marshal.GetLastWin32Error();

            try
            {
                if (!LookupPrivilegeValueW(null, SE_BACKUP_NAME, out long luid))
                    return Marshal.GetLastWin32Error();

                var privileges = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Luid = luid,
                    Attributes = SE_PRIVILEGE_ENABLED
                };

                // AdjustTokenPrivileges meldet auch dann Erfolg, wenn das Recht dem Konto fehlt:
                // maßgeblich ist der Fehlercode danach (ERROR_NOT_ALL_ASSIGNED)
                if (!AdjustTokenPrivileges(token, false, ref privileges, 0, IntPtr.Zero, IntPtr.Zero))
                    return Marshal.GetLastWin32Error();

                return Marshal.GetLastWin32Error();
            }
            finally
            {
                CloseHandle(token);
            }
        }

        private static bool IsNetworkPath(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(path);
                if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
                    return true;

                string? root = Path.GetPathRoot(fullPath);
                return !string.IsNullOrEmpty(root) && new DriveInfo(root).DriveType == DriveType.Network;
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException)
            {
                return false;
            }
        }

        private static long GetClusterSize(string path)
        {
            try
            {
                string? root = Path.GetPathRoot(Path.GetFullPath(path));
                if (!string.IsNullOrEmpty(root))
                {
                    if (!root.EndsWith(Path.DirectorySeparatorChar))
                        root += Path.DirectorySeparatorChar;

                    if (GetDiskFreeSpaceW(root, out uint sectorsPerCluster, out uint bytesPerSector, out _, out _))
                    {
                        long clusterSize = (long)sectorsPerCluster * bytesPerSector;
                        if (clusterSize > 0)
                            return clusterSize;
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException)
            {
                // Ungültiger Pfad: Standardwert verwenden, der Scan meldet den Fehler selbst
            }

            return GlobalConst.SIZEVIEWER_DEFAULT_CLUSTER_BYTES;
        }

        // Pfade ab MAX_PATH brauchen für die Windows-API das Präfix \\?\
        private static string ToExtendedPath(string path)
        {
            if (path.Length < MAX_PATH || path.StartsWith(@"\\?\", StringComparison.Ordinal))
                return path;

            return path.StartsWith(@"\\", StringComparison.Ordinal)
                ? @"\\?\UNC\" + path[2..]
                : @"\\?\" + path;
        }

        private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint SE_PRIVILEGE_ENABLED = 0x0002;
        private const string SE_BACKUP_NAME = "SeBackupPrivilege";

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public long Luid;
            public uint Attributes;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValueW(string? systemName, string name, out long luid);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges(
            IntPtr tokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
            ref TOKEN_PRIVILEGES newState,
            uint bufferLength,
            IntPtr previousState,
            IntPtr returnLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetCompressedFileSizeW(string lpFileName, out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceW(
            string lpRootPathName,
            out uint lpSectorsPerCluster,
            out uint lpBytesPerSector,
            out uint lpNumberOfFreeClusters,
            out uint lpTotalNumberOfClusters);
    }
}
