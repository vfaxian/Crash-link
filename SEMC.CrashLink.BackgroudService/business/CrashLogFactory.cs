using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SEMC.CrashLink.BackgroudService.interfaces;
using SEMC.CrashLink.BackgroudService.model;
using SEMC.CrashLink.BackgroudService.utils;
using System.IO;

namespace SEMC.CrashLink.BackgroudService.business
{
    class CrashLogFactory
    {
        private AFileNamePhaser mCrashSSRPhaser;
        private AFileNamePhaser mCrashYMDPhaser;
        private AFileNamePhaser mCoreDumpPhaser;
        private AFileNamePhaser mBugReportpPhaser;

        public CrashLogFactory()
        {
            if (mCrashSSRPhaser == null)
            {
                mCrashSSRPhaser = new CrashSSRPharser();
            }

            if (mCrashYMDPhaser == null)
            {
                mCrashYMDPhaser = new CrashYMDPharser();
            }

            if (mCoreDumpPhaser == null)
            {
                mCoreDumpPhaser = new CoreDumpPharser();
            }

            if (mBugReportpPhaser == null)
            {
                mBugReportpPhaser = new BugReportPharser();
            }

        }

        public CrashFileNameInfo buildFileNameInfo(CrashFileTypeInfo fileTypeInfo) {
            //check the file name's validity
            string fileName = fileTypeInfo.FileName;

            if (!fileTypeInfo.IsLut)
            {
                if (fileName.StartsWith("Crash-YMD-HMS"))
                {
                    if (FileOperator.validateFileName(mCrashYMDPhaser, fileName))
                    {
                        return mCrashYMDPhaser.doPharse(fileTypeInfo);
                    }
                }

                if (fileName.StartsWith("Crash-SSR-YMD-HMS"))
                {
                    if (FileOperator.validateFileName(mCrashSSRPhaser, fileName))
                    {
                        return mCrashSSRPhaser.doPharse(fileTypeInfo);
                    }
                }

                if (fileName.StartsWith("BugReport"))
                {
                    if (FileOperator.validateFileName(mBugReportpPhaser, fileName))
                    {
                        return mBugReportpPhaser.doPharse(fileTypeInfo);
                    }
                }

                if (fileName.StartsWith("CoreDump"))
                {
                    if (FileOperator.validateFileName(mCoreDumpPhaser, fileName))
                    {
                        return mCoreDumpPhaser.doPharse(fileTypeInfo);
                    }
                }
            }
            CrashFileNameInfo fileNameInfo = new CrashFileNameInfo();
            fileNameInfo.DeliveryTime = null;
            fileNameInfo.FileName = fileName;
            fileNameInfo.FullName = fileTypeInfo.FullName;
            fileNameInfo.IsFile = fileTypeInfo.IsFile;
            fileNameInfo.Imei = null;
            fileNameInfo.IsValid = false;
            fileNameInfo.IsLut = fileTypeInfo.IsLut;
            fileNameInfo.IsOlder = fileTypeInfo.IsOlder;
            fileNameInfo.From = fileTypeInfo.From;

            return fileNameInfo;

        }
    }
}
