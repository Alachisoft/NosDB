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
using Alachisoft.NosDB.Common.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Util
{
    public class PollingOperation
    {
        private string _methodName;
        private List<Object> _parameters;
        
             

        public PollingOperation(string methodName) 
        {
            _methodName = methodName;
            _parameters = new List<object>();
        }

     
        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }
        
        public List<Object> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }
    }
}
