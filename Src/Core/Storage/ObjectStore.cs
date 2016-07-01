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
using System.Collections.Generic;
using System.IO;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.IO;
using Alachisoft.NosDB.Serialization.Formatters;

namespace Alachisoft.NosDB.Core.Storage
{
    public class ObjectStore : IEnumerable<KeyValuePair<string, object>> 
    {
        private UFileManager manager;
        private readonly object syncObj = new object();
        private IDataSerializer serializer = new CompactObjectSerializer();
        private HashVector<string,object> objectQueue = new HashVector<string, object>();

        public ObjectStore(string fileName)
        {
            if (File.Exists(fileName))
                manager = new UFileManager(fileName, serializer);
            else
            {
                manager = UFileManager.Create(fileName, serializer, true);
            }
            objectQueue = new HashVector<string, object>();
        }

        public void Put(string name, object mapReference)
        {
            lock (syncObj)
            {
                if (!objectQueue.ContainsKey(name))
                    objectQueue.Add(name, mapReference);
                else objectQueue[name] = mapReference;
            }
        }

        public object Get(string name)
        {
            lock (syncObj)
            {
                return manager.ReadObject(name);
            }
        }

        public bool Contains(string name)
        {
            return manager.ContainsObject(name);
        }
        
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return manager.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return manager.GetEnumerator();
        }

        public void Flush()
        {
            object backup;
            lock (syncObj)
            {
                foreach (var key in objectQueue.Keys)
                {
                    backup = manager.ReadObject(key);
                    try
                    {
                        manager.WriteObject(key, objectQueue[key]);
                    }
                    finally
                    {
                        if (backup != null)
                            manager.WriteObject(key, backup);
                    }
                }
                objectQueue.Clear();
            }
        }

        public class CompactObjectSerializer : IDataSerializer
        {
            public byte[] Serialize(object map)
            {
                return CompactBinaryFormatter.ToByteBuffer(map, "");
            }

            public object Deserialize(byte[] stream)
            {
                return CompactBinaryFormatter.FromByteBuffer(stream, "");
            }

            public T Deserialize<T>(byte[] stream)
            {
                return (T)CompactBinaryFormatter.FromByteBuffer(stream, "");
            }
        }
    }
}
