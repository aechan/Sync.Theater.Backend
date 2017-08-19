using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Theater;
using Sync.Theater.Utils;

namespace Sync.Theater.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigManager.loadLevel = SyncTheater_ConfigLoadLevel.CONSOLE;
            SyncLogger.LogLevel = SyncLogger_LogLevel.DEBUG;

            SyncTheater.Start();

            System.Console.ReadLine();

            SyncTheater.Stop();
        }
    }
}
