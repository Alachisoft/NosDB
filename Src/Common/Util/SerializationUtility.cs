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
using System.Text;
using System.Collections;
using Alachisoft.NosDB.Common.DataStructures.Clustered;

namespace Alachisoft.NosDB.Common.Util
{
    /// <summary>
    /// Note: 
    /// 
    /// Serialization Scheme: Methods in this class follow the below given routine
    /// 1. Flag [True,False]. true in case of data 
    /// if True:
    ///     2. "Size" of the structure
    /// 3. actual data i.e Key and value both as objects.
    /// 
    /// Deserialization Scheme: 
    /// 1. Read Flag
    ///     Incase of False: return null
    /// 2. Read size
    /// 3. extract data (Key,Value) and cast them accordingly
    /// 4. return the casted data structure
    /// </summary>
    public class SerializationUtility
    {
        public static long GetInt64(byte[] bytes, long start)
        {
            return unchecked(
                (
                    (bytes[start + 0] << 56) |
                    (bytes[start + 1] << 48) |
                    (bytes[start + 2] << 40) |
                    (bytes[start + 3] << 32) |
                    (bytes[start + 4] << 24) |
                    (bytes[start + 5] << 16) |
                    (bytes[start + 6] << 8) |
                    (bytes[start + 7] << 0)
                    ));
        }

        public static void PutInt64(byte[] bytes, int start, long value)
        {
            bytes[start + 0] = (byte)(value >> 56);
            bytes[start + 1] = (byte)(value >> 48);
            bytes[start + 2] = (byte)(value >> 40);
            bytes[start + 3] = (byte)(value >> 32);
            bytes[start + 4] = (byte)(value >> 24);
            bytes[start + 5] = (byte)(value >> 16);
            bytes[start + 6] = (byte)(value >> 8);
            bytes[start + 7] = (byte)(value >> 0);
        }

        /// <summary>
        /// serializes dictionary. Incase of empty dictionary a boolean of value= "false" is serialized ; 
        /// else serializes boolean,count and keys,values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Q"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="writer"></param>
        public static void SerializeDictionary (IDictionary dictionary, Serialization.IO.CompactWriter writer)
        {

            if (dictionary== null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(dictionary.Count);
                for (IDictionaryEnumerator i = dictionary.GetEnumerator(); i.MoveNext(); )
                {
                    writer.WriteObject(i.Key);
                    writer.WriteObject(i.Value);
                }

            }
        }

        /// <summary>
        /// serializes dictionary. Incase of empty dictionary a boolean of value= "false" is serialized ; 
        /// else serializes boolean,count and keys,values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Q"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="writer"></param>
        public static void SerializeDictionary<K,V>(IDictionary<K,V> dictionary, Serialization.IO.CompactWriter writer)
        {

            if (dictionary == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(dictionary.Count);
                for (IEnumerator<KeyValuePair<K,V>> i = dictionary.GetEnumerator(); i.MoveNext(); )
                {
                    writer.WriteObject(i.Current.Key);
                    writer.WriteObject(i.Current.Value);
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Dictionary<T, V> DeserializeDictionary<T,V>(Serialization.IO.CompactReader reader)
        {
            T key;
            V val;
            bool flag = reader.ReadBoolean();

            if (flag)
            {
                Dictionary<T, V> dictionary = null;
                if(typeof(string).Equals(typeof(T)))
                    dictionary = new Dictionary<T, V>((IEqualityComparer<T>)StringComparer.InvariantCultureIgnoreCase);
                else
                    dictionary = new Dictionary<T, V>();

                int Length = reader.ReadInt32();
                for (int i = 0; i < Length; i++)
                {
                    key = (T)reader.ReadObject();
                    val = (V)reader.ReadObject();

                    dictionary.Add(key, val);
                }
                return dictionary;
            }
            else
                return null;
        }

             

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="writer"></param>
        public static void SerializeList<T>(List<T> list, Serialization.IO.CompactWriter writer)
        {
            if (list == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(list.Count);
                for (int i = 0; i < list.Count;i++ )
                {
                    writer.WriteObject(list[i]);
                    
                }
            }
        }

        public static void SerializeArrayList(ArrayList list, Serialization.IO.CompactWriter writer)
        {
            if (list == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    writer.WriteObject(list[i]);

                }
            }
        }


        public static void SerializeClusteredArray<T>(ClusteredArray<T> array,
            Serialization.IO.CompactWriter writer)
        {
            if (array == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(array.Length);
                writer.Write(array.LengthThreshold);
                for (int i = 0; i < array.Length; i++)
                {
                    writer.WriteObject(array[i]);
                }
            }
        }


        public static void SerializeClusteredList<T>(ClusteredList<T> list, Serialization.IO.CompactWriter writer)
        {
            if (list == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    writer.WriteObject(list[i]);

                }
            }
        }

        public static ClusteredArray<T> DeserializeClusteredArray<T>(Serialization.IO.CompactReader reader)
        {
            bool flag = reader.ReadBoolean();

            if (flag)
            {
                int length = reader.ReadInt32();
                int threshold = reader.ReadInt32();
                ClusteredArray<T> array = new ClusteredArray<T>(threshold, length);

                for (int i = 0; i < length; i++)
                    array[i] = (T)reader.ReadObject();

                return array;
            }
            else
                return null;
        }
        
        public static ClusteredList<T> DeserializeClusteredList<T>(Serialization.IO.CompactReader reader)
        {
            bool flag = reader.ReadBoolean();

            if (flag)
            {
                int length = reader.ReadInt32();
                ClusteredList<T> list = new ClusteredList<T>();

                for (int i = 0; i < length; i++)
                    list.Add((T)reader.ReadObject());

                return list;
            }
            else
                return null;
        }

        public static List<T> DeserializeList<T>(Serialization.IO.CompactReader reader)
        {
             bool flag = reader.ReadBoolean();

             if (flag)
             {
                 int length = reader.ReadInt32();
                 List<T> list = new List<T>();

                 for (int i = 0; i < length; i++)
                     list.Add((T)reader.ReadObject());

                 return list;
             }
             else
                 return null;
        }

        public static ArrayList DeserializeArrayList(Serialization.IO.CompactReader reader)
        {
            bool flag = reader.ReadBoolean();

            if (flag)
            {
                int length = reader.ReadInt32();
                ArrayList list = new ArrayList();

                for (int i = 0; i < length; i++)
                    list.Add(reader.ReadObject());

                return list;
            }
            else
                return null;
        }

        #region CQ structures
        /// <summary>
        /// Serializes dictionary containing a list used only in CQ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dList"></param>
        /// <param name="writer"></param>
        public static void SerializeDictionaryList<T, V>(Dictionary<T, List<V>> dList, Serialization.IO.CompactWriter writer)
        {
            if (dList == null)
            {
                writer.Write(false);
                return;
            }
            else 
            {
                writer.Write(true);
                writer.Write(dList.Count);
                for (IDictionaryEnumerator i = dList.GetEnumerator(); i.MoveNext(); )
                {
                    writer.WriteObject(i.Key);
                   
                    SerializeList<V>((List<V>)i.Value, writer);
                }
            }
        }

        public static Dictionary<T, List<V>> DeserializeDictionaryList<T,V>(Serialization.IO.CompactReader reader)
        {

             bool flag = reader.ReadBoolean();

             if (flag)
             {
                 T key;
                 
                 int dictionarylength = reader.ReadInt32();
                 Dictionary<T, List<V>> dList = new Dictionary<T, List<V>>();
                 for (int i = 0; i < dictionarylength; i++)
                 {
                     List<V> valueList;
                     key = (T)reader.ReadObject();
                     valueList = DeserializeList<V>(reader);
                     dList.Add(key, valueList);
                 }
                 return dList;
             }
             else
                 return null;
        }

        /// <summary>
        /// Serializes containing a dictionary containing a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="dList"></param>
        /// <param name="writer"></param>
        public static void SerializeDDList<T, V, K>(Dictionary<T, Dictionary<V, List<K>>> dList, Serialization.IO.CompactWriter writer)
        {
            if (dList == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(dList.Count);
                for (IDictionaryEnumerator i = dList.GetEnumerator(); i.MoveNext(); )
                {
                    writer.WriteObject(i.Key);

                    SerializeDictionaryList<V, K>((Dictionary<V, List<K>>)i.Value, writer);
                }
            }
        }

        public static Dictionary<T, Dictionary<V, List<K>>> DeserializeDDList<T, V, K>(Serialization.IO.CompactReader reader)
        {
             bool flag = reader.ReadBoolean();

             if (flag)
             {
                 T key;
                
                 int dictionarylength = reader.ReadInt32();
                 Dictionary<T, Dictionary<V, List<K>>> dList = new Dictionary<T, Dictionary<V, List<K>>>();
                 for (int i = 0; i < dictionarylength; i++)
                 {
                     Dictionary<V, List<K>> valueList;
                     key = (T)reader.ReadObject();
                     valueList = DeserializeDictionaryList<V, K>(reader);
                     dList.Add(key, valueList);
                 }


                 return dList;
             }
             else
                 return null;
        }


        public static void SerializeDD<T, V, K>(Dictionary<T, Dictionary<V, K>> dList, Serialization.IO.CompactWriter writer)
        {
            if (dList == null)
            {
                writer.Write(false);
                return;
            }
            else
            {
                writer.Write(true);
                writer.Write(dList.Count);
                for (IDictionaryEnumerator i = dList.GetEnumerator(); i.MoveNext(); )
                {
                    writer.WriteObject(i.Key);

                    SerializeDictionary<V,K>((Dictionary<V, K>)i.Value, writer);
                }
            }
        }

        public static Dictionary<T, Dictionary<V, K>> DeserializeDD<T, V, K>(Serialization.IO.CompactReader reader)
        {
            bool flag = reader.ReadBoolean();

            if (flag)
            {
                T key;
               
                int dictionarylength = reader.ReadInt32();
                Dictionary<T, Dictionary<V, K> >dList = new Dictionary<T, Dictionary<V, K>>();
                for (int i = 0; i < dictionarylength; i++)
                {
                    Dictionary<V, K> valueList;
                    key = (T)reader.ReadObject();
                    valueList = DeserializeDictionary<V,K>(reader);
                    dList.Add(key, valueList);
                }
                return dList;
            }
            else
                return null;
        }
        #endregion

    }
}
