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
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Storage.Indexing;
using CSharpTest.Net.Serialization;

namespace Alachisoft.NosDB.Core.Storage.Indexing
{
   

 
    

    public class IndexOpSerializer<T> : ISerializer<IndexOp<T>>
    {
        public static IndexOpSerializer<T> Global = new IndexOpSerializer<T>(); 

        public void WriteTo(IndexOp<T> value, System.IO.Stream stream)
        {

        }

        public IndexOp<T> ReadFrom(System.IO.Stream stream)
        {
            return null;
        }
    }
}
