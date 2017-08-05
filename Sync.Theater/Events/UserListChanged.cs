using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Events
{
    public class UserListChanged
    {
        public static string Notify(string[] userList)
        {
            var list = new UserListChangedNotification();
            list.UserList = userList;
            return JsonConvert.SerializeObject(userList);
        }
    }

    public class UserListChangedNotification
    {
        public string CommandType = nameof(UserListChangedNotification);
        public string[] UserList { get; set; }
    }
}
