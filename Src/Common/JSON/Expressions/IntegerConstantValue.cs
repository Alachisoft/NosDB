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
    public class IntegerConstantValue : ConstantValue, IEvaluable
    {
        public IntegerConstantValue(string lexeme)
        {
            Constant = Convert.ToInt64(lexeme);
        }

        public IntegerConstantValue(long lexeme)
        {
            Constant = lexeme;
        }

        public void SetNegative()
        {
            Constant = Convert.ToInt64(Constant) * -1; 
        }

        private IComparable PerformArithmeticOperation(IEvaluable evaluable, IJSONDocument document, ArithmeticOperation operation)
        {
            IJsonValue value1, value2;

            if (!Evaluate(out value1, document))
                return null;

            if(!evaluable.Evaluate(out value2, document))
                return null;

            TypeCode actualType1, actualType2;
            FieldDataType fieldDataType1 = JSONType.GetJSONType(value1.Value, out actualType1);
            FieldDataType fieldDataType2 = JSONType.GetJSONType(value2.Value, out actualType2);
            if (fieldDataType1.CompareTo(fieldDataType2) != 0)
                return null;
            return Evaluator.PerformArithmeticOperation(value1, actualType1, value2, actualType2, operation,
                fieldDataType1);
        }

        #region IEvaluable members

        public string InString
        {
            get { return Constant.ToString(); }
        }

        public EvaluationType EvaluationType { get { return EvaluationType.Constant; } }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = new NumberJsonValue(Constant);
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Addition);
        }

        public IComparable Subtract(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Subtraction);
        }

        public IComparable Multiply(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Multiplication);
        }

        public IComparable Divide(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Division);
        }

        public IComparable Modulate(IEvaluable evaluable, IJSONDocument document)
        {
            return PerformArithmeticOperation(evaluable, document, ArithmeticOperation.Modulus);
        }

        public System.Collections.Generic.List<Attribute> Attributes
        {
            get { return new List<Attribute>(); }
        }

        public System.Collections.Generic.List<Function> Functions
        {
            get { return new List<Function>(); }
        }

        public void AssignConstants(IList<IParameter> parameters) { }

        #endregion


        public void Print(System.IO.TextWriter output)
        {
            output.Write("IntegerConstant:{"+Constant.ToString()+"}");
        }


        public string CaseSensitiveInString
        {
            get { return Constant.ToString(); }
        }
    }
}