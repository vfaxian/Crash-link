using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;

namespace SEMC.CrashLink.BackgroudService
{
    class UnzipFileHelper
    {
        private List<string> lutLogsList = new List<string>();
        private List<string> systemLogList = new List<string>();

        #region method
        public void RenameAndMoveLUTAppCrashLog()
        {
            lutLogsList.Clear();
            systemLogList.Clear();

            DirectoryInfo lutDir = new DirectoryInfo(configData.LUTPath);
            FileInfo[] files = lutDir.GetFiles("*.*", SearchOption.TopDirectoryOnly);

            foreach (FileInfo item in files)
            {
                if(item.Name.StartsWith("BugReport"))
                {
                    if (item.Length < 102400)
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
                    else {
                        lutLogsList.Add(item.Name);
                    }
                }
                else if ( item.Name.StartsWith("Crash-YMD"))
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
                    else if ((DateTime.Now.Subtract(item.CreationTime).TotalDays > 7))
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (IOException ex)
                        {
                            Trace.WriteLine("When delete file its size is smaller than 1000 KB: " + ex.Message);
                        }
                    }
                    else
                    {
                        lutLogsList.Add(item.Name);
                    }
                }
                else if (item.Name.StartsWith("mcd"))
                {
                    if (DateTime.Now.Subtract(item.CreationTime).TotalDays > 7)
                        try
                        {
                            item.Delete();
                        }
                        catch (IOException ex)
                        {
                            Trace.WriteLine("When deleted mcd log: " + ex.Message);
                        }
                }
                else {
                    lutLogsList.Add(item.Name);
                }
            }

            MoveBugReportFileandCollectSystemCrash(lutLogsList);
        }

        public void UnzipSystemLogWithNewName()
        {
            Trace.WriteLine("LUT System CrashLog count:" + systemLogList.Count);
            foreach(string item in systemLogList)
            {
                string source = configData.LUTPath + "\\" + item;
                string destination = configData.LUTPath + "\\" + "lutcrashlink" + "\\";
                try
                {
                    Trace.WriteLine("try to unzip file " + source);
                    UnzipSystemCrashLog(source, destination);
                    DeleteFiles(source);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("When deal with zipped file exception happened: " + ex.Message);
                }
            }
        }
        //move bug report file to Indox folder
        public void MoveBugReportFileandCollectSystemCrash(List<string> lutLogs)
        {
            string oldPath ;
            string newPath;
            foreach (string item in lutLogs)
            {
                if (item.Contains("BugReport"))
                {
                    oldPath = configData.LUTPath + "\\" + item;
                    newPath = configData.LUTPath + "\\" +"lutcrashlink"+ "\\"+ "LUT_" + item;
                    //move bug report file to indox folder
                    MoveFilesWithNewName(oldPath,newPath);
                }
                else if (item.StartsWith("Crash-YMD",false,null))
                {
                    //collect the system crash file to a list
                    systemLogList.Add(item);
                }
                else
                {
                    Trace.WriteLine("This is mcd log, don't deal with");
                }
            }
        }

        //move bug report file with new name to  Indox folder
        public void MoveFilesWithNewName(string oldPath, string newPath)
        {
            FileInfo fileinfor = new FileInfo(oldPath);
            FileInfo newfile = new FileInfo(newPath);
            try
            {
                if (fileinfor.Exists)
                {
                    if (newfile.Exists)
                    {
                        newfile.Delete();
                    }
                    fileinfor.MoveTo(newPath);
                }
                else
                {
                    Trace.WriteLine(oldPath.ToString() + " is not exist");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception occurs when move lut bug report file " + ex.Message);
            }
        }

        //unzip the system crash log with "LUT" prefix for file name to Indox folder
        public static bool UnzipSystemCrashLog(string source, string destination)
        {
            string logName;
            string filePath;
            bool bRet = true;
            ZipInputStream s = new ZipInputStream(File.OpenRead(source));
            ZipEntry theEntry;
            try
            {
                if ((theEntry = s.GetNextEntry()) != null)
                {
                    logName = Path.GetDirectoryName(theEntry.Name);
                    destination = destination + logName;
                    Directory.CreateDirectory(destination);
                }
                else
                {
                    s.Close();
                    bRet = false;
                    return bRet;
                }
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string fileName = Path.GetFileName(theEntry.Name);

                    if (fileName != String.Empty)
                    {
                        //uncompress the file to specified direcotry
                        filePath = destination + "\\" + Path.GetFileName(theEntry.Name);
                        FileStream streamWriter = File.Create(filePath);

                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }

                        streamWriter.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error happened when unzip file " + ex.Message);
                bRet = false;
            }
            finally
            {
                s.Close();
            }
            return bRet;
        }

        public void DeleteFiles(string fileName)
        {
            FileInfo systemlogfile = new FileInfo(fileName);
            if (systemlogfile.Exists)
            {
                systemlogfile.Delete();
            }
            else
            {
                Trace.WriteLine("The file " + fileName + "doesn't exist");
            }
        }
        #endregion

        #region UnzipFile
        /// <summary>
        /// Function: extracting the file of zip format
        /// </summary>
        /// <param name="zipFilePath">the path of unZipFile</param>
        /// <param name="unZipDir">default is the same level directory and the same name</param>
        /// <returns>whether success</returns>
        public static bool unZipFile(string zipFilePath, string unZipDir)
        {

            if (zipFilePath == string.Empty)
            {
                Trace.WriteLine("The file " + zipFilePath + " can't be NULL");
                return false;
            }
            if (!File.Exists(zipFilePath))
            {
                Trace.WriteLine("The file " + zipFilePath + "doesn't exist");
                return false;
            }
            if (!Path.GetFileName(zipFilePath).ToLower().EndsWith("zip"))
            {
                Trace.WriteLine("The zip file " + zipFilePath + "isn't correct!");
                return false;
            }
            //default is the same level directory and the same name
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("//"))
                unZipDir += "//";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            try
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
                {

                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(unZipDir + directoryName);
                        }
                        if (!directoryName.EndsWith("//"))
                            directoryName += "//";
                        if (fileName != String.Empty)
                        {
                            using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                            {

                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = s.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }//while
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error happened when unzip file " + ex.Message);
                return false;
            }
            return true;
        }//endUnZip
        #endregion
    }
}
