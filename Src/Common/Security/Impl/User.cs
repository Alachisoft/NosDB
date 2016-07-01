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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Security.Impl.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Common.Security.Impl
{
    public class User : IUser, ICompactSerializable
    {
        public User()
        {
        }

        /// <summary>
        /// Use only when setting flag for local or domain account
        /// </summary>
        /// <param name="username"></param>
        public User(string username)
        {
            Username = username;
            AccountType = SSPIUtility.IsValidLocalAccount(username) ? AccountType.LocalAccount : AccountType.DomainAccount;
        }

        public AccountType AccountType { set; get; }

        public SecurityInformationTypes SecurityInformationType
        {
            get{ return SecurityInformationTypes.User; }
        }

        public string Username { set; get; }

        public bool IsAuthenticated { set; get; }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            this.Username = reader.ReadObject() as string;
            this.AccountType = (AccountType)reader.ReadObject();

        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(this.Username);
            writer.WriteObject(this.AccountType);

        }

        public override int GetHashCode()
        {
            return Username.ToLower().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool isEqual = false;
            var user = obj as User;
            if (user != null)
            {
                user = user.Clone() as User;
                var thisUser = Clone() as User;
                if (user!=null && thisUser != null)
                {
                    if (thisUser.AccountType == AccountType.LocalAccount)
                    {
                        if (thisUser.Username.Contains('\\'))
                        {
                            thisUser.Username = thisUser.Username.Split('\\')[1];
                        }
                    }
                    if (user.AccountType == AccountType.LocalAccount)
                    {
                        if (user.Username.Contains('\\'))
                            user.Username = user.Username.Split('\\')[1];
                    }
                    if (!string.IsNullOrEmpty(user.Username))
                        isEqual = user.Username.Equals(thisUser.Username, StringComparison.CurrentCultureIgnoreCase);
                }
            }
            return isEqual;
        }

        #region IClonable members
        public object Clone()
        {
            return new User 
            {   Username = Username,
                IsAuthenticated = IsAuthenticated,
                AccountType = AccountType
            };
        }
        #endregion
    }
}
