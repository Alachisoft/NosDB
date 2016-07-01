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
using System.Text.RegularExpressions;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class StringConstantValue : ConstantValue, IEvaluable, IJsonValue
    {
        private bool _possibleWildCard;
        
        public StringConstantValue(string value)
        {
            Constant = value.Trim(new [] { '\''});
        }

        public bool PossibleWildCard
        {
            set { _possibleWildCard = value; }
        }

        public bool WildCompare(string value)
        {
            return Regex.IsMatch(value, "^" + Constant.ToString().Replace("%", ".*").Replace("_", ".") + "$");
        }

        #region IEvaluable members

        public string InString
        {
            get { return "'" + Constant + "'"; }
        }

        public EvaluationType EvaluationType
        {
            get
            {
                if (!_possibleWildCard)
                {
                    return EvaluationType.Constant;
                }

                var value = (string) Constant;

                if (value.Contains("%") || value.Contains("[") && value.Contains("]") || value.Contains("_"))
                {
                    return EvaluationType.SingleVariable;
                }

                return EvaluationType.Constant;
            }
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            if (EvaluationType.Equals(EvaluationType.Constant))
            {
                value =  new StringJsonValue((string) Constant);
            }
            else
            {
                value = this;
            }
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            IJsonValue value1, value2;
            if (!Evaluate(out value1, document))
            {
                return null;
            }
            if (!evaluable.Evaluate(out value2, document))
            {
                return null;
            }
            TypeCode actualType1, actualType2;
            FieldDataType fieldDataType1 = JSONType.GetJSONType(value1.Value, out actualType1);
            FieldDataType fieldDataType2 = JSONType.GetJSONType(value2.Value, out actualType2);
            if (fieldDataType1.CompareTo(fieldDataType2) != 0)
                return null;
            return Evaluator.PerformArithmeticOperation(value1, actualType1, value2, actualType2, ArithmeticOperation.Addition,
                fieldDataType1);
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

        public List<Attribute> Attributes
        {
            get { return new List<Attribute>(); }
        }

        public System.Collections.Generic.List<Function> Functions
        {
            get { return new List<Function>(); }
        }

        public void AssignConstants(IList<IParameter> parameters) { }

        #endregion

        #region IJsonValue members 

        public FieldDataType DataType
        {
            get { return FieldDataType.String; }
        }

        public object Value
        {
            get { return Constant; }
        }

        #endregion


        public TypeCode NativeType
        {
            get { return TypeCode.String; }
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("StringConstant:{"+Constant+"}");
        }


        public string CaseSensitiveInString
        {
            get { return "'" + Constant + "'"; }
        }
    }
}