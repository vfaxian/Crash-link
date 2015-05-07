using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SEMC.CrashLink.BackgroudService.interfaces;

namespace SEMC.CrashLink.BackgroudService.business
{
    //Crash-YMD-HMS-19001031-053138-00440245058631-C6603
    //Crash-YMD-HMS-20131113-070731-004402146574987-C5502
    class CrashYMDPharser : AFileNamePhaser
    {
        public override string getDeliveryTime(string fileName)
        {
            string deliveryTime = null;

            string[] comps = fileName.Split('-');
            if (comps.Length >= 7)
            {
                string date = comps[3];
                string time = comps[4];

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

            if (comps.Length >= 7)
            {
                string imei = comps[5];

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
