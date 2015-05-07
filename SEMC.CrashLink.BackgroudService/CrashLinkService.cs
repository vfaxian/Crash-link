using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace SEMC.CrashLink.BackgroudService
{
    public partial class CrashLinkService : ServiceBase
    {
        private CrashLogLinkService sv;
        private MonitorService ms;
        //private UnzipLutService unZipsv;

        public CrashLinkService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log.instance().LogName = "CrashLinkService";
                Trace.WriteLine("Start CrashLinkService..............");
                StartCrashLinkService();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error during initialization (OnStart):\n" + ex.ToString());
                throw (ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                Trace.WriteLine("Stop CrashLink services.............");
                StopCrashLinkService();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Eorror occurs when try to stop CrashLink services\n" + ex.ToString());
            }
        }

        private void StartCrashLinkService()
        {
            sv = new CrashLogLinkService();
            ms = new MonitorService();
            //unZipsv = new UnzipLutService();
            ms.Start();
            sv.Start();
            //unZipsv.Start();
        }

        private void StopCrashLinkService()
        {
            ms.Stop();
            sv.Stop();
            //unZipsv.Stop();
        }
        //Analysize service 
        
        //Monitor disk space service
        
    }
}
