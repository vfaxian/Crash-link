using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SEMC.CrashLink.BackgroudService.interfaces;

namespace SEMC.CrashLink.BackgroudService.business
{
    //CoreDump_20131114_023428-004402451402675-C6916

    class CoreDumpPharser : AFileNamePhaser
    {
        public override string getDeliveryTime(string fileName) {
            string deliveryTime = null;
            char[] seperators = { '_', '-' };
            string[] comps = fileName.Split(seperators);

            if(comps.Length >= 5){
                string date = comps[1];
                string time = comps[2];

                if (date.Length != 8 || time.Length != 6)
                {
                    Trace.WriteLine("The date time format is not correct: " + date + " " + time);
                    return deliveryTime;
                }
                else {
                    try
                    {
                        Int32.Parse(date);
                        Int32.Parse(time);

                        deliveryTime = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" +
                            date.Substring(6, 2) + " " + time.Substring(0, 2) + ":" +
                            time.Substring(2, 2) + ":" + time.Substring(4, 2);
                    }
                    catch (FormatException ex)
                    {
                        Trace.WriteLine("The delivey time is not correct: " + ex.Message);
                    }
                }
            }
            return deliveryTime;
        }

        public override string getImeiNumber(string fileName) {
            string imeiNumber = null;
            char[] seperators = { '_', '-' };
            string[] comps = fileName.Split(seperators);

            if(comps.Length >= 5){
                string imei =comps[3];

                try
                {
                    Int64.Parse(imei);

                    imeiNumber = imei;
                }
                catch (FormatException ex)
                {
                    Trace.WriteLine("The format of imei is not correct: " + ex.Message);
                }
            }

            return imeiNumber;
        }
    }
}
