using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Theater.Utils;

namespace Sync.Theater
{
    class TestProgram
    {
        public static void Main(string[] args)
        {
            SyncLogger.LogLevel = SyncLogger_LogLevel.DEBUG;

            SyncTheater.Start();

        }
    }
}
