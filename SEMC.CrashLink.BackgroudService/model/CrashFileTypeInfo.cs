using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SEMC.CrashLink.BackgroudService.utils;

namespace SEMC.CrashLink.BackgroudService.model
{
    class CrashFileTypeInfo
    {
        private bool isLut;
        private string fileName;
        private string fullName;
        private bool isFile;
        private bool isOlder;
        private EFrom from;


        public bool IsLut {
            set
            {
                isLut = value;
            }
            get {
                return isLut;
            }
        }

        public string FileName {
            set {
                fileName = value;
            }
            get {
                return fileName;            
            }
        }

        public string FullName {
            set {
                fullName = value;            
            }
            get {
                return fullName;
            }
        }

        public bool IsFile {
            set {
                isFile = value;
            }
            get {
                return isFile;           
            }
        }

        public bool IsOlder {
            set {
                isOlder = value;
            }
            get {
                return isOlder;
            }
        }

        public EFrom From {
            set {
                from = value;            
            }
            get {
                return from;
            }
        }
    }
}
