using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Events
{
    public class PermissionsChanged
    {
        public static string Notify(UserPermissionLevel level, SyncService ser)
        {
            var perm = new PermissionsChangedNotification();
            perm.Level = level;
            return JsonConvert.SerializeObject(perm);
        }
    }

    public class PermissionsChangedNotification
    {
        public string CommandType = nameof(PermissionsChangedNotification);
        public UserPermissionLevel Level { get; set; }
    }
}
