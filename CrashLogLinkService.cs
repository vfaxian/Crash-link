using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Xml;
namespace SEMC.CrashLink.BackgroudService
{
    class CrashLogLinkService
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


        private int mSyncInterval = 60;



        private folderHelper folder_helper = new folderHelper();
        private DBHelper db_heper = new DBHelper();
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
                throw new Exception("The service is already running");
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
                throw new Exception("The service is not running");
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

            Log.instance().WriteLine("start CrashLogLinkService ServiceLoop");
            while (!mThreadStop)
            {
                try
                {
                      configData.currentDate = folder_helper.geStringtDate(); //get current date and convert it to string that will be used for folder name
                      folder_helper.createNewFolder(configData.currentDate); //creat matched file folder and nomatched file folder
                      DateTime processStart = DateTime.Now;
                      List<string> systfolderList2 = new List<string>();
                      List<string> appFileList2 = new List<string>();
                      List<string> appCrashList = new List<string>();
                      List<string> appCrashList2 = new List<string>();
                     
                      Trace.WriteLine("Start processing incoming log, start time is: " + processStart.ToLocalTime());
                      List<string> SysforderList = folder_helper.getSysLogFolderNameList(ref systfolderList2);

                      List<string> appFileList = folder_helper.getAppLogNameList(ref appFileList2);//get bugreport files's name

                      //extract the LUT System Crash Log, CoreDump, Crash-SSR-YMD-HMS
                      List<string> lutUnzipedSystemLogList = folder_helper.getLutUnzipSystemLog(appFileList, ref appCrashList);
                      List<string> lutUnzipedSystemLogList2 = folder_helper.getLutUnzipSystemLog(appFileList2, ref appCrashList2);

                      db_heper.DealWithLogMoreThan8(SysforderList, appCrashList);
                      db_heper.DealWithLogBTW2To8(systfolderList2, appCrashList2);
                      db_heper.DealWithLutSystemCrashFiles(lutUnzipedSystemLogList, lutUnzipedSystemLogList2);

                      db_heper.updateOKMoreThan8();
                      db_heper.updateOKBTW2To8();
                      db_heper.updateLutSystemCrashOkMT8();
                      db_heper.updateLutSystemCrashOkBT28();
                      db_heper.updateFailedLutSystemCrashData();
                      db_heper.updateFailedData();
                      
                      DateTime processEnd = DateTime.Now;
                      Trace.WriteLine("End processing the incoming log, end time is: " + processEnd.ToLocalTime());
                      TimeSpan timeSpan = DateTime.Now.Subtract(processStart);
                       int count = SysforderList.Count + appFileList.Count;
                       double result = (timeSpan.TotalMilliseconds / count);
                      Trace.WriteLine(" " + count + " records are processed" );
                      Trace.WriteLine("========== processing a piece of log  spends" + result.ToString() + " millionseconds  ==========" );
                }
                catch (Exception exception)
                {
                    string str = exception.Message;
                    Trace.WriteLine(str);
                }
 
                Trace.WriteLine("Start CrashLogLinkService again after 60 Minutes..");
         
                Thread.Sleep(TimeSpan.FromMinutes(mSyncInterval));
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
