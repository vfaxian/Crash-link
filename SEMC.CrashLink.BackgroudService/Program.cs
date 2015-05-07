using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
namespace SEMC.CrashLink.BackgroudService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            {
                new CrashLinkService()
            };
            ServiceBase.Run(ServicesToRun);
            ///for test
            /// //
            //CrashLogLinkService sv = new CrashLogLinkService();
            //sv.Start();


          //  MonitorService sv = new MonitorService();
         //   sv.Start();

         //Thread.Sleep(TimeSpan.FromDays(1));
        }
    }
}
