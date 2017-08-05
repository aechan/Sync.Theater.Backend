using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sync.Theater.Models;
using Newtonsoft.Json;

namespace Sync.Theater.Events
{
    public class SyncQueueChanged
    {
        public static string Notify(SyncQueue Queue)
        {
            var queue = new SyncQueueChangedNotification();
            queue.Queue = Queue;
            return JsonConvert.SerializeObject(queue);
        }
    }

    public class SyncQueueChangedNotification
    {
        public string CommandType = nameof(SyncQueueChangedNotification);
        public SyncQueue Queue { get; set; }
    }

    public class SyncQueueCommandHandler
    {

    }


}
