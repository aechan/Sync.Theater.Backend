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
            if (message.CommandType == CommandType.REGISTERUSER) // User wants to register
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
            else if(message.CommandType == CommandType.LOGINUSER) // User wants to login
            {
                throw new NotImplementedException();
            }
            else if(message.CommandType == CommandType.MODIFYQUEUE)    // if the owner or trusted user modifies the queue we need to broadcast that change to all clients
            {
                if(s.Permissions == UserPermissionLevel.OWNER || s.Permissions == UserPermissionLevel.TRUSTED)
                {
                    room.CurrentQueue.Name = message.Queue.Name;
                    room.CurrentQueue.QueueIndex = message.Queue.QueueIndex;
                    room.CurrentQueue.URLs = message.Queue.URLs.ToObject<string[]>();

                    var res = new
                    {
                        CommandType = CommandType.QUEUEUPDATE,
                        Queue = new
                        {
                            Name = room.CurrentQueue.Name,
                            QueueIndex = room.CurrentQueue.QueueIndex,
                            URLs = room.CurrentQueue.URLs
                        }
                    };

                    room.BroadcastExcept(JsonConvert.SerializeObject(res), s);
                }
            }
            else if (message.CommandType == CommandType.UPGRADEUSERPERMISSIONS) // owner wants to upgrade someone to TRUSTED permissions
            {
                if(s.Permissions == UserPermissionLevel.OWNER)
                {
                    SyncService target = room.GetServiceByNickname(message.TargetNickname);

                    target.Permissions = message.PermissionsLevel;

                }
            }
            else if (message.CommandType == CommandType.SYNCSTATE) // Owner wants other to sync to their video state (automatic)
            {
                if (s.Permissions == UserPermissionLevel.OWNER)
                {
                    var res = new
                    {
                        CommandType = CommandType.SYNCSTATE,
                        State = new
                        {
                            Paused = message.Paused,
                            Time = message.Time
                        }
                    };

                    room.Broadcast(JsonConvert.SerializeObject(res));
                }
            }
            else if(message.CommandType == CommandType.ADDLIKE)
            {
                var res = new
                {
                    CommandType = CommandType.UPDATELIKES,
                    Likes = room.AddLike()
                };

                room.Broadcast(JsonConvert.SerializeObject(res));
            }
        }

        public static string PermissionsChangedNotification(UserPermissionLevel level)
        {
            var res = new
            {
                CommandType = CommandType.PERMISSIONSCHANGEDNOTIFICATION,
                PermissionLevel = level
            };

            return JsonConvert.SerializeObject(res);
        }
    }

    public enum CommandType
    {
        // User management commands
        REGISTERUSER,
        LOGINUSER,
        UPGRADEUSERPERMISSIONS,
        PERMISSIONSCHANGEDNOTIFICATION,

        // Queue commands
        MODIFYQUEUE,
        ADDQUEUE,
        DELETEQUEUE,
        GETALL,
        GETONE,

        // Video controls
        SYNCSTATE,

        // server response commands
        QUEUEUPDATE,
        SETUSERNICKNAME,
        SENDUSERLIST,

        ADDLIKE,
        UPDATELIKES


    }


}
