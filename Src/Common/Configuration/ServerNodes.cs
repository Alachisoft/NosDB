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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Alachisoft.NosDB.Common.Configuration
{
    public class ServerNodes : ICloneable, ICompactSerializable
    {
        Dictionary<string, ServerNode> _servers = new Dictionary<string, ServerNode>(StringComparer.InvariantCultureIgnoreCase);

        
        [JsonProperty(PropertyName = "Nodes")]
        public Dictionary<string, ServerNode> Nodes
        {
            get { return _servers; }
            set 
            {

                if (_servers == null)
                {
                    _servers = new Dictionary<string, ServerNode>(StringComparer.InvariantCultureIgnoreCase);
                }
                _servers.Clear();

                if(value != null)
                {
                    
                    Dictionary<string,ServerNode>.Enumerator e = value.GetEnumerator();
                    while(e.MoveNext())
                    {
                        _servers.Add(e.Current.Key, e.Current.Value);
                    }

                }
            }
        }

        [ConfigurationSection("node")]
        public ServerNode[] NodesDom
        {
            get { return _servers.Values.ToArray(); }
            set 
            {
                _servers = new Dictionary<string, ServerNode>(StringComparer.InvariantCultureIgnoreCase);
                foreach(ServerNode node in value )
                {
                    _servers.Add(node.Name, node);
                }
            }
        }

        public void AddNode(ServerNode node)
        {
            lock (_servers)
            {
                _servers.Add(node.Name, node);
            }
        }

        public void AddNode(string name, ServerNode node)
        {
            lock (_servers)
            {
                _servers.Add(name, node);
            }
        }

        public void RemoveNode(string name)
        {
            lock (_servers)
            {
                if(_servers.ContainsKey(name))
                _servers.Remove(name);
            }
        }

        public bool ContainsNode(string name)
        {
            return _servers.ContainsKey(name);
        }

        public ServerNode GetNode(string name)
        {
            lock (_servers)
            {
                if (_servers.ContainsKey(name))
                    return _servers[name];
                return null;
            }
        }

        #region ICloneable Member
        public object Clone()
        {
            ServerNodes serverNodes = new ServerNodes();
            serverNodes.Nodes = Nodes != null ? Nodes.Clone<string,ServerNode>(): null;

            return serverNodes;
        }
        #endregion

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Nodes = SerializationUtility.DeserializeDictionary<string, ServerNode>(reader);
            //Nodes = reader.ReadObject() as ServerNode[];            
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            SerializationUtility.SerializeDictionary<string, ServerNode>(Nodes, writer);
            //writer.WriteObject(Nodes);
        }
        #endregion

        public ServerNode GetServerNode(string ip)
        {
            if (_servers.ContainsKey(ip))
                return _servers[ip];
            return null;
            //throw new Exception("the requested node is not part of the configuration.");
        }
    }
}
