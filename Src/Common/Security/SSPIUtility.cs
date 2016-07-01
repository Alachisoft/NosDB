// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Security.SSPI;
using Alachisoft.NosDB.Common.Security.SSPI.Contexts;
using Alachisoft.NosDB.Common.Security.SSPI.Credentials;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Alachisoft.NosDB.Common.Security
{
    public class SSPIUtility
    {
        private static string _domainName = "";
        public static string DomainName
        {
            get
            {
                if (string.IsNullOrEmpty(_domainName))
                {
                    uint domainNameCapacity = 512;
                    var domainName = new StringBuilder((int)domainNameCapacity);
                    GetComputerNameEx(COMPUTER_NAME_FORMAT.ComputerNameDnsDomain, domainName, ref domainNameCapacity);
                    _domainName = domainName.ToString();
                }
                return _domainName;
            }
        }
        public static string GetServicePrincipalName(string SPNClassName, IPAddress ipAddress)
        {
            var hostEntry = Dns.GetHostEntry(ipAddress);
            string servicePrincipalName = SPNClassName + "/" + hostEntry.HostName;
            return servicePrincipalName;
        }

        public static bool IsValidUser(string username)
        {
            if (!username.Contains('\\'))
                throw new SecurityException(ErrorCodes.Security.INVALID_DOMAIN_USERNAME);
            if (IsValidLocalAccount(username))
                return true;
            if (IsDomainEnvironment)
            {
                if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsDebugEnabled)
                    LoggerManager.Instance.SecurityLogger.Debug("SSPIUtility.IsValidUser", "Domain Environment");

                try
                {
                    IntPtr _sid = IntPtr.Zero; //pointer to binary form of SID string.
                    int _sidLength = 0;   //size of SID buffer.
                    int _domainLength = 0;  //size of domain name buffer.
                    int _use;     //type of object.
                    StringBuilder _domain = new StringBuilder(); //stringBuilder for domain name.
                    int _error = 0;
                    string _sidString = "";

                    //first call of the function only returns the sizes of buffers (SDI, domain name)
                    bool isValid = LookupAccountName(null, username, _sid, ref _sidLength, _domain, ref _domainLength, out _use); _error = Marshal.GetLastWin32Error();

                    if (_error != 122) //error 122 (The data area passed to a system call is too small) - normal behaviour.
                    {
                        //throw (new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { new Win32Exception(_error).Message }));
                    }
                    else
                    {
                        _domain = new StringBuilder(_domainLength); //allocates memory for domain name
                        _sid = Marshal.AllocHGlobal(_sidLength); //allocates memory for SID
                        bool _rc = LookupAccountName(null, username, _sid, ref _sidLength, _domain, ref _domainLength, out _use);

                        if (_rc == false)
                        {
                            _error = Marshal.GetLastWin32Error();
                            Marshal.FreeHGlobal(_sid);
                            throw (new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { new Win32Exception(_error).Message }));
                        }
                        else
                        {
                            // converts binary SID into string
                            _rc = ConvertSidToStringSid(_sid, ref _sidString);
                        }
                        if (_rc)
                            return true;

                        //try
                        //{
                        //    System.DirectoryServices.DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry("LDAP://" + DomainName);

                        //    DirectorySearcher searcher = new DirectorySearcher(entry);
                        //    searcher.Filter = "(&(samaccountname=" + accountname + "))";

                        //    SearchResultCollection sResults = null;
                        //    //perform search on active directory
                        //    sResults = searcher.FindAll();

                        //    if (sResults.Count > 0)
                        //        return true;
                        //}
                        //catch (Exception exc)
                        //{
                        //    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsErrorEnabled)
                        //    {
                        //        LoggerManager.Instance.SecurityLogger.Error("SSPIUtility.IsValidUser", exc);
                        //    }
                        //}
                    } 
                }
                catch (Exception exc)
                {
                    throw (new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { exc.Message }));
                }
            }
            return IsValidSystemAccount(username);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupAccountName([In, MarshalAs(UnmanagedType.LPTStr)] string systemName, [In, MarshalAs(UnmanagedType.LPTStr)] string accountName, IntPtr sid, ref int cbSid, StringBuilder referencedDomainName, ref int cbReferencedDomainName, out int use);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ConvertSidToStringSid(IntPtr sid, [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid);

        public static bool IsValidLocalAccount(string username)
        {
          //  return true;
            if (username.Contains('\\'))
            {
                try
                {
                    IntPtr _sid = IntPtr.Zero; //pointer to binary form of SID string.
                    int _sidLength = 0;   //size of SID buffer.
                    int _domainLength = 0;  //size of domain name buffer.
                    int _use;     //type of object.
                    StringBuilder _domain = new StringBuilder(); //stringBuilder for domain name.
                    int _error = 0;
                    string _sidString = "";

                    //first call of the function only returns the sizes of buffers (SDI, domain name)
                    bool isValid = LookupAccountName(null, username, _sid, ref _sidLength, _domain, ref _domainLength, out _use); _error = Marshal.GetLastWin32Error();

                    if (_error != 122) //error 122 (The data area passed to a system call is too small) - normal behaviour.
                    {
                        throw (new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { new Win32Exception(_error).Message }));
                    }
                    else
                    {
                        _domain = new StringBuilder(_domainLength); //allocates memory for domain name
                        _sid = Marshal.AllocHGlobal(_sidLength); //allocates memory for SID
                        bool _rc = LookupAccountName(null, username, _sid, ref _sidLength, _domain, ref _domainLength, out _use);

                        if (_rc == false)
                        {
                            _error = Marshal.GetLastWin32Error();
                            Marshal.FreeHGlobal(_sid);
                            throw (new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { new Win32Exception(_error).Message }));
                        }
                        else
                        {
                            // converts binary SID into string
                            _rc = ConvertSidToStringSid(_sid, ref _sidString);
                        }
                        if (_rc)
                            return true;

                    }
                }
                catch (Exception exc)
                {
                    //falling back to previous local account verification model
                    string accountname = username.Contains('\\') ? username.Split('\\')[1] : username;
                    var entry = new DirectoryEntry("WinNT://" + Environment.MachineName);
                    try
                    {
                        DirectoryEntry uentry = entry.Children.Find(accountname);
                        if (uentry != null)
                            return true;
                    }
                    catch (Exception ex)
                    { }
                    throw (new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { exc.Message + ". Username: " + username }));
                }
            }
         
            return false;
        }

        public static bool IsDomainMachine(string username)
        {
            string machinename = null;
            string[] splitstrings = username.Split('\\');
            if (splitstrings.Count() > 1)
            {
                string machinestring = splitstrings[1];
                splitstrings = machinestring.Split('$');
                if (splitstrings.Count() > 0)
                    machinename = splitstrings[0];
            }
            if (machinename != null)
            {
                try
                {
                    var host = Dns.GetHostEntry(machinename);
                    return !(host == null);
                }
                catch(Exception exc)
                {
                    return false;
                }
            }
            return false;
        }

        public static bool IsValidSystemAccount(string username)
        {
            string accountname = string.Empty;
            if (username.Contains('\\'))
                accountname = username.Split('\\')[1];
            else
                accountname = username;
            SelectQuery query = new SelectQuery("Select * from Win32_SystemAccount where Name=\"" + accountname + "\"");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            var coll = searcher.Get();
            if (coll.Count > 0)
                return true;
            return IsValidServiceAccount(username);
        }

        public static bool IsValidServiceAccount(string username)
        {
            string accountname = string.Empty;
            if (username.Contains('\\'))
                accountname = username.Split('\\')[1];
            else
                accountname = username;
            SelectQuery query = new SelectQuery("Select * from Win32_Service where Name=\"" + accountname + "\"");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            var coll = searcher.Get();
            if (coll.Count > 0)
                return true;
            return IsValidNoSDbService(username);
        }

        public static bool IsValidNoSDbService(string username)
        {
            if (username.Equals(@"NT SERVICE\" + MiscUtil.NOSDB_DBSVC_NAME) || username.Equals(@"NT SERVICE\" + MiscUtil.NOSDB_CSVC_NAME) || username.Equals(@"NT SERVICE\" + MiscUtil.NOSDB_DISTSVC_NAME))
                return true;
            else
                return IsDomainMachine(username);
        }

        public static bool IsValidGroup(string username)
        {
            try
            {
                using (var ctx = new PrincipalContext(ContextType.Domain))
                {
                    var user = GroupPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, username);

                    if (user != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception exc) //as not a domain user verify if it is a workgroup or local user
            {
                try
                {
                    using (var ctx = new PrincipalContext(ContextType.Machine))
                    {
                        var user = GroupPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, username);

                        if (user != null)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public static bool IsLocalServer(IPAddress ipAddress)
        {
            var currentHostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            bool ifExist = currentHostAddresses.Contains<IPAddress>(ipAddress);
            return ifExist;
        }

        public static bool IsServerOnDomain(IPAddress ipAddress)
        {
            var host = Dns.GetHostByAddress(ipAddress);
            return false;
        }

        public static bool IsDomainEnvironment
        {
            get
            {
                return DomainName.Length > 0;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetComputerNameEx(
            COMPUTER_NAME_FORMAT NameType,
            StringBuilder lpBuffer,
            ref uint lpnSize);

        enum COMPUTER_NAME_FORMAT
        {
            ComputerNameNetBIOS,
            ComputerNameDnsHostname,
            ComputerNameDnsDomain,
            ComputerNameDnsFullyQualified,
            ComputerNamePhysicalNetBIOS,
            ComputerNamePhysicalDnsHostname,
            ComputerNamePhysicalDnsDomain,
            ComputerNamePhysicalDnsFullyQualified
        }

        public static bool IsSPNRegistered { get; set; }

        const Int32 ERROR_BUFFER_OVERFLOW = 111;
        const Int32 NO_ERROR = 0;

        #region Native functions
        /// <summary>
        /// Signature for DsGetSPN, which constructs an array of one or more SPNs. 
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="serviceClass"></param>
        /// <param name="serviceName"></param>
        /// <param name="InstancePort"></param>
        /// <param name="cInstanceNames"></param>
        /// <param name="pInstanceNames"></param>
        /// <param name="pInstancePorts"></param>
        /// <param name="SpnCount"></param>
        /// <param name="SpnArrayPointer"></param>
        /// <returns></returns>
        [DllImport("ntdsapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern UInt32 DsGetSpn
        (
            DS_SPN_NAME_TYPE ServiceType,
            string serviceClass,
            string serviceName,
            ushort InstancePort,
            ushort cInstanceNames,
            string[] pInstanceNames,
            ushort[] pInstancePorts,
            ref Int32 SpnCount,
            ref System.IntPtr SpnArrayPointer
        );

        /// <summary>
        /// ENUM for spn Type
        /// </summary>
        public enum DS_SPN_NAME_TYPE
        {
            DS_SPN_DNS_HOST = 0,
            DS_SPN_DN_HOST = 1,
            DS_SPN_NB_HOST = 2,
            DS_SPN_DOMAIN = 3,
            DS_SPN_NB_DOMAIN = 4,
            DS_SPN_SERVICE = 5
        }

        /// <summary>
        /// DsCrackSpn parses a spn into its component strings
        /// </summary>
        /// <param name="pszSPN"></param>
        /// <param name="pcServiceClass"></param>
        /// <param name="serviceClass"></param>
        /// <param name="pcServicename"></param>
        /// <param name="serviceName"></param>
        /// <param name="pcInstanceName"></param>
        /// <param name="instanceName"></param>
        /// <param name="pinstancePort"></param>
        /// <returns></returns>
        [DllImport("Ntdsapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern UInt32 DsCrackSpn
        (
            string pszSPN,
            ref Int32 pcServiceClass,
            StringBuilder serviceClass,
            ref Int32 pcServicename,
            StringBuilder serviceName,
            ref Int32 pcInstanceName,
            StringBuilder instanceName,
            out ushort pinstancePort
        );

        /// <summary>
        /// DsWriteAccountSpn writes an array of SPNs to the servicePrincipalName attribute of a
        /// specified user or computer object in AD.
        /// </summary>
        /// <param name="hDS"></param>
        /// <param name="Operation"></param>
        /// <param name="pszAccount"></param>
        /// <param name="cSpn"></param>
        /// <param name="SPNArray"></param>
        /// <returns></returns>
        [DllImport("ntdsapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint DsWriteAccountSpn
        (
           System.IntPtr hDS,
           DS_SPN_WRITE_OP Operation,
           string pszAccount,
           Int32 cSpn,
           System.IntPtr SPNArray
         );

        /// <summary>
        /// DSBind binds to a domain controller/Domain
        /// </summary>
        /// <param name="DomainControllerName"></param>
        /// <param name="DnsDomainName"></param>
        /// <param name="phDS"></param>
        /// <returns></returns>
        [DllImport("ntdsapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint DsBind
        (
           string DomainControllerName,
           string DnsDomainName,
           out System.IntPtr phDS
        );

        /// <summary>
        /// Enum for spn Writing operation
        /// </summary>
        public enum DS_SPN_WRITE_OP
        {
            DS_SPN_ADD_SPN_OP = 0,
            DS_SPN_REPLACE_SPN_OP = 1,
            DS_SPN_DELETE_SPN_OP = 2
        }
        
        public const int ErrorSuccess = 0;

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetGetJoinInformation(string server, out IntPtr domain, out NetJoinStatus status);

        [DllImport("Netapi32.dll")]
        public static extern int NetApiBufferFree(IntPtr Buffer);

        public enum NetJoinStatus
        {
            NetSetupUnknownStatus = 0,
            NetSetupUnjoined,
            NetSetupWorkgroupName,
            NetSetupDomainName
        }
        #endregion

        private static string CreateSCP(string distinguishedName, string spn)
        {
            string adsName = ServiceName;
            string keyword = (Guid.NewGuid()).ToString();
            DirectoryEntry p = GetComputerObject(Environment.MachineName);
            if (null == p)
                return null;

            // verify the valid ads name
            if (!adsName.Contains("CN="))
                adsName = "CN=" + adsName;

            DirectoryEntry scp_entry = null;

            // open an existing scp
            try
            {
                scp_entry = p.Children.Find(adsName);
            }
            catch (DirectoryServicesCOMException)
            { }

            // create the new child record
            if (null == scp_entry)
            {
                scp_entry = p.Children.Add(adsName, "serviceConnectionPoint");
            }

            // build our scp object

            // fill the values
            scp_entry.Properties["keywords"].Value = keyword;
            //scp.svcKeyword = keyword;
            scp_entry.Properties["serviceDNSName"].Value = p.Properties["dNSHostName"].Value;
            //scp.svcBinding = bindingOptions;
            //scp_entry.Properties["serviceBindingInformation"].Value = bindingOptions;

            // commit the changes
            scp_entry.CommitChanges();
            p.CommitChanges();
            return scp_entry.Properties["distinguishedName"].Value as string;
        }

        public static DirectoryEntry GetComputerObject(string objectName)
        {
            DirectorySearcher mySearcher = new DirectorySearcher();
            mySearcher.Filter = "(&(objectClass=computer)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
            SearchResult result = mySearcher.FindOne();
            if (result == null)
                return null;

            return result.GetDirectoryEntry();
        }

        private static string GetSvcDistinguishedName()
        {
            if (IsDomainEnvironment)
            {
                var ctx = new PrincipalContext(ContextType.Machine);
                var user = GroupPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, @"NT SERVICE\" + ServiceName);
                string distinguishedaName = user.DistinguishedName;
                return distinguishedaName;
            }
            return null;
        }

        public static void RegisterSpn(bool register)
        {
            IsSPNRegistered = false;
            try
            {
                var domain = Domain.GetComputerDomain();
                var domainname = Dns.GetHostEntry("localhost");
                var userPrincipal = GetDistinguishedName(Dns.GetHostName());
                //var userPrincipal = GetSvcDistinguishedName();
                //if (userPrincipal == null)
                //    userPrincipal = GetDistinguishedName(Dns.GetHostName());
                // Input Variable values
                string spn = string.Empty;
                if(ServiceName.Equals(MiscUtil.NOSDB_CSVC_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    spn = GetServicePrincipalName(MiscUtil.NOSCONF_SPN, domainname.AddressList[0]); // spn name
                    //spn += (":" + ConfigurationSettings<CSHostSettings>.Current.Port);
                }
                else if (ServiceName.Equals(MiscUtil.NOSDB_DBSVC_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    spn = GetServicePrincipalName(MiscUtil.NOSDB_SPN, domainname.AddressList[0]); // spn name
                    //spn += (":" + ConfigurationSettings<DBHostSettings>.Current.Port);
                }
                //spn += (":" + ServiceName);
                //string dn = CreateSCP(userPrincipal, spn);
                //if (ValidateSPN(spn))
                //{
                string servicePrincipalName = userPrincipal;
                    //"CN=ACTUser,CN=Users,DC=SHAOLINT,DC=COM"; // DistinguishedName of the user/Computer
                string domainControllerName = domain.DomainControllers[0].Name; // Domain controller name
                string dnsDomainName = domain.Name; // DNS domain name

                int serviceClassSize = 1;
                int serviceNameSize = 1;
                int instanceNameSize = 1;
                ushort port;

                StringBuilder sTemp = new StringBuilder(1);

                // Initial call to DsCrackSpn should result in BUFFER_OVERFLOW...
                uint crackSpnResult = DsCrackSpn(spn, ref serviceClassSize, sTemp, ref serviceNameSize,
                    sTemp, ref instanceNameSize, sTemp, out port);

                // Check for buffer overflow
                if (crackSpnResult == ERROR_BUFFER_OVERFLOW)
                {
                    // Resize our SB's
                    StringBuilder serviceClass = new StringBuilder(serviceClassSize);
                    StringBuilder serviceName = new StringBuilder(serviceNameSize);
                    StringBuilder instanceName = new StringBuilder(instanceNameSize);

                    // Crack this spn using DsCrackSpn
                    crackSpnResult = DsCrackSpn(spn, ref serviceClassSize, serviceClass, ref serviceNameSize,
                        serviceName, ref instanceNameSize, instanceName, out port);

                    // If Success
                    if (crackSpnResult == NO_ERROR)
                    {
                        string[] hostArray = {instanceName.ToString()};
                        ushort[] portArray = {port};
                        ushort spnCount = 1;
                        IntPtr spnArrayPointer = new IntPtr();
                        Int32 spnArrayCount = 0;

                        // Call to DsBind to get handle for Directory
                        System.IntPtr hDS;
                        uint result = DsBind(domainControllerName, dnsDomainName, out hDS);

                        if (result != NO_ERROR)
                        {
                            IsSPNRegistered = false;
                            AppUtil.LogEvent("SPN is not registered. Error Code : " + result,
                                System.Diagnostics.EventLogEntryType.Error);
                            return;
                        }

                        // Call to DsgetSpn to construct an spn
                        uint getSPNResult = DsGetSpn(DS_SPN_NAME_TYPE.DS_SPN_DN_HOST, serviceClass.ToString(),
                            null, port, spnCount, hostArray, portArray, ref spnArrayCount, ref spnArrayPointer);

                        if (getSPNResult == NO_ERROR)
                        {
                            if (register)
                            {
                                // Call the CSDsWriteAccountSPN for writing this spn to the object
                                uint dsWriteSpnResult = DsWriteAccountSpn(hDS, DS_SPN_WRITE_OP.DS_SPN_ADD_SPN_OP,
                                    servicePrincipalName, spnArrayCount, spnArrayPointer);

                                if (dsWriteSpnResult == NO_ERROR)
                                {
                                    IsSPNRegistered = true;
                                    AppUtil.LogEvent(
                                        "SPN is registered. Please check the user/Computer object for ServicePrincipalName.",
                                        System.Diagnostics.EventLogEntryType.Information);
                                    //Console.WriteLine("DsWriteAccountSpn : "+ spn + " Succeed. Please check the user/Computer object for ServicePrincipalName.");
                                    //Console.ReadKey();
                                }
                                else
                                {
                                    IsSPNRegistered = false;
                                    AppUtil.LogEvent("SPN is not registered. Error Code : " + dsWriteSpnResult,
                                        System.Diagnostics.EventLogEntryType.Error);
                                    //Console.WriteLine("DsWriteAccountSpn Failed.");
                                    return;
                                }
                            }
                            else
                            {
                                // Call the CSDsWriteAccountSPN for writing this spn to the object
                                uint dsWriteSpnResult = DsWriteAccountSpn(hDS, DS_SPN_WRITE_OP.DS_SPN_DELETE_SPN_OP,
                                    servicePrincipalName, spnArrayCount, spnArrayPointer);

                                if (dsWriteSpnResult == NO_ERROR)
                                {
                                    IsSPNRegistered = false;
                                    AppUtil.LogEvent(
                                        "SPN is unregistered.",
                                        System.Diagnostics.EventLogEntryType.Information);
                                    //Console.WriteLine("DsWriteAccountSpn : "+ spn + " Succeed. Please check the user/Computer object for ServicePrincipalName.");
                                    //Console.ReadKey();
                                }
                                else
                                {
                                    IsSPNRegistered = true;
                                    AppUtil.LogEvent("SPN is not unregistered. Error Code : " + dsWriteSpnResult,
                                        System.Diagnostics.EventLogEntryType.Error);
                                    //Console.WriteLine("DsWriteAccountSpn Failed.");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            IsSPNRegistered = false;
                            AppUtil.LogEvent("SPN is not registered. Error Code : " + getSPNResult,
                                System.Diagnostics.EventLogEntryType.Error);
                            return;
                        }
                    }
                    else
                    {
                        IsSPNRegistered = false;
                        AppUtil.LogEvent("SPN is not registered. Error Code : " + crackSpnResult,
                            System.Diagnostics.EventLogEntryType.Error);
                        return;
                    }
                }
            }
            catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectNotFoundException exc)
            {
                IsSPNRegistered = false;
                AppUtil.LogEvent("SPN is not registered. Machine is not part of Domain.",
                    System.Diagnostics.EventLogEntryType.Information);
                return;
            }
            catch (Exception exc)
            {
                IsSPNRegistered = false;
                AppUtil.LogEvent("SPN is not registered. Machine is not part of Domain.",
                    System.Diagnostics.EventLogEntryType.Error);
                return;
            }
        }

        private static string GetDistinguishedName(string computerName)
        {
            PrincipalContext oCtx = new PrincipalContext(ContextType.Domain);
            ComputerPrincipal oPrincipal = ComputerPrincipal.FindByIdentity(oCtx, computerName);
            string dn = oPrincipal.DistinguishedName;
            return dn;
        }

        private static string DistinguishedName
        {
            get
            {
                string distinguishedName = string.Empty;
                Principal principal = Principal.FindByIdentity(new PrincipalContext(ContextType.Domain), ServiceName);
                if (principal != null)
                    distinguishedName = principal.DistinguishedName;
                return distinguishedName;
            }
        }

        static bool ValidateSPN(string spn)
        {
            try
            {
                const string queryFormat = "(ServicePrincipalName={0})";
                using (Domain localDomain =
                    Domain.GetComputerDomain())
                {
                    DirectoryEntry de;
                    try
                    {
                        de = localDomain.GetDirectoryEntry();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        return true;
                    }
                    using (DirectorySearcher search = new DirectorySearcher(de
                        ))
                    {

                        search.Filter = string.Format(queryFormat, spn);
                        search.SearchScope = SearchScope.Subtree;

                        SearchResultCollection collection = search.FindAll();

                        if (collection.Count >= 1)
                        {
                            IsSPNRegistered = true;
                            AppUtil.LogEvent(
                                "SPN is registered. Please check the user/Computer object for ServicePrincipalName.",
                                System.Diagnostics.EventLogEntryType.Information);
                            return false;
                        }
                        else if (collection.Count == 0)
                            return true;
                    }
                }
            }
            catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectNotFoundException exc)
            {
                return true;
            }
            catch (DirectoryServicesCOMException exc)
            {
                return true;
            }
            catch (Exception exc)
            {
                return true;
            }
            return false;
        }

        public static ServerCredential RemoteServerCredential
        {
            get
            {
                if (IsSPNRegistered)
                    return new ServerCredential(PackageNames.Kerberos);
                else
                {
                    SSPIException exc = new SSPIException("no spn registered", SecurityStatus.WrongPrincipal);
                    throw new SecurityException(ErrorCodes.Security.SSPI_ERROR, new string[] { exc.Message });
                }
            }
        }

        public static ServerCredential LocalServerCredential
        {
            get
            {
                return new ServerCredential(PackageNames.Ntlm);
            }
        }

        public static ServerContext GetServerContext(ServerCredential serverCredential)
        {
            return new ServerContext(
                    serverCredential,
                    ContextAttrib.AcceptIntegrity |
                    ContextAttrib.ReplayDetect |
                    ContextAttrib.SequenceDetect |
                    ContextAttrib.MutualAuth |
                    ContextAttrib.Delegate |
                    ContextAttrib.Confidentiality
                );
        }

        public static ClientCredential GetClientCredentials(string servicePrincipalName = null)
        {
            if (servicePrincipalName != null)
            {
                if (!ValidateSPN(servicePrincipalName)) //returns false if spn registered
                {
                    if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
                    {
                        LoggerManager.Instance.SecurityLogger.Info("SSPIUtility.GetClientCredentials", "Package = kerberos, spn = " + servicePrincipalName);
                    }
                    return new ClientCredential(PackageNames.Kerberos);
                }
            }
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.SecurityLogger.Info("SSPIUtility.GetClientCredentials", "Package = NTLM, spn = " + servicePrincipalName);
            }
            return new ClientCredential(PackageNames.Ntlm);
        }

        public static ClientContext GetClientContext(ClientCredential clientCredential, string SPN)
        {
            if (LoggerManager.Instance.SecurityLogger != null && LoggerManager.Instance.SecurityLogger.IsInfoEnabled)
            {
                LoggerManager.Instance.SecurityLogger.Info("SSPIUtility.GetClientContext", "Package = " + clientCredential.SecurityPackage + ", spn = " + SPN);
            }
            if (clientCredential.SecurityPackage == PackageNames.Kerberos)
            {
                return new ClientContext(
                       clientCredential,
                       SPN,
                       ContextAttrib.InitIntegrity |
                       ContextAttrib.ReplayDetect |
                       ContextAttrib.SequenceDetect |
                       ContextAttrib.MutualAuth |
                       ContextAttrib.Delegate |
                       ContextAttrib.Confidentiality
                   );
            }
            else if (clientCredential.SecurityPackage == PackageNames.Ntlm)
            {
                return new ClientContext(
                       clientCredential,
                       "",
                       ContextAttrib.InitIntegrity |
                       ContextAttrib.ReplayDetect |
                       ContextAttrib.SequenceDetect |
                       ContextAttrib.MutualAuth |
                       ContextAttrib.Delegate |
                       ContextAttrib.Confidentiality
                   );
            }
            else
            {
                throw new SecurityException(ErrorCodes.Security.CERTIFICATE_ERROR);
            }
        }

        private static string serviceName = null;

        public static string ServiceName
        {
            get
            {
                if (string.IsNullOrEmpty(serviceName))
                {
                    int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service where ProcessId = " + processId);
                    ManagementObjectCollection collection = searcher.Get();
                    serviceName = (string)collection.Cast<ManagementBaseObject>().First()["Name"];
                }
                return serviceName;
            }
        }

        public static string GetCurrentLogin()
        {
            ////get service logon
            //int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service where ProcessId = " + processId);
            //ManagementObjectCollection collection = searcher.Get();
            //var serviceName = (string)collection.Cast<ManagementBaseObject>().First()["Name"];
            //ManagementObject wmiService = new ManagementObject("Win32_Service.Name='" + serviceName + "'");
            //wmiService.Get();
            //string user = wmiService["startname"].ToString();
            //return user;

            //string domainname = string.Empty;
            //string username = string.Empty;
            //try
            //{
            //    using (var ctx = new PrincipalContext(ContextType.Domain))
            //    {
            //        domainname = Environment.UserDomainName;
            //        var user = UserPrincipal.Current;
            //        username = user.SamAccountName;
            //    }
            //}
            //catch (Exception exc)
            //{
            //    using (var ctx = new PrincipalContext(ContextType.Machine))
            //    {
            //        domainname = ctx.ConnectedServer;
            //        var user = UserPrincipal.Current;
            //        username = user.SamAccountName;
            //    }
            //}
            //return domainname + "\\" + username;

            try
            {
                var identity = WindowsIdentity.GetCurrent();
                return identity.Name;
            }
            catch (InvalidCastException exc)
            {
                var windIdentity = WindowsIdentity.GetCurrent();
                return windIdentity.Name;
            }
            catch (Exception exc)
            {
                var windIdentity = WindowsIdentity.GetCurrent();
                return windIdentity.Name;
            }
        }

        public static string MachineName
        {
            get
            {
                SelectQuery query = new SelectQuery("Select * from Win32_ComputerSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (var sid in searcher.Get())
                {
                    string name = sid["Name"] as string;
                    string domainname = (sid["Domain"] as string).Split('.')[0];
                    return domainname + "\\" + name + "$";
                }
                return "";
            }
        }

        public static string GetHostName()
        {
            string hostname = Dns.GetHostName().ToLower();
            string fshn = "";
            try
            {
                var hostname1 = Environment.UserName;
                var domain = Environment.UserDomainName;
                fshn = domain + @"\" + hostname1;
            }
            catch (Exception exc)
            {
                fshn = hostname;
            }
            return fshn;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public static bool LogonUser(string username, string password, out SafeTokenHandle safeTokenHandle)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("username or password cannot be null or empty");
            }
            else
            {
                string domain = DomainName;
                const int LOGON32_PROVIDER_DEFAULT = 0;
                const int LOGON32_LOGON_INTERACTIVE = 2;

                return LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);
            }
        }
    }
}
