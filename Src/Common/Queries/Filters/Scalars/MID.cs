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

namespace Alachisoft.NosDB.Common.Queries.Filters.Scalars
{
    public class MID : IScalarFunction
    {
        private int _start, _length = 0;

        public MID(int start, int length = 0)
        {
            _start = start;
            _length = length;
        }

        public object Execute(string functionName, object[] parameters)
        {
            try
            {
                if (parameters.Length > 0)
                {
                    string parameter = parameters[0].ToString();
                    return parameter.Substring(_start, (_length != 0 ? _length : parameter.Length));
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "Function MID failed to execute on parameter " + parameters[0] + " for start " + _start +
                       " and length " + _length + ", " + ex.Message;
            }
        }
    }
}
