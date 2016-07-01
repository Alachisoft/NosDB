using System;
using System.Net;
using System.Text.RegularExpressions;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Net;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common
{
    public class ConnectionStringBuilder
    {
        //private IPAddress[] _datasource;
        private String[] _datasource;
        private IPAddress _clientIP;
        private int _configserverPort = 9950;
        private string _database;
        private int _connectiontimeout = 90;
        private string _userName;
        private string _password;
        private string _connectionString;
        private bool _isLocalInstance = false;
        private bool _integeratedSecurity = true;

        private const string DATASOURCE = "(?i)(data source\\s*=\\s*(?<datasource>[^;]+)\\s*;)";
        private const string PORT = "(?i)(port\\s*=\\s*(?<port>\\d+)\\s*;)";
        private const string DATABASE = "(?i)(database\\s*=\\s*(?<database>[^;]+)\\s*;)";
        private const string LOCAL = "(?i)(local instance\\s*=\\s*(?<local>true|false|yes|no)\\s*;)";
        private const string TIMEOUT = "(?i)(connection timeout\\s*=\\s*(?<timeout>\\d+)\\s*;)";
        private const string SECURITY = "(?i)(Integrated Security\\s*=\\s*(?<security>yes|no|true|false)\\s*;)";
        private const string CREDENTIALS = "(?i)(User ID\\s*=\\s*(?<username>[^;]+)\\s*;\\s*password\\s*=\\s*(?<password>[^;]+)\\s*;)";
        private const string CLIENTIP = "(?i)(Client Ip\\s*=\\s*(?<clientip>[^;]+)\\s*;)";

        #region Properties

        public String[] DataSource
        {
            get { return _datasource/* == null ? null : _datasource.ToString()*/; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Database", "Value cannot be null");
                _datasource = value;
                //ResolveHostAddress(value);
            }
        }

        public int Port
        {
            get { return _configserverPort; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Invalid value specified.", "ConfigurationServerPort");
                _configserverPort = value;
            }
        }

        public bool IsLocalInstance
        {
            get { return _isLocalInstance; }
            set { _isLocalInstance = value; }
        }

        public string Database
        {
            get { return _database; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Database", "Value cannot be null");
                _database = value;
            }
        }

        public string ConnectionString
        {
            get { return ConvertToString(false); }
            set
            {
                ParseConnectionString(value);
                _connectionString = value;
            }
        }

        public bool IntegeratedSecurity
        {
            get { return _integeratedSecurity; }
            set { _integeratedSecurity = value; }
        }

        public int ConnectionTimeout
        {
            get { return _connectiontimeout; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Invalid value specified.", "Timeout");
                _connectiontimeout = value;
            }
        }

        public string UserId
        {
            set
            {
                if (!string.IsNullOrEmpty(_password))
                    _integeratedSecurity = false;
                _userName = value;
            }
            get { return _userName; }
        }

        public string Password
        {
            set
            {
                if (!string.IsNullOrEmpty(_userName))
                    _integeratedSecurity = false;
                _password = value;
            }
            get { return _password; }
        }

        public string ClientIP
        {
            get { return _clientIP == null ? null : _clientIP.ToString(); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("ClientIP", "Value cannot be null");
                ResolveLoaclIPAddress(value);
            }
        }

        #endregion

        public ConnectionStringBuilder() { }

        public ConnectionStringBuilder(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString", "Value cannot be null.");

            ParseConnectionString(connectionString);
            _connectionString = connectionString;
        }

        private void ParseConnectionString(string connectionString)
        {
            //do not remove - used for validation purpose
            _connectionString = connectionString;
            Match match;
            if (MatchConnectionString(connectionString, DATASOURCE, false, out match))
                ResolveHostAddress(GetAttribute(match, "datasource"));
            if (MatchConnectionString(connectionString, PORT, true, out match))
                _configserverPort = Int32.Parse(GetAttribute(match, "port"));
            if (MatchConnectionString(connectionString, DATABASE, false, out match))
                _database = GetAttribute(match, "database");
            if (MatchConnectionString(connectionString, LOCAL, true, out match))
            {
                _isLocalInstance = IsTrue(GetAttribute(match, "local"));
            }
            if (MatchConnectionString(connectionString, TIMEOUT, true, out match))
                ConnectionTimeout = Int32.Parse(GetAttribute(match, "timeout"));

            if (MatchConnectionString(connectionString, SECURITY, true, out match))
            {
                IntegeratedSecurity = IsTrue(GetAttribute(match, "security"));
            }

            if (!IntegeratedSecurity && MatchConnectionString(connectionString, CREDENTIALS, true, out match))
            {
                _userName = GetOptionalAttribute(match, "username").ToLower();
                _password = GetOptionalAttribute(match, "password");
                if (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_password))
                {
                    throw new DistributorException(ErrorCodes.Distributor.INVALID_CONNECTION_STRING, new string[] { connectionString });
                }
                IntegeratedSecurity = false;
            }
            else
            {
                IntegeratedSecurity = true;
            }
            if (MatchConnectionString(connectionString, CLIENTIP, true, out match))
                ResolveLoaclIPAddress(GetAttribute(match, "clientip"));

            if (_connectionString.Trim().Length > 0)
            {
                throw new DistributorException(ErrorCodes.Distributor.INVALID_CONNECTION_STRING, new string[] { _connectionString });
            }
        }

        private bool IsTrue(string value)
        {
            if ("yes".Equals(value, StringComparison.OrdinalIgnoreCase) || "true".Equals(value, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private bool MatchConnectionString(string connectionString, string pattern, bool isOptional, out Match match)
        {
            match = Regex.Match(connectionString, pattern, RegexOptions.ExplicitCapture);
            if (!match.Success && !isOptional)
                throw new ArgumentException("Invalid connection string specified", "connectionString");
            if (match.Success)
            {
                //todo need a trigger to find what didn't match
                Regex regex = new Regex(pattern);
                _connectionString = regex.Replace(_connectionString, "");
            }
            return match.Success;
        }

        private string GetAttribute(Match match, string attribute)
        {
            var databaseGroup = match.Groups[attribute];
            if (databaseGroup.Success)
            {
                return databaseGroup.Value.Trim();
            }
            var message = string.Format("The connection string '{0}' is not valid. Attribute '" + attribute + "' not defined", ConnectionString);
            throw new Exception(message);
        }

        private string GetOptionalAttribute(Match match, string attribute)
        {
            var databaseGroup = match.Groups[attribute];
            if (databaseGroup.Success)
            {
                return databaseGroup.Value.Trim();
            }
            return null;
        }

        private void ResolveHostAddress(string host)
        {
            List<String> addresses = new List<string>();

            if (host.Equals("localhost"))
                _datasource = new String[] { DnsCache.Resolve(Dns.GetHostName()).ToString() };
            else
                _datasource = GetDataSource(host);
        }
        private String[] GetDataSource(String host)
        {
            List<String> ds = new List<String>();
            try
            {
                String[] servers = host.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String server in servers)
                {
                    try
                    {
                        IPAddress addr = DnsCache.Resolve(server);

                        if (addr != null)
                            ds.Add(addr.ToString());
                    }
                    catch (Exception)
                    { }
                }
            }
            catch (Exception)
            {
            }

            return ds.Count > 0 ? ds.ToArray() : null;
        }

        private void ResolveLoaclIPAddress(string localIP)
        {
            _clientIP = NetworkUtil.GetVerifedLocalIP(localIP);
        }

        private string ConvertToString(bool isClientAttribute)
        {
            string connectionString = string.Empty;
            if (_datasource != null)
            {
                String servers = String.Empty;
                foreach (string server in _datasource) servers += server + ",";

                servers = servers.Substring(0, servers.Length - 1);

                connectionString = "Data Source = " + servers + ";";
            }
            connectionString += " Port = " + _configserverPort + ";";
            connectionString += " Database = " + _database + ";";
            connectionString += " Local Instance = " + _isLocalInstance + ";";
            connectionString += " Connection Timeout = " + _connectiontimeout + ";";
            if (!string.IsNullOrEmpty(_userName) || !string.IsNullOrEmpty(_password))
            {
                connectionString += " User Id = " + _userName + "; Password = " + _password + ";";
                connectionString += " Integrated Security = " + _integeratedSecurity + ";";
            }
            else
            {
                connectionString += " Integrated Security = " + _integeratedSecurity + ";";
            }

            if (isClientAttribute)
            {
                if (_clientIP != null)
                    connectionString += "Client IP = " + _clientIP.ToString() + ";";
            }

            return connectionString;
        }
        public override string ToString()
        {
            return ConvertToString(true);
        }
    }
}
