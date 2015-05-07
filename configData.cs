using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SEMC.CrashLink.BackgroudService
{
    class configData
    {
        static public string PCCPath = @""; //The source directory
       static public string LUTPath = @""; //The source directory
       static public string LogName = "crashLogLink";
       static public string strconn = @"Data Source=SERVERNAME\PRD1;Initial Catalog=**;Persist Security Info=True;User ID=**;Password=**;Connect Timeout=300;Pooling=true";
       static public string currentDate = "";
       static public long lTotalFolderSize = 1099511627776;//1024 * 1024 * 1024 * 10124;  4378086565
    }
}
