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
        private List<string> lutsystemLogList = new List<string>();

        public void RenameAndMoveLUTAppCrashLog()
        {
            lutLogsList.Clear();
            systemLogList.Clear();
            lutsystemLogList.Clear();

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
                }//Crash-SSR-YMD-HMS-20130607-164039-004402451053221-ymmh5y0n5zeaouu
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
                else if(item.Name.StartsWith("CoreDump") || item.Name.StartsWith("Crash-SSR-YMD-HMS"))
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
                        lutLogsList.Add(item.Name);
                    }
                }
                else
                {
                     lutLogsList.Add(item.Name);
                }
            }

            MoveBugReportFileandCollectSystemCrash(lutLogsList);
        }

        public void UnzipSystemLogWithNewName()
        {
            Trace.WriteLine("LUT Crash-YMD-HMS log count: " + systemLogList.Count);
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

        public void MoveLutSystemCrashLogWithoutUnzip()
        {
            Trace.WriteLine("LUT CoreDump and Crash-SSR-YMD-HMS log count: " + lutsystemLogList.Count);
            string oldPath;
            string newPath;
            foreach (string item in lutsystemLogList)
            {
                oldPath = configData.LUTPath + "\\" + item;
                newPath = configData.LUTPath + "\\" + "lutcrashlink" + "\\" + "LUT_" + item;
                MoveFilesWithNewName(oldPath, newPath);
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
                else if (item.StartsWith("CoreDump", false, null) || item.StartsWith("Crash-SSR-YMD-HMS", false, null))
                {
                    lutsystemLogList.Add(item);
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
                    ;//Trace.WriteLine(oldPath.ToString() + " is not exist");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception occurs when move lut bug report file " + ex.Message);
            }
        }

        //unzip the system crash log with "LUT" prefix for file name to Indox folder
        public bool UnzipSystemCrashLog(string source, string destination)
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
                    logName = "LUT_" + Path.GetDirectoryName(theEntry.Name);
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

    }
}
