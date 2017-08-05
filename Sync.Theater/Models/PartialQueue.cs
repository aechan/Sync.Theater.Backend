using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Models
{
    public class PartialQueue
    {
        public string Name { get; set; }
        public int QueueIndex { get; set; }
        public string[] URLs { get; set; }
    }
}
