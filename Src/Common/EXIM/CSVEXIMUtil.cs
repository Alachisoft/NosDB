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
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Recovery;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.Enum;

namespace Alachisoft.NosDB.Common.EXIM
{
    public class CSVEXIMUtil : EXIMBase
    {
        string _fileExtension = ".csv";
        string _key = "_key";
        private int position;// reader
        public CSVEXIMUtil()
        { }

        #region EXIMBase
        public override RecoveryOperationStatus Write(EXIMDataType dataType, string path, string collection,string fileName,string database, List<Server.Engine.IJSONDocument> docList)
        {

            RecoveryOperationStatus state = base.ValidatePath(path, RecoveryJobType.Export);
            if (state.Status == RecoveryStatus.Success)
            {
                try
                {
                    bool first = true;
                    IDictionary<int, string> headerPositionMap = null;
                    string file=string.Empty;
                    if (!string.IsNullOrEmpty(fileName))
                        file = Path.Combine(path, fileName + _fileExtension);
                    else
                    {
                        string defaultName = database + "_" + collection;
                        file = Path.Combine(path, defaultName + _fileExtension);
                    }
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        if (docList.Count > 0)
                        {
                            foreach (JSONDocument document in docList)
                            {
                                if (first)
                                {
                                    WriteHeaderRow(document, out headerPositionMap, writer);
                                    first = false;
                                }
                                else
                                {
                                   IDictionary<string, object> valueMap = GetCsvReadyValues(document);

                                    string _delim = ",";
                                    string vString = string.Empty;
                                    int pos = 0;
                                    foreach (int hPos in headerPositionMap.Keys)
                                    {
                                        if (pos != 0)
                                        {
                                            vString += _delim;
                                        }
                                        if (valueMap.ContainsKey(headerPositionMap[hPos]))
                                        {
                                            vString += valueMap[headerPositionMap[hPos]].ToString();
                                        }
                                        else
                                        {
                                            vString += _delim;
                                        }
                                        pos++;
                                    }
                                    writer.WriteLine(vString);
                                }
                            }
                            writer.Close();
                        }


                    }

                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.EXIMLogger.Error("CSVEXIMUtil.Export()", exp.ToString());
                    state.Status = RecoveryStatus.Failure;
                    state.Message = exp.ToString();
                }

                return state;
            }
            else
            {
                return state;
            }
        }

        public override IEnumerable<List<JSONDocument>> Read(EXIMDataType dataType, string path)
        {
            List<JSONDocument> items = new List<JSONDocument>();

            RecoveryOperationStatus state = base.ValidatePath(path, RecoveryJobType.Import);
            if (state.Status == RecoveryStatus.Success)
            {
                if (ValidateExtension(path))
                {
                    using (Stream stream = new FileStream(path, FileMode.Open,FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        // if file is not empty
                        if (reader.Peek() > 0)
                        {
                            position = 0;
                            IDictionary<int, string> header = ReadHeaderRow(reader);
                            IDictionary<int, object> value = null;
                            while (!reader.EndOfStream)
                            {
                                value = ReadValueRow(reader);
                                if (header.Count != value.Count)
                                    throw new InvalidDataException("Invalid data provided the number of columns is not equal to header row");

                                JSONDocument doc = CreateJSONDocument(header, value);

                                if (doc != null)
                                {
                                    if (items.Count <= base.ChunkSize)
                                    {
                                        items.Add(doc);
                                    }
                                    else
                                    {
                                        items.Add(doc);
                                        yield return items;
                                        items.Clear();                                         
                                    }
                                }
                            }
                            if (items.Count > 0)
                                yield return items;
                        }
                        else
                            yield return items;
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid file extension");
                }
            }
            else
            {
                throw new ArgumentException("Invalid file path provided");
            }
        }

        internal override bool ValidateExtension(string path)
        {
            if (path.ToLower().EndsWith(_fileExtension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region helper methods
        /// <summary>
        /// converts json to csv and populates header
        /// </summary>
        /// <param name="json"></param>
        /// <param name="headerPositionMap"></param>
        /// <param name="file"></param>
        private void WriteHeaderRow(JSONDocument json, out IDictionary<int, string> headerPositionMap, StreamWriter file)
        {
            string _delim = ",";
            headerPositionMap = new HashVector<int, string>();
            string headerString = string.Empty;
            string valueString = string.Empty;
            IEnumerator e = json.GetEnumerator();
            int position = 0;
            while (e.MoveNext())
            {
                if (position != 0)
                {
                    headerString += _delim;
                    valueString += _delim;
                }
                object obj = e.Current;
                var type = obj.GetType();
                if (type.IsGenericType)
                {
                    if (type == typeof(KeyValuePair<string, object>))
                    {
                        var key = type.GetProperty("Key");
                        var value = type.GetProperty("Value");
                        var keyObj = key.GetValue(obj, null);
                        var valueObj = value.GetValue(obj, null);

                        headerPositionMap.Add(position, keyObj.ToString());

                        headerString += keyObj.ToString();

                        if (valueObj.GetType() == typeof(System.Collections.ArrayList) || valueObj.GetType() == typeof(JSONDocument))
                        {
                            // remove internal qoutes and append to start and end 
                            string rem = "\"";
                            rem += valueObj.ToString().Replace("\"", string.Empty);
                            rem += "\"";
                            valueString += rem;
                        }
                        else
                        {
                            if (valueObj.GetType() == typeof(string))
                            {
                                var val = new StringBuilder();
                                val.Append("\"");
                                val.Append(valueObj.ToString());
                                val.Append("\"");

                                valueString += val.ToString();
                            }
                            else
                            {
                                valueString += valueObj.ToString();
                            }
                        }
                        position++;
                    }
                }
            }
            file.WriteLine(headerString);
            file.WriteLine(valueString);
        }

        private IDictionary<string, object> GetCsvReadyValues(JSONDocument json)
        {
            IDictionary<string, object> valueMap = new HashVector<string, object>();
            IEnumerator e = json.GetEnumerator();

            while (e.MoveNext())
            {
                object obj = e.Current;
                var type = obj.GetType();
                if (type.IsGenericType)
                {
                    if (type == typeof(KeyValuePair<string, object>))
                    {
                        var key = type.GetProperty("Key");
                        var value = type.GetProperty("Value");
                        var keyObj = key.GetValue(obj, null);
                        var valueObj = value.GetValue(obj, null);

                        if (valueObj.GetType() == typeof(System.Collections.ArrayList) || valueObj.GetType() == typeof(JSONDocument))
                        {
                            // remove internal qoutes and append to start and end 
                            string rem = "\"";
                            rem += valueObj.ToString().Replace("\"", string.Empty);
                            rem += "\"";
                            valueMap.Add(keyObj.ToString(), rem);
                        }
                        else
                        {
                            if (valueObj.GetType() == typeof(string))
                            {
                                var val = new StringBuilder();
                                val.Append("\"");
                                val.Append(valueObj.ToString());
                                val.Append("\"");

                                valueMap.Add(keyObj.ToString(), val.ToString());
                            }
                            else
                            {
                                valueMap.Add(keyObj.ToString(), valueObj.ToString());
                            }
                        }

                    }
                }

            }
            return valueMap;
        }

        /// <summary>
        /// assigns datatypes to values csv values
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private DataType GetDataType(string str)
        {
            bool boolValue;
            Int32 intValue;
            Int64 bigintValue;
            double doubleValue;
            DateTime dateValue;
            float floatValue;

            // Place checks higher in if-else statement to give higher priority to type.
            if (!string.IsNullOrEmpty(str))
            {                
                Regex containPattern = new Regex(@"^[a-z0-9]*$", RegexOptions.IgnoreCase);
                Regex floatPattern = new Regex("^[0-9]*[.]*[0-9]*$");
                
                if (!containPattern.IsMatch(str))
                {
                    if (DateTime.TryParse(str, out dateValue))
                        return DataType.DATETIME;
                    else if (bool.TryParse(str, out boolValue))
                        return DataType.BOOLEAN;
                    else if (float.TryParse(str, out floatValue))
                        return DataType.FLOAT;
                    else return DataType.STRING;
                }
                else if (floatPattern.IsMatch(str))
                {
                    if (Int32.TryParse(str, out intValue))
                    {
                        if (str.StartsWith("0"))
                            return DataType.STRING;
                        else
                            return DataType.INTEGER;
                    }

                    else if (Int64.TryParse(str, out bigintValue))
                        return DataType.LONG;
                    else if (double.TryParse(str, out doubleValue))
                        return DataType.DOUBLE;
                    else if (float.TryParse(str, out floatValue))
                        return DataType.FLOAT;
                    else return DataType.STRING;
                }
                else
                    return DataType.STRING;
            }
            else
            {
                return DataType.SYSTEM_NULL;
            }
        }

        private IDictionary<int, string> ReadHeaderRow(StreamReader reader)
        {
            var header = new Dictionary<int, string>();
            int headerPos = 0;
            try
            {
                var value = new StringBuilder();
                char character;
                bool continueReading = true;
                // read characters till enter
                while (ReadNextCharacter(out character, reader))
                {
                    switch (character)
                    {
                        case '\r':
                            break;
                        case ' ':
                        case '\t':
                            // Trim spaces and tabs at the beginning of unquoted values
                            if (value.Length > 0)
                                value.Append(character);
                            break;
                        case '\n':
                            if (continueReading)
                            {
                                header.Add(headerPos, value.ToString());
                                headerPos++;
                            }
                            continueReading = false;
                            break;
                        case ',':
                            if (continueReading)
                            {
                                header.Add(headerPos, value.ToString());
                                headerPos++;
                            }
                            value.Clear();
                            continueReading = true;
                            break;
                        default:
                            if (!continueReading)
                                throw new InvalidDataException("Invalid Data " + character + "read");
                            value.Append(character);
                            break;

                    }

                    if (!continueReading)
                        break;
                }
                return header;
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.EXIMLogger.Error("CSVEXIMUtil.ReadHeader()", exp.ToString());
            }
            return header;
        }

        private IDictionary<int, object> ReadValueRow(StreamReader reader)
        {
            var valueMap = new Dictionary<int, object>();
            int valuePos = 0;
            try
            {
                var value = new StringBuilder();
                char character;
                bool continueReading = true;
                // read characters till enter
                while (ReadNextCharacter(out character, reader))
                {
                    switch (character)
                    {
                        case '\r':
                            break;
                        case ' ':
                        case '\t':
                            // replace empty value with string.empty and replace null in json inplace of it
                            if (value.Length > 0)
                                value.Append(character);
                            else
                                value.Append(string.Empty);
                            break;
                        case '\n':
                            if (continueReading)
                            {
                                valueMap.Add(valuePos, value.ToString());
                                valuePos++;
                            }
                            continueReading = false;
                            break;
                        case '"':
                            string val = ReadQuotedValue(reader);
                            value.Append(val);
                            break;
                        case ',':
                            if (continueReading)
                            {
                                valueMap.Add(valuePos, value.ToString());
                                valuePos++;
                            }
                            value.Clear();
                            continueReading = true;
                            break;
                        default:
                            if (!continueReading)
                                throw new InvalidDataException("Invalid Data " + character + "read");
                            value.Append(character);
                            break;

                    }
                    if (!continueReading)
                        break;
                }
                // add remaining values to map
                if(value.Length >0 && continueReading)
                {
                    valueMap.Add(valuePos, value.ToString());
                    value.Clear();
                }
                return valueMap;
            }
            catch (Exception exp)
            {
                if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                    LoggerManager.Instance.EXIMLogger.Error("CSVEXIMUtil.ReadValue()", exp.ToString());
            }
            return valueMap;
        }

        private bool ReadNextCharacter(out char character, StreamReader reader)
        {
            position += 1;
            var rchar = reader.Read();
            character = rchar < 0 ? default(char) : (char)rchar;
            return rchar >= 0;
        }

        private string ReadQuotedValue(StreamReader reader)
        {
            // reads all qouted text, including one that spans obve rmultiple lines
            var result = new StringBuilder();
            char character;
            while (ReadNextCharacter(out character, reader))
            {
                switch (character)
                {
                    case '"':
                        if (reader.Peek() != '"')
                        {
                            var resultString = result.ToString();
                            return resultString;
                        }

                        ReadNextCharacter(out character, reader);
                        result.Append('"');
                        break;
                    case ' ':
                        // Trim spaces at the beginning
                        if (result.Length > 0)
                            result.Append(character);
                        break;
                    default:
                        result.Append(character);
                        break;
                }
            }

            throw new InvalidDataException("Invalid data found " + character);
        }

        private JSONDocument CreateJSONDocument(IDictionary<int, string> header, IDictionary<int, object> value)
        {
            JSONDocument document = null;
            int pos = 0;
            var jsonString = new StringBuilder();

            // append starting brace
            jsonString.Append(@"{");

            foreach (KeyValuePair<int, string> kvp in header)
            {
                //key tag
                jsonString.Append('\"' + kvp.Value + '\"' + ":");

                object val = value[kvp.Key].ToString();
                DataType type = GetDataType(val.ToString());

                if (header[pos].Equals(_key))
                {
                    jsonString.Append('\"' + val.ToString() + '\"');
                }
                else
                {
                    if (type == DataType.STRING || type== DataType.DATETIME)
                        jsonString.Append('\"' + val.ToString() + '\"');
                    else if (type == DataType.SYSTEM_NULL)
                    {
                        // append null in place of any nullable value
                        jsonString.Append("null");
                    }
                    else
                        jsonString.Append(val.ToString());
                }

                if (pos < value.Count)
                {
                    jsonString.Append(',');
                }
                pos++;
            }

            // append ending brace
            jsonString.Append(@"}");
            document = JsonConvert.DeserializeObject<JSONDocument>(jsonString.ToString());
            return document;
        }
        #endregion

    }

    //enum dataType
    //{
    //    System_Boolean = 0,
    //    System_Int32 = 1,
    //    System_Int64 = 2,
    //    System_Double = 3,
    //    System_DateTime = 4,
    //    System_String = 5,
    //    System_Null = 6,
    //    System_Float = 7
    //}
}
