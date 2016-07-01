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
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class BooleanConstantValue : ConstantValue, IEvaluable
    {
        private string _lexeme; //Track case sensitive value i.e FaLsE
        public BooleanConstantValue(bool value, string lexeme = null)
        {
            Constant = value;
            _lexeme = lexeme;
        }

        #region IEvaluable members

        public string InString
        {
            get { return Constant.ToString(); }
        }

        public string CaseSensitiveInString
        {
            get { return _lexeme ?? Constant.ToString(); }
        }
        public EvaluationType EvaluationType { get { return EvaluationType.Constant; } }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value =  new BooleanJsonValue((bool)Constant);
            return true;
        }

        public System.IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public System.IComparable Subtract(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public System.IComparable Multiply(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public System.IComparable Divide(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public System.IComparable Modulate(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
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
            output.Write("BooleanConstant:{"+Constant+"}");
        }


    }
}
