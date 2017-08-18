using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Utils
{
    public class SyncLogger
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
            else if (LogLevel == SyncLogger_LogLevel.PRODUCTION)
            {
                FileStream fs = new FileStream(Sync.Theater.ConfigManager.Config.LogFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.Write("{1} [{0}] ", prefix, DateTime.Now.ToString());
                sw.WriteLine(message, args);
                sw.Flush();
                sw.Close();
            }
        }

        
    }

    public enum SyncLogger_LogLevel
    {
        DEBUG,
        PRODUCTION
    }
}
