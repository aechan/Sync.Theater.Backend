using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Service
{
    public partial class STService : ServiceBase
    {
        public STService()
        {
            InitializeComponent();
            Sync.Theater.Utils.SyncLogger.LogLevel = Utils.SyncLogger_LogLevel.PRODUCTION;
            ConfigManager.loadLevel = SyncTheater_ConfigLoadLevel.SERVICE;
        }

        protected override void OnStart(string[] args)
        {
            SyncTheater.Start();


        }

        protected override void OnStop()
        {
            SyncTheater.Stop();
        }
    }
}
