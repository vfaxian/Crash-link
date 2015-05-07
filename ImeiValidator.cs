using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SEMC.CrashLink.BackgroudService
{
    public class ImeiValidator
    {
        /// <summary>
        /// The length of an IMEI
        /// </summary>
        public static int ImeiLength
        {
            get { return 15; }
        }

        /// <summary>
        /// Calculates the check digit for an IMEI.
        /// </summary>
        /// <param name="imei"></param>
        /// <returns></returns>
        public static int CalcCheckDigit(string imei)
        {
            if (imei.Length < 14)
            {
                throw new Exception(imei + " is not enough digits (IMEI validation)");
            }

            int sum = 0;

            for (int i = 0; i < 14; i += 2)
            {
                int digit = int.Parse(imei.Substring(i, 1));
                sum += digit;
            }

            for (int i = 1; i < 14; i += 2)
            {
                int digit = int.Parse(imei.Substring(i, 1));
                int temp = 2 * digit;
                sum += temp;
                if (temp >= 10) { sum -= 9; }
            }

            string strTemp = sum.ToString();
            int cd = 10 - int.Parse(strTemp.Substring(strTemp.Length - 1));
            return (cd == 10 ? 0 : cd);
        }

        /// <summary>
        /// Validates the IMEI or throws an exception
        /// </summary>
        /// <param name="imei"></param>
        public static void Validate(string imei)
        {
            if (imei.Length == 15) //for IMEI
            {
                int cd = int.Parse(imei.Substring(14, 1));
                int calcCd = CalcCheckDigit(imei);
                if (cd != calcCd)
                {
                    throw new Exception("Check digit doesn't match. Expected=" + calcCd + " but got " + cd + " from " + imei + " (IMEI validation)");
                }
            }
            else if (imei.Length == 14) //for MEID
            {
                Regex isMEID = new Regex(@"^[0-F]{14}");
                if (!isMEID.IsMatch(imei.ToUpper()))
                {
                    throw new Exception("MEID check digit doesn't match: " + imei);
                }
            }
            else
            {
                throw new Exception(imei + " is not enough digits (IMEI validation)");
            }

            //if (imei.Length < ImeiLength)
            //{
            //    throw new Exception(imei + " is not enough digits (IMEI validation)");
            //}

            //int cd = int.Parse(imei.Substring(14,1));
            //int calcCd = CalcCheckDigit(imei);
            //if (cd != calcCd)
            //{
            //    throw new Exception("Check digit doesn't match. Expected=" + calcCd + " but got " + cd + " from " + imei + " (IMEI validation)");
            //}
        }
    }
}
