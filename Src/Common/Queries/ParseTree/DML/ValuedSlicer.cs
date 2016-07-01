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
using System.IO;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;
using Attribute = Alachisoft.NosDB.Common.JSON.Expressions.Attribute;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class ValuedSlicer : IEvaluable
    {
        private readonly IEvaluable _expression;
        private readonly ValueList _values;

        public ValuedSlicer(IEvaluable expression, ValueList values)
        {
            _expression = expression;
            _values = values;
        }

        #region IEvaluable members.

        public string InString
        {
            get
            {
                string value = "(" + _expression.InString + ") SLICE MATCH (";

                for (int i = 0; i < _values.Count; i++)
                {
                    value += _values[i].InString;
                    if (i != _values.Count - 1)
                    {
                        value += ",";
                    }
                }

                return value + ")";
            }
        }

        public string CaseSensitiveInString
        {
            get
            {
                return _expression.ToString(); // dont't need case sensitive name for value slicer
                //string value = "(" + _expression.CaseSensitiveInString + ") SLICE MATCH (";

                //for (int i = 0; i < _values.Count; i++)
                //{
                //    value += _values[i].CaseSensitiveInString;
                //    if (i != _values.Count - 1)
                //    {
                //        value += ",";
                //    }
                //}

                //return value + ")";
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
            get { return _expression.Attributes; }
        }

        public List<Function> Functions
        {
            get { return _expression.Functions; }
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var value in _values)
            {
                value.AssignConstants(parameters);
            }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = null;
            IJsonValue outJsonValue;

            if (!_expression.Evaluate(out outJsonValue, document))
            {
                return false;
            }

            if (outJsonValue.DataType != FieldDataType.Array && outJsonValue.DataType != FieldDataType.Embedded)
            {
                return false;
            }

            List<IJsonValue> returnValues = new List<IJsonValue>();

            ArrayJsonValue arrayValue;
            if (outJsonValue is EmbeddedList)
            {
                arrayValue = new ArrayJsonValue(((EmbeddedList)outJsonValue).WrapedValue.ToArray());
            }
            else
                arrayValue = (ArrayJsonValue)outJsonValue;
            
            for (int i = 0; i < _values.Count; i++)
            {
                IJsonValue compValue;
                if (!_values[i].Evaluate(out compValue, document))
                {
                    return false;
                }

                if(arrayValue.Contains(compValue))
                {
                    if (compValue.DataType == FieldDataType.Array)
                        returnValues.AddRange(((ArrayJsonValue)compValue).WrapedValue);
                    else if (compValue.DataType == FieldDataType.Embedded)
                        returnValues.AddRange(((EmbeddedList)compValue).WrapedValue);
                    else
                        returnValues.Add(compValue);
                }
                else
                {
                    return false;
                }
            }

            value = new ArrayJsonValue(returnValues.ToArray());
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Subtract(IEvaluable evaluable, IJSONDocument document)
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

        public void Print(TextWriter output)
        {
            output.Write("ValuedSlicer:{");
            output.Write(_expression.InString +
                " SLICE MATCH :<");

            for (int i = 0; i < _values.Count; i++)
            {
                output.Write(_values[i].InString);

                if (i != _values.Count - 1)
                {
                    output.Write(",");
                }
            }
            output.Write(">}");
        }

        public override string ToString()
        {
            return _expression.ToString();
        }

        #endregion


    }
}
