using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerRunnerProxy.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
                { 
                    new FiddlerRunnerProxyWindowsService() 
                };

            ServiceBase.Run(servicesToRun);
        }
    }
}
