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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.EXIM
{
    public class JSONEXIMUtil : EXIMBase
    {
        string _fileExtension = ".json";

        public JSONEXIMUtil()
        { }

        public override RecoveryOperationStatus Write(EXIMDataType dataType, string path, string collection, string fileName, string database, List<IJSONDocument> docList)
        {

            RecoveryOperationStatus state = base.ValidatePath(path, RecoveryJobType.Export);
            if (state.Status == RecoveryStatus.Success)
            {
                try
                {
                    string file = string.Empty;
                    if (!string.IsNullOrEmpty(fileName))
                        file = Path.Combine(path, fileName + _fileExtension);
                    else
                    {
                        string defaultName = database + "_" + collection;
                        file = Path.Combine(path, defaultName + _fileExtension);
                    }

                    using (StreamWriter sw = new StreamWriter(file))
                    using (JsonWriter jw = new JsonTextWriter(sw))
                    {
                        jw.Formatting = Formatting.Indented;

                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        serializer.Serialize(jw, docList);
                        jw.Close();
                    }


                }
                catch (Exception exp)
                {
                    if (LoggerManager.Instance.EXIMLogger != null && LoggerManager.Instance.RecoveryLogger.IsErrorEnabled)
                        LoggerManager.Instance.EXIMLogger.Error("JSONEXIMUtil.Export()", exp.ToString());
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
                    using (JsonReader jsonReader = new JsonTextReader(reader))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        var itemList = serializer.Deserialize<JSONDocument[]>(jsonReader);

                        if (itemList != null)
                        {
                            foreach (var item in itemList)
                            {
                                if (items.Count <= base.ChunkSize)
                                {
                                    items.Add(item);
                                }
                                else
                                {
                                    items.Add(item);
                                    yield return items;
                                    items.Clear();
                                }
                            }
                            // if data is less than chunk size
                            if (items.Count > 0)
                                yield return items;
                        }
                        stream.Close();
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

        public override void Dispose()
        {
            throw new NotImplementedException();
        }


    }
}
