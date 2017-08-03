using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater
{
    public class Config
    {
        public string JWTSecret { get; private set; }
        public int Port { get; private set; }
        public string DBUsername { get; private set; }
        public string DBPassword { get; private set; }
        public string SQLConnectionString { get; private set; }


        public static Config LoadJson()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }
}
