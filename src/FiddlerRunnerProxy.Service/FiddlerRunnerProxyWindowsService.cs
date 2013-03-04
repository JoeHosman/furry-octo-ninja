using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FiddlerRunnerProxy.Service
{
    public partial class FiddlerRunnerProxyWindowsService : ServiceBase
    {
        public FiddlerRunnerProxyWindowsService()
        {
            InitializeComponent();

            SetupWindowsServiceLog();
        }

        private void SetupWindowsServiceLog()
        {
            string sourceName = "MySource";
            string logName = "MyNewLog";
            if (!EventLog.SourceExists(sourceName))
            {

                EventLog.CreateEventSource(sourceName, logName);
            }

            eventLog1.Source = sourceName;
            eventLog1.Log = logName;
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart");
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop");
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }  
    }
}
