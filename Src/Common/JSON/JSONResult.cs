using System;
using System.Collections.Generic;
using Alachisoft.NoSQL.Common.JSON.Indexing;
using Alachisoft.NoSQL.Common.Serialization.IO;
using Alachisoft.NoSQL.Common.Util;

namespace Alachisoft.NoSQL.Common.JSON
{
    public class JSONResult : JSONDocument
    {
        private Dictionary<string, object> _aggregations;

        public virtual AttributeValue HashField { get; set; }
        public virtual AttributeValue SortField { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is JSONResult)
            {
                if (SortField != null)
                {
                    JSONResult target = (JSONResult) obj;
                    return SortField.Equals(target.SortField);
                }
                return base.Equals(obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashField == null ? base.GetHashCode() : HashField.GetHashCode();
        }

        public override int CompareTo(object obj)
        {
            if (obj is JSONResult)
            {
                if (SortField != null)
                {
                    JSONResult target = (JSONResult) obj;
                    return SortField.CompareTo(target.SortField);
                }
                throw new ArgumentException();
            }
            throw new ArgumentException();
        }

        public bool ContainsAggregations
        {
            get { return Aggregations != null; }
        }

        public Dictionary<string, object> Aggregations
        {
            get { return _aggregations; }
        }

        public bool TryGetAggregation(string name, out object result)
        {
            result = null;
            if (_aggregations == null)
                return false;
            return _aggregations.TryGetValue(name, out result);
        }

        public object GetAggregation(string name)
        {
            return _aggregations[name];
        }

        public void SetAggregation(string name, object result)
        {
            if (_aggregations == null)
                _aggregations = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _aggregations[name] = result;
        }

        public void AddAggregation(string name, object result)
        {
            if (_aggregations == null)
                _aggregations = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _aggregations.Add(name, result);
        }

        public void RemoveAggregation(string name)
        {
            _aggregations.Remove(name);
            if (_aggregations.Count == 0) _aggregations = null;
        }

        public bool ContainsAggregation(string name)
        {
            return _aggregations.ContainsKey(name);
        }

        public static JSONResult ToJSONResult(JSONDocument document)
        {
            JSONResult result = new JSONResult();
            result.Key = document.Key;
            result._values = document.ToDictionary();
            return result;
        }

        public override void Serialize(CompactWriter writer)
        {
            base.Serialize(writer);
            writer.WriteObject(HashField);
            writer.WriteObject(SortField);
            SerializationUtility.SerializeDictionary<string, object>(_aggregations, writer);
        }

        public override void Deserialize(CompactReader reader)
        {
            base.Deserialize(reader);
            HashField = reader.ReadObject() as AttributeValue;
            SortField = reader.ReadObject() as AttributeValue;
            _aggregations = SerializationUtility.DeserializeDictionary<string, object>(reader);
        }
    }
}
