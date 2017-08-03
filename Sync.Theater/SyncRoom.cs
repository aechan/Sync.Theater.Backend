using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater
{
    class SyncRoom
    {
        public string RoomCode { get; private set; }
        public SyncService Service { get; set; }
        public int ActiveUsers { get; set; }

        public SyncRoom()
        {
            this.RoomCode = RandomString(6);
        }

        public SyncRoom(string code)
        {
            this.RoomCode = code;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
