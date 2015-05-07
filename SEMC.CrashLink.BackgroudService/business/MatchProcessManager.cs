using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using SEMC.CrashLink.BackgroudService.model;
using SEMC.CrashLink.BackgroudService.utils;
namespace SEMC.CrashLink.BackgroudService.business
{
    class MatchProcessManager
    {
        private CrashLogsInfo getCrashLogInfo(SqlConnection conn, string sql, CrashFileNameInfo nameInfo, string imei) {
            CrashLogsInfo logInfo = null;

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = nameInfo.DeliveryTime;
            cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imei;

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0)
            {
                logInfo = new CrashLogsInfo();
                logInfo.PhoneTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                logInfo.LogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                logInfo.PhoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                logInfo.DeliveryTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                logInfo.WorkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                logInfo.Sw1Revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);
                logInfo.IsLut = nameInfo.IsLut;
                logInfo.IsFile = nameInfo.IsFile;
                logInfo.FullName = nameInfo.FullName;
                logInfo.FileName = nameInfo.FileName;
                logInfo.IsSuccess = true;
                logInfo.CrashType = FileOperator.getCrashType(nameInfo.FileName);
                logInfo.From = nameInfo.From;



            }
            else if(nameInfo.IsOlder){
                logInfo = new CrashLogsInfo();
                logInfo.PhoneTransId = 0;
                logInfo.LogName = null;
                logInfo.PhoneModelId = getPhoneModelId(conn, nameInfo.FileName);
                logInfo.DeliveryTime = getDeliverTime(nameInfo.DeliveryTime);
                logInfo.WorkingName = null;
                logInfo.Sw1Revision = null;
                logInfo.IsLut = nameInfo.IsLut;
                logInfo.IsFile = nameInfo.IsFile;
                logInfo.FullName = nameInfo.FullName;
                logInfo.FileName = nameInfo.FileName;
                logInfo.IsSuccess = false;
                logInfo.CrashType = FileOperator.getCrashType(nameInfo.FileName);
                logInfo.From = nameInfo.From;
            }


            return logInfo;
        }

        public void matchCrashFile(List<CrashFileNameInfo> nameInfos, ref List<CrashLogsInfo> logInfos)
        {
            CrashLogsInfo logInfo = null;

            using (SqlConnection conn = new SqlConnection(configData.strconn)) {
                conn.Open();

                string sql = "";

                SqlCommand cmd = new SqlCommand(sql, conn);

                try {
                    foreach(CrashFileNameInfo nameInfo in nameInfos){

                        //the file name is correct and not from lut directory should match the MTBF server
                        if (nameInfo.IsValid && !nameInfo.IsLut)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = nameInfo.DeliveryTime;
                            cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = nameInfo.Imei;

                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            DataSet ds = new DataSet();
                            adapter.Fill(ds);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                logInfo = new CrashLogsInfo();
                                logInfo.PhoneTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                logInfo.LogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                logInfo.PhoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                logInfo.DeliveryTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                logInfo.WorkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                logInfo.Sw1Revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);
                                logInfo.IsLut = nameInfo.IsLut;
                                logInfo.IsFile = nameInfo.IsFile;
                                logInfo.FullName = nameInfo.FullName;
                                logInfo.FileName = nameInfo.FileName;
                                logInfo.IsSuccess = true;
                                logInfo.CrashType = FileOperator.getCrashType(nameInfo.FileName);
                                logInfo.From = nameInfo.From;

                                logInfos.Add(logInfo);
                            }
                            else if (nameInfo.Imei.Length == 15)
                            {
                                string imei = nameInfo.Imei;
                                imei = imei.Substring(0, 14);

                                logInfo = getCrashLogInfo(conn, sql, nameInfo, imei);
                                if (logInfo != null)
                                {
                                    logInfos.Add(logInfo);
                                }
                            }
                            else if (nameInfo.Imei.Length == 14) {
                                string imei = nameInfo.Imei;

                                int checkDigit = CalcCheckDigit(imei);
                                imei = imei.PadRight(15, Char.Parse(checkDigit.ToString()));

                                logInfo = getCrashLogInfo(conn, sql, nameInfo, imei);
                                if (logInfo != null)
                                {
                                    logInfos.Add(logInfo);
                                }
                            }
                        }
                        else 
                        {
                            logInfo = new CrashLogsInfo();
                            logInfo.PhoneTransId = 0;
                            logInfo.LogName = null;
                            logInfo.PhoneModelId = getPhoneModelId(conn, nameInfo.FileName);
                            logInfo.DeliveryTime = getDeliverTime(nameInfo.DeliveryTime);
                            logInfo.WorkingName = null;
                            logInfo.Sw1Revision = null;
                            logInfo.IsLut = nameInfo.IsLut;
                            logInfo.IsFile = nameInfo.IsFile;
                            logInfo.FullName = nameInfo.FullName;
                            logInfo.FileName = nameInfo.FileName;
                            logInfo.IsSuccess = false;
                            logInfo.CrashType = FileOperator.getCrashType(nameInfo.FileName);
                            logInfo.From = nameInfo.From;

                            logInfos.Add(logInfo);
                        }
                    }
                }
                catch(SqlException ex){
                    Trace.WriteLine("Exception occured when do matching: " + ex.Message);
                }
            }
        }


        private long getPhoneModelId(SqlConnection conn, String fileName) {
            long phoneModelId = 0;
            /*
            string shortName = null;
            
            shortName = getShortName(fileName);
            string sql = "SELECT top 1 PhoneModel_id FROM tblphoneModel with(nolock)  "
                 + "WHERE ShortName =@pShortName";

            SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Clear();
            cmd.Parameters.Add("@pShortName", SqlDbType.VarChar).Value = shortName;
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);

            if (ds.Tables[0].Rows.Count > 0)
                phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
            **/
            return phoneModelId;
        }

        private DateTime getDeliverTime(string deliveryTime) {
            DateTime delivTime;
            if (deliveryTime == null)
            {
                delivTime = DateTime.Parse("1970-01-01");
            }
            else if(!DateTime.TryParse(deliveryTime, out delivTime)) {
                delivTime = DateTime.Parse("1970-01-01");
            }
            return delivTime;
        }

        private string getShortName(string fileName) {
            return "";
        }

        private int CalcCheckDigit(string imei)
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
    }
}
