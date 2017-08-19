using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Theater.Utils;
using System.Reflection;

namespace Sync.Theater
{
    public static class ConfigManager
    {
        private static Config _config;
        private static SyncLogger Logger;
        public static SyncTheater_ConfigLoadLevel loadLevel;
        private static string path;

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
                    if(loadLevel == SyncTheater_ConfigLoadLevel.CONSOLE) { path = "../../config.json"; }
                    if(loadLevel == SyncTheater_ConfigLoadLevel.SERVICE)
                    {
                        path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"config.json");
                    }
                    Logger.Log("Config not yet loaded, reading config.json..");
                    if (File.Exists(path))
                    {
                        using (StreamReader r = new StreamReader(path))
                        {
                            string json = r.ReadToEnd();
                            _config = JsonConvert.DeserializeObject<Config>(json);
                            Log(_config);
                        }
                    }
                    else
                    {
                        Logger.Log("Could not load config from file: {0}, aborting...", Path.GetFullPath(path));
                        Sync.Theater.SyncTheater.Stop();
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

    public enum SyncTheater_ConfigLoadLevel
    {
        CONSOLE,
        SERVICE
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
        public string AnimalNamesFileRelativePath { get; set; }
        public string AdjectivesFileRelativePath { get; set; }
    }


}
