using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SEMC.CrashLink.BackgroudService.utils;

namespace SEMC.CrashLink.BackgroudService.model
{
    class CrashLogsInfo
    {
        private string fullName;
        private string fileName;
        private bool isFile;
        private long phoneTransId;
        private string logName;
        private long phoneModelId;
        private DateTime deliveryTime;
        private string workingName;
        private string sw1Revision;
        private string crashType;
        private bool isLut;
        private bool isSuccess;
        private EFrom from;

        public string FullName
        {
            set
            {
                fullName = value;
            }
            get
            {
                return fullName;
            }
        }

        public string FileName
        {
            set
            {
                fileName = value;
            }
            get
            {
                return fileName;
            }
        }

        public bool IsFile
        {
            set
            {
                isFile = value;
            }
            get
            {
                return isFile;
            }
        }

        public long PhoneTransId {
            set {
                phoneTransId = value;
            }
            get {
                return phoneTransId;
            }
        }

        public string LogName {
            set {
                logName = value;
            }
            get {
                return logName;
            }
        }

        public long PhoneModelId {
            set {
                phoneModelId = value;
            }
            get {
                return phoneModelId;
            }
        }

        public DateTime DeliveryTime {
            set {
                deliveryTime = value;
            }
            get {
                return deliveryTime;
            }
        }

        public string WorkingName {
            set {
                workingName = value;
            }
            get {
                return workingName;
            }
        }

        public string Sw1Revision {
            set {
                sw1Revision = value;
            }
            get {
                return sw1Revision;
            }
        }

        public string CrashType {
            set {
                crashType = value;
            }
            get {
                return crashType;
            }
        }

        public bool IsLut
        {
            set
            {
                isLut = value;
            }
            get
            {
                return isLut;
            }
        }

        public bool IsSuccess {
            set {
                isSuccess = value;
            }
            get {
                return isSuccess;            
            }
        }

        public EFrom From
        {
            set
            {
                from = value;
            }
            get
            {
                return from;
            }
        }
    }
}
