using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SEMC.CrashLink.BackgroudService
{
    class configData
    {
        static public string PCCPath = @"\Inbox";
        static public string LUTPath = @"\LUT";
        static public string EXTPath1 = @"\PCC-external\decrypt";
        static public string EXTPath2 = @"\PCC-externalTest\decrypt";
        static public string NEW_TARGET_PATH = @"SERVER DIRECTION";
        static public string LUT_ZIP_PATH = @"\LUT";
        static public string ODM_PATH = @"\ODM";
        static public string ODM_SHORT_NAME_LIST = "";
        static public string LogName = "crashLogLink";
        static public string strconn = @"Data Source=DB Server Name\PRD1;Initial Catalog=;Persist Security Info=True;User ID=*;Password=*;Connect Timeout=300;Pooling=true";
        static public string currentDate = "";
        static public long lTotalFolderSize = 1099511627776;//1024 * 1024 * 1024 * 10124;  4378086565
    }
}