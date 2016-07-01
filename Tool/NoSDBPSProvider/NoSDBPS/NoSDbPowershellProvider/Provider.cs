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
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;
using Alachisoft.NosDB.Common.Configuration;


namespace Alachisoft.NosDB.NosDBPS
{
    #region NoSDBProvider

    [CmdletProvider("NoSDB", ProviderCapabilities.ExpandWildcards)]
    public class Provider : NavigationCmdletProvider
    {

        #region Drive manipulation
       
        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            // Check if the drive object is null.
            if (drive == null)
            {
                WriteError(new ErrorRecord(
                           new ArgumentNullException("drive"),
                           "NullDrive",
                           ErrorCategory.InvalidArgument,
                           null));

                return null;
            }
            if (String.IsNullOrEmpty(drive.Root) )
            {
                WriteError(new ErrorRecord(
                           new ArgumentException("drive.Root"),
                           "NoRoot",
                           ErrorCategory.InvalidArgument,
                           drive));

                return null;
            }
            NosDBPSDriveInfo noSPSDrive = new NosDBPSDriveInfo(drive);
            return noSPSDrive;
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            if (drive == null)
            {
                WriteError(new ErrorRecord(
                           new ArgumentNullException("drive"),
                           "NullDrive",
                           ErrorCategory.InvalidArgument,
                           drive));
                return null;
            }
            NosDBPSDriveInfo noSDrive = drive as NosDBPSDriveInfo;
            return noSDrive;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            PSDriveInfo drive = new PSDriveInfo(ProviderUtil.DRIVE_NAME, this.ProviderInfo, ProviderUtil.DRIVE_ROOT, "", null);
            NosDBPSDriveInfo nosdrive = new NosDBPSDriveInfo(drive);
            Collection<PSDriveInfo> drives = new Collection<PSDriveInfo>() { nosdrive };
            return drives;
        }

        #endregion

        #region Item Methods

        protected override void GetItem(string path)
        {
            //if(PathIsDrive(path))
            //{
            //    WriteItemObject(this.PSDriveInfo, path, true);
            //}

            //string[] pathNodes = SplitPath(path);
            //string[] pathChunks = SplitPath(path);
            //NodeDetail thisNode;
            //if (new NoSDbDetail(pathChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
            //{

            //}



            
        }

        
        protected override void SetItem(string path, object values)
        {
            throw new NotImplementedException();
        }
        protected override bool ItemExists(string path)
        {
            
            if (PathIsDrive(path))
            {
                return true;
            }
            string[] pathChunks = SplitPath(path);
            if (path.EndsWith("*"))
            {
                
                NodeDetail parent = null;
                if (TryGetParentDetail(path, out parent))
                {
                    string partialName = pathChunks[pathChunks.Length - 1].Split('*')[0];
                    foreach (string child in parent.ChilderanName)
                    {
                        if (child.ToLower().StartsWith(partialName.ToLower()))
                            return true;
                    }
                }
            }
            
            else
            {
                NodeDetail thisNode;
                if (new NoSDbDetail(pathChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
                {
                    return thisNode.IsValid;
                }
            }
            return false;

        } 

        protected override bool IsValidPath(string path)
        {
            if (PathIsDrive(path))
            {
                return true;
            }
            string[] pathChunks = SplitPath(path);
            NodeDetail thisNode;
            if (new NoSDbDetail(pathChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
            {
                return thisNode.IsValid;
            }

            return false;
        }

        #endregion Item Overloads

        #region Container Overloads

        protected override void GetChildItems(string path, bool recurse)
        {
            if (PathIsDrive(path))
            {
                NodeDetail thisNode=new NoSDbDetail(null, this.PSDriveInfo);
                WriteItemObject("\n    Context: " + path + "\n", path, false);
                PrintableTable childs = thisNode.ChilderanTable;
                WriteItemObject(childs.GetTableRows(), path, false);
                WriteItemObject("", path, false);
            }
            else
            {
                try
                {
                    ConfigurationConnection.UpdateClusterConfiguration();
                    ConfigurationConnection.UpdateDatabaseClusterInfo();
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                string[] pathChunks = SplitPath(path);
                NodeDetail thisNode;
                if (new NoSDbDetail(pathChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
                {

                    WriteItemObject("\n    Context: " + path + "\n", path, false);
                    PrintableTable childs = thisNode.ChilderanTable;
                    WriteItemObject(childs.GetTableRows(), path, false);
                    WriteItemObject("", path, false);
                }
            }
        } 

        protected override bool HasChildItems(string path)
        {
            return true;
        } 

        protected override void NewItem(string path, string type, object newItemValue)
        {
          throw new Exception("Cann't create " + path);
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            throw new NotImplementedException();
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            throw new NotImplementedException();
        }

        #endregion Container Overloads

        #region Navigation
        
        protected override string MakePath(string parent, string child)
        {
            string result = base.MakePath(parent, child);
            return result;
        }

        protected override bool IsItemContainer(string path)
        {
            if(PathIsDrive(path))
            {
                return true;
            }
            string[] pathChunks = SplitPath(path);
            ClusterConfiguration configuration = ConfigurationConnection.ClusterConfiguration;
            NodeDetail thisNode;
            if (new NoSDbDetail(pathChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
            {
                  return thisNode.IsContainer;
            }
                return false;

        }

        protected override string[] ExpandPath(string path)
        {
            List<string> childs = new List<string>();
            string[] pathChunks = SplitPath(path);
            NodeDetail thisNode = null;
            if (pathChunks[pathChunks.Length - 1].EndsWith("*"))
            {
                NodeDetail parent = null;
                if(TryGetParentDetail(path,out parent))
                {
                    if (parent.ChilderanName!=null)
                    {
                        string partialName = pathChunks[pathChunks.Length - 1].Split('*')[0];
                        string parentPath = path.Substring(0, path.LastIndexOf(ProviderUtil.SEPARATOR));
                        foreach (string child in parent.ChilderanName)
                        {
                            if (child.ToLower().StartsWith(partialName.ToLower()))
                                childs.Add(parentPath + "\\" + child);
                        }
                    }
                }
            }

            return childs.ToArray();
        }

        protected override string GetChildName(string path)
        {
            if (ConfigurationConnection.ClusterConfiguration == null)
                throw new Exception("Unable to resolve path, as not connected with any configuration server.");
            return base.GetChildName(path);
            /*
            string result = string.Empty;
            if (this.PathIsDrive(path))
            {
                return path;
            }
            string[] pathChunks = SplitPath(path);
            if (ConfigurationConnection.ConfigCluster== null)
            {

                string errorString = "cann't resolve path: " + path;
                errorString += "\nDetails: Configuration Manager Should be Created using New-ConfigManager cmdlet.\n";
                errorString += "or should be connected using Connect-ConfigManager cmdlet. \n";
                errorString += "Use Get-Help cmdlet for detailed help";
                throw new Exception(errorString);
            }
            else if (pathChunks[0] != ConfigurationConnection.ConfigCluster.Name)
            {
                if (pathChunks[0].EndsWith("*"))
                {
                    if(ConfigurationConnection.ConfigCluster.Name.ToLower().StartsWith(pathChunks[0].Split('*')[0].ToLower()))
                    {
                        return ConfigurationConnection.ConfigCluster.Name;
                    }
                }
                else
                {
                    throw new Exception("Do not recognize configuration manager: " + pathChunks[0]);
                }
            }
            NodeDetail thisNode=null;
            if (pathChunks[pathChunks.Length - 1].EndsWith("*"))
            {
                //string partialName = pathChunks[pathChunks.Length - 1].Split('*')[0];

                //string[] parentChunks = new string[pathChunks.Length - 1];
                //Array.Copy(pathChunks, 0, parentChunks, 0, pathChunks.Length - 1);
                //if (new NoSDbDetail(parentChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
                //{
                //    if (thisNode.ChilderanName != null)
                //    {
                //        foreach (string child in thisNode.ChilderanName)
                //        {
                //            if (child.ToLower().StartsWith(partialName.ToLower()))
                //                return child;

                //        }
                //    }

                //}
                return string.Empty;
            }
            else
            {
                if (new NoSDbDetail(pathChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
                {
                    return thisNode.NodeName;

                }
                else
                {
                    try
                    {
                        ConfigurationConnection.UpdateClusterConfiguration();
                    }
                    catch (Exception ex)
                    {
                        WriteError(new ErrorRecord(ex, "", ErrorCategory.ReadError, null));
                    }
                    if (new NoSDbDetail(pathChunks, this.PSDriveInfo).TryGetNodeDetail(out thisNode))
                    {
                        return thisNode.NodeName;
                    }
                    throw new Exception("Cann't resolve path: " + path);

                }

            }
            
            
 */
        }
        
        protected override void MoveItem(string path, string destination)
        {
            throw new NotImplementedException();
        }

        #endregion Navigation

        private bool PathIsDrive(string path)
        {
            if (String.IsNullOrEmpty(path.Replace(this.PSDriveInfo.Root, "")) ||
                String.IsNullOrEmpty(path.Replace(this.PSDriveInfo.Root + ProviderUtil.SEPARATOR, "")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string[] SplitPath(string path)
        {
            string normalPath = NormalizePath(path);

            string pathNoDrive = normalPath.Replace(this.PSDriveInfo.Root
                                           + ProviderUtil.SEPARATOR, "");

            return pathNoDrive.Split(ProviderUtil.SEPARATOR.ToCharArray());
        }
        private bool TryGetParentDetail(string path, out NodeDetail thisNode)
        {
            string[] pathChunks = SplitPath(path);
            if (pathChunks.Length == 1)
            {
                thisNode = new NoSDbDetail(null, PSDriveInfo);
                return true;
            }
            var parentChunks = new string[pathChunks.Length - 1];
            Array.Copy(pathChunks, 0, parentChunks, 0, pathChunks.Length - 1);
            return (new NoSDbDetail(parentChunks, PSDriveInfo).TryGetNodeDetail(out thisNode));
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

    #endregion NoSDBProvider
}

    









