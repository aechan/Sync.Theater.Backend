using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Utils
{
    class SyncLogger
    {
        
        private string prefix;
        private ConsoleColor prefixColor;
        public static SyncLogger_LogLevel LogLevel;

        public SyncLogger(string prefix, ConsoleColor prefixColor = ConsoleColor.White)
        {
            this.prefix = prefix;
            this.prefixColor = prefixColor;
        }

        public void Log(string message, params object[] args)
        {
            if (LogLevel == SyncLogger_LogLevel.DEBUG)
            {
                // write an optionally colored prefix
                Console.ForegroundColor = prefixColor;
                Console.Write("{1} [{0}] ", prefix, DateTime.Now.ToString());
                Console.ResetColor();

                // write the rest of the message and format
                Console.WriteLine(message, args);
            }
        }
    }

    public enum SyncLogger_LogLevel
    {
        DEBUG,
        PRODUCTION
    }
}
