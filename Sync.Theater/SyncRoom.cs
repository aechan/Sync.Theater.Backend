using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using Sync.Theater.Utils;
using Sync.Theater.Models;
using Sync.Theater.Events;

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

        public SyncRoom() : this(RandomString(6)) { }

        public SyncRoom(string code)
        {
            this.Logger = new SyncLogger("Room " + code, ConsoleColor.Cyan);
            this.Services = new List<SyncService>();
            this.RoomCode = code;
        }

        public void AddService(SyncService Service)
        {
            Service.MessageRecieved += Service_MessageRecieved;
            Service.ConnectionOpenedOrClosed += Service_ConnectionOpenedOrClosed;
            Services.Add(Service);
        }

        private void Service_ConnectionOpenedOrClosed(ConnectionAction action, SyncService s)
        {
            if(action == ConnectionAction.OPENED)
            {
                Logger.Log("Client [{0}] connected. {1} clients online in room {2}.", s.Nickname, Services.Count, RoomCode);
            }
            else
            {
                int index = Services.FindIndex(x => x.ID == s.ID);
                Services.RemoveAt(index);
                Logger.Log("Client [{0}] disconnected. {1} clients online in room {2}.", s.Nickname, Services.Count, RoomCode);
            }


            ActiveUsers = Services.Count;
            ReassessOwnership(s, action);

        }

        private void Service_MessageRecieved(dynamic message, SyncService s)
        {
            RecievedCommandInterpreter.InterpretCommand(message, s, this);
        }

        private void SendUserList(SyncService user)
        {
            List<string> userlist = new List<string>(Services.Count);

            foreach(var sr in Services)
            {
                userlist.Add(sr.Nickname);
            }

            //user.SendMessage(UserListChanged.Notify(userlist.ToArray()));
        }

        public void Broadcast(string message)
        {
            foreach(var sr in Services)
            {
                sr.SendMessage(message);
            }
        }

        private SyncService GetServiceByNickname(string nick)
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
