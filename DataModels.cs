using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SEMC.CrashLink.BackgroudService
{

    class CrashLogInfo
    {
        private long phoneTransId;
        private string logName;
        private long phoneModelId;
        private DateTime deliverTime;
        private string workingName;
        private string sw1Revision;


        public long PhonTransId
        {
            set {
                phoneTransId = value;
            }
            get {
                return phoneTransId;
            }
        }

        public string LogName
        {
            set {
                logName = value;
            }
            get {
                return logName;
            }
        }

        public long PhoneModeId
        {
            set {
                phoneModelId = value;
            }
            get {
                return phoneModelId;            
            }
        }

        public DateTime DeliverTime
        {
            set {
                deliverTime = value;
            }
            get {
                return deliverTime;
            }
        }

        public string WorkingName
        {
            set {
                workingName = value;
            }
            get
            {
                return workingName;
            }
        }

        public string Sw1Revision
        {
            set {
                sw1Revision = value;
            }
            get {
                return sw1Revision;
            }
        }

    }
}
