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
        public delegate void ConnectionOpenOrCloseHandler(ConnectionAction action, IWebSocketSession s );
        public event ConnectionOpenOrCloseHandler ConnectionOpenedOrClosed = delegate { };

        public delegate void ServerMessageRecievedHandler(dynamic message, IWebSocketSession s);
        public event ServerMessageRecievedHandler ServerMessageRecieved = delegate { };

        public delegate void Client2ClientMessageRecievedHandler(dynamic message, IWebSocketSession s);
        public event Client2ClientMessageRecievedHandler Client2ClientMessageRecieved = delegate { };

        public delegate void BroadcastMessageRecievedHandler(dynamic message, IWebSocketSession s);
        public event BroadcastMessageRecievedHandler BroadcastMessageRecieved = delegate { };

        protected override void OnMessage(MessageEventArgs e)
        {
            dynamic message = JsonConvert.DeserializeObject<dynamic>(e.Data);

            Console.WriteLine("Message recipient type: {0}", message.Recipient);

            switch (message.Recipient)
            {
                case 0:
                    Console.WriteLine("R");
                    ServerMessageRecieved(message, this);
                    break;
                case (int)MessageRecipientType.CLIENT2CLIENT:
                    Client2ClientMessageRecieved(message, this);
                    break;
                case (int)MessageRecipientType.BROADCAST:
                    BroadcastMessageRecieved(message, this);
                    break;
            }
        }

        protected override void OnOpen()
        {
            ConnectionOpenedOrClosed(ConnectionAction.OPENED, this);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            
            ConnectionOpenedOrClosed(ConnectionAction.CLOSED, this);
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

    public enum ConnectionAction
    {
        OPENED,
        CLOSED
    }
}
