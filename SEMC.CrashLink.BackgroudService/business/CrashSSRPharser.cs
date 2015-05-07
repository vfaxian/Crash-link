using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SEMC.CrashLink.BackgroudService.interfaces;

namespace SEMC.CrashLink.BackgroudService.business
{
    //Crash-SSR-YMD-HMS-20131113-233449-004402540819491-SO-01F
    //Crash-SSR-YMD-HMS-19700515-075255-00440245122536-C6903
    class CrashSSRPharser : AFileNamePhaser
    {
        public override string getDeliveryTime(string fileName)
        {
            string deliveryTime = null;

            string[] comps = fileName.Split('-');
            if (comps.Length >= 8)
            {
                string date = comps[4];
                string time = comps[5];

                if (date.Length != 8 || time.Length != 6)
                {
                    Trace.WriteLine("The date time format is not correct: " + date + " " + time);
                    return deliveryTime;
                }
                else
                {
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
        public override string getImeiNumber(string fileName)
        {
            string imeiNumber = null;
            string[] comps = fileName.Split('-');

            if (comps.Length >= 8)
            {
                string imei = comps[6];

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
