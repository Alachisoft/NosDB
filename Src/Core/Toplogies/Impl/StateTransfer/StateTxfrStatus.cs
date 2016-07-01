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
    //public class EntityStatusInfo
    //{
    //    string EntityName { get; set; }
    //    StateTxfrStatus Status { get; set; }

    //    public EntityStatusInfo(string entityName, StateTxfrStatus status)
    //    {
    //        this.EntityName = entityName;
    //        this.Status = status;
    //    }

    //    public override bool Equals(EntityStatusInfo info)
    //    {
    //       return info.EntityName == EntityName;
    //    }
    //}

    public enum StateTxfrStatus : int
    {        
        Waiting=0,

        Running = 1,

        Failed=2,
        
        CompletedSuccessfully = 3,
    }
}
