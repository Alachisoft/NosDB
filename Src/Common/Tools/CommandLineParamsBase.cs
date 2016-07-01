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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Tools
{
    public class CommandLineParamsBase
    {
        static private bool _printLogo = true;
        static private bool _overwrite = false;
        static private bool _usage = false;
        static private string _server = string.Empty;
        static private int _port = 9950;
        static private string _configCluster = string.Empty;


        public CommandLineParamsBase()
        {

        }

        [ArgumentAttribute(@"/?", false)]
        public bool IsUsage
        {
            get { return _usage; }
            set { _usage = value; }
        }

        [ArgumentAttribute(@"/s", @"/server")]
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        [ArgumentAttribute(@"/p", @"/port", 9950)]
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        [ArgumentAttribute(@"/u", @"/configcluster")]
        public string ConfigCluster
        {
            get { return _configCluster; }
            set { _configCluster = value; }
        }

        [ArgumentAttribute(@"/nologo", true)]
        public bool IsLogo
        {
            get { return _printLogo; }
            set { _printLogo = value; }
        }

        public void SetLocalAddress()
        {
            System.Net.IPAddress localAddress = null;
            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            if (hostEntry.AddressList != null)
            {
                foreach (System.Net.IPAddress addr in hostEntry.AddressList)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localAddress = addr;
                        break;
                    }
                }
            }
            Server = localAddress.ToString();
            Port = 9950;

        }
    }
}
