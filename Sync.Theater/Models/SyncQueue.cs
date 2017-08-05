using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Models
{
    public class SyncQueue
    {
        public int CurrentIndex { get; set; }
        public string[] URLs { get; set; }
    }
}
