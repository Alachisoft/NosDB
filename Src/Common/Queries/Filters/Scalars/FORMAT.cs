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
    public class FORMAT : IScalarFunction
    {
        private string _format;

        public FORMAT(string format)
        {
            _format = format;
        }

        public object Execute(string functionName, object[] parameters)
        {
            try
            {
                if (parameters.Length > 0)
                {
                    return string.Format(_format, parameters[0]);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "Function FORMAT failed to execute on parameter " + parameters[0] + " with format " +_format + ", " + ex.Message;
            }
        }
    }
}
