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
    public class Replace : IUpdation
    {
        private IEvaluable _targetField;
        private KeyValuePair<IEvaluable,IEvaluable>[] _evaluators;
        private UpdateType _type;

        public Replace(IEvaluable targetField, KeyValuePair<IEvaluable, IEvaluable>[] evaluators, UpdateType type)
        {
            _targetField = targetField;
            _evaluators = evaluators;
            _type = type;
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var eval in _evaluators)
            {
                eval.Key.AssignConstants(parameters);
                eval.Value.AssignConstants(parameters);
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
                        IJsonValue value;
                        if (_evaluators[0].Value.Evaluate(out value, document))
                        {
                            return Attributor.TryUpdate(document, value, attribute, true);
                        }
                        return false;

                    case UpdateType.Array:
                        Array array;
                        if (Attributor.TryGetArray(document, out array, attribute))
                        {
                            var values = new ClusteredArrayList(array.Length + _evaluators.Length);
                            values.AddRange(array);

                            bool isChangeApplicable = false;
                            foreach (var keyValuePair in _evaluators)
                            {
                                IJsonValue oldValue, newValue;

                                if (keyValuePair.Key.Evaluate(out oldValue, document) &&
                                    keyValuePair.Value.Evaluate(out newValue, document))
                                {
                                    int index = -1;
                                    while ((index = values.IndexOf(oldValue.Value)) >= 0)
                                    {
                                        //bugfix: infinite loop for replace 1=1
                                        //New and old values are same and values contain old value
                                        if (oldValue.CompareTo(newValue) == 0)
                                        {
                                            isChangeApplicable = true;
                                            break;
                                        }
                                        values[index] = newValue.Value;
                                        isChangeApplicable = true;
                                    }
                                }
                            }
                            return isChangeApplicable && Attributor.TrySetArray(document, values.ToArray(), attribute);
                        }
                        return false;
                }
                return false;
            }
            return false;
        }

        public List<Function> GetFunctions()
        {
            List<Function>  functions = new List<Function>();
            if (_evaluators != null)
                foreach (var evaluable in _evaluators)
                {
                    if(evaluable.Value != null && evaluable.Value.Functions.Count > 0)
                        functions.AddRange(evaluable.Value.Functions);
                }
            return functions;
        }

        public UpdateFunction Function
        {
            get { return UpdateFunction.Replace; }
        }

        public UpdateType Type
        {
            get { return _type; }
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("Rename:{");
            output.Write("TargetField=");
            if (_targetField != null)
            {
                _targetField.Print(output);
            }
            else
            {
                output.Write("null");
            }

            output.Write(",Replacements=");
            if (_evaluators != null)
            {
                output.Write("[");
                for (int i = 0; i < _evaluators.Length; i++)
                {
                    output.Write("Old=");
                    if (_evaluators[i].Key != null)
                    {
                        _evaluators[i].Key.Print(output);
                    }
                    else
                    {
                        output.Write("null");
                    }
                    output.Write(",");
                    output.Write("New=");
                    if (_evaluators[i].Value != null)
                    {
                        _evaluators[i].Value.Print(output);
                    }
                    else
                    {
                        output.Write("null");
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


        public string UpdateString
        {
            get
            {
                string updString = string.Empty;
                if (_evaluators != null && _evaluators.Length > 0 && _evaluators[0].Key != null &&
                    _evaluators[0].Value != null)
                {
                    switch (_type)
                    {
                        case UpdateType.Array:
                            updString += _targetField.InString + " REPLACE (";
                            for (int i = 0; i < _evaluators.Length; i++)
                            {
                                updString += _evaluators[i].Key.InString + "=" + _evaluators[i].Value.InString + (i + 1 == _evaluators.Length ? "" : ",");
                            }
                            updString += ")";
                            break;
                        case UpdateType.Single:
                            updString = _targetField.InString + " = " + _evaluators[0].Value.InString;
                            break;
                    }
                }
                return updString;
            }
        }
    }
}
