﻿// /*
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
using Alachisoft.NosDB.Common.Net;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl
{
    [Serializable]
    public class Server
    {
        private Status status;

        public Server(Address address,Status status) 
        {
            Address = address;
            Status = status;

        }
        public Address Address { get; set; }
        public Status Status
        {
            set { status = value; }
            get { return status; } 
        }
        // removed all the other code that as it was irrelevant

        public bool Equals(Server other)
        {
            // First two lines are just optimizations
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return this.Address.Equals(other.Address);
        }

        public override bool Equals(object obj)
        {
            // Again just optimization
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            // Actually check the type, should not throw exception from Equals override
            if (obj.GetType() != this.GetType()) return false;

            // Call the implementation from IEquatable
            return Equals((Server)obj);
        }

        public override int GetHashCode()
        {
           return  this.Address.GetHashCode();           
        }

    }
}
