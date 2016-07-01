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
using Alachisoft.NosDB.Common.DataStructures.Clustered;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;
using Attribute = Alachisoft.NosDB.Common.JSON.Expressions.Attribute;

namespace Alachisoft.NosDB.Common.Queries.Updation
{
    public class Add : IUpdation
    {
        private IEvaluable _targetField;
        private IEvaluable[] _evaluator;
        private UpdateType _type;

        public Add(IEvaluable targetField, IEvaluable[] evaluator, UpdateType type)
        {
            _targetField = targetField;
            _evaluator = evaluator;
            _type = type;
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var eval in _evaluator)
            {
                eval.AssignConstants(parameters);
            }
        }

        public bool Apply(IJSONDocument document)
        {
            var attribute = _targetField as Attribute;
            if (attribute != null)
            {
                switch (Type)
                {
                    case UpdateType.Single:
                        IJsonValue newValue;
                        if (_evaluator[0].Evaluate(out newValue, document))
                        {
                            return Attributor.TryUpdate(document, newValue, attribute, true);
                        }
                        return false;

                    case UpdateType.Array:
                        Array array;
                        if (Attributor.TryGetArray(document, out array, attribute))
                        {
                            var values = new ClusteredArrayList(array.Length + _evaluator.Length);
                            values.AddRange(array);
                            bool additionDone = false;
                            foreach (var evaluable in _evaluator)
                            {
                                if (evaluable.Evaluate(out newValue, document))
                                {
                                    additionDone = true;
                                    values.Add(newValue.Value);
                                }
                            }
                            return additionDone && Attributor.TrySetArray(document, values.ToArray(), attribute);
                        }
                        return false;
                }
                return false;
            }
            return false;
        }

        public List<Function> GetFunctions()
        {
            var functions = new List<Function>();
            foreach (var evalator in _evaluator)
            {
                functions.AddRange(evalator.Functions);
            }
            return functions;
        }

        public UpdateFunction Function
        {
            get { return UpdateFunction.Add; }
        }

        public UpdateType Type
        {
            get { return _type; }
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("Add:{");
            output.Write("TargetField=");
            if (_targetField != null)
            {
                _targetField.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write(",Evaluations=");
            if (_evaluator != null)
            {
                output.Write("[");
                for (int i = 0; i < _evaluator.Length; i++)
                {
                    _evaluator[i].Print(output);
                    if(i!=_evaluator.Length-1)
                        output.Write(",");
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }
            output.Write(",Type="+_type.ToString());
            output.Write("}");
        }


        public string UpdateString
        {
            get
            {
                string updString = string.Empty;
                if (_evaluator != null && _evaluator.Length > 0 && _evaluator[0] != null)
                {
                    switch (_type)
                    {
                        case UpdateType.Array:
                            updString += _targetField.InString + " ADD (";
                            for (int i = 0; i < _evaluator.Length; i++)
                            {
                                updString += _evaluator[i].InString + (i + 1 == _evaluator.Length ? "" : ",");
                            }
                            updString += ")";
                            break;
                        case UpdateType.Single:
                            updString = _targetField.InString + " = " + _evaluator[0].InString;
                            break;
                    }
                }
                return updString;
            }
        }
    }
}
