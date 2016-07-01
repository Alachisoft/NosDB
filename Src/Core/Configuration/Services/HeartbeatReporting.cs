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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public class HeartbeatReporting
    {
        private IDictionary<string, Hashtable> _heartbeatReporter;
        object _lock = new object();

        public HeartbeatReporting()
        {
            _heartbeatReporter = new Dictionary<string, Hashtable>(StringComparer.InvariantCultureIgnoreCase);
        }

        private string GetKey(string cluster,string shard)
        {
            string key = cluster + ":" + shard;
            return key.ToLower();
        }
        public void AddToReport(string cluster, string shard, ServerNode node)
        {
            string key = GetKey(cluster, shard);

            lock (_lock)
            {
                if (_heartbeatReporter.ContainsKey(key))
                {
                    Hashtable table = _heartbeatReporter[key];
                    if (table != null)
                    {
                        if (table.Contains(node.Name.ToLower()))
                            table[node.Name.ToLower()] = DateTime.Now;
                        else
                            table.Add(node.Name.ToLower(), DateTime.Now);
                    }
                }
                else
                {

                    Hashtable table = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
                    table.Add(node.Name.ToLower(), DateTime.Now);

                    _heartbeatReporter.Add(key, table);
                }
            }
            //Save();
        }

        public void RemoveFromReport(string cluster, string shard, ServerNode node)
        {
            string key = GetKey(cluster, shard);
            lock (_lock)
            {
                if (_heartbeatReporter.ContainsKey(key))
                {
                    Hashtable table = _heartbeatReporter[key];
                    if (table != null)
                    {
                        if (table.Contains(node.Name.ToLower()))
                            table.Remove(node.Name.ToLower());
                    }
                }
            }
            //Save();
           
        }

        public Hashtable GetReportTable(string cluster, string shard)
        {
            string key = GetKey(cluster, shard);
            lock (_lock)
            {
                if (_heartbeatReporter.ContainsKey(key))
                {
                    return _heartbeatReporter[key];
                }
            }
            return null;
        }

        public DateTime? GetHeartBeat(string cluster, string shard, ServerNode node)
        {
            string key = GetKey(cluster, shard);
            lock (_lock)
            {
                if (_heartbeatReporter.ContainsKey(key))
                {
                    Hashtable table = _heartbeatReporter[key];
                    if (table != null)
                    {
                        if (table.Contains(node.Name.ToLower()))
                            return (DateTime)table[node.Name.ToLower()];

                    }
                }
            }
            return null;
           
        }
    
       public void Save()
       {
           string metaFilePath = ConfigurationSettings<CSHostSettings>.Current.BasePath + "//heartBeat.bin";

           string path = Path.GetDirectoryName(metaFilePath);
           if (Directory.Exists(path))
           {
               Stream stream = File.Open(metaFilePath, FileMode.Create);

               try
               {

                   //BinaryFormatter bformatter = new BinaryFormatter();
                   byte[] data = Alachisoft.NosDB.Serialization.Formatters.CompactBinaryFormatter.ToByteBuffer(_heartbeatReporter, String.Empty);

                   int len = data.Length;

                   stream.Write(BitConverter.GetBytes(len), 0, 4);
                   stream.Write(data, 0, data.Length);

                   stream.Close();
               }
               catch (Exception ex)
               {
                   stream.Close();
               }
           }
       }

       public void Load()
       {
           string metaFilePath = ConfigurationSettings<CSHostSettings>.Current.BasePath + "//heartBeat.bin";
           if (File.Exists(metaFilePath))
           {
               Stream stream = File.Open(metaFilePath, FileMode.Open);
               try
               {

                   byte[] len = new byte[4];
                   stream.Read(len, 0, 4);
                   int length = BitConverter.ToInt32(len, 0);
                   byte[] data = new byte[length];
                   stream.Read(data, 0, length);

                   _heartbeatReporter = Alachisoft.NosDB.Serialization.Formatters.CompactBinaryFormatter.FromByteBuffer(data, string.Empty) as Dictionary<string, Hashtable>;


                   stream.Close();
               }
               catch (Exception ex)
               {
                   stream.Close();
               }
           }
       }
    
    }
}
