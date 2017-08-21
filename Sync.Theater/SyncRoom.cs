using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using Sync.Theater.Utils;
using Sync.Theater.Models;
using Sync.Theater.Events;
using Newtonsoft.Json;

namespace Sync.Theater
{
    public class SyncRoom
    {
        public string RoomCode { get; private set; }
        public List<SyncService> Services { get; set; }
        public int ActiveUsers { get; set; }

        private SyncLogger Logger;

        public SyncService Owner;

        public PartialQueue CurrentQueue;

        private int likes;

        public SyncRoom() : this(RandomString(6)) { }

        public SyncRoom(string code)
        {
            this.Logger = new SyncLogger("Room " + code, ConsoleColor.Cyan);
            this.Services = new List<SyncService>();
            this.RoomCode = code;
            this.CurrentQueue = new PartialQueue();
            this.likes = 0;
            Logger.Log("initialized.");
        }

        public string AddService(SyncService Service)
        {
            Service.MessageRecieved += Service_MessageRecieved;
            Service.ConnectionOpenedOrClosed += Service_ConnectionOpenedOrClosed;
            Services.Add(Service);

            return Service.ID;
        }

        public int AddLike()
        {
            likes += 1;
            return likes;
        }

        private void Service_ConnectionOpenedOrClosed(ConnectionAction action, SyncService s)
        {
            if(action == ConnectionAction.OPENED)
            {
                Logger.Log("Client [{0}] connected. {1} clients online in room {2}.", s.Nickname, Services.Count, RoomCode);

                if (CurrentQueue.URLs != null)
                {
                    // update new user with queue
                    var res = new
                    {
                        CommandType = CommandType.QUEUEUPDATE.Value,
                        Queue = new
                        {
                            Name = CurrentQueue.Name,
                            QueueIndex = CurrentQueue.QueueIndex,
                            URLs = CurrentQueue.URLs
                        }
                    };

                    s.SendMessage(JsonConvert.SerializeObject(res));
                }


                // notify service of their nickname
                var res1 = new
                {
                    CommandType = CommandType.SETUSERNICKNAME.Value,
                    Nickname = s.Nickname
                };

                s.SendMessage(JsonConvert.SerializeObject(res1));


                // notify service of like count
                var res2 = new
                {
                    CommandType = CommandType.UPDATELIKES.Value,
                    Likes = likes
                };

                s.SendMessage(JsonConvert.SerializeObject(res2));
            }
            else
            {
                int index = Services.FindIndex(x => x.ID == s.ID);
                Services.RemoveAt(index);
                Logger.Log("Client [{0}] disconnected. {1} clients online in room {2}.", s.Nickname, Services.Count, RoomCode);
            }


            ActiveUsers = Services.Count;
            ReassessOwnership(s, action);

            SendUserList();

        }

        private void Service_MessageRecieved(dynamic message, SyncService s)
        {
            RecievedCommandInterpreter.InterpretCommand(message, s, this);
        }

        public void SendUserList()
        {

            List<dynamic> userlist = new List<dynamic>(Services.Count);

            foreach(var sr in Services)
            {

                userlist.Add(new {
                    Nickname = sr.Nickname,
                    PermissionLevel = sr.Permissions
                });
            }

            var res = new
            {
                CommandType = CommandType.SENDUSERLIST.Value,
                Userlist = userlist
            };
            
            Broadcast(JsonConvert.SerializeObject(res));
            
        }

        public void Broadcast(string message)
        {
            for(int i = 0; i<Services.Count; i++)
            {
                Services[i].SendMessage(message);
            }
        }

        public void BroadcastExcept(string message, SyncService exception)
        {
            for (int i = 0; i < Services.Count; i++)
            {
                if (Services[i].ID != exception.ID)
                {
                    Services[i].SendMessage(message);
                }
            }
        }

        public SyncService GetServiceByNickname(string nick)
        {
            return Services.First(sr => sr.Nickname == nick);
        }

        private void ReassessOwnership(SyncService deltaUser, ConnectionAction action)
        {
            var oldOwner = Owner;
            // base case if only one user remains online
            if (ActiveUsers == 1)
            {
                Owner = Services.First();
            }
            else if( action == ConnectionAction.CLOSED && deltaUser.ID == Owner.ID && ActiveUsers > 0)
            {
                // set the owner to the last joined user if current owner left
                Owner = Services.First();
                
            }
            else if (ActiveUsers == 0)
            {
                Logger.Log("Room {0} has no owner, will be destroyed in 1 minute.", RoomCode);
                Owner = null;
            }

            if(Owner!=null && Owner.Permissions != UserPermissionLevel.OWNER)
            {
                Owner.Permissions = UserPermissionLevel.OWNER;
            }
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
