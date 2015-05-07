using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SEMC.CrashLink.BackgroudService.interfaces;
using SEMC.CrashLink.BackgroudService.model;
using SEMC.CrashLink.BackgroudService.Properties;

namespace SEMC.CrashLink.BackgroudService.utils
{
    class FileOperator
    {
        public static bool validateFileName(AFileNamePhaser phaser, String fileName) {
            string imeiNumber = phaser.getImeiNumber(fileName);
            string deliveryTime = phaser.getDeliveryTime(fileName);

            if(deliveryTime != null){
                try
                {
                    DateTime.Parse(deliveryTime);
                }
                catch(Exception ex){
                    Trace.WriteLine("delivery time is not null, but " + ex.Message);
                    return false;
                }
            }

            if (deliveryTime == null || imeiNumber == null)
                return false;
            else
                return true;
        }

        public static string getCrashType(String fileName) {
            string crashType = null;
            if (fileName.Contains("CoreDump") ||
                    fileName.Contains("Crash-SSR-YMD-HMS") ||
                    fileName.Contains("Crash-YMD-HMS"))
            {
                crashType = "Core";
            }

            if (fileName.Contains("BugReport"))
            {
                crashType = "App";
            }

            return crashType;
        }


        public static void getCrashFile(string filePath, ref List<CrashFileTypeInfo> fileList, EFrom from) {
            bool isLut = false;
            CrashFileTypeInfo fileTypeInfo = null;
            List<FileInfo> tempFileList = new List<FileInfo>();
            List<DirectoryInfo> tempDirList = new List<DirectoryInfo>();

            DirectoryInfo dirInfo = new DirectoryInfo(filePath);
            if (!dirInfo.Exists) {
                Trace.WriteLine("The directory path: " + filePath + " doesn't exist");
                return;
            }

            if(from == EFrom.LUT){
                isLut = true;
            }

            FileInfo[] crashFiles = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            DirectoryInfo[] crashDirs = dirInfo.GetDirectories("*-*", SearchOption.TopDirectoryOnly);

            tempFileList = deleteInvalidFile(crashFiles);

            foreach(DirectoryInfo dir in crashDirs){
                DateTime createTime = dir.CreationTime;
                TimeSpan diff = DateTime.Now.Subtract(createTime);
                if(diff.TotalHours >= 2){
                    tempDirList.Add(dir);
                }
            }

            foreach (FileInfo file in tempFileList) {
                fileTypeInfo = new CrashFileTypeInfo();
                fileTypeInfo.FileName = file.Name;
                fileTypeInfo.FullName = file.FullName;
                fileTypeInfo.IsFile = true;
                fileTypeInfo.IsLut = isLut;
                fileTypeInfo.IsOlder = (DateTime.Now.Subtract(file.CreationTime).TotalHours >= 8);
                fileTypeInfo.From = from;

                fileList.Add(fileTypeInfo);
            }

            foreach (DirectoryInfo dir in tempDirList)
            {
                fileTypeInfo = new CrashFileTypeInfo();
                fileTypeInfo.FileName = dir.Name;
                fileTypeInfo.FullName = dir.FullName;
                fileTypeInfo.IsFile = false;
                fileTypeInfo.IsLut = isLut;
                fileTypeInfo.IsOlder = (DateTime.Now.Subtract(dir.CreationTime).TotalHours >= 8);
                fileTypeInfo.From = from;

                fileList.Add(fileTypeInfo);
            }
        }


        public static List<FileInfo> deleteInvalidFile(FileInfo[] files)
        {
            List<FileInfo> logFileList = new List<FileInfo>();
            
            foreach (FileInfo item in files)
            {
                if (item.Name.StartsWith("BugReport"))
                { 
                    int index = item.Name.IndexOf('.');
                    string suffix = item.Name.Substring(index);
                    if (suffix.Length == 3) {
                        DateTime createTime = item.CreationTime;
                        TimeSpan diff = DateTime.Now.Subtract(createTime);
                        if (diff.TotalHours >= 2)
                        {
                            logFileList.Add(item);
                        }
                    }

                }//Crash-SSR-YMD-HMS-20130607-164039-004402451053221-ymmh5y0n5zeaouu
                else if (item.Name.StartsWith("Crash-YMD"))
                {
                    if (item.Length < 51200000)
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (IOException ex)
                        {
                            Trace.WriteLine("When delete file its size is smaller than 50000 KB: " + ex.Message);
                        }
                    }
                    else {

                        DateTime createTime = item.CreationTime;
                        TimeSpan diff = DateTime.Now.Subtract(createTime);
                        if (diff.TotalHours >= 2)
                        {
                            logFileList.Add(item);
                        }
                    }
                }
                else if (item.Name.StartsWith("mcd") || item.Name.StartsWith("mdm"))
                {
                    if (DateTime.Now.Subtract(item.CreationTime).TotalDays > 7)
                        try
                        {
                            item.Delete();
                        }
                        catch (IOException ex)
                        {
                            Trace.WriteLine("When deleted mcd or mdm log: " + ex.Message);
                        }
                }
                else if (item.Name.StartsWith("CoreDump") || item.Name.StartsWith("Crash-SSR-YMD-HMS"))
                {
                    if (item.Length < 1024000)
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (IOException ex)
                        {
                            Trace.WriteLine("When delete file its size is smaller than 100 KB: " + ex.Message);
                        }
                    }
                    else
                    {
                        DateTime createTime = item.CreationTime;
                        TimeSpan diff = DateTime.Now.Subtract(createTime);
                        if (diff.TotalHours >= 2)
                        {
                            logFileList.Add(item);
                        }
                    }
                }
                else
                {
                    int index = item.Name.IndexOf('.');
                    string suffix = item.Name.Substring(index);
                    if(suffix.Length != 1){
                        DateTime createTime = item.CreationTime;
                        TimeSpan diff = DateTime.Now.Subtract(createTime);
                        if (diff.TotalHours >= 2)
                        {
                            logFileList.Add(item);
                        }
                    }
                }
            }

            return logFileList;
        }

        public static string geStringtDate()
        {

            string sRet = "";

            DateTime createdTime_ = DateTime.Now;

            string year = "" + createdTime_.Year;
            string month = "" + createdTime_.Month;
            string day = "" + createdTime_.Day;
            if (month.Length == 1)
            {
                month = "0" + month;
            }
            if (day.Length == 1)
            {
                day = "0" + day;
            }
            sRet = year + month + day;

            return sRet;

        }
        public static void createDateFolder(string date)
        {
            string path = configData.PCCPath + @"\" + configData.LogName + @"\" + date;
            DirectoryInfo dinfo = new DirectoryInfo(path);
            if (!dinfo.Exists)
            {
                dinfo.Create();

                DirectoryInfo dFailedinfo = new DirectoryInfo(path + "NoMatch");
                dFailedinfo.Create();
            }
        }


        public static void DeleteOldFileByValidDeliverTime()
        {
            Settings.Default.Reload();
            int deleteDays = Settings.Default.DeleteDays;
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string selectcmd = "SELECT * FROM tblCrashLink with(nolock) " +
                        "WHERE Isvisible=1 and DeliverTime>'2001-01-01' and datediff(day, DeliverTime,'" + date + "') > " + deleteDays;

            List<string> selectResult = UpdateIsVisibleAndRetrievePath(selectcmd);
            Trace.WriteLine("Delete old log fies by DeliverTime, count: " + selectResult.Count.ToString());
            DeleteOldModelLog(selectResult);
        }

        private static List<string> UpdateIsVisibleAndRetrievePath(string sqlcmds)
        {
            List<string> path = new List<string>();
            using (SqlConnection conn = new SqlConnection(configData.strconn))
            {
                conn.Open();

                try
                {
                    SqlCommand selectcmd = new SqlCommand(sqlcmds, conn);

                    SqlCommand updatecmd = new SqlCommand("sp_CrashLinkUpdate", conn);
                    updatecmd.CommandType = CommandType.StoredProcedure;
                    updatecmd.Parameters.Add(new SqlParameter("@Isvisible", SqlDbType.Bit, 0, "Isvisible"));
                    //updatecmd.Parameters.Add(new SqlParameter("@CrashLink", SqlDbType.VarChar, 800, "CrashLink"));
                    updatecmd.Parameters.Add(new SqlParameter("@CrashLink_id", SqlDbType.BigInt, 0, "CrashLink_id"));
                    updatecmd.UpdatedRowSource = UpdateRowSource.None;

                    System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter();
                    adapter.SelectCommand = selectcmd;
                    adapter.UpdateCommand = updatecmd;

                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "tblCrashLink");

                    foreach (DataRow row in ds.Tables["tblCrashLink"].Rows)
                    {
                        path.Add(Convert.ToString(row["CrashLink"]));
                        row["IsVisible"] = 0;
                        //row["CrashLink"] = "";
                    }
                    adapter.Update(ds, "tblCrashLink"); //update DB to make the modification take affect
                }
                catch (SqlException ex)
                {
                    string str = ex.Message;
                    Trace.WriteLine("When query or update table tblCrashLink, exception occurs: " + str);
                }
            }
            return path;
        }

        private static void DeleteOldModelLog(List<string> pathes)
        {
            int mainDirLen = configData.PCCPath.Length;
            Trace.WriteLine(" *** length: " + pathes.Count);
            foreach (string path in pathes)
            {
                try
                {
                    if (path.Substring(mainDirLen).Contains('.'))
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        else
                        {
                            Trace.WriteLine("The File {0} does not exist", path);
                        }
                    }
                    else // this is a direcotry
                    {
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        else
                        {
                            Trace.WriteLine("The directory {0} does not exist", path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string str = ex.Message;
                    Trace.WriteLine(" ===) The File path is : " + path + "  length: " + mainDirLen);
                    //Trace.WriteLine("When try to delete crash link , exception occurs: " + str);
                }
            }
        }


        public static void DeleteOldFileByMatchDate()
        {
            Settings.Default.Reload();
            int deleteDays = Settings.Default.DeleteDays;
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string selectcmd = "SELECT * FROM tblCrashLink with(nolock) " +
            "WHERE Isvisible=1  and datediff(day, MatchDate,'" + date + "') > " + deleteDays;

            List<string> selectResult = UpdateIsVisibleAndRetrievePath(selectcmd);
            Trace.WriteLine("Delete old log fies by MatchDate, count: " + selectResult.Count.ToString());
            //if(selectResult.Count != 0 )
            DeleteExpiredLogFile(deleteDays);
        }

        private static void DeleteExpiredLogFile(int deleteDays)
        {
            string crashLogLinkPath = configData.PCCPath + "\\" + configData.LogName;
            List<string> oldLogs = GetOldCrashLogLink(crashLogLinkPath);

            DeleteOldFileOnDate(oldLogs, deleteDays);
        }

        private static List<string> GetOldCrashLogLink(string crashloglinkpath)
        {
            List<string> olderlinks = new List<string>();
            DirectoryInfo[] subDirs = null;
            try
            {
                DirectoryInfo logLinkRoot = new DirectoryInfo(crashloglinkpath);
                if (logLinkRoot.Exists)
                {
                    subDirs = logLinkRoot.GetDirectories();

                    foreach (DirectoryInfo item in subDirs)
                    {
                        olderlinks.Add(item.Name);
                    }
                }
                else
                {
                    Trace.WriteLine("The crash link log path doesn't exist: " + crashloglinkpath.ToString());
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception occurs when read log folders in GetOldCrashLogLink" + ex.ToString());
            }

            return olderlinks;
        }

        private static void DeleteOldFileOnDate(List<string> crashLogFolders, int deleteDays)
        {
            // anaylize folder's name to pick out the folders that match the deletion conditions
            int year = 0;
            int month = 0;
            int day = 0;
            int count = 0;
            foreach (string foldername in crashLogFolders)
            {
                int result;
                if (!Int32.TryParse(foldername.Substring(0, 8), out result))
                    continue;
                year = Int32.Parse(foldername.Substring(0, 4));
                month = Int32.Parse(foldername.Substring(4, 2));
                day = Int32.Parse(foldername.Substring(6, 2));
                try
                {
                    DateTime folderDate = new DateTime(year, month, day);

                    if (DateTime.Now.Subtract(folderDate).Days > deleteDays)
                    {
                        string tempPath = configData.PCCPath + "\\" + configData.LogName + "\\" + foldername;
                        Directory.Delete(tempPath, true);
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(configData.PCCPath + "\\" + configData.LogName + "\\" + foldername + " Exception occurs when delete folder by MatchDate:" + ex.ToString());
                }
            }
            Trace.WriteLine("There are " + count + " folders has been deleted according to the MatchDate are older than " + deleteDays + " days");
        }

        public static double getDiskFreeSize()
        {
            ulong freeBytesAvailable = 0;
            ulong totalNumberOfBytes = 0;
            ulong totalNumberOfFreeBytes = 0;
            double freeSize;

            GetDiskFreeSpaceEx(configData.PCCPath, out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes);
            Trace.WriteLine("The Available Free Bytes : " + freeBytesAvailable + " *** The Total Number of Bytes : " + totalNumberOfBytes);
            freeSize = ((double)freeBytesAvailable) / totalNumberOfBytes;
            return freeSize;
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetDiskFreeSpaceEx(
                string lpDirectoryName,
                out UInt64 lpFreeBytesAvailable,
                out UInt64 lpTotalNumberOfBytes,
                out UInt64 lpTotalNumberOfFreeBytes);

    }
}
