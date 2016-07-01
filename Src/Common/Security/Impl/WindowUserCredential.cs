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
using Alachisoft.NosDB.Common.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Security.Impl
{
    public class WindowUserCredential : IUserCredential
    {
        public Enums.CredentialType CredentialType { get { return Enums.CredentialType.Windows; } }

        public string Username { set; get; }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Username))
                return 0;
            return Username.GetHashCode() ^ CredentialType.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            bool isEqual = false;
            WindowUserCredential userCredential = obj as WindowUserCredential;
            if (userCredential != null)
            {
                isEqual = this.Username.Equals(userCredential.Username);
            }
            return isEqual;
        }
    }
}
