using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SEMC.CrashLink.BackgroudService.utils;

namespace SEMC.CrashLink.BackgroudService.model
{
    class CrashFileNameInfo
    {
        private string imei;
        private string deliveryTime;
        private string fileName;
        private string fullName;
        private bool isFile;
        private bool isLut;
        private bool isvalid;
        private bool isOlder;
        private EFrom from;

        public string Imei {
            set {
                imei = value;
            }
            get {
                return imei;
            }
        }

        public string DeliveryTime {
            set {
                deliveryTime = value;            
            }
            get {
                return deliveryTime;
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

        public bool IsValid {
            set {
                isvalid = value;
            }
            get {
                return isvalid;
            }
        }

        public bool IsOlder
        {
            set
            {
                isOlder = value;
            }
            get
            {
                return isOlder;
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
