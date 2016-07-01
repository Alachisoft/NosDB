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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class DocumentEvaluable : IEvaluable, IEnumerable<KeyValuePair<string, IEvaluable>>
    {
        private readonly List<KeyValuePair<string, IEvaluable>> _document = 
            new List<KeyValuePair<string, IEvaluable>>();

        public void AddAttributeValue(KeyValuePair<string, IEvaluable> pair)
        {
            _document.Add(pair);
        }

        #region IEvaluable members

        public string InString
        {
            get
            {
                string value = "{";

                for (int i = 0; i < _document.Count; i++)
                {
                    value += "\"" + _document[i].Key + "\":" ;

                    if (_document[i].Value is StringConstantValue)
                    {
                        value += "\"" + _document[i].Value.InString.Trim('\'') + "\"";
                    }
                    else
                    {
                        value += _document[i].Value.InString;
                    }

                    if (i != _document.Count - 1)
                    {
                        value += ",";
                    }
                }

                return value + "}";
            }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = null;
            JSONDocument doc = new JSONDocument();
            foreach (var pair in _document)
            { 
                IJsonValue outValue;
                if (pair.Value.Evaluate(out outValue, document))
                {
                    doc.Add(pair.Key, outValue.Value);
                }
                else
                {
                    return false;
                }
            }

            if (doc.Count > 0)
            {
                value = new ObjectJsonValue(doc);
            }
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Subtract(IEvaluable evaluable,IJSONDocument document)
        {
            return null;
        }

        public IComparable Multiply(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Divide(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Modulate(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var pair in _document)
            {
                pair.Value.AssignConstants(parameters);
            }
        }

        public EvaluationType EvaluationType
        {
            get
            {
                return EvaluationType.AllVariable;
            }
        }

        public List<Attribute> Attributes
        {
            get
            {
                List<Attribute> attributes = new List<Attribute>();
                foreach (var pair in _document)
                {
                    attributes.AddRange(pair.Value.Attributes);
                }
                return attributes;
            }
        }

        public List<Function> Functions
        {
            get
            {
                List<Function> functions = new List<Function>();
                foreach (var pair in _document)
                {
                    functions.AddRange(pair.Value.Functions);
                }
                return functions;
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<string, IEvaluable>> members

        public IEnumerator<KeyValuePair<string, IEvaluable>> GetEnumerator()
        {
            return _document.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void Print(System.IO.TextWriter output)
        {
            output.Write("DocumentEvaluable:{");
            output.Write("List=");
            if (_document != null)
            {
                output.Write("[");
                for (int i = 0; i < _document.Count; i++)
                {
                    output.Write("{");
                    output.Write("key=" + (_document[i].Key ?? "null"));
                    output.Write(", value=");
                    if (_document[i].Value != null)
                    {
                        _document[i].Value.Print(output);
                    }
                    else
                    {
                        output.Write("null");
                    }
                    output.Write("}");
                    if (i != _document.Count - 1)
                    {
                        output.Write(",");
                    }
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }


        public string CaseSensitiveInString
        {
            get
            {
                string value = "{";

                for (int i = 0; i < _document.Count; i++)
                {
                    value += "\"" + _document[i].Key + "\":";

                    if (_document[i].Value is StringConstantValue)
                    {
                        value += "\"" + _document[i].Value.CaseSensitiveInString.Trim('\'') + "\"";
                    }
                    else
                    {
                        value += _document[i].Value.CaseSensitiveInString;
                    }

                    if (i != _document.Count - 1)
                    {
                        value += ",";
                    }
                }

                return value + "}";
            }
        }
    }
}
