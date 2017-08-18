using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Theater.Utils;

namespace Sync.Theater
{
    public static class ConfigManager
    {
        private static Config _config;
        private static SyncLogger Logger;

        public static Config Config
        {
            get
            {
                if (Logger == null)
                {
                    Logger = new SyncLogger("ConfigManager", ConsoleColor.Yellow);
                }
                if (_config == null)
                {
                    Logger.Log("Config not yet loaded, reading config.json..");
                    using (StreamReader r = new StreamReader("../../config.json"))
                    {
                        string json = r.ReadToEnd();
                        _config = JsonConvert.DeserializeObject<Config>(json);
                        Log(_config);
                    }
                }
                return _config;
            }
            private set
            {
                _config = value;
            }
        }

        private static void Log(object @object)
        {
            foreach (var property in @object.GetType().GetProperties())
            {
                if (property.Name != "SQLConnectionString")
                {
                    Logger.Log(property.Name + ": " + property.GetValue(@object, null).ToString());
                }
                else
                {
                    Logger.Log("SQLConnectionString ommitted..");
                }
            }
        }

    }

    public class Config
    {
        public string JWTSecret { get; set; }
        public int Port { get; set; }
        public string DBUsername { get; set; }
        public string DBPassword { get; set; }
        public string SQLConnectionString { get; set; }
        public string LogFilePath { get; set; }
        public string HTTPRelativeBasePath { get; set; }
    }


}
