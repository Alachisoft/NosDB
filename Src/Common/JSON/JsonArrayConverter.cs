using Alachisoft.NoSDB.Common.Server.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NoSDB.Common.JSON
{
    public class JsonArrayConverter
    {
        public static IJsonValue[] GetArray(IJSONDocument document, string attribute)
        {
            if (document.GetAttributeDataType(attribute) != ExtendedJSONDataTypes.Array)
                return null;
            return ToIJsonList((ArrayList)document[attribute]).ToArray();
        }

        public static List<IJsonValue> ToIJsonList(IEnumerable arrayList)
        {
            List<IJsonValue> jsonList = new List<IJsonValue>();
            foreach (var value in arrayList)
            {
                if (value == null)
                    jsonList.Add(new NullValue());
                else if (IsNumber(value))
                    jsonList.Add(new NumberJsonValue(value));
                else if (value is bool)
                    jsonList.Add(new BooleanJsonValue((bool)value));
                else if (value is string)
                    jsonList.Add(new StringJsonValue(value as string));
                else if (value is DateTime)
                    jsonList.Add(new DateTimeJsonValue((DateTime)value));
                else if (value is Array || value is ArrayList)
                    jsonList.Add(new ArrayJsonValue(ToIJsonList((IEnumerable)value).ToArray()));
                else
                    jsonList.Add(new ObjectJsonValue((JSONDocument)value));
            }
            return jsonList;
        }

        private static bool IsNumber(object value)
        {
            if (value == null)
                return false;

            Type valueType = value.GetType();
            TypeCode actualType = Type.GetTypeCode(valueType);
            switch (actualType)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }
    }
}
