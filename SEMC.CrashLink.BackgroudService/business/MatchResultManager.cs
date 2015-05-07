using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using SEMC.CrashLink.BackgroudService.model;
using SEMC.CrashLink.BackgroudService.utils;

namespace SEMC.CrashLink.BackgroudService.business
{
    class MatchResultManager
    {

        string newPath = null;

        public void handleMatchResults(List<CrashLogsInfo> logInfos) {
            string path = configData.PCCPath + @"\" + configData.LogName + @"\" + configData.currentDate;
            int year = Int32.Parse(configData.currentDate.Substring(0, 4));
            int month = Int32.Parse(configData.currentDate.Substring(4, 2));
            int day = Int32.Parse(configData.currentDate.Substring(6, 2));
            DateTime matchDate = new DateTime(year, month, day);
            int number1 = 0;
            int number2 = 0;
            MTBFTableAdapters.tblCrashLinkTableAdapter resultUpdater = new MTBFTableAdapters.tblCrashLinkTableAdapter();
            
            foreach (CrashLogsInfo logInfo in logInfos) {
                if (moveFile(logInfo))
                {
                    ++number1;
                    try
                    {
                        resultUpdater.Insert(logInfo.PhoneTransId, newPath, true, logInfo.IsSuccess, logInfo.PhoneModelId,
                            logInfo.WorkingName, logInfo.Sw1Revision, logInfo.LogName, logInfo.DeliveryTime, matchDate, logInfo.CrashType, logInfo.IsLut);
                    }
                    catch (SqlException ex)
                    {
                        Trace.WriteLine("Failed to insert matching result: " + ex.Message + " file: " + logInfo.FullName);
                    }
                }
                else {
                    ++number2;
                    //delete the file if it cannot be moved
                    try {
                        if (logInfo.IsFile)
                            File.Delete(logInfo.FullName);
                        else
                            Directory.Delete(logInfo.FullName, true);
                    }
                    catch(IOException ex){
                        Trace.WriteLine("Exception occured when delete the file or dir that cannot be moved: " + ex.Message);
                    }
                }
                newPath = null;
            }

            Trace.WriteLine("********* crash reports successfully moved: " + number1 + " *********");
            Trace.WriteLine("********* crash reports failed to be moved: " + number2 + " *********");
        }

        public bool moveFile(CrashLogsInfo logInfo) {
            string oldPath = configData.PCCPath + @"\" + configData.LogName + @"\" + configData.currentDate;
            newPath = configData.NEW_TARGET_PATH + @"\" + configData.LogName + @"\" + configData.currentDate;
            string newNoDatePath = configData.NEW_TARGET_PATH;
            bool flag = false;
            EFrom from = logInfo.From;

            switch(from){
                case EFrom.CMM:
                    if(logInfo.IsSuccess)
                        newPath = oldPath + @"\" + logInfo.FileName;
                    else
                        newPath = oldPath + "NoMatch" + @"\" + logInfo.FileName;
                    flag = doMove(logInfo, newPath);
                    break;
                case EFrom.LUT:
                    //LUT will be not create date path and match the MTBF
                    newPath = newNoDatePath + @"\" + logInfo.FileName;
                    flag = doMove(logInfo, newPath);
                    break;
                case EFrom.EXT1:
                    newPath = newNoDatePath + @"\" + logInfo.FileName;
                    flag = doMove(logInfo, newPath);
                    break;
                case EFrom.EXT2:
                    newPath = newNoDatePath + @"\" + logInfo.FileName;
                    flag = doMove(logInfo, newPath);
                    break;
                default:
                    break;

            }
            return flag;
        }

        #region move file or directory method
        private bool doMove(CrashLogsInfo logInfo, string newPath)
        {
            bool flag = false;
            try
            {
                if (logInfo.IsFile)
                {
                    FileInfo file = new FileInfo(logInfo.FullName);
                    if (file.Exists)
                    {
                        file.MoveTo(newPath);
                        flag = true;
                    }
                    else
                    {
                        Trace.WriteLine(logInfo.FullName + " does not exist");
                    }
                }
                else
                {
                    string strSourceDir = logInfo.FullName;
                    if (Directory.Exists(strSourceDir))
                    {
                        moveDirectory(strSourceDir, newPath, true);
                        flag = true;
                    }
                    else
                    {
                        Trace.WriteLine(logInfo.FullName + " does not exist");
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine("Faile to move file: " + logInfo.FullName + " " + ex.Message);
            }
            return flag;
        }

        private void moveDirectory(string strSourceDir, string strDestDir, bool bDelSource)
        {
            if (Directory.GetDirectoryRoot(strSourceDir) == Directory.GetDirectoryRoot(strDestDir))
            {
                Directory.Move(strSourceDir, strDestDir);
            }
            else
            {
                try
                {
                    CopyDirectory(new DirectoryInfo(strSourceDir), new DirectoryInfo(strDestDir));
                    if (bDelSource) Directory.Delete(strSourceDir, true);
                }
                catch (Exception subEx)
                {
                    throw subEx;
                }
            }
        }

        private void CopyDirectory(DirectoryInfo diSourceDir, DirectoryInfo diDestDir)
        {
            if (!diDestDir.Exists) diDestDir.Create();
            FileInfo[] fiSrcFiles = diSourceDir.GetFiles();
            foreach (FileInfo fiSrcFile in fiSrcFiles)
            {
                String path = fiSrcFile.FullName;
                //hidden the file.
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
                fiSrcFile.CopyTo(Path.Combine(diDestDir.FullName, fiSrcFile.Name));
                // show the file.
                path = diDestDir.FullName + @"\" + fiSrcFile.Name;
                FileAttributes fileAttr = File.GetAttributes(path);
                fileAttr = RemoveAttribute(fileAttr, FileAttributes.Hidden);
                File.SetAttributes(path, fileAttr);
            }
            DirectoryInfo[] diSrcDirectories = diSourceDir.GetDirectories();
            foreach (DirectoryInfo diSrcDirectory in diSrcDirectories)
            {
                CopyDirectory(diSrcDirectory, new DirectoryInfo(Path.Combine(diDestDir.FullName, diSrcDirectory.Name)));
            }
        }

        private FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
        #endregion
    }
}
