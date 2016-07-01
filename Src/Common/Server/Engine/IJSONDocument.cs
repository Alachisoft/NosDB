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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Common.Server.Engine
{
    public interface IJSONDocument : ICloneable, IComparable, IEnumerable<KeyValuePair<string, object>>, ICompactSerializable
    {
        object this[string attribute] { get; set; }
        
        string Key { get; set; }
        int Count { get; }
        long Size { get; }

        void Add(string attribute, IJSONDocument value);
        void Add(string attribute, DateTime value);
        void Add(string attribute, Array value);
        void Add(string attribute, bool value);
        void Add(string attribute, double value);
        void Add(string attribute, short value);
        void Add(string attribute, int value);
        void Add(string attribute, long value);
        void Add(string attribute, float value);
        void Add(string attribute, string value);
        void Add(string attribute, object value);

        bool Contains(string attribute);
        ICollection<string> GetAttributes();
        ExtendedJSONDataTypes GetAttributeDataType(string attribute);
        short GetAsInt16(string attribute);
        int GetAsInt32(string attribute);
        long GetAsInt64(string attribute);
        float GetAsFloat(string attribute);
        double GetAsDouble(string attribute);
        decimal GetAsDecimal(string attribute);
        string GetString(string attribute);
        bool GetBoolean(string attribute);
        DateTime GetDateTime(string attribute);
        IJSONDocument GetDocument(string attribute);
        string GetToString(string attribute);
        T[] GetArray<T>(string attribute);
        
        T Get<T>(string attribute);
        bool TryGet(string attribute, out object value);
        bool TryGet<T>(string attribute, out T value);

        void Remove(string attribute);
        void Clear();

        T Parse<T>();
        void GenerateDocumentKey();
    }
}
