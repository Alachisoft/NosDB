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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Configuration.Services;
using System.Management.Automation;

namespace Alachisoft.NosDB.NosDBPS
{
    public abstract class NodeDetail
    {
        public PathType NodeType { get; set; }
        public string NodeName { get; set; }
        public bool IsContainer { get; set; }
        public bool IsValid { get; set; }
        public PrintableTable ChilderanTable { get; set; }
        public object Configuration { get; set; }
        internal string[] PathChunks { get; set; }
        internal NosDBPSDriveInfo Drive { get; set; }
        public List<string> ChilderanName { get; set; }
        
        public abstract bool TryGetNodeDetail(out NodeDetail nodeDetail);

        public string[] SplitPath(string path)
        {
            string normalPath = NormalizePath(path);

            string pathNoDrive = normalPath.Replace(Drive.Root
                                           + ProviderUtil.SEPARATOR, "");

            return pathNoDrive.Split(ProviderUtil.SEPARATOR.ToCharArray());
        }

        private string NormalizePath(string path)
        {
            string result = path;

            if (!String.IsNullOrEmpty(path))
            {
                result = path.Replace("/", ProviderUtil.SEPARATOR);
            }

            return result;
        } 
    }
}
