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
using Alachisoft.NosDB.Common;
using System;

namespace Alachisoft.NosDB.Client
{
    internal class RouterToken
    {
        private readonly string _databaseName;
        //private readonly System.Net.IPAddress[] _configServerIp;
        private readonly String[] _configServerIp;
        private readonly int _configServerPort;
        private readonly bool _windowSecurity;
        private readonly string _username;
        private readonly string _password;
        private readonly string _cluster;

        public RouterToken(ConnectionStringBuilder connectionString)
        {
            _databaseName = connectionString.Database;
            _configServerIp = connectionString.DataSource;//System.Net.IPAddress.Parse(connectionString.DataSource);
            _configServerPort = connectionString.Port;
            _windowSecurity = connectionString.IntegeratedSecurity;
            _cluster = connectionString.IsLocalInstance ? MiscUtil.LOCAL : MiscUtil.CLUSTERED;
            if (!_windowSecurity)
            {
                _username = connectionString.UserId;
                _password = connectionString.Password;
            }
        }

        public override int GetHashCode()
        {
            if (!_windowSecurity)
            {
                return _cluster.GetHashCode() + _databaseName.GetHashCode() + GetConfigServerHashCode() +
                       _configServerPort.GetHashCode() + _username.GetHashCode() + _password.GetHashCode();
            }
            return _cluster.GetHashCode()+_databaseName.GetHashCode() + GetConfigServerHashCode() +
                   _configServerPort.GetHashCode();
        }

        private int GetConfigServerHashCode()
        {
            int hashCode = 0;
            if (_configServerIp != null)
            {
                foreach (String addr in _configServerIp)
                {
                    if (addr != null)
                        hashCode += addr.GetHashCode();
                }
            }

            return hashCode;

        }

        private bool ConfigServerEqual(String[] other)
        {
            if (this._configServerIp == null && other == null) return true;

            if ((this._configServerIp == null && other != null) || (other == null && this._configServerIp != null)) return false;

            if (this._configServerIp.Length != other.Length) return false;

            for (int index = 0; index < _configServerIp.Length; index++)
                if (!_configServerIp[index].Equals(other[index])) return false;


            return true;
        }

        public override bool Equals(object obj)
        {
            var token = obj as RouterToken;
            if (token == null) return false;
            var tokobj = token;
            if (!_databaseName.Equals(tokobj._databaseName))
                return false;
            if (!_cluster.Equals(tokobj._cluster))
                return false;
            if (!ConfigServerEqual(tokobj._configServerIp))//if (!_configServerIp.Equals(tokobj._configServerIp))
                return false;
            if (_configServerPort != tokobj._configServerPort)
                return false;
            if (!_windowSecurity && !tokobj._windowSecurity) //if both are nosdb auth
            {
                if (!_username.Equals(tokobj._username) && !_password.Equals(token._password))
                    return false;
            }
            else if (_windowSecurity != tokobj._windowSecurity)
                return false;

            return true;
        }
    }
    //internal class RouterToken
    //{
    //    private readonly string _databaseName;
    //    private readonly System.Net.IPAddress _configServerIp;
    //    private readonly int _configServerPort;
    //    private readonly bool _windowSecurity;
    //    private readonly string _username;

    //    public RouterToken(ConnectionStringBuilder connectionString)
    //    {
    //        _databaseName = connectionString.Database;
    //        _configServerIp = System.Net.IPAddress.Parse(connectionString.DataSource);
    //        _configServerPort = connectionString.Port;
    //        _windowSecurity = connectionString.IntegeratedSecurity;
    //        if (!_windowSecurity)
    //        {
    //            _username = connectionString.UserId;
    //        }
    //    }

    //    public override int GetHashCode()
    //    {
    //        return _databaseName.GetHashCode() + _configServerIp.GetHashCode() +
    //               _configServerPort.GetHashCode();
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        var token = obj as RouterToken;
    //        if (token == null) return false;
    //        var tokobj = token;
    //        if (!_databaseName.Equals(tokobj._databaseName))
    //            return false;
    //        if (!_configServerIp.Equals(tokobj._configServerIp))
    //            return false;
    //        if (_configServerPort != tokobj._configServerPort)
    //            return false;
    //        if (!_windowSecurity && !tokobj._windowSecurity) //if both are nosdb auth
    //        {
    //            if (!_username.Equals(tokobj._username))
    //                return false;
    //        }
    //        else if (_windowSecurity != tokobj._windowSecurity)
    //            return false;

    //        return true;
    //    }
    //}
}