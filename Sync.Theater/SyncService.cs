using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Sync.Theater
{

    class SyncService : WebSocketBehavior
    {

        public delegate void UserCountChangedHandler(int count);
        public event UserCountChangedHandler UserCountChanged = delegate { };

        public delegate void ServerMessageRecievedHandler(dynamic message);
        public event ServerMessageRecievedHandler ServerMessageRecieved = delegate { };

        public delegate void Client2ClientMessageRecievedHandler(dynamic message);
        public event Client2ClientMessageRecievedHandler Client2ClientMessageRecieved = delegate { };

        public delegate void BroadcastMessageRecievedHandler(dynamic message);
        public event BroadcastMessageRecievedHandler BroadcastMessageRecieved = delegate { };

        protected override void OnMessage(MessageEventArgs e)
        {
            dynamic message = JsonConvert.DeserializeObject<dynamic>(e.Data);

            switch (message.Recipient)
            {
                case MessageRecipientType.SERVER:
                    ServerMessageRecieved(message);
                    break;
                case MessageRecipientType.CLIENT2CLIENT:
                    Client2ClientMessageRecieved(message);
                    break;
                case MessageRecipientType.BROADCAST:
                    BroadcastMessageRecieved(message);
                    break;
            }
            

        }

        protected override void OnOpen()
        {
            // notify subscribers that user count has changed
            UserCountChanged(Sessions.Count);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            // notify subscribers that user count has changed
            UserCountChanged(Sessions.Count);
        }

        protected void ReassessOwner()
        {

        }

    }

    public enum MessageRecipientType
    {
        SERVER,
        CLIENT2CLIENT,
        BROADCAST
    }
}
