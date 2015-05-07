using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using SEMC.CrashLink.BackgroudService.model;
using SEMC.CrashLink.BackgroudService.utils;
using System.Collections;

namespace SEMC.CrashLink.BackgroudService.business
{
    class CrashLinkServer
    {
        private CrashLogFactory logFactory;
        private MatchProcessManager processManager;
        private MatchResultManager resultManager;
        private List<CrashFileNameInfo> fileNameInfoList = new List<CrashFileNameInfo>();
        private List<CrashLogsInfo> logInfoList = new List<CrashLogsInfo>();
        private List<CrashFileTypeInfo> crashFileList = new List<CrashFileTypeInfo>();

        private static CrashLinkServer mInstance = null;

        private CrashLinkServer() {
            if (logFactory == null)
            {
                logFactory = new CrashLogFactory();
            }

            if (processManager == null)
            {
                processManager = new MatchProcessManager();
            }

            if (resultManager == null)
            {
                resultManager = new MatchResultManager();
            }
        }

        #region methods

        public static CrashLinkServer getCrashLinkServer(){
            if(mInstance == null)
                mInstance = new CrashLinkServer();

            return mInstance;
        }

        public void doService(){
            //the ODM crash files will move to ODM dir from LUT
            moveODMFiles(configData.LUTPath, configData.ODM_PATH);

            configData.currentDate = FileOperator.geStringtDate();
            FileOperator.createDateFolder(configData.currentDate);
            clearContains();

            //get all crash report files
            getCrashFiles(ref crashFileList);

            Trace.WriteLine("********* The Total number of Crash Reports is : " + crashFileList.Count + " *********");
            //from the name of crash file, get the delivry time and imei
            foreach(CrashFileTypeInfo fileTypeInfo in crashFileList){
                CrashFileNameInfo nameInfo = logFactory.buildFileNameInfo(fileTypeInfo);
                fileNameInfoList.Add(nameInfo);
            }

            Trace.WriteLine("********* The valid Crash Reports is : " + fileNameInfoList.Count + " *********");
            //do mathing with delivery time and imei
            processManager.matchCrashFile(fileNameInfoList, ref logInfoList);

            Trace.WriteLine("********* After do matching, there have " + logInfoList.Count + " crash reports *********");
            //move crash reports to result directory and record the match result into DB.
            resultManager.handleMatchResults(logInfoList);
        }

        public void getCrashFiles(ref List<CrashFileTypeInfo> fileList) {
            FileOperator.getCrashFile(configData.PCCPath, ref fileList, EFrom.CMM);
            FileOperator.getCrashFile(configData.LUTPath, ref fileList, EFrom.LUT);
            FileOperator.getCrashFile(configData.EXTPath1, ref fileList, EFrom.EXT1);
            FileOperator.getCrashFile(configData.EXTPath2, ref fileList, EFrom.EXT2);
        }

        private void clearContains()
        {
            crashFileList.Clear();
            fileNameInfoList.Clear();
            logInfoList.Clear();
        }
        private void moveFilterFiles(string fromDir, string toDir)
        {
            Hashtable shortNames = new Hashtable();
            getShortNameList(ref shortNames, configData.ODM_SHORT_NAME_LIST);

            if (shortNames.Count > 0)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(fromDir);
                if (!dirInfo.Exists)
                {
                    Trace.WriteLine("The shor name directory path: " + fromDir + " doesn't exist");
                    return;
                }

                FileInfo[] crashFiles = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                int count = 0;
                foreach (FileInfo file in crashFiles)
                {
                    //short name is behind the last "-", if it is too short(length less than 3)
                    //it will be use "second last + last one".
                    string fullName = file.Name;
                    string[] split = fullName.Split('-');
                    int length = split.Length;
                    if (length <= 2)
                        return;
                    string shortName = split[length - 1];
                    shortName = shortName.Split('.')[0];
                    if (shortName.Length <= 3)
                        shortName = split[length - 2] + "-" + shortName;
                    if (shortNames.ContainsKey(shortName.ToUpper()))
                    {
                        try
                        {
                            file.MoveTo(toDir + @"\" + fullName);
                            count++;
                        }
                        catch (IOException ex)
                        {
                            Trace.WriteLine("Error occured when move ODM crash files: " + file.FullName
                                + ", The details of info is:");
                            Trace.WriteLine(ex.Message);
                        }
                    }
                }
                Trace.WriteLine("move ODM files counts is: " + count);
            }
        }

        private void getShortNameList(ref Hashtable shortNames, string shortNameList)
        {
            FileInfo file = new FileInfo(shortNameList);
            if (!file.Exists)
            {
                Trace.WriteLine("The ODM short name list file: " + shortNameList + " is not exist!");
                return;
            }
            try
            {
                FileStream fs = new FileStream(shortNameList, FileMode.Open);
                StreamReader m_streamReader = new StreamReader(fs);
                m_streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                string strLine = m_streamReader.ReadLine();
                do
                {
                    if (strLine != null && strLine != "")
                    {
                        strLine = strLine.Trim().ToUpper();
                        if (!shortNames.ContainsKey(strLine))
                            shortNames.Add(strLine, 1);
                    }
                    strLine = m_streamReader.ReadLine();
                } while (strLine != null);

                m_streamReader.Close();
                m_streamReader.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error occured when read ODM short names:");
                Trace.WriteLine(e.Message);
            }
        }
        #endregion
    }
}
