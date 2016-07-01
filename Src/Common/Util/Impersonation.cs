using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Alachisoft.NosDB.Common.Util
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class Impersonation : IDisposable
    {
        private SafeTokenHandle _handle;
        private WindowsImpersonationContext _context;

        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        public Impersonation(string domainUsername, string password)
        {
            if (string.IsNullOrEmpty(domainUsername))
                throw new Exception("username cannot be null");

            if (!domainUsername.Contains('\\'))
                throw new Exception("Invalid username specified.");

            string[] splittedStrings = domainUsername.Split('\\');

            Impersonate(splittedStrings[0], splittedStrings[1], password);
        }

        public Impersonation(string domain, string username, string password)
        {
            Impersonate(domain, username, password);
        }

        public void Impersonate(string domain, string username, string password)
        {
            var ok = LogonUser(username, domain, password,
                LOGON32_LOGON_NEW_CREDENTIALS, 0, out _handle);
            if (!ok)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new ApplicationException(
                    string.Format(
                        "Could not access {0}. Make sure you have sufficient access rights to this location.", domain));
            }

            _context = WindowsIdentity.Impersonate(this._handle.DangerousGetHandle());
        }

        public void Dispose()
        {
            this._context.Dispose();
            this._handle.Dispose();
        }

        public static void ValidateAccess(string path, string domainUsername, string password)
        {
            using (new Impersonation(domainUsername, password))
            {
                try
                {
                    string tempDirectory = Path.Combine(path, Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDirectory);
                    if (Directory.Exists(tempDirectory))
                        Directory.Delete(tempDirectory);
                }
                catch (Exception)
                {
                    throw new ApplicationException(
                    string.Format(
                        "Could not access {0}. Make sure you have sufficient access rights to this location.", path));
                }
            }
        }

        #region Native Calls

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        #endregion
    }
}
