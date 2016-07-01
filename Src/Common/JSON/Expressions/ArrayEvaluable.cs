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
    public class ArrayEvaluable : IEvaluable
    {
        readonly List<IEvaluable> _array = new List<IEvaluable>();

        public void AddElement(IEvaluable element)
        {
            _array.Add(element);
        }

        #region IEvaluable members

        public string InString
        {
            get
            {
                string value = "[";

                for (int i = 0; i < _array.Count; i++)
                {
                    if (_array[i] is StringConstantValue)
                    {
                        value += "\"" + _array[i].InString.Trim('\'') + "\"";
                    }
                    else
                    {
                        value += _array[i].InString;
                    }

                    if (i != _array.Count - 1)
                    {
                        value += ",";
                    }
                }

                return value + "]";
            }
        }

        public string CaseSensitiveInString
        {
            get
            {
                string value = "[";

                for (int i = 0; i < _array.Count; i++)
                {
                    if (_array[i] is StringConstantValue)
                    {
                        value += "\"" + _array[i].CaseSensitiveInString.Trim('\'') + "\"";
                    }
                    else
                    {
                        value += _array[i].CaseSensitiveInString;
                    }

                    if (i != _array.Count - 1)
                    {
                        value += ",";
                    }
                }

                return value + "]";
            }
        }


        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = null;
            List<IJsonValue> values = new List<IJsonValue>();
            foreach (var element in _array)
            {
                IJsonValue outValue;
                if (element.Evaluate(out outValue, document))
                {
                    values.Add(outValue);
                }
                else
                {
                    return false;
                }
            }

            if (values.Count > 0)
            {
                value = new ArrayJsonValue(values.ToArray());
            }
            else // if Array is empty
            {
                value = new ArrayJsonValue(new IJsonValue[0]);
            }
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

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var elem in _array)
            {
                elem.AssignConstants(parameters);
            }
        }

        public EvaluationType EvaluationType
        {
            get
            {
                EvaluationType evaluation = EvaluationType.Constant;
                foreach (var evaluable in _array)
                {
                    switch (evaluable.EvaluationType)
                    {
                        case EvaluationType.AllVariable:
                            return EvaluationType.AllVariable;

                        case  EvaluationType.MultiVariable:
                            evaluation = EvaluationType.MultiVariable;
                            break;

                        case EvaluationType.SingleVariable:
                            if (evaluation != EvaluationType.MultiVariable)
                            {
                                if (evaluation == EvaluationType.SingleVariable)
                                {
                                    evaluation = EvaluationType.MultiVariable;
                                }
                                else
                                {
                                    evaluation = EvaluationType.SingleVariable;
                                }
                            }
                            break;
                    }
                }
                return evaluation;
            }
        }

        public List<Attribute> Attributes
        {
            get
            {
                List<Attribute> attributes = new List<Attribute>();
                foreach (var elem in _array)
                {
                    attributes.AddRange(elem.Attributes);
                }
                return attributes;
            }
        }

        public List<Function> Functions
        {
            get
            {
                List<Function> attributes = new List<Function>();
                foreach (var elem in _array)
                {
                    attributes.AddRange(elem.Functions);
                }
                return attributes;
            }
        }

        #endregion

        public void Print(System.IO.TextWriter output)
        {
            output.Write("ArrayEvaluable:{");
            output.Write("Array=");
            if (_array != null)
            {
                output.Write("[");
                for (int i = 0; i < _array.Count; i++)
                {
                    _array[i].Print(output);
                    if (i != _array.Count-1)
                        output.Write(",");
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }



    }
}
