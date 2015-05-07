using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace SEMC.CrashLink.BackgroudService
{
    class folderHelper
    {
        public List<string> getSysLogFolderNameList(ref List<string> systemLog)
        {
            List<string> list = new List<string>();
            string lutsysPath = configData.LUTPath + "\\" + "";
            DirectoryInfo dir = new DirectoryInfo(configData.PCCPath);
            DirectoryInfo lutdir = new DirectoryInfo(lutsysPath);
            try
            {
                //  FileInfo[] files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                DirectoryInfo[] folderList = dir.GetDirectories();
                DirectoryInfo[] lutsysList = lutdir.GetDirectories();

                foreach (DirectoryInfo tem in folderList)
                {
                    DateTime createTime = tem.CreationTime;
                    TimeSpan diff = DateTime.Now.Subtract(createTime);
                    if (tem.Name != configData.LogName)
                    {
                        if (diff.TotalHours < 8 && diff.TotalHours >= 2)
                            systemLog.Add(tem.Name);
                        else if (diff.TotalHours >= 8)
                            list.Add(tem.Name);
                    }
                }

                foreach (DirectoryInfo item in lutsysList)
                {
                    DateTime createTime = item.CreationTime;
                    TimeSpan diff = DateTime.Now.Subtract(createTime);
                    if (item.Name != configData.LogName)
                    {
                        if (diff.TotalHours < 8 && diff.TotalHours >= 2)
                            systemLog.Add(item.Name);
                        else if (diff.TotalHours >= 8)
                            list.Add(item.Name);
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("In getSysLogFolderNameList() exception happened: " + ex.Message);
            }
            return list;
        }


        public List<string> getAppLogNameList(ref List<string> appLog)
        {
            List<FileInfo> filteredfile = new List<FileInfo>();
            filteredfile.Clear();
            List<string> list = new List<string>();
            string lutsysPath = configData.LUTPath + "\\" + "";
            DirectoryInfo dir = new DirectoryInfo(configData.PCCPath);
            DirectoryInfo lutdir = new DirectoryInfo(lutsysPath);
            try
            {
                FileInfo[] files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                FileInfo[] lutfiles = lutdir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
              //  DirectoryInfo[] folderList = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);

                foreach (FileInfo tem in files)
                {
                    //Trace.WriteLine(tem.Name + "'s size is " + tem.Length);
                    DateTime createTime = tem.CreationTime;
                    TimeSpan diff = DateTime.Now.Subtract(createTime);
                    if (diff.TotalHours < 8 && diff.TotalHours >= 2)
                        appLog.Add(tem.Name);
                    else if (diff.TotalHours >= 8)
                        list.Add(tem.Name);
                }

                foreach(FileInfo item in lutfiles)
                {
                    DateTime createTime = item.CreationTime;
                    TimeSpan diff = DateTime.Now.Subtract(createTime);
                    if (diff.TotalHours < 8 && diff.TotalHours >= 2)
                        appLog.Add(item.Name);
                    else if (diff.TotalHours >= 8)
                        list.Add(item.Name);
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("In getAppLogNameList() exception happened: " + ex.Message);
            }
            return list;
        }

        public List<string> getLutUnzipSystemLog(List<string> allZipfiles, ref List<string> appCrashList)
        {
            List<string> lutSystemList = new List<string>();
            try
            {
                foreach (string item in allZipfiles)
                {
                    if (item.StartsWith("a special words", false, null) || item.StartsWith("a special words", false, null))
                    {
                        lutSystemList.Add(item);
                        //allZipfiles.Remove(item);
                    }
                    else
                    {
                        appCrashList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("In getLutUnzipSystemLog() exception happened: " + ex.Message);
            }

            return lutSystemList;
        }

        public string geStringtDate()
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
            sRet = year  + month   + day;

            return sRet;

        }
        public void createNewFolder(string date)
        {
            string path = configData.PCCPath+@"\"+configData.LogName+@"\"+date;
            DirectoryInfo dinfo = new DirectoryInfo(path);
            if (!dinfo.Exists) 
            {
                dinfo.Create();

                DirectoryInfo dFailedinfo = new DirectoryInfo(path+"NoMatch");
                dFailedinfo.Create();
            }
        }

        public double getDiskFreeSize()
        {
            ulong freeBytesAvailable = 0;
            ulong totalNumberOfBytes = 0;
            ulong totalNumberOfFreeBytes = 0;
            double freeSize;

            GetDiskFreeSpaceEx(configData.PCCPath, out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes);
            Trace.WriteLine("The Available Free Bytes : " +  freeBytesAvailable + " *** The Total Number of Bytes : " + totalNumberOfBytes);
            freeSize = ((double)freeBytesAvailable) / totalNumberOfBytes;
            return freeSize;
        }

        public long geFreedSize()
        {
            long lRet = configData.lTotalFolderSize;
            string path = configData.PCCPath ;
            DirectoryInfo dinfo = new DirectoryInfo(path);
          //  lRet = DirSize(dinfo);
            //Dir(s)  991,147,868,160 bytes free
            string strDosRet = Execute("dir " + path, 60*1000);
            int iBegin = strDosRet.IndexOf("Dir(s)  ");
            int iEnd = strDosRet.IndexOf(" bytes free");
            string strNumber = strDosRet.Substring(iBegin + "Dir(s)  ".Length);
            strNumber = strNumber.Substring(0, strNumber.Length - " bytes free".Length-1);
            strNumber = strNumber.Replace(",","");
            lRet = Convert.ToInt64(strNumber);
            return lRet;

        }

        [DllImport("kernel32.dll")]
        private static extern bool GetDiskFreeSpaceEx(
                string lpDirectoryName,
                out UInt64 lpFreeBytesAvailable,
                out UInt64 lpTotalNumberOfBytes,
                out UInt64 lpTotalNumberOfFreeBytes);

        private   long DirSize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                Size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                Size += DirSize(di);
            }
            return (Size);
        }

        private string Execute(string dosCommand, int milliseconds)
        {
            string output = "";
            if (dosCommand != null && dosCommand != "")
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + dosCommand;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())
                    {
                        if (milliseconds == 0)
                            process.WaitForExit();
                        else
                            process.WaitForExit(milliseconds);
                        output = process.StandardOutput.ReadToEnd();
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }
    }
}
