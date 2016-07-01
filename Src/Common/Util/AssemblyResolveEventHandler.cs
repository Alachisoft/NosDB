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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alachisoft.NosDB.Common.Util
{
    public class AssemblyResolveEventHandler
    {
        //static AssemblyResolveEventHandler()
        //{

        //    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(DeployAssemblyHandler);

        //}

        public static string DatabasePath = "";

        public static System.Reflection.Assembly DeployAssemblyHandler(object sender, ResolveEventArgs args)
        {
            Assembly asm = null;
            string asmName = new AssemblyName(args.Name).Name;
            if (!Path.GetExtension(asmName).Equals(".dll"))
            {
                asmName += ".dll";
            }

            string deployAssemblyDirPath = string.Empty;
            if (!(string.IsNullOrEmpty(DatabasePath)))
            {
                try
                {
                    //deployAssemblyDirPath = Path.Combine(AppUtil.DeployedAssemblyDir, DatabasePath);
                    asm = Assembly.LoadFrom(DatabasePath + "\\" + asmName);
                    return asm;
                }
                catch (Exception ex) { }
            }
            else
            {
                deployAssemblyDirPath = AppUtil.DeployedAssemblyDir;
                string[] deployDirectories = Directory.GetDirectories(deployAssemblyDirPath);

                foreach (string deploy in deployDirectories)
                {
                    try
                    {
                        //asm = Assembly.LoadFrom(deploy + "\\" + asmName);
                        break;
                    }
                    catch (Exception ex) { }
                }
            }
            return asm;
        }
    }
}
