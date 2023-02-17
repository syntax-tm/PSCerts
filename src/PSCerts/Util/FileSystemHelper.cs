using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace PSCerts.Util
{
    public static class FileSystemHelper
    {
        public static FileSecurity AddAccessControl(string fileName, string identity, FileSystemRights rights = FileSystemRights.Read,
                                                    AccessControlType accessType = AccessControlType.Allow)
        {
            var fileInfo = new FileInfo(fileName);
            return AddAccessControl(fileInfo, identity, rights, accessType);
        }

        public static FileSecurity AddAccessControl(FileInfo fileInfo, string identity, FileSystemRights rights = FileSystemRights.Read,
                                                    AccessControlType accessType = AccessControlType.Allow)
        {
            var rule = new FileSystemAccessRule(identity, rights, accessType);
            return AddAccessControl(fileInfo, rule);
        }

        public static FileSecurity AddAccessControl(string fileName, FileSystemAccessRule rule)
        {
            var fi = new FileInfo(fileName);
            return AddAccessControl(fi, rule);
        }

        public static FileSecurity AddAccessControl(FileInfo fileInfo, FileSystemAccessRule rule)
        {
            var acl = GetAccessControl(fileInfo);
            acl.AddAccessRule(rule);
            SetAccessControl(fileInfo, acl);
            return acl;
        }

        public static void SetAccessControl(string fileName, FileSecurity acl)
        {
            var fi = new FileInfo(fileName);
            SetAccessControl(fi, acl);
        }

        public static void SetAccessControl(FileInfo fileInfo, FileSecurity acl)
        {
#if NETFRAMEWORK
            File.SetAccessControl(fileInfo.FullName, acl);
#else
            fileInfo.SetAccessControl(acl);
#endif
        }

        public static AuthorizationRuleCollection GetAccessRules(string fileName)
        {
            var fi = new FileInfo(fileName);
            return GetAccessRules(fi);
        }

        public static AuthorizationRuleCollection GetAccessRules(FileInfo fileInfo)
        {
#if NETFRAMEWORK
            var acl = File.GetAccessControl(fileInfo.FullName, AccessControlSections.All);
#else
            var acl = fileInfo.GetAccessControl(AccessControlSections.All);
#endif
            var rules = acl.GetAccessRules(true, true, typeof(NTAccount));
            return rules;
        }

        public static FileSecurity GetAccessControl(string fileName)
        {
            var fi = new FileInfo(fileName);
            return GetAccessControl(fi);
        }

        public static FileSecurity GetAccessControl(FileInfo fileInfo)
        {
#if NETFRAMEWORK
            var acl = File.GetAccessControl(fileInfo.FullName, AccessControlSections.All);
#else
            var acl = fileInfo.GetAccessControl(AccessControlSections.All);
#endif
            return acl;
        }

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

            return Directory.Exists(resolvedPath)
                ? new DirectoryInfo(resolvedPath)
                : throw new ArgumentException("Invalid path.", nameof(path));
        }
        
        private static string ResolveSymLink(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                throw new IOException($"Path '{path}' does not exist.");
            }
            
            var directoryHandle = Native.CreateFile(path, 0, 2, IntPtr.Zero, (int) ECreationDisposition.OpenExisting, (int) EFileAttributes.BackupSemantics, IntPtr.Zero);
            if (directoryHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var result = new StringBuilder(512);
            var mResult = Native.GetFinalPathNameByHandle(directoryHandle, result, result.Capacity, 0);

            if (mResult < 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var finalPath = result.ToString();
            return finalPath.StartsWith(@"\\?\")
                ? finalPath.Substring(4)
                : finalPath;
        }
    }
}
