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
using System.ComponentModel;
using System.Reflection;

namespace Alachisoft.NosDB.Common.Security.Impl.Enums
{
    public static class DatabaseRoleDescription
    {
        public static string GetDescription(this DatabaseRole source)
        {
            string description;
            try
            {
                FieldInfo fi = source.GetType().GetField(source.ToString());

                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0) 
                    return attributes[0].Description;

                description = source.ToString();
            }
            catch (Exception)
            {
                return "";
            }

            return description;
        }
    }
}