using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace TCPListenerService
{
    public partial class TcpListenerService : ServiceBase
    {
        public const string sourceName = "TcpListener";
        public List<SimulatorTcpListener> hostListeners = new List<SimulatorTcpListener>();

        public TcpListenerService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            TraceMessage(string.Format("{0} Starting...", sourceName));
            //Start Listerns

        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Start();
            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("{0} Error occured during Startup:\r\n{1}", sourceName, ex.Message));

                foreach (SimulatorTcpListener simulator in this.hostListeners)
                {
                    try
                    {
                        simulator.ForceStop();
                    }
                    catch (Exception ex2)
                    {
                        ErrorMessage(string.Format("{0} After Startup Error occured, another error occured while trying to stop a Host:\r\n{1}", sourceName, ex2.ToString()));
                    } 
                }
            }
        }

        protected override void OnStop()
        {
            foreach (SimulatorTcpListener simulator in this.hostListeners)
            {
                simulator.ForceStop();
            }
        }

        private void TraceMessage(string traceMessage)
        {
            EventLog.WriteEntry(sourceName, traceMessage, EventLogEntryType.Information);
        }

        private void ErrorMessage(string errorMessage)
        {
            EventLog.WriteEntry(sourceName, errorMessage, EventLogEntryType.Error);
        }
    }
}
