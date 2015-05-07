//#define SW1REVISION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using  SEMC.CrashLink.BackgroudService.Properties;


namespace SEMC.CrashLink.BackgroudService
{
    //Crash-YMD-HMS-20110708-132107-004402142464936-LT15i
    class DBHelper
    {
        public List<string> dOklist = new List<string>();
        public List<string> dFailedlist = new List<string>();
        public List<string> fOklist = new List<string>();
        public List<string> fFailedlist = new List<string>();
        public List<long> dOkPhoneTransIdList = new List<long>();
        public List<long> fOkPhoneTransIdList = new List<long>();
        public List<string> dlogNameList = new List<string>();
        public List<string> flogNameList = new List<string>();
        public List<long> dphoneModelIdList = new List<long>();
        public List<long> fphoneModelIdList = new List<long>();
        public List<DateTime> dDeliverTimeList = new List<DateTime>();
        public List<DateTime> fDeliverTimeList = new List<DateTime>();
        public List<string> dworkingNameList = new List<string>();
        public List<string> fworkingNameList = new List<string>();
        public List<string> dsw1revisionList = new List<string>();
        public List<string> fsw1revisionList = new List<string>();

        public List<string> d28Oklist = new List<string>();
        public List<string> f28Oklist = new List<string>();
        public List<long> d28OkPhoneTransIdList = new List<long>();
        public List<long> f28OkPhoneTransIdList = new List<long>();
        public List<long> d28phoneModelIdList = new List<long>();
        public List<long> f28phoneModelIdList = new List<long>();
        public List<string> d28logNameList = new List<string>();
        public List<string> f28logNameList = new List<string>();
        public List<DateTime> d28DeliverTimeList = new List<DateTime>();
        public List<DateTime> f28DeliverTimeList = new List<DateTime>();
        public List<string> d28workingNameList = new List<string>();
        public List<string> f28workingNameList = new List<string>();
        public List<string> d28sw1revisionList = new List<string>();
        public List<string> f28sw1revisionList = new List<string>();

        public List<CrashLogInfo> lutSystemCrash8OkList = new List<CrashLogInfo>();
        public List<string> lutSystemCrash8OkItems = new List<string>();
        public List<string> lutSystemCrash8FailItems = new List<string>();
        public List<CrashLogInfo> lutSystemCrash28OkList = new List<CrashLogInfo>();
        public List<string> lutSystemCrash28OkItems = new List<string>();

#if SW1REVISION
        public List<long> dSW1IdList = new List<long>();
        public List<long> fSW1IdList = new List<long>();
        public List<string> dSW1RevisionList = new List<string>();
        public List<string> fSW1RevisionList = new List<string>();
#endif
        string strconn = configData.strconn;
        string sysCrash = "Core";
        string appCrash = "App";
        int mainDirLen = configData.PCCPath.Length;
        int iCount = 100;
         public void DealWithLogMoreThan8(List<string> forderSysList, List<string> forderAppList)
        {
             // used for the crash log which created time is older than 8 hours
            dOklist.Clear();
            dFailedlist.Clear();
            dOkPhoneTransIdList.Clear();
            dphoneModelIdList.Clear();
            dlogNameList.Clear();
            dDeliverTimeList.Clear();
            dworkingNameList.Clear();
            dsw1revisionList.Clear();


            fOklist.Clear();
            fFailedlist.Clear();
            fOkPhoneTransIdList.Clear();
            fphoneModelIdList.Clear();
            flogNameList.Clear();
            fDeliverTimeList.Clear();
            fworkingNameList.Clear();
            fsw1revisionList.Clear();

#if SW1REVISION
            dSW1IdList.Clear();
            dSW1RevisionList.Clear();
            fSW1IdList.Clear();
            fSW1RevisionList.Clear();
#endif


            string temdate = "";
            string imeinumber="";

            using (SqlConnection conn = new SqlConnection(strconn))
            {
                conn.Open();
                foreach (string temname in forderSysList)
                {
                    //Crash-YMD-HMS-20110620-161000-004402142464936-LT15i
                    if (!isAvailiableSysCrashFolder(temname))
                    {
                        dFailedlist.Add(temname);
                        continue;
                    }
                    try
                    {
                        string sql = "SELECT top 1 T1.PhoneTrans_id, T1.SW1_id, T1.DeliverTime, T1.PhoneModel_id, T1.LogName , T2.Workingname, T3.Sw1Revision "
                            + " FROM tblphonetrans T1 with(nolock)  LEFT JOIN Tblphonemodel T2 ON T1.PhoneModel_id =  T2.PhoneModel_id "
                            + " LEFT JOIN TblSw1 T3 ON T1.SW1_id = T3.SW1_id "
                            + " WHERE DeliverTime=@pInDeliverTime  "
                            + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";

                        
                        DateTime dlvTime;
                        temdate = getSysCrashDate(temname);
                        if (!DateTime.TryParse(temdate, out dlvTime))
                        {
                            dlvTime = DateTime.Parse("1970-01-01");
                        }

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        imeinumber = getSysCrashImei(temname);
                        cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString() ;
                        cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DateTime dDeliverTime;
                            long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                            string dlogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                            long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                            //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                            DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                            string dworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                            string dsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);
   
                            dOkPhoneTransIdList.Add(phonTransId);
                            dlogNameList.Add(dlogName);
                            dphoneModelIdList.Add(phoneModelId);
                            dworkingNameList.Add(dworkingName);
                            dsw1revisionList.Add(dsw1revision);
                            dDeliverTimeList.Add(dDeliverTime);
                            dOklist.Add(temname);                          
                        }
                        else if (imeinumber.Length == 15)
                        {
                            //remove the check digit
                            imeinumber=imeinumber.Remove(14, 1);

                            //use the imei without check digit to do query from phonetrans
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                            cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                            adapter = new SqlDataAdapter(cmd);
                            ds = new DataSet();
                            adapter.Fill(ds);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DateTime dDeliverTime;
                                long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                string dlogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                string dworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                string dsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                dOkPhoneTransIdList.Add(phonTransId);
                                dlogNameList.Add(dlogName);
                                dphoneModelIdList.Add(phoneModelId);
                                dworkingNameList.Add(dworkingName);
                                dsw1revisionList.Add(dsw1revision);
                                dDeliverTimeList.Add(dDeliverTime);
                                dOklist.Add(temname);

                            }
                            else
                            {
                                dFailedlist.Add(temname);
                            }
                        }
                        else if (imeinumber.Length == 14)
                        {
                            // Calculate the check digit 
                            int checkDigit = ImeiValidator.CalcCheckDigit(imeinumber);
                            // Make up new Imei with the check digit
                            imeinumber = imeinumber.PadRight(15, Char.Parse(checkDigit.ToString()));

                            // Use Imei with the check digit to query phonetrans
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                            cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                            adapter = new SqlDataAdapter(cmd);
                            ds = new DataSet();
                            adapter.Fill(ds);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DateTime dDeliverTime;
                                long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                string dlogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                string dworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                string dsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                dOkPhoneTransIdList.Add(phonTransId);
                                dlogNameList.Add(dlogName);
                                dphoneModelIdList.Add(phoneModelId);
                                dworkingNameList.Add(dworkingName);
                                dsw1revisionList.Add(dsw1revision);
                                dDeliverTimeList.Add(dDeliverTime);
                                dOklist.Add(temname);

                            }
                            else
                            {
                                dFailedlist.Add(temname);
                            }
                        }
                        else
                        {
                            dFailedlist.Add(temname);
                        }
                    }
                    catch (Exception exception) 
                    {
                        string str = exception.Message;
                        Trace.WriteLine("Execptions: " + str);            
                        dFailedlist.Add(temname);
                    }

                }
#if SW1REVISION
                //query the sw1revision with the sw1_id 
                foreach (long id in dSW1IdList)
                {
                    try
                    {
                        string sql = "SELECT SW1Revision FROM tblsw1 with(nolock) WHERE SW1_Id=@sw1id";
                        SqlCommand cmd = new SqlCommand(sql, conn);

                        cmd.Parameters.Add("@sw1id", SqlDbType.BigInt).Value = id;

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string dSW1Revision = Convert.ToString(ds.Tables[0].Rows[0]["SW1Revision"]);

                            dSW1RevisionList.Add(dSW1Revision);
                        }
                        else
                        {
                            Trace.WriteLine("Error: SW1_id " + id + " doesn't have corresponding SW1Revision");
                        }
                    }
                    catch (SqlException ex)
                    {
                        Trace.WriteLine("Error: error occurs when query SW1Revision with SW1_id " + ex.Message);
                    }
                }
#endif

                foreach (string temname in forderAppList)
                {
                    //Crash-YMD-HMS-20110620-161000-004402142464936-LT15i
                    if (!isAvailiableAppCrashFolder(temname))
                    {
                        fFailedlist.Add(temname);
                        continue;
                    }

                    try 
                    {
                        string sql = "SELECT top 1 T1.PhoneTrans_id, T1.SW1_id, T1.DeliverTime, T1.PhoneModel_id, T1.LogName , T2.Workingname, T3.Sw1Revision "
                            + " FROM tblphonetrans T1 with(nolock)  LEFT JOIN Tblphonemodel T2 ON T1.PhoneModel_id =  T2.PhoneModel_id "
                            + " LEFT JOIN TblSw1 T3 ON T1.SW1_id = T3.SW1_id "
                            + " WHERE DeliverTime=@pInDeliverTime  "
                            + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";
                        //String sql = "SELECT top 1 PhoneTrans_id, SW1_id, DeliverTime, PhoneModel_id, LogName  FROM tblphonetrans with(nolock)   "
                        //      + "WHERE DateDiff(ss,DeliverTime,@pInDeliverTime)=0  "
                        //      + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";
                        DateTime dlvTime;
                        temdate = getAppCrashDate(temname);
                        if (!DateTime.TryParse(temdate, out dlvTime))
                        {
                            dlvTime = DateTime.Parse("1970-01-01");
                        }

                        imeinumber = getAppCrashImei(temname);

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                        cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            DateTime fDeliverTime;
                            long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                            string flogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                            long rphoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                            //DateTime fDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                            DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out fDeliverTime);
                            string fworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                            string fsw1Revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                            fOkPhoneTransIdList.Add(phonTransId);
                            flogNameList.Add(flogName);
                            fphoneModelIdList.Add(rphoneModelId);
                            fDeliverTimeList.Add(fDeliverTime);
                            fworkingNameList.Add(fworkingName);
                            fsw1revisionList.Add(fsw1Revision);
                            fOklist.Add(temname);                         
                        }
                        else if (imeinumber.Length == 15)
                        {
                            //remove the check digit
                            imeinumber = imeinumber.Remove(14, 1);

                            //use the imei without check digit to do query from phonetrans
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                            cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                            adapter = new SqlDataAdapter(cmd);
                            ds = new DataSet();
                            adapter.Fill(ds);

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DateTime fDeliverTime;
                                long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                string flogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                long rphoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                //DateTime fDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out fDeliverTime);
                                string fworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                string fsw1Revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                fOkPhoneTransIdList.Add(phonTransId);
                                flogNameList.Add(flogName);
                                fphoneModelIdList.Add(rphoneModelId);
                                fDeliverTimeList.Add(fDeliverTime);
                                fworkingNameList.Add(fworkingName);
                                fsw1revisionList.Add(fsw1Revision);
                                fOklist.Add(temname);
                            }
                            else
                            {
                                fFailedlist.Add(temname);
                            }
                        }
                        else if (imeinumber.Length == 14)
                        {
                            // Calculate the check digit 
                            int checkDigit = ImeiValidator.CalcCheckDigit(imeinumber);
                            // Make up new Imei with the check digit
                            imeinumber = imeinumber.PadRight(15, Char.Parse(checkDigit.ToString()));

                            // Use Imei with the check digit to query phonetrans
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                            cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                            adapter = new SqlDataAdapter(cmd);
                            ds = new DataSet();
                            adapter.Fill(ds);

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DateTime fDeliverTime;
                                long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                string flogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                long rphoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                //DateTime fDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out fDeliverTime);
                                string fworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                string fsw1Revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                fOkPhoneTransIdList.Add(phonTransId);
                                flogNameList.Add(flogName);
                                fphoneModelIdList.Add(rphoneModelId);
                                fDeliverTimeList.Add(fDeliverTime);
                                fworkingNameList.Add(fworkingName);
                                fsw1revisionList.Add(fsw1Revision);
                                fOklist.Add(temname);
                            }
                            else
                            {
                                fFailedlist.Add(temname);
                            }
                        }
                        else
                        {
                            fFailedlist.Add(temname);
                        }
                    }catch(Exception exception) 
                    {
                        string str = exception.Message;
                        Trace.WriteLine(str);
                        fFailedlist.Add(temname);
                    }
                }
#if SW1REVISION
                //query the sw1revision with the sw1_id 
                foreach (long id in fSW1IdList)
                {
                    try
                    {
                        string sql = "SELECT SW1Revision FROM tblsw1 with(nolock) WHERE SW1_Id=@sw1id";
                        SqlCommand cmd = new SqlCommand(sql, conn);

                        cmd.Parameters.Add("@sw1id", SqlDbType.BigInt).Value = id;

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            string fSW1Revision = Convert.ToString(ds.Tables[0].Rows[0]["SW1Revision"]);

                            fSW1RevisionList.Add(fSW1Revision);
                        }
                        else
                        {
                            Trace.WriteLine("Error: SW1_id " + id + " doesn't have corresponding SW1Revision");
                        }
                    }
                    catch (SqlException ex)
                    {
                        Trace.WriteLine("Error: error occurs when query SW1Revision with SW1_id " + ex.Message);
                    }
                }
#endif
            }
        }

         public void DealWithLogBTW2To8(List<string> sysFolderList, List<string> appFileList)
         {
             d28Oklist.Clear();
             d28OkPhoneTransIdList.Clear();
             d28phoneModelIdList.Clear();
             d28logNameList.Clear();
             d28DeliverTimeList.Clear();
             d28workingNameList.Clear();
             d28sw1revisionList.Clear();


             f28Oklist.Clear();
             f28OkPhoneTransIdList.Clear();
             f28phoneModelIdList.Clear();
             f28logNameList.Clear();
             f28DeliverTimeList.Clear();
             f28workingNameList.Clear();
             f28sw1revisionList.Clear();

             string temdate = "";
             string imeinumber = "";

             using (SqlConnection conn = new SqlConnection(strconn))
             {
                 conn.Open();
                 foreach (string temname in sysFolderList)
                 {
                     //Crash-YMD-HMS-20110620-161000-004402142464936-LT15i
                     if (!isAvailiableSysCrashFolder(temname))
                     {
                         continue;
                     }
                     try
                     {
                         string sql = "SELECT top 1 T1.PhoneTrans_id, T1.SW1_id, T1.DeliverTime, T1.PhoneModel_id, T1.LogName , T2.Workingname, T3.Sw1Revision "
                             + " FROM tblphonetrans T1 with(nolock)  LEFT JOIN Tblphonemodel T2 ON T1.PhoneModel_id =  T2.PhoneModel_id "
                             + " LEFT JOIN TblSw1 T3 ON T1.SW1_id = T3.SW1_id "
                             + " WHERE DeliverTime=@pInDeliverTime  "
                             + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";
                         //String sql = "SELECT top 1 PhoneTrans_id, SW1_id, DeliverTime,PhoneModel_id, LogName FROM tblphonetrans with(nolock)   "
                         //            + "WHERE DateDiff(ss,DeliverTime,@pInDeliverTime)=0  "
                         //            + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";
                         DateTime dlvTime;
                         //DateTime.TryParse(getSysCrashDate(temname), out dlvTime);

                         temdate = getSysCrashDate(temname);
                         if (!DateTime.TryParse(temdate, out dlvTime))
                         {
                             dlvTime = DateTime.Parse("1970-01-01");
                         }
                         imeinumber = getSysCrashImei(temname);
                         SqlCommand cmd = new SqlCommand(sql, conn);
                         cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                         cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                         SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                         DataSet ds = new DataSet();
                         adapter.Fill(ds);

                         if (ds.Tables[0].Rows.Count > 0)
                         {
                             DateTime dDeliverTime;
                             long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                             string dlogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                             long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                             //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                             DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                             string dworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                             string dsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                             d28OkPhoneTransIdList.Add(phonTransId);
                             d28logNameList.Add(dlogName);
                             d28phoneModelIdList.Add(phoneModelId);
                             d28DeliverTimeList.Add(dDeliverTime);
                             d28workingNameList.Add(dworkingName);
                             d28sw1revisionList.Add(dsw1revision);
                             d28Oklist.Add(temname);
                         }
                         else if( imeinumber.Length == 15)
                         {
                             //remove the check digit
                             imeinumber = imeinumber.Remove(14, 1);

                             //use the imei without check digit to do query from phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);

                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime dDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string dlogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                 string dworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string dsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 d28OkPhoneTransIdList.Add(phonTransId);
                                 d28logNameList.Add(dlogName);
                                 d28phoneModelIdList.Add(phoneModelId);
                                 d28DeliverTimeList.Add(dDeliverTime);
                                 d28workingNameList.Add(dworkingName);
                                 d28sw1revisionList.Add(dsw1revision);
                                 d28Oklist.Add(temname);
                             }
                         }
                         else if (imeinumber.Length == 14)
                         {
                             // Calculate the check digit 
                             int checkDigit = ImeiValidator.CalcCheckDigit(imeinumber);
                             // Make up new Imei with the check digit
                             imeinumber = imeinumber.PadRight(15, Char.Parse(checkDigit.ToString()));

                             // Use Imei with the check digit to query phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);

                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime dDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string dlogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                 string dworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string dsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 d28OkPhoneTransIdList.Add(phonTransId);
                                 d28logNameList.Add(dlogName);
                                 d28phoneModelIdList.Add(phoneModelId);
                                 d28DeliverTimeList.Add(dDeliverTime);
                                 d28workingNameList.Add(dworkingName);
                                 d28sw1revisionList.Add(dsw1revision);
                                 d28Oklist.Add(temname);
                             }
                         }
                     }
                     catch (Exception exception)
                     {
                         string str = exception.Message;
                         Trace.WriteLine("Exceptions: " + str + "  "+temname);
                     }

                 }

                 foreach (string temname in appFileList)
                 {
                     //Crash-YMD-HMS-20110620-161000-004402142464936-LT15i
                     if (!isAvailiableAppCrashFolder(temname))
                     {
                         continue;
                     }

                     try
                     {
                         string sql = "SELECT top 1 T1.PhoneTrans_id, T1.SW1_id, T1.DeliverTime, T1.PhoneModel_id, T1.LogName , T2.Workingname, T3.Sw1Revision "
                             + " FROM tblphonetrans T1 with(nolock)  LEFT JOIN Tblphonemodel T2 ON T1.PhoneModel_id =  T2.PhoneModel_id "
                             + " LEFT JOIN TblSw1 T3 ON T1.SW1_id = T3.SW1_id "
                             + " WHERE DeliverTime=@pInDeliverTime  "
                             + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";

                         //String sql = "SELECT top 1 PhoneTrans_id, SW1_id, DeliverTime, PhoneModel_id, LogName  FROM tblphonetrans with(nolock)   "
                         //      + "WHERE DateDiff(ss,DeliverTime,@pInDeliverTime)=0  "
                         //      + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";
                         DateTime dlvTime;
                         temdate = getAppCrashDate(temname);
                         if (!DateTime.TryParse(temdate, out dlvTime))
                         {
                             dlvTime = DateTime.Parse("1970-01-01");
                         }
                         imeinumber = getAppCrashImei(temname);

                         SqlCommand cmd = new SqlCommand(sql, conn);
                         cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                         cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                         SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                         DataSet ds = new DataSet();
                         adapter.Fill(ds);

                         if (ds.Tables[0].Rows.Count > 0)
                         {
                             DateTime fDeliverTime;
                             long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                             string flogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                             long rphoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                             //DateTime fDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                             DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out fDeliverTime);
                             string fworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                             string fsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                             f28OkPhoneTransIdList.Add(phonTransId);
                             f28logNameList.Add(flogName);
                             f28phoneModelIdList.Add(rphoneModelId);
                             f28DeliverTimeList.Add(fDeliverTime);
                             f28workingNameList.Add(fworkingName);
                             f28sw1revisionList.Add(fsw1revision);
                             f28Oklist.Add(temname);
                         }
                         else if (imeinumber.Length == 15)
                         {
                             //remove the check digit
                             imeinumber = imeinumber.Remove(14, 1);

                             //use the imei without check digit to do query from phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);

                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime fDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string flogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long rphoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime fDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out fDeliverTime);
                                 string fworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string fsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 f28OkPhoneTransIdList.Add(phonTransId);
                                 f28logNameList.Add(flogName);
                                 f28phoneModelIdList.Add(rphoneModelId);
                                 f28DeliverTimeList.Add(fDeliverTime);
                                 f28workingNameList.Add(fworkingName);
                                 f28sw1revisionList.Add(fsw1revision);
                                 f28Oklist.Add(temname);
                             }
                         }
                         else if (imeinumber.Length == 14)
                         {
                             // Calculate the check digit 
                             int checkDigit = ImeiValidator.CalcCheckDigit(imeinumber);
                             // Make up new Imei with the check digit
                             imeinumber = imeinumber.PadRight(15, Char.Parse(checkDigit.ToString()));

                             // Use Imei with the check digit to query phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);

                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime fDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string flogName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long rphoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime fDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out fDeliverTime);
                                 string fworkingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string fsw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 f28OkPhoneTransIdList.Add(phonTransId);
                                 f28logNameList.Add(flogName);
                                 f28phoneModelIdList.Add(rphoneModelId);
                                 f28DeliverTimeList.Add(fDeliverTime);
                                 f28workingNameList.Add(fworkingName);
                                 f28sw1revisionList.Add(fsw1revision);
                                 f28Oklist.Add(temname);
                             }
                         }
                     }
                     catch (Exception exception)
                     {
                         string str = exception.Message;
                         Trace.WriteLine(str);
                     }
                 }
             }
         }

         public void DealWithLutSystemCrashFiles(List<string> lutSysCrashFile8, List<string> lutSysCrashFile28)
         {
             lutSystemCrash8OkList.Clear();
             lutSystemCrash8OkItems.Clear();
             lutSystemCrash8FailItems.Clear();
             lutSystemCrash28OkList.Clear();
             lutSystemCrash28OkItems.Clear();


             string temdate = "";
             string imeinumber = "";
             CrashLogInfo itemInfo;

             using (SqlConnection conn = new SqlConnection(strconn))
             { 
                 conn.Open();

                 foreach (string temname in lutSysCrashFile8)
                 {
                     if (!isValidLutSystemCrashFile(temname))
                     {
                         lutSystemCrash8FailItems.Add(temname);
                         continue;
                     }

                     try
                     {
                         string sql = "SELECT top 1 T1.PhoneTrans_id, T1.SW1_id, T1.DeliverTime, T1.PhoneModel_id, T1.LogName , T2.Workingname, T3.Sw1Revision "
                            + " FROM tblphonetrans T1 with(nolock)  LEFT JOIN Tblphonemodel T2 ON T1.PhoneModel_id =  T2.PhoneModel_id "
                            + " LEFT JOIN TblSw1 T3 ON T1.SW1_id = T3.SW1_id "
                            + " WHERE DeliverTime=@pInDeliverTime  "
                            + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";

                         DateTime dlvTime;
                         temdate = getLutSystemCrashDate(temname);
                         if (!DateTime.TryParse(temdate, out dlvTime))
                         {
                             dlvTime = DateTime.Parse("1970-01-01");
                         }

                         SqlCommand cmd = new SqlCommand(sql, conn);
                         imeinumber = getLutSystemCrashImei(temname);
                         cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                         cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                         SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                         DataSet ds = new DataSet();
                         adapter.Fill(ds);

                         if (ds.Tables[0].Rows.Count > 0)
                         {
                             DateTime dDeliverTime;
                             long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                             string logName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                             long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                             //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                             DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                             string workingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                             string sw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                             itemInfo = new CrashLogInfo();

                             itemInfo.PhonTransId = phonTransId;
                             itemInfo.LogName = logName;
                             itemInfo.PhoneModeId = phoneModelId;
                             itemInfo.DeliverTime = dDeliverTime;
                             itemInfo.WorkingName = workingName;
                             itemInfo.Sw1Revision = sw1revision;

                             lutSystemCrash8OkList.Add(itemInfo);
                             lutSystemCrash8OkItems.Add(temname);
                         }
                         else if(imeinumber.Length == 15)
                         {
                             imeinumber = imeinumber.Remove(14, 1);

                             //use the imei without check digit to do query from phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);

                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime dDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string logName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                 string workingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string sw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 itemInfo = new CrashLogInfo();

                                 itemInfo.PhonTransId = phonTransId;
                                 itemInfo.LogName = logName;
                                 itemInfo.PhoneModeId = phoneModelId;
                                 itemInfo.DeliverTime = dDeliverTime;
                                 itemInfo.WorkingName = workingName;
                                 itemInfo.Sw1Revision = sw1revision;

                                 lutSystemCrash8OkList.Add(itemInfo);
                                 lutSystemCrash8OkItems.Add(temname);
                             }
                             else
                             {
                                 lutSystemCrash8FailItems.Add(temname);
                         
                             }
                         }
                         else if (imeinumber.Length == 14)
                         {
                             // Calculate the check digit 
                             int checkDigit = ImeiValidator.CalcCheckDigit(imeinumber);
                             // Make up new Imei with the check digit
                             imeinumber = imeinumber.PadRight(15, Char.Parse(checkDigit.ToString()));

                             // Use Imei with the check digit to query phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);
                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime dDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string logName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                 string workingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string sw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 itemInfo = new CrashLogInfo();

                                 itemInfo.PhonTransId = phonTransId;
                                 itemInfo.LogName = logName;
                                 itemInfo.PhoneModeId = phoneModelId;
                                 itemInfo.DeliverTime = dDeliverTime;
                                 itemInfo.WorkingName = workingName;
                                 itemInfo.Sw1Revision = sw1revision;

                                 lutSystemCrash8OkList.Add(itemInfo);
                                 lutSystemCrash8OkItems.Add(temname);
                             }
                             else
                             {
                                 lutSystemCrash8FailItems.Add(temname);
                             }
                         }
                         else
                         {
                             lutSystemCrash8FailItems.Add(temname);
                            
                         }
                     }
                     catch(Exception ex)
                     {
                         Trace.WriteLine(ex.Message);
                         lutSystemCrash8FailItems.Add(temname);
                     }
                 }

                 foreach (string temname in lutSysCrashFile28)
                 {
                     if (!isValidLutSystemCrashFile(temname))
                     {
                         continue;
                     }

                     try
                     {
                         string sql = "SELECT top 1 T1.PhoneTrans_id, T1.SW1_id, T1.DeliverTime, T1.PhoneModel_id, T1.LogName , T2.Workingname, T3.Sw1Revision "
                            + " FROM tblphonetrans T1 with(nolock)  LEFT JOIN Tblphonemodel T2 ON T1.PhoneModel_id =  T2.PhoneModel_id "
                            + " LEFT JOIN TblSw1 T3 ON T1.SW1_id = T3.SW1_id "
                            + " WHERE DeliverTime=@pInDeliverTime  "
                            + " AND phone_id  = (SELECT phone_id FROM tblphone with(nolock) where imei =@pIn_IMEI)";

                         DateTime dlvTime;
                         temdate = getLutSystemCrashDate(temname);
                         if (!DateTime.TryParse(temdate, out dlvTime))
                         {
                             dlvTime = DateTime.Parse("1970-01-01");
                         }

                         SqlCommand cmd = new SqlCommand(sql, conn);
                         imeinumber = getLutSystemCrashImei(temname);
                         cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                         cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                         SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                         DataSet ds = new DataSet();
                         adapter.Fill(ds);

                         if (ds.Tables[0].Rows.Count > 0)
                         {
                             DateTime dDeliverTime;
                             long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                             string logName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                             long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                             //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                             DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                             string workingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                             string sw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                             itemInfo = new CrashLogInfo();

                             itemInfo.PhonTransId = phonTransId;
                             itemInfo.LogName = logName;
                             itemInfo.PhoneModeId = phoneModelId;
                             itemInfo.DeliverTime = dDeliverTime;
                             itemInfo.WorkingName = workingName;
                             itemInfo.Sw1Revision = sw1revision;
                             lutSystemCrash28OkList.Add(itemInfo);
                             lutSystemCrash28OkItems.Add(temname);
  
                         }
                         else if (imeinumber.Length == 15)
                         {
                             imeinumber = imeinumber.Remove(14, 1);

                             //use the imei without check digit to do query from phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);

                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime dDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string logName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                 string workingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string sw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 itemInfo = new CrashLogInfo();

                                 itemInfo.PhonTransId = phonTransId;
                                 itemInfo.LogName = logName;
                                 itemInfo.PhoneModeId = phoneModelId;
                                 itemInfo.DeliverTime = dDeliverTime;
                                 itemInfo.WorkingName = workingName;
                                 itemInfo.Sw1Revision = sw1revision;

                                 lutSystemCrash28OkList.Add(itemInfo);
                                 lutSystemCrash28OkItems.Add(temname);

                             }
                         }
                         else if (imeinumber.Length == 14)
                         {
                             // Calculate the check digit 
                             int checkDigit = ImeiValidator.CalcCheckDigit(imeinumber);
                             // Make up new Imei with the check digit
                             imeinumber = imeinumber.PadRight(15, Char.Parse(checkDigit.ToString()));

                             // Use Imei with the check digit to query phonetrans
                             cmd.Parameters.Clear();
                             cmd.Parameters.Add("@pInDeliverTime", SqlDbType.DateTime).Value = dlvTime.ToString();
                             cmd.Parameters.Add("@pIn_IMEI", SqlDbType.VarChar).Value = imeinumber;
                             adapter = new SqlDataAdapter(cmd);
                             ds = new DataSet();
                             adapter.Fill(ds);
                             if (ds.Tables[0].Rows.Count > 0)
                             {
                                 DateTime dDeliverTime;
                                 long phonTransId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneTrans_id"]);
                                 string logName = Convert.ToString(ds.Tables[0].Rows[0]["LogName"]);
                                 long phoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                                 //DateTime dDeliverTime = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliverTime"]);
                                 DateTime.TryParse(ds.Tables[0].Rows[0]["DeliverTime"].ToString(), out dDeliverTime);
                                 string workingName = Convert.ToString(ds.Tables[0].Rows[0]["Workingname"]);
                                 string sw1revision = Convert.ToString(ds.Tables[0].Rows[0]["Sw1Revision"]);

                                 itemInfo = new CrashLogInfo();
                                 itemInfo.PhonTransId = phonTransId;
                                 itemInfo.LogName = logName;
                                 itemInfo.PhoneModeId = phoneModelId;
                                 itemInfo.DeliverTime = dDeliverTime;
                                 itemInfo.WorkingName = workingName;
                                 itemInfo.Sw1Revision = sw1revision;

                                 lutSystemCrash28OkList.Add(itemInfo);
                                 lutSystemCrash28OkItems.Add(temname);

                             }
                         }
                     }
                     catch (Exception ex)
                     {
                         Trace.WriteLine(ex.Message);
                     }
                 }
             }


         }

         private bool isAvailiableSysCrashFolder(string name)
         {
             bool bRet = false;
             try
             {
                 string[] paraArray = name.Split('-');
                 if (paraArray.Length >= 7 && (name.StartsWith("Crash-YMD", false, null)|| name.StartsWith("LUT_Crash-YMD",false,null)))
                 {
                     long rst1, rst2;
                     string delivertime = paraArray[3] + paraArray[4];
                     string imei = paraArray[5];

                     Int64.TryParse(delivertime, out rst1);
                     Int64.TryParse(imei, out rst2);

                     if (rst1 > 0 && rst2 > 0)
                         bRet = true;
                     else
                         bRet = false;
                 }
             }
             catch(Exception ex)
             {
                 Trace.WriteLine("In isAvailiableSysCrashFolder exception happens " + ex.Message);
                 bRet = false;
             }
             return bRet;
         }

         private bool isValidLutSystemCrashFile(string name)
         {
             bool bRet = false;
             char[] seperators = {'_', '-'};
             try
             {
                 string[] paraArray = name.Split(seperators);
                 if (name.StartsWith("LUT_CoreDump", false, null) && paraArray.Length >= 6)
                 {
                     long rst1, rst2;
                     string delivertime = paraArray[2] + paraArray[3];
                     string imei = paraArray[4];

                     Int64.TryParse(delivertime, out rst1);
                     Int64.TryParse(imei, out rst2);

                     if (rst1 > 0 && rst2 > 0)
                         bRet = true;
                     else
                         bRet = false;
                 }
                 else if (name.StartsWith("LUT_Crash-SSR-YMD-HMS", false, null) && paraArray.Length >= 9)
                 {
                     long rst1, rst2;
                     string delivertime = paraArray[5] + paraArray[6];
                     string imei = paraArray[7];

                     Int64.TryParse(delivertime, out rst1);
                     Int64.TryParse(imei, out rst2);

                     if (rst1 > 0 && rst2 > 0)
                         bRet = true;
                     else
                         bRet = false;
                 }
                 else
                 {
                     bRet = false;
                 }
             }
             catch (Exception ex)
             {
                 Trace.WriteLine("In isValidLutSystemCrashFile exception happens " + ex.Message);
                 bRet = false;
             }

             return bRet;

         }

         private string getLutSystemCrashDate(string filename)
         {
             string strRet = "";
             string date;
             string time;
             char[] seperators = { '_', '-' };

             if (!isValidLutSystemCrashFile(filename))
                 return strRet;
             //LUT_CoreDump_19700101_000000-004402451025617-C6906 LUT_Crash-SSR-YMD-HMS-20130623-100843-004402146658715-C6802
             string[] paraArray = filename.Split(seperators);
             if(filename.StartsWith("LUT_CoreDump", false, null))
             {
                 date = paraArray[2];
                 time = paraArray[3];
             }
             else if (filename.StartsWith("LUT_Crash-SSR-YMD-HMS", false, null))
             {
                 date = paraArray[5];
                 time = paraArray[6];
             }
             else
             {
                 return strRet;
             }

             if (time.Length != 6)
                 return strRet;

             strRet = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + time.Substring(0, 2) + ":" + time.Substring(2, 2) + ":" + time.Substring(4, 2);

             return strRet;

         }

         private string getLutSystemCrashImei(string filename)
         {
             string strRet = "";
             char[] seperators = { '_', '-' };
             if (!isValidLutSystemCrashFile(filename))
                 return strRet;

             string[] paraArray = filename.Split(seperators);
             if (filename.StartsWith("LUT_CoreDump", false, null))
             {
                 strRet = paraArray[4];
             }
             else if (filename.StartsWith("LUT_Crash-SSR-YMD-HMS", false, null))
             {
                 strRet = paraArray[7];
             }
             else
             {
                 strRet = "";
             }

             return strRet;

         }

        private string getSysCrashDate(string filename)
        {
            string strRet="";
            if (!isAvailiableSysCrashFolder(filename))
                return strRet;
            string[] paraArray = filename.Split('-');

            string date = paraArray[3];
            string time = paraArray[4];
            if (time.Length != 6)
                return strRet;
            strRet = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + time.Substring(0, 2) + ":" + time.Substring(2, 2) + ":" + time.Substring(4, 2);
            
            return strRet;
        }

        private string getSysCrashImei(string filename)
        {
            string strRet = "";
            if (!isAvailiableSysCrashFolder(filename))
                return strRet;
            string[] paraArray = filename.Split('-');

            string imei = paraArray[5];
            strRet = imei;

            return strRet;
        }


        private bool isAvailiableAppCrashFolder(string name)
        {
            bool bRet = false;
            try
            {
                string[] paraArray = name.Split('-');
                if (paraArray.Length >= 5 && (name.StartsWith("BugReport", false, null) || name.StartsWith("LUT_BugReport",false,null))&& name.IndexOf(".gz") > 0)
                {
                    long rst1, rst2;
                    string time = paraArray[3];
                    time = time.TrimEnd('Z');
                    string delivertime = paraArray[2] +time;
                    string imei = paraArray[1];

                    Int64.TryParse(delivertime, out rst1);
                    Int64.TryParse(imei, out rst2);

                    if (rst1 > 0 && rst2 > 0)
                        bRet = true;
                    else
                        bRet = false;
                }
            }
            catch
            {
            }
            return bRet;
        }
        
        
        private string getAppCrashDate(string filename)
        {
            string strRet = "";
            if (!isAvailiableAppCrashFolder(filename))
                return strRet;
            string[] paraArray = filename.Split('-');

            string date = paraArray[2];
            string time = paraArray[3];
            time = time.TrimEnd('Z');
            strRet = date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + time.Substring(0, 2) + ":" + time.Substring(2, 2) + ":" + time.Substring(4, 2);

            return strRet;
        }

        private string getAppCrashImei(string filename)
        {
            string strRet = "";
            if (!isAvailiableAppCrashFolder(filename))
                return strRet;
            string[] paraArray = filename.Split('-');

            string imei = paraArray[1];
            strRet = imei;

            return strRet;
        }

        public void updateLutSystemCrashOkMT8()
        {
            MTBF.tblCrashLinkDataTable table = new MTBF.tblCrashLinkDataTable();
            string path = configData.PCCPath + @"\" + configData.LogName + @"\" + configData.currentDate;
            int year = Int32.Parse(configData.currentDate.Substring(0, 4));
            int month = Int32.Parse(configData.currentDate.Substring(4, 2));
            int day = Int32.Parse(configData.currentDate.Substring(6, 2));
            DateTime matchDate = new DateTime(year, month, day);

            MTBFTableAdapters.tblCrashLinkTableAdapter a = new MTBFTableAdapters.tblCrashLinkTableAdapter();
            Trace.WriteLine("******** The match LUT System Crash Count: " + lutSystemCrash8OkList.Count);
            for (int i = 0; i < lutSystemCrash8OkList.Count; i++)
            {
                bool islut = true;
                string oldPath;
                string newPath;

                oldPath = configData.LUTPath + @"\" + "lutcrashlink" + @"\" + lutSystemCrash8OkItems[i];
                newPath = path + @"\" + lutSystemCrash8OkItems[i];

                try
                {

                    if (MoveData(oldPath, newPath, 1))
                    {
                        a.Insert(lutSystemCrash8OkList[i].PhonTransId, newPath, true, true, lutSystemCrash8OkList[i].PhoneModeId,
                            lutSystemCrash8OkList[i].WorkingName, lutSystemCrash8OkList[i].Sw1Revision, lutSystemCrash8OkList[i].LogName,
                            lutSystemCrash8OkList[i].DeliverTime, matchDate, sysCrash, islut);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("updateLutSystemCrashOkMT8 System: " + ex.Message);
                }
            }

            lutSystemCrash8OkItems.Clear();
            lutSystemCrash8OkList.Clear();
        }

        public void updateLutSystemCrashOkBT28()
        {
            MTBF.tblCrashLinkDataTable table = new MTBF.tblCrashLinkDataTable();
            string path = configData.PCCPath + @"\" + configData.LogName + @"\" + configData.currentDate;
            int year = Int32.Parse(configData.currentDate.Substring(0, 4));
            int month = Int32.Parse(configData.currentDate.Substring(4, 2));
            int day = Int32.Parse(configData.currentDate.Substring(6, 2));
            DateTime matchDate = new DateTime(year, month, day);

            MTBFTableAdapters.tblCrashLinkTableAdapter a = new MTBFTableAdapters.tblCrashLinkTableAdapter();
            for (int i = 0; i < lutSystemCrash28OkList.Count; i++)
            {

                bool islut = true;
                string oldPath;
                string newPath;

                oldPath = configData.LUTPath + @"\" + "lutcrashlink" + @"\" + lutSystemCrash28OkItems[i];
                newPath = path + @"\" + lutSystemCrash28OkItems[i];

                try
                {

                    if (MoveData(oldPath, newPath, 1))
                    {
                        a.Insert(lutSystemCrash28OkList[i].PhonTransId, newPath, true, true, lutSystemCrash28OkList[i].PhoneModeId,
                            lutSystemCrash28OkList[i].WorkingName, lutSystemCrash28OkList[i].Sw1Revision, lutSystemCrash28OkList[i].LogName,
                            lutSystemCrash28OkList[i].DeliverTime, matchDate, sysCrash, islut);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("updateLutSystemCrashOkMT8 System: " + ex.Message);
                }
            }

            lutSystemCrash28OkItems.Clear();
            lutSystemCrash28OkList.Clear();
        }

        public void updateOKMoreThan8(  )
        {
            
            MTBF.tblCrashLinkDataTable table = new MTBF.tblCrashLinkDataTable();
            string path = configData.PCCPath+@"\"+configData.LogName+@"\"+configData.currentDate;
            int year = Int32.Parse(configData.currentDate.Substring(0,4));
            int month = Int32.Parse(configData.currentDate.Substring(4,2));
            int day = Int32.Parse(configData.currentDate.Substring(6,2));
            DateTime matchDate = new DateTime(year,month,day);

            MTBFTableAdapters.tblCrashLinkTableAdapter a = new MTBFTableAdapters.tblCrashLinkTableAdapter();
            for (int i = 0; i < dOkPhoneTransIdList.Count; i++)
            {
              bool islut = false;
              string oldPath;
              string newPath;

              if (dOklist[i].StartsWith("LUT", false, null))
              {
                  islut = true;
                  oldPath = configData.LUTPath + @"\" + "lutcrashlink" + @"\" + dOklist[i];
              }
              else
              {
                  islut = false;
                  oldPath = configData.PCCPath + @"\" + dOklist[i];
              }
              newPath = path + @"\" + dOklist[i];
              try
              {

                  if (MoveData(oldPath, newPath, 0))
                  {
                      a.Insert(dOkPhoneTransIdList[i], newPath, true, true, dphoneModelIdList[i],dworkingNameList[i],dsw1revisionList[i] ,dlogNameList[i], dDeliverTimeList[i], matchDate, sysCrash, islut);
                  }
              }
              catch (Exception ex)
              {
                  Trace.WriteLine("updateOKData System: " + ex.Message);
              }
            }

            for (int i = 0; i < fOkPhoneTransIdList.Count; i++ )
            {
              bool islut = false;
              string oldPath;
              string newPath;

              if (fOklist[i].StartsWith("LUT", false, null))
              {
                  islut = true;
                  oldPath = configData.LUTPath + @"\" + "lutcrashlink" + @"\" + fOklist[i];
              }
              else
              {
                  islut = false;
                  oldPath = configData.PCCPath + @"\" + fOklist[i];
              }

              newPath = path + @"\" + fOklist[i];
              try
              {
                  if (MoveData(oldPath, newPath, 1))
                  {
                      a.Insert(fOkPhoneTransIdList[i], newPath, true, true, fphoneModelIdList[i],fworkingNameList[i],fsw1revisionList[i], flogNameList[i], fDeliverTimeList[i], matchDate, appCrash, islut);
                  }
              }
              catch (Exception ex)
              {
                  Trace.WriteLine("updateOKData App: " + ex.Message);
              }
            }


            dOklist.Clear();
            dOkPhoneTransIdList.Clear();
            dphoneModelIdList.Clear();
            dlogNameList.Clear();
            dDeliverTimeList.Clear();
            dworkingNameList.Clear();
            dsw1revisionList.Clear();


            fOklist.Clear();
            fOkPhoneTransIdList.Clear();
            fphoneModelIdList.Clear();
            flogNameList.Clear();
            fDeliverTimeList.Clear();
            fworkingNameList.Clear();
            fsw1revisionList.Clear();

        }

        public void updateOKBTW2To8()
        {

            MTBF.tblCrashLinkDataTable table = new MTBF.tblCrashLinkDataTable();
            string path = configData.PCCPath + @"\" + configData.LogName + @"\" + configData.currentDate;
            int year = Int32.Parse(configData.currentDate.Substring(0, 4));
            int month = Int32.Parse(configData.currentDate.Substring(4, 2));
            int day = Int32.Parse(configData.currentDate.Substring(6, 2));
            DateTime matchDate = new DateTime(year, month, day);

            MTBFTableAdapters.tblCrashLinkTableAdapter a = new MTBFTableAdapters.tblCrashLinkTableAdapter();
            for (int i = 0; i < d28OkPhoneTransIdList.Count; i++)
            {
                bool islut = false;
                string oldPath;
                string newPath;

                if (d28Oklist[i].StartsWith("LUT", false, null))
                {
                    islut = true;
                    oldPath = configData.LUTPath + @"\" + "lutcrashlink" + @"\" + d28Oklist[i];
                }
                else
                {
                    islut = false;
                    oldPath = configData.PCCPath + @"\" + d28Oklist[i];
                }
                newPath = path + @"\" + d28Oklist[i];
                try
                {

                    if (MoveData(oldPath, newPath, 0))
                    {
                        a.Insert(d28OkPhoneTransIdList[i], newPath, true, true, d28phoneModelIdList[i], d28workingNameList[i],d28sw1revisionList[i],d28logNameList[i], d28DeliverTimeList[i], matchDate, sysCrash, islut);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("updateOKData System: " + ex.Message);
                }
            }

            for (int i = 0; i < f28OkPhoneTransIdList.Count; i++)
            {
                bool islut = false;
                string oldPath;
                string newPath;

                if (f28Oklist[i].StartsWith("LUT", false, null))
                {
                    islut = true;
                    oldPath = configData.LUTPath + @"\" + "lutcrashlink" + @"\" + f28Oklist[i];
                }
                else
                {
                    islut = false;
                    oldPath = configData.PCCPath + @"\" + f28Oklist[i];
                }

                newPath = path + @"\" + f28Oklist[i];
                try
                {
                    if (MoveData(oldPath, newPath, 1))
                    {
                        a.Insert(f28OkPhoneTransIdList[i], newPath, true, true, f28phoneModelIdList[i],f28workingNameList[i],f28sw1revisionList[i], f28logNameList[i], f28DeliverTimeList[i], matchDate, appCrash, islut);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("updateOKData App: " + ex.Message);
                }
            }


            d28Oklist.Clear();
            d28OkPhoneTransIdList.Clear();
            d28phoneModelIdList.Clear();
            d28logNameList.Clear();
            d28DeliverTimeList.Clear();
            d28workingNameList.Clear();
            d28sw1revisionList.Clear();


            f28Oklist.Clear();
            f28OkPhoneTransIdList.Clear();
            f28phoneModelIdList.Clear();
            f28logNameList.Clear();
            f28DeliverTimeList.Clear();
            f28workingNameList.Clear();
            f28sw1revisionList.Clear();

        }

        private bool isAvalibleName(string strCrashLink)
        {
            bool bRet = false;
            string[] paraArray = strCrashLink.Split('-');
            try
            {
                if (strCrashLink.IndexOf(".gz") > 0)
                {
                    if (isAvailiableAppCrashFolder(strCrashLink))
                    {
                        bRet = true;
                    }
                }
                else
                {
                    if (isAvailiableSysCrashFolder(strCrashLink))
                    {
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;
                    }
                }

            }
            catch(Exception ex)
            {
                Trace.WriteLine("exception occurs in function *isAvalibleName* : " + ex.ToString());
            }
            return bRet;
        }
        private string getShortName(string strCrashLink)
        {
            int indexi = 0;
            int indexa = 0;
            int index = 0;
            int gzindex = 0;
            string strShortName = "";
            if (!isAvalibleName(strCrashLink))
                return strShortName;
            string[] paraArray = strCrashLink.Split('-');
            gzindex = strCrashLink.IndexOf(".gz");

            try 
            {
                if (gzindex > 0)
                {
                    for (int i = 0; i <= 3; i++)
                    {
                        index = index + paraArray[i].Length + 1;
                    }
                    strShortName = strCrashLink.Substring(index, (gzindex - index));
                }
                else
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        index = index + paraArray[i].Length + 1;
                    }
                    strShortName = strCrashLink.Substring(index);
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Exception occurs in function *getShortNameupdate* : " + ex.ToString());
            }
            return strShortName;
        }

        private string getLutSystemCrashShortName(string strCrashLink)
        {
            //LUT_CoreDump_20130206_113509-004402450707876-SGP321
            string strShotName = "";
            if (!isValidLutSystemCrashFile(strCrashLink))
            {
                return strShotName;
            }
            char[] seperators = {'_', '-'};
            string[] paraArray = strCrashLink.Split(seperators);
            int zipIndex = strCrashLink.IndexOf(".zip");
            int index = 0;

            if (strCrashLink.StartsWith("LUT_CoreDump"))
            {
                for (int i = 0; i <= 4; i++)
                {
                    index = index + paraArray[i].Length + 1;
                }

                strShotName = strCrashLink.Substring(index, (zipIndex-index));
            }
            else if (strCrashLink.StartsWith("LUT_Crash-SSR-YMD-HMS"))//LUT_Crash-SSR-YMD-HMS-20130607-163142-004402451053221-ymmh5y0n5zeaouu
            {
                for (int i = 0; i <= 7; i++ )
                {
                    index = index + paraArray[i].Length + 1;
                }

                strShotName = strCrashLink.Substring(index, (zipIndex-index));
                
            }

            return strShotName;
        }

        private long getPhoneModelId(string strCrashLink)
        {
            long PhoneModelId =0;
            string shortName;

            if (strCrashLink.StartsWith("LUT_Crash-SSR-YMD-HMS") || strCrashLink.StartsWith("LUT_CoreDump"))
                shortName = getLutSystemCrashShortName(strCrashLink);
            else
                shortName = getShortName(strCrashLink);

            using (SqlConnection conn = new SqlConnection(strconn))
            {
                conn.Open();

                String sql = "SELECT top 1 PhoneModel_id FROM tblphoneModel with(nolock)  "
                                 + "WHERE ShortName =@pShortName";



                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add("@pShortName", SqlDbType.VarChar).Value = shortName;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                  PhoneModelId = Convert.ToInt64(ds.Tables[0].Rows[0]["PhoneModel_id"]);
                  
  
            }
            return PhoneModelId;
        }


        bool MoveData(string oldPath,string newPath,int iFlag)
        {
            bool bRet = false;
             try
                {
                    if (iFlag > 0)
                    {
                        FileInfo finfor = new FileInfo(oldPath);
                        if (finfor.Exists)
                        {

                            finfor.MoveTo(newPath);

                            bRet = true;
                        }
                        else
                        {
                            ;//Trace.WriteLine(oldPath.ToString() + " is not exist");
                        }
                    }
                    else
                    {
                        DirectoryInfo dinfo = new DirectoryInfo(oldPath);
                        if (dinfo.Exists)
                        {

                            dinfo.MoveTo(newPath);

                            bRet = true;
                        }
                    }
                 
                }
             catch
             {
             }
            return bRet;
        }

        public static List<string> ReadProductInfo(string folder_path)
        {
            string file_path = folder_path + @"\" + "productinfo";
            List<string> infos = new List<string>();
            if (File.Exists(file_path))
            {
                string[] content = File.ReadAllLines(file_path);
                if (content.Length == 0)
                {
                    infos.Add(" ");
                    infos.Add(" ");
                    return infos;
                }
                string productinfo = content[0].ToString();

                string[] detail = productinfo.Split('\\');
                if (detail.Length == 4)
                {
                    infos.Add(detail[1]);
                    infos.Add(detail[2]);
                }
                else
                {
                    infos.Add(" ");
                    infos.Add(" ");
                }
            }
            else
            {
                infos.Add(" ");
                infos.Add(" ");
            }

            return infos;
        }

        public void updateFailedLutSystemCrashData()
        {
            bool islut = true;
            string oldPath;
            string newPath;
            MTBF.tblCrashLinkDataTable table = new MTBF.tblCrashLinkDataTable();
            string path = configData.PCCPath + @"\" + configData.LogName + @"\" + configData.currentDate + "NoMatch";
            int year = Int32.Parse(configData.currentDate.Substring(0, 4));
            int month = Int32.Parse(configData.currentDate.Substring(4, 2));
            int day = Int32.Parse(configData.currentDate.Substring(6, 2));
            DateTime matchDate = new DateTime(year, month, day);
            int i = 0;
            MTBFTableAdapters.tblCrashLinkTableAdapter a = new MTBFTableAdapters.tblCrashLinkTableAdapter();
            Trace.WriteLine("******** nomatching Lut system crash count: " + lutSystemCrash8FailItems.Count);
            foreach(string item in lutSystemCrash8FailItems)
            {
                oldPath = configData.LUTPath + "\\" + "lutcrashlink" + "\\" + item;
                newPath = path + @"\" + item;

                try
                {
                    if (MoveData(oldPath, newPath, 1))
                    {
                        DateTime deliverTime;
                        if (DateTime.TryParse(getLutSystemCrashDate(item), out deliverTime))
                            a.Insert(0, newPath, true, false, getPhoneModelId(newPath), null, null, null, deliverTime, matchDate, appCrash, islut);
                        else
                            a.Insert(0, newPath, true, false, getPhoneModelId(newPath), null, null, null, DateTime.Parse("1970-01-01"), matchDate, appCrash, islut);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("updateFailedData App: " + ex.Message);
                }
            }
        }

        public void updateFailedData()
        {
            MTBF.tblCrashLinkDataTable table = new MTBF.tblCrashLinkDataTable();
            string path = configData.PCCPath + @"\" + configData.LogName + @"\" + configData.currentDate + "NoMatch";
            int year = Int32.Parse(configData.currentDate.Substring(0,4));
            int month = Int32.Parse(configData.currentDate.Substring(4,2));
            int day = Int32.Parse(configData.currentDate.Substring(6,2));
            DateTime matchDate = new DateTime(year, month, day);
            int i = 0;
            MTBFTableAdapters.tblCrashLinkTableAdapter a = new MTBFTableAdapters.tblCrashLinkTableAdapter();

            //LUT_Crash-YMD-HMS-20120221-143134-004402145044057-LT22i
            foreach (string tem in dFailedlist)
            {
                bool islut = false;
                string oldPath;
                string newPath;

                if(tem.StartsWith("LUT"))
                {
                    islut = true;
                    oldPath = configData.LUTPath + "\\" + "lutcrashlink" + "\\" + tem;
                }
                else
                {
                    islut = false;
                    oldPath = configData.PCCPath + @"\" + tem;
                }
                //get the productinfo from the crash folder

                newPath = path + @"\" + tem;
                try
                {
                    List<string> productInfo = ReadProductInfo(oldPath);
                    string workingName = productInfo[1].ToString();
                    string sw1revision = productInfo[0].ToString();
                    if (MoveData(oldPath, newPath, 0))
                    {
                        DateTime deliverTime;
                        if (DateTime.TryParse(getSysCrashDate(tem), out deliverTime))
                            a.Insert(0, newPath, true, false, getPhoneModelId(tem), workingName, sw1revision, null, deliverTime, matchDate, sysCrash, islut);
                        else
                            a.Insert(0, newPath, true, false, getPhoneModelId(tem),workingName,sw1revision,null, DateTime.Parse("1970-01-01"), matchDate, sysCrash, islut);
                    }
                }catch(Exception ex)
                {
                    Trace.WriteLine("updateFailedData System: " + ex.Message + "   " + oldPath);
                }
            }

            foreach (string tem in fFailedlist)
            {

                bool islut = false;
                string oldPath;
                string newPath;
                if (tem.StartsWith("LUT", false, null))
                {
                    islut = true;
                    oldPath = configData.LUTPath + @"\" + "lutcrashlink" + @"\" + tem;
                }
                else
                {
                    islut = false;
                    oldPath = configData.PCCPath + @"\" + tem;
                }

                newPath = path + @"\" + tem;
                try
                {
                    if (MoveData(oldPath, newPath, 1))
                    {
                        DateTime deliverTime;
                        if(DateTime.TryParse(getAppCrashDate(tem), out deliverTime))
                            a.Insert(0, newPath, true, false, getPhoneModelId(newPath),null,null, null, deliverTime, matchDate, appCrash, islut);
                        else
                            a.Insert(0, newPath, true, false, getPhoneModelId(newPath),null, null, null, DateTime.Parse("1970-01-01"), matchDate, appCrash, islut);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("updateFailedData App: " + ex.Message);
                }
            }

            dFailedlist.Clear();
            fFailedlist.Clear();
        }

        private List<string> UpdateIsVisibleAndRetrievePath(string sqlcmds)
        {
            List<string> path = new List<string>();
            using (SqlConnection conn = new SqlConnection(strconn))
            {
                conn.Open();

                try
                {
                    SqlCommand selectcmd = new SqlCommand(sqlcmds, conn);

                    SqlCommand updatecmd = new SqlCommand("sp_CrashLinkUpdate", conn);
                    updatecmd.CommandType = CommandType.StoredProcedure;
                    updatecmd.Parameters.Add(new SqlParameter("@Isvisible", SqlDbType.Bit, 0, "Isvisible"));
                    //updatecmd.Parameters.Add(new SqlParameter("@CrashLink", SqlDbType.VarChar, 800, "CrashLink"));
                    updatecmd.Parameters.Add(new SqlParameter("@CrashLink_id", SqlDbType.BigInt, 0, "CrashLink_id"));
                    updatecmd.UpdatedRowSource = UpdateRowSource.None;

                    System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter();
                    adapter.SelectCommand = selectcmd;
                    adapter.UpdateCommand = updatecmd;

                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "tblCrashLink");

                    foreach (DataRow row in ds.Tables["tblCrashLink"].Rows)
                    {
                        path.Add(Convert.ToString(row["CrashLink"]));
                        row["IsVisible"] = 0;
                        //row["CrashLink"] = "";
                    }
                    adapter.Update(ds, "tblCrashLink"); //update DB to make the modification take affect
                }
                catch (SqlException ex)
                {
                    string str = ex.Message;
                    Trace.WriteLine("When query or update table tblCrashLink, exception occurs: " + str);
                }
            }
            return path;
        }

        private void DeleteOldModelLog(List<string> pathes)
        {
            foreach (string path in pathes)
            {
                try
                {
                    if (path.Substring(mainDirLen).Contains('.'))
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        else
                        {
                            Trace.WriteLine("The File {0} does not exist", path);
                        }
                    }
                    else // this is a direcotry
                    {
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        else
                        {
                            Trace.WriteLine("The directory {0} does not exist", path);
                        }
                    }
                }
                catch(Exception ex)
                {
                    string str = ex.Message;
                    Trace.WriteLine("When try to delete crash link , exception occurs: " + str);
                }
            }
        }

        public void DeleteOldFileByValidDeliverTime()
        {
            Settings.Default.Reload();
            int deleteDays = Settings.Default.DeleteDays;
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string selectcmd =  "SELECT * FROM tblCrashLink with(nolock) " +
                        "WHERE Isvisible=1 and DeliverTime>'2001-01-01' and datediff(day, DeliverTime,'" + date + "') > " + deleteDays;

            List<string> selectResult = UpdateIsVisibleAndRetrievePath(selectcmd);
            Trace.WriteLine("Delete old log fies by DeliverTime, count: " + selectResult.Count.ToString());
            DeleteOldModelLog(selectResult);
        }

        public void DeleteOldFileByMatchDate()
        {
            Settings.Default.Reload();
            int deleteDays = Settings.Default.DeleteDays;
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string selectcmd = "SELECT * FROM tblCrashLink with(nolock) " +
            "WHERE Isvisible=1  and datediff(day, MatchDate,'" + date + "') > " + deleteDays;

            List<string> selectResult = UpdateIsVisibleAndRetrievePath(selectcmd);
            Trace.WriteLine("Delete old log fies by MatchDate, count: " + selectResult.Count.ToString());
            //if(selectResult.Count != 0 )
                DeleteExpiredLogFile(deleteDays);
        }

        public void DeleteOldFileByPhoneModel()
        {
            List<string> phoneModel_ids  = new List<string>();
            Settings.Default.Reload();
            string[] workingNames = Settings.Default.DeleteModels.Split('#');

            phoneModel_ids = GetPhoneModeIDFromWorkingName(workingNames);

            string selectcmd = "SELECT * FROM tblCrashLink with(nolock) " +
                        "WHERE Isvisible=1  and PhoneModel_Id in (";
            foreach (string item in phoneModel_ids)
            {
                selectcmd = selectcmd + item + ",";
            }
            selectcmd = selectcmd.TrimEnd(',');
            selectcmd = selectcmd + ")";
            List<string> selectResult = UpdateIsVisibleAndRetrievePath(selectcmd);
            Trace.WriteLine("Delete old log fies by PhoneModel, count: " + selectResult.Count);
            DeleteOldModelLog(selectResult);
        }


        private List<string> GetOldCrashLogLink(string crashloglinkpath)
        {
            List<string> olderlinks = new List<string>();
            DirectoryInfo[ ] subDirs =null;
            try
            {
                DirectoryInfo logLinkRoot = new DirectoryInfo(crashloglinkpath);
                if (logLinkRoot.Exists)
                {
                    subDirs = logLinkRoot.GetDirectories();

                    foreach (DirectoryInfo item in subDirs)
                    {
                        olderlinks.Add(item.Name);
                    }
                }
                else
                {
                    Trace.WriteLine("The crash link log path doesn't exist: " + crashloglinkpath.ToString());
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Exception occurs when read log folders in GetOldCrashLogLink" + ex.ToString());
            }

            return olderlinks;
        }

        private void DeleteOldFileOnDate(List<string> crashLogFolders, int deleteDays)
        { 
            // anaylize folder's name to pick out the folders that match the deletion conditions
            int year = 0;
            int month = 0;
            int day = 0;
            int count = 0;
            foreach (string foldername in crashLogFolders)
            {
                int result;
                if(! Int32.TryParse(foldername.Substring(0, 8), out result))
                    continue;
                year = Int32.Parse(foldername.Substring(0, 4));
                month = Int32.Parse(foldername.Substring(4, 2));
                day = Int32.Parse(foldername.Substring(6, 2));
                try
                {
                    DateTime folderDate = new DateTime(year, month, day);

                    if (DateTime.Now.Subtract(folderDate).Days > deleteDays)
                    {
                        string tempPath = configData.PCCPath + "\\" + configData.LogName + "\\" + foldername;
                        Directory.Delete(tempPath, true);
                        count++;
                    }
                }
                catch(Exception ex)
                {
                    Trace.WriteLine("Exception occurs when delete folder by MatchDate:" + ex.ToString());
                }
            }

            Trace.WriteLine("There are " + count + " folders has been deleted according to the MatchDate are older than " + deleteDays+ " days");
        }

        private void DeleteExpiredLogFile( int deleteDays)
        {
            string crashLogLinkPath = configData.PCCPath + "\\" + configData.LogName;
            List<string> oldLogs = GetOldCrashLogLink(crashLogLinkPath);

            DeleteOldFileOnDate(oldLogs, deleteDays);
        }

        private List<string> GetPhoneModeIDFromWorkingName(string[] workingnames)
        {
            List<string> phoneModel_ids = new List<string>();
            using (SqlConnection conn = new SqlConnection(strconn))
            {
                conn.Open();
                for (int i = 0; i < workingnames.Length; i++)
                {
                    string sqlcmd = "Select PhoneModel_Id from tblphonemodel with(nolock) where workingname='"+workingnames[i]+"'";
                    try
                    {
                        SqlCommand selectcmd = new SqlCommand(sqlcmd, conn);
                        System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter();
                        adapter.SelectCommand = selectcmd;
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);
                        for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                        {
                            phoneModel_ids.Add(ds.Tables[0].Rows[j][0].ToString());
                        }

                        if (workingnames.Length == 1 && workingnames[0].Equals(""))
                        {
                            phoneModel_ids.Add("177");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("Exception occurs when get PhoneModel_id from workingname: " + ex.ToString());
                    }
                }
            }

            return phoneModel_ids;
        }


     
    }
}
