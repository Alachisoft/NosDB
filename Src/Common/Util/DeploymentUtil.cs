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
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Logger;

namespace Alachisoft.NosDB.Common.Util
{
    public class DeploymentUtil
    {
        public static void GetDeployment(IConfigurationSession configSession, string deploymentId)
        {
            IDictionary<string, Byte[]> assArr;
            try
            {
                assArr = configSession.GetDeploymentSet(deploymentId);
            }
            catch (Exception exception)
            {

                if (LoggerManager.Instance.StorageLogger != null && LoggerManager.Instance.StorageLogger.IsErrorEnabled)
                    LoggerManager.Instance.StorageLogger.Error("DeploymentUtil.GetDeployment()", "Error Getting Deployment. Deployment Id: " + deploymentId + " " + exception.Message);
                throw new Exception("Error Getting Deployment" + exception.Message);
            }

            if (assArr.Count <= 0) return;
            string path = (Path.Combine(Path.Combine(Path.Combine(AppUtil.InstallDir, "database"), "deployment"), deploymentId));
            Directory.CreateDirectory(path.Trim());
            foreach (KeyValuePair<string, Byte[]> pair in assArr)
            {
                if (File.Exists(path + "\\" + pair.Key)) continue;
                var fs = new FileStream(path + "\\" + pair.Key, FileMode.Create, FileAccess.Write);
                fs.Write(pair.Value, 0, pair.Value.Length);
                fs.Flush();
                fs.Close();
            }
        }
    }

}
