using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace Sync.Theater
{
    class SyncRoom
    {
        public string RoomCode { get; private set; }
        public SyncService Service { get; set; }
        public int ActiveUsers { get; set; }

        private List<IWebSocketSession> Connections;

        public SyncRoom() : this(RandomString(6)) { }

        public SyncRoom(string code)
        {
            Console.WriteLine("New SyncRoom {0} initialized.", code);
            this.Service = new SyncService();
            this.Connections = new List<IWebSocketSession>();
            this.RoomCode = code;
            this.Service.ServerMessageRecieved += Service_ServerMessageRecieved;
            this.Service.BroadcastMessageRecieved += Service_BroadcastMessageRecieved;
            this.Service.Client2ClientMessageRecieved += Service_Client2ClientMessageRecieved;
            this.Service.ConnectionOpenedOrClosed += Service_ConnectionOpenedOrClosed;
        }

        private void Service_ConnectionOpenedOrClosed(ConnectionAction action, IWebSocketSession s)
        {
            if(action == ConnectionAction.OPENED)
            {
                Connections.Add(s);
            }
            else
            {
                int index = Connections.FindIndex(x => x.ID == s.ID);
                Connections.RemoveAt(index);
            }

            Console.WriteLine("Client [{0}] connected. {1} clients online in room {2}.", s.ID, Connections.Count, RoomCode);
        }

        private void Service_Client2ClientMessageRecieved(dynamic message)
        {
            throw new NotImplementedException();
        }

        private void Service_BroadcastMessageRecieved(dynamic message)
        {
            throw new NotImplementedException();
        }

        private void Service_ServerMessageRecieved(dynamic message)
        {
            if (message.Command == "REGISTER")
            {
                if(UserAuth.RegisterUser(message.Username, message.Email, message.Password))
                {

                }
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
