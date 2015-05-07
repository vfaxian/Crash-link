using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace SEMC.CrashLink.BackgroudService
{
    class LUTUploadLogService
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



        private UnzipFileHelper unzipfile_help = new UnzipFileHelper();
        //private DBHelper db_heper = new DBHelper();
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
                throw new Exception("LUT log service is running");
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
        public string Status
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

            Log.instance().WriteLine("start LUTUploadedLogService ServiceLoop");
            while (!mThreadStop)
            {
                try
                {
                     //inItConfig();
                    unzipfile_help.RenameAndMoveLUTAppCrashLog();
                    unzipfile_help.UnzipSystemLogWithNewName();
                    unzipfile_help.MoveLutSystemCrashLogWithoutUnzip();
                }
                catch (Exception exception)
                {
                    string str = exception.Message;
                    Trace.WriteLine(str);
                }

                Trace.WriteLine("Start LUTUploadedLogService again after 60 Minutes..");

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
