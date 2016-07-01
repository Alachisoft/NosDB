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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    /// <summary>
    /// 
    /// </summary>
    public class LoggingIdentity
    {
        public String DBName { get; private set; }
        public String ColName { get; private set; }
        public Int32 BucketID { get; internal set; }

        public LoggingIdentity(String dbName, String colName, Int32 bucketID)
        {
            DBName = dbName;
            ColName = colName;
            BucketID = bucketID;
        }

        public override int GetHashCode()
        {
            return (DBName != null ? DBName.GetHashCode() : 0) + (ColName != null ? ColName.GetHashCode() : 0) + BucketID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            LoggingIdentity other = obj as LoggingIdentity;

            if (other != null)
            {
                if (DBName != null && DBName.Equals(other.DBName) && ColName != null && ColName.Equals(other.ColName) && BucketID == other.BucketID)
                    return true;
            }

            return false;
        }
    }
}
