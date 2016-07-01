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


namespace Alachisoft.NosDB.NosDBPS
{/*
    public class ShardInfo 
    {
        private List<ServerInfo> _servers = new List<ServerInfo>();
        private string _name;
        private int _port;
        
        public string Name { get { return _name; } set { _name = value; } }

        public int Port { get { return _port; } set { _port = value; } }

        public ServerInfo[] Servers
        {
            get
            {
                if (_servers == null)
                {
                    _servers = new List<ServerInfo>();
                }
                return _servers.ToArray();
            }
            
            set
            {
                if (_servers == null)
                {
                    _servers = new List<ServerInfo>();
                }

                _servers.Clear();

                if (value != null)
                    _servers.AddRange(value);
            }
        }
    }*/

    public class ServerDetail 
    {
        public string IPAddress { set; get;}

        public int Priority{ set; get;}
    }
}
