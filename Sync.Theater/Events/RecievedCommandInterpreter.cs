using Newtonsoft.Json;
using Sync.Theater.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater.Events
{
    public class RecievedCommandInterpreter
    {
        static SyncLogger Logger;

        static RecievedCommandInterpreter()
        {
            Logger = new SyncLogger("CommandInterpreter", ConsoleColor.Green);
        }

        public static void InterpretCommand(dynamic message, SyncService s, SyncRoom room)
        {
            if (message.CommandType == CommandType.REGISTERUSER)
            {
                var success = false;

                if (UserAuth.RegisterUser((string)message.Username, (string)message.Email, (string)message.Password))
                {
                    Logger.Log("Client {0} successfully registered as {1}.", s.Nickname, message.Username);
                    s.Nickname = message.Username;

                    success = true;
                }
                else
                {
                    Logger.Log("Something went wrong with registration");
                     
                }

                var res = new
                {
                    CommandType = "RegisterUserSuccess",
                    Value = success
                };

                s.SendMessage(JsonConvert.SerializeObject(res));
            }
            else if(message.CommandType == CommandType.LOGINUSER)
            {
                throw new NotImplementedException();
            }
            else if(message.CommandType == CommandType.SYNCQUEUECHANGED)    // if the owner or trusted user modifies the queue we need to broadcast that change to all clients
            {
                if(s.Permissions == UserPermissionLevel.OWNER || s.Permissions == UserPermissionLevel.TRUSTED)
                {
                    room.CurrentQueue.Name = message.Queue.Name;
                    room.CurrentQueue.QueueIndex = message.Queue.QueueIndex;
                    room.CurrentQueue.URLs = message.Queue.URLs;

                    var res = new
                    {
                        CommandType = "QueueUpdate",
                        Queue = new
                        {
                            Name = room.CurrentQueue.Name,
                            QueueIndex = room.CurrentQueue.QueueIndex,
                            URLs = room.CurrentQueue.URLs
                        }
                    };

                    room.Broadcast(JsonConvert.SerializeObject(res));
                }
            }
        }
    }

    public enum CommandType
    {
        REGISTERUSER,
        LOGINUSER,
        SYNCQUEUECHANGED
    }


}
