using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SEMC.CrashLink.BackgroudService.model;

namespace SEMC.CrashLink.BackgroudService.interfaces
{
    abstract class AFileNamePhaser
    {

        public CrashFileNameInfo doPharse(CrashFileTypeInfo fileInfo)
        {
            CrashFileNameInfo fileNameInfo = new CrashFileNameInfo();

            string fileName = fileInfo.FileName;

            fileNameInfo.IsValid = true;
            fileNameInfo.IsLut = fileInfo.IsLut;
            fileNameInfo.DeliveryTime = getDeliveryTime(fileName);
            fileNameInfo.Imei = getImeiNumber(fileName);
            fileNameInfo.FileName = fileName;
            fileNameInfo.FullName = fileInfo.FullName;
            fileNameInfo.IsFile = fileInfo.IsFile;
            fileNameInfo.IsOlder = fileInfo.IsOlder;
            fileNameInfo.From = fileInfo.From;

            return fileNameInfo;
        }

        public abstract string getDeliveryTime(string fileName);

        public abstract string getImeiNumber(string fileName);
    }
}
