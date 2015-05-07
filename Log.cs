using System;
using System.Collections.Generic;
using System.Text;

namespace SEMC.CrashLink.BackgroudService
{
    /// <summary>
    /// This class writes System.Diagnostics.Trace's into a file that depends on the LogName
    /// property of this class and the date.
    /// 
    /// Each new day a new log file is created with a date stamp
    /// </summary>
    public class Log : System.Diagnostics.TraceListener
    {
        #region Singleton

        /// <summary>
        /// The static singleton object
        /// </summary>
        private static Log instance_ = null;

        /// <summary>
        /// A lock to protect single threaded access when creating the one singleton object
        /// </summary>
        private static System.Threading.Mutex instanceLock_ = new System.Threading.Mutex();

        /// <summary>
        /// Returns the singleton instance of this class
        /// </summary>
        /// <returns></returns>
        public static Log instance()
        {
            // Test if an instance of this class has been created
            if (instance_ == null)
            {
                // No instance created, lock the mutex to ensure that only one instance of
                // this class can be created
                instanceLock_.WaitOne();
                try
                {
                    // Verify again (just in case another thread was waiting for the lock
                    // when the first instance was created)
                    if (instance_ == null)
                    {
                        instance_ = new Log();
                    }
                }
                finally
                {
                    instanceLock_.ReleaseMutex();
                }
            }
            return instance_;
        }
        #endregion

        private string name_ = "MTBF_PCC_";

        /// <summary>
        /// Name of the log file
        /// </summary>
        public string LogName
        {
            get
            {
                return name_;
            }
            set
            {
                name_ = value;
            }
        }

        /// <summary>
        /// This class can only be instantiated private by the static access
        /// method instance(). When instantiated; this class adds itself as a listener
        /// to System.Diagnostics.Trace.Listeners
        /// </summary>
        private Log()
        {
            // Create a file log
            try
            {
                System.Diagnostics.Trace.Listeners.Add(this);
            }
            catch (Exception e)
            {
                string error = e.Message;
            }
            System.Diagnostics.Trace.AutoFlush = true;
        }

        private class LogFile : IDisposable
        {
            #region Constructor and static access method
            /// <summary>
            /// Creates a new log file, opens a System.IO.TextWriter for creation.
            /// The log file name will be of format "SEMC.Communication_2005-01-01.txt"
            /// </summary>
            private LogFile()
            {
                string path = @"C:\Program Files\Sony Ericsson\CRLNK";
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
                System.IO.FileInfo fi = new System.IO.FileInfo( path + "\\Logs\\" + Log.instance().LogName + "_" + year + "-" + month + "-" + day + ".txt");
                writer_ = fi.CreateText();
            }

            /// <summary>
            /// Returns a LogFile (for the current day).
            /// </summary>
            /// <returns></returns>
            public static LogFile getLogFile()
            {
                // Create a new instance if none is set
                if (instance_ == null)
                {
                    instanceLock_.WaitOne();
                    try
                    {
                        if (instance_ == null)
                        {
                            instance_ = new LogFile();
                        }
                    }
                    finally
                    {
                        instanceLock_.ReleaseMutex();
                    }
                }
                else
                {
                    // A log file already exists, check if we should create a new one.
                    if (instance_.CreatedTime.Day != System.DateTime.Now.Day)
                    {
                        instanceLock_.WaitOne();
                        try
                        {
                            // Open a new log file
                            if (instance_.CreatedTime.Day != System.DateTime.Now.Day)
                            {
                                // Dispose old instance
                                instance_.Dispose();

                                // Create the new instance which will be returned
                                instance_ = new LogFile();
                            }
                        }
                        finally
                        {
                            instanceLock_.ReleaseMutex();
                        }
                    }
                }
                return instance_;
            }
            #endregion
            #region Getters and setters
            /// <summary>
            /// Returns the exact time when the LogFile object was created
            /// </summary>
            public System.DateTime CreatedTime
            {
                get
                {
                    return createdTime_;
                }
            }

            /// <summary>
            /// Returns a System.IO.TextWriter for the log file
            /// </summary>
            public System.IO.TextWriter Writer
            {
                get
                {
                    return writer_;
                }
            }
            #endregion
            #region Members
            /// <summary>
            /// The time stamp when the log file was created.
            /// </summary>
            private System.DateTime createdTime_ = System.DateTime.Now;

            /// <summary>
            /// Lock to protect multiple instances of a this class to be created
            /// </summary>
            private static System.Threading.Mutex instanceLock_ = new System.Threading.Mutex();

            /// <summary>
            /// The active instance of the log file
            /// </summary>
            private static LogFile instance_ = null;

            /// <summary>
            /// The writer
            /// </summary>
            private System.IO.TextWriter writer_ = null;
            #endregion
            #region IDisposable Members
            public void Dispose()
            {
                writer_.Close();
            }
            #endregion
        }

        #region Implementation of System.Diagnostics.TraceListener
        /// <summary>
        /// Implementation of System.Diagnostics.TraceListener
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            LogFile.getLogFile().Writer.Write(System.DateTime.Now.ToLongTimeString() + "\t" + message);
        }

        /// <summary>
        /// Implementation of System.Diagnostics.TraceListener
        /// </summary>
        public override void Flush()
        {
            LogFile.getLogFile().Writer.Flush();
        }

        /// <summary>
        /// Implementation of System.Diagnostics.TraceListener
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            Write(message + "\r\n");
        }
        #endregion
    }
}
