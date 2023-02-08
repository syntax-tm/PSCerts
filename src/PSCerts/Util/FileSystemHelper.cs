using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PSCerts.Util
{
    public static class FileSystemHelper
    {
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr securityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFinalPathNameByHandle([In] SafeFileHandle hFile, [Out] StringBuilder lpszFilePath, [In] int cchFilePath, [In] int dwFlags);

        private const int CREATION_DISPOSITION_OPEN_EXISTING = 3;
        private const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        
        public static bool IsReparsePoint(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint);
        }

        public static bool IsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public static FileSystemInfo ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var resolvedPath = IsReparsePoint(path)
                ? ResolveSymLink(path)
                : path;
            
            if (File.Exists(resolvedPath))
            {
                return new FileInfo(resolvedPath);
            }

            if (Directory.Exists(resolvedPath))
            {
                return new DirectoryInfo(resolvedPath);
            }

            throw new ArgumentException("Invalid path.", nameof(path));
        }
        
        private static string ResolveSymLink(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                throw new IOException($"Path '{path}' does not exist.");
            }
            
            var directoryHandle = CreateFile(path, 0, 2, IntPtr.Zero, (int) ECreationDisposition.OpenExisting, (int) EFileAttributes.BackupSemantics, IntPtr.Zero);
            if (directoryHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var result = new StringBuilder(512);
            var mResult = GetFinalPathNameByHandle(directoryHandle, result, result.Capacity, 0);

            if (mResult < 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var finalPath = result.ToString();
            if (finalPath.StartsWith(@"\\?\"))
            {
                return finalPath.Substring(4);
            }

            return finalPath;
        }

    }
}
