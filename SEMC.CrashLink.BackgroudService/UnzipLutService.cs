using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;
using SEMC.CrashLink.BackgroudService.utils;

namespace SEMC.CrashLink.BackgroudService
{
    class UnzipLutService
    {
        #region Member variables
        /// <summary>
        /// The thread running this service
        /// </summary>
        private System.Threading.Thread mThread = null;

        /// <summary>
        /// The thread is / should be stopped
        /// </summary>
        protected volatile bool mThreadStop = false;

        /// <summary>
        /// The warnings
        /// </summary>
        protected string mWarnings = null;


        private int mSyncInterval = 120;



        /// <summary>
        /// Event sent when the thread is stopping
        /// </summary>
        protected ManualResetEvent mThreadStoppingEvent = new ManualResetEvent(false);
        #endregion

        #region Thread start and stop

        /// <summary>
        /// Starts the service
        /// </summary>
        public void Start()
        {
            if (mThread != null)
            {
                throw new Exception("The UnzipLutService is already running");
            }

            // Initial state
            mThreadStop = false;
            mThreadStoppingEvent.Reset();

            mThread = new Thread(new ThreadStart(OuterServiceLoop));
            mThread.IsBackground = true;
            mThread.Start();
        }

        /// <summary>
        /// Stops the service
        /// </summary>
        public void Stop()
        {
            if (mThread == null)
            {
                throw new Exception("The UnzipLutService is not running");
            }

            // Notify
            mThreadStop = true;
            mThreadStoppingEvent.Set();

            // Wait for the thread to die (or time-out)
            mThread.Join(5000);
            if (mThread.IsAlive)
            {
                // Kill it!
                mThread.Abort();
            }

            mThread = null;

        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns any warnings
        /// </summary>
        public virtual string Warnings
        {
            get
            {
                return mWarnings;
            }
        }

        /// <summary>
        /// This should be implemented by the service itself
        /// </summary>
        public   string Status
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method is called when the thread is started, it wraps and logs exceptions
        /// thrown by the service.
        /// </summary>
        public void OuterServiceLoop()
        {
            try
            {
                ServiceLoop();
            }
            catch (Exception ex)
            {
               Log.instance().WriteLine(this + " service loop aborted:\n" + ex.ToString());
            }
            finally
            {
            }
        }

        /// <summary>
        /// The thread's entry point
        /// </summary>
        private void ServiceLoop()
        {

            Log.instance().WriteLine("start UnzipLutService ServiceLoop");
            while (!mThreadStop)
            {

                try
                {
                    unZipFile(configData.LUT_ZIP_PATH);

                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error occured when unzip: " + ex.ToString());
                }
                Trace.WriteLine("Start Minutes again after " + mSyncInterval + " Minutes..");
                Thread.Sleep(TimeSpan.FromMinutes(mSyncInterval));
            }

        }

        private void unZipFile(string path)
        {

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            if (!dirInfo.Exists)
            {
                Trace.WriteLine("The directory path: " + path + " doesn't exist");
                return;
            }

            FileInfo[] crashFiles = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);

            //only get the files created by one hour ago.
            List<FileInfo> tempFileList = FileOperator.deleteInvalidFile(crashFiles);
            Trace.WriteLine(crashFiles.Length + "  , is: " + tempFileList.Count);
            string fullName = null;
            foreach (FileInfo file in tempFileList)
            {

                fullName = file.Name;
                Trace.WriteLine("--File Name is " + file.FullName);
                if (fullName.ToLower().StartsWith("crash-") && fullName.EndsWith("zip"))
                {
                    if (UnzipFileHelper.unZipFile(file.FullName, configData.LUTPath + @"\" + Path.GetFileNameWithoutExtension(file.FullName)))
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Error occured when delete: " + file.FullName + ", The details of info is: " + ex.ToString());
                        }
                    }
                    else
                    {
                        Trace.WriteLine("This is not success when unZip " + file.FullName);
                    }
                }
                else
                {
                    try
                    {
                        if (!Directory.Exists(configData.LUTPath)) Directory.CreateDirectory(configData.LUTPath);
                        file.MoveTo(Path.Combine(configData.LUTPath, file.Name));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("This is not success when move the " + file.FullName + " error info: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Doesn't do anything by default
        /// </summary>
        public virtual void CleanStatus()
        {
            mWarnings = null;
        }
        #endregion
    }
}
