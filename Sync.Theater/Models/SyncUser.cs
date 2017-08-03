using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Models
{
    class SyncUser
    {
        public string Username { get; private set; }
        public int Id { get; private set; }
        public string Email { get; private set; }

        public SyncUser(int Id, string Email, string Username)
        {
            this.Id = Id;
            this.Email = Email;
            this.Username = Username;
        }
        
    }
}
