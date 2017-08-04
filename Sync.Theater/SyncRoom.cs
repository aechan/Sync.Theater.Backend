﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using Sync.Theater.Utils;

namespace Sync.Theater
{
    public class SyncRoom
    {
        public string RoomCode { get; private set; }
        public List<SyncService> Services { get; set; }
        public int ActiveUsers { get; set; }

        private SyncLogger Logger;

        private SyncService Owner;

        public SyncRoom() : this(RandomString(6)) { }

        public SyncRoom(string code)
        {
            this.Logger = new SyncLogger("Room " + code, ConsoleColor.Cyan);
            this.Services = new List<SyncService>();
            this.RoomCode = code;
        }

        public void AddService(SyncService Service)
        {
            Service.ServerMessageRecieved += Service_ServerMessageRecieved;
            Service.BroadcastMessageRecieved += Service_BroadcastMessageRecieved;
            Service.Client2ClientMessageRecieved += Service_Client2ClientMessageRecieved;
            Service.ConnectionOpenedOrClosed += Service_ConnectionOpenedOrClosed;
            Services.Add(Service);
        }

        private void Service_ConnectionOpenedOrClosed(ConnectionAction action, SyncService s)
        {
            if(action == ConnectionAction.OPENED)
            {
                Logger.Log("Client [{0}] connected. {1} clients online in room {2}.", s.ID, Services.Count, RoomCode);
            }
            else
            {
                int index = Services.FindIndex(x => x.ID == s.ID);
                Services.RemoveAt(index);
                Logger.Log("Client [{0}] disconnected. {1} clients online in room {2}.", s.ID, Services.Count, RoomCode);
            }


            ActiveUsers = Services.Count;
            ReassessOwnership(s, action);

        }

        private void Service_Client2ClientMessageRecieved(dynamic message, SyncService s)
        {
            throw new NotImplementedException();
        }

        private void Service_BroadcastMessageRecieved(dynamic message, SyncService s)
        {
            throw new NotImplementedException();
        }

        private void Service_ServerMessageRecieved(dynamic message, SyncService s)
        {
            
            if (message.Command == "REGISTER")
            {
                if(UserAuth.RegisterUser(message.Username, message.Email, message.Password))
                {
                    Logger.Log("Client {0} successfully registered as {1}.", s.ID, message.Username);
                }
                else
                {
                    Logger.Log ("Something went wrong with registration");
                }
            }
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
