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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Updation
{
    public class Rename : IUpdation
    {
        private IEvaluable _targetField;
        private IEvaluable _newName;

        public Rename(IEvaluable targetField, IEvaluable newName)
        {
            _targetField = targetField;
            _newName = newName;
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
        }

        public bool Apply(Common.Server.Engine.IJSONDocument document)
        {
            var attribute = _targetField as Attribute;
            if (attribute != null)
            {
                return Attributor.TryRename(document, _newName.ToString(), attribute);
            }
            return false;
        }

        public List<Function> GetFunctions()
        {
            return new List<Function>();
        }

        public UpdateFunction Function
        {
            get { return UpdateFunction.Rename; }
        }

        public UpdateType Type
        {
            get { return UpdateType.Single; }
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
            output.Write(",NewName="); 
            if (_newName != null)
            {
                _newName.Print(output);
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
                string updateStr = string.Empty;
                if (_targetField != null && _newName != null)
                {
                    updateStr += "RENAME " + _targetField.InString + " TO " + _newName.InString;
                }
                return updateStr;
            }
        }
    }
}
