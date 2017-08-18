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

        static Dictionary<CommandType, Action<dynamic, SyncService, SyncRoom>> RegisteredInterpreters;

        static RecievedCommandInterpreter()
        {
            Logger = new SyncLogger("CommandInterpreter", ConsoleColor.Green);
            RegisteredInterpreters = new Dictionary<CommandType, Action<dynamic, SyncService, SyncRoom>>();

            // register interpreter modules
            RegisterCommandInterpreter(CommandType.ADDLIKE, OnAddLike);
            RegisterCommandInterpreter(CommandType.REGISTERUSER, OnRegisterUser);
            RegisterCommandInterpreter(CommandType.LOGINUSER, OnLoginUser);
            RegisterCommandInterpreter(CommandType.MODIFYQUEUE, OnModifyQueue);
            RegisterCommandInterpreter(CommandType.UPGRADEUSERPERMISSIONS, OnUpgradeUserPermissions);
            RegisterCommandInterpreter(CommandType.SYNCSTATE, OnSyncState);
            RegisterCommandInterpreter(CommandType.CHAT, OnChat);

        }


        public static void UnregisterCommandInterpreter(CommandType command)
        {
            RegisteredInterpreters.Remove(command);
        }

        public static void InterpretCommand(dynamic message, SyncService s, SyncRoom room)
        {
            Action<dynamic, SyncService, SyncRoom> act = RegisteredInterpreters[(CommandType)message.CommandType];

            act.Invoke(message, s, room);
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


        public static void RegisterCommandInterpreter(CommandType command, Action<dynamic, SyncService, SyncRoom> callback)
        {
            if (!RegisteredInterpreters.ContainsKey(command))
            {
                RegisteredInterpreters.Add(command, callback);
            }
            else
            {
                Logger.Log("Cannot bind multiple callbacks to one command type...");
            }
        }

        public static void OnAddLike(dynamic message, SyncService s, SyncRoom room)
        {
            var res = new
            {
                CommandType = CommandType.UPDATELIKES,
                Likes = room.AddLike()
            };

            room.Broadcast(JsonConvert.SerializeObject(res));
        }

        public static void OnRegisterUser(dynamic message, SyncService s, SyncRoom room)
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
                CommandType = CommandType.REGISTERUSER,
                Value = success
            };

            s.SendMessage(JsonConvert.SerializeObject(res));
        }

        public static void OnLoginUser(dynamic message, SyncService s, SyncRoom room)
        {
            throw new NotImplementedException();
        }

        public static void OnModifyQueue(dynamic message, SyncService s, SyncRoom room)
        {
            if (s.Permissions == UserPermissionLevel.OWNER || s.Permissions == UserPermissionLevel.TRUSTED)
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

        public static void OnUpgradeUserPermissions(dynamic message, SyncService s, SyncRoom room)
        {
            if (s.Permissions == UserPermissionLevel.OWNER)
            {
                SyncService target = room.GetServiceByNickname(message.TargetNickname);

                if (message.Upgrade)
                {
                    target.Permissions++;
                }
                else
                {
                    target.Permissions--;
                }
            }
        }

        public static void OnSyncState(dynamic message, SyncService s, SyncRoom room)
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

        public static void OnChat(dynamic message, SyncService s, SyncRoom room)
        {
            room.Broadcast(JsonConvert.SerializeObject(message));
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
        UPDATELIKES,
        CHAT


    }


}
