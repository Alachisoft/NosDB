using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Alachisoft.NosDB.NosDBPS
{
   [Cmdlet("Get", "NosDBVersion")]
   public class NosDBInstallationType:PSCmdlet
    {
      
       protected override void ProcessRecord()
       {
           string logo = AppUtil.GetUtilityLogo("Get-NosDBVersion");
           WriteObject(logo + "\n");
           WriteObject("Edition Installed: OpenSource NosDB Server");
           PrintUserInfo();
          
       }

       public void PrintUserInfo()
       {
           string USER_KEY = RegHelper.ROOT_KEY + @"\UserInfo";
           string firstName = (string)RegHelper.GetRegValue(USER_KEY, "firstname", 4);
           string lastName = (string)RegHelper.GetRegValue(USER_KEY, "lastname", 4);
           string company = (string)RegHelper.GetRegValue(USER_KEY, "company", 4);
           string email = (string)RegHelper.GetRegValue(USER_KEY, "email", 4);

           WriteObject("This product is registered to \nUser\t:\t" + firstName + " " + lastName + "\nEmail\t:\t" + email + "\nCompany\t:\t" + company+"\n");
           
          // return LicenseManager.LicenseMode(null);
       }

       public void PrintLogo()
       {
           string logo ="\n" +@"Alachisoft (R) NosDB Utility Version 1.3.0.0" +
               "\n" + @"Copyright (C) Alachisoft 2016. All rights reserved.";
           WriteObject(logo);
       }

    }
}
