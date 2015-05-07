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
    class MonitorService
    {
        #region Member variables
        /// <summary>
        /// The thread running this service
        /// </summary>
        private System.Threading.Thread mThread = null;
      
        private long lUsedForderSize = 0;

        /// <summary>
        /// The thread is / should be stopped
        /// </summary>
        protected volatile bool mThreadStop = false;

        /// <summary>
        /// The warnings
        /// </summary>
        protected string mWarnings = null;


        private int mSyncInterval = 1;



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
                throw new Exception("The MonitorService is already running");
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
                throw new Exception("The MonitorService is not running");
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

            Log.instance().WriteLine("start MonitorService ServiceLoop");
            while (!mThreadStop)
            {

                try
                {

                    while (OverFolderSize())
                    {
                        DeleteByValidDeliverTimer();
                        DeleteByMatchDate();
                        //DeleteByPhoneModel();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Error occured when delete: " + ex.ToString());
                }

                //  DebugLog.WriteLog("Start IDD Services again after 10 Minutes..");
                Trace.WriteLine("Start CrasLinkMonitorService again after 60 Minutes..");
                Thread.Sleep(TimeSpan.FromHours(mSyncInterval));
            }

        }

        private void DeleteByValidDeliverTimer()
        {
            db_heper.DeleteOldFileByValidDeliverTime();
        }

        private void DeleteByMatchDate()
        {
            db_heper.DeleteOldFileByMatchDate();
        }

        private void DeleteByPhoneModel()
        {
            db_heper.DeleteOldFileByPhoneModel();
        }

        private bool OverFolderSize()
        {
            bool bRet = false;
            /*
            long freeSize =  folder_helper.geFreedSize();
            double percent = ((double)freeSize / configData.lTotalFolderSize);
             */
            double percent = folder_helper.getDiskFreeSize();
            string cmt = string.Format("The Free disk space is {0:p}", percent);
            Trace.WriteLine(cmt.ToString());
            if (percent < 0.15)
                bRet = true;

            return bRet;
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
