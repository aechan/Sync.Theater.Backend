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

        static Dictionary<string, Action<dynamic, SyncService, SyncRoom>> RegisteredInterpreters;

        static RecievedCommandInterpreter()
        {
            Logger = new SyncLogger("CommandInterpreter", ConsoleColor.Green);
            RegisteredInterpreters = new Dictionary<string, Action<dynamic, SyncService, SyncRoom>>();

            // register interpreter modules
            RegisterCommandInterpreter(CommandType.ADDLIKE.Value, OnAddLike);
            RegisterCommandInterpreter(CommandType.REGISTERUSER.Value, OnRegisterUser);
            RegisterCommandInterpreter(CommandType.LOGINUSER.Value, OnLoginUser);
            RegisterCommandInterpreter(CommandType.MODIFYQUEUE.Value, OnModifyQueue);
            RegisterCommandInterpreter(CommandType.UPGRADEUSERPERMISSIONS.Value, OnUpgradeUserPermissions);
            RegisterCommandInterpreter(CommandType.SYNCSTATE.Value, OnSyncState);
            RegisterCommandInterpreter(CommandType.CHAT.Value, OnChat);
            RegisterCommandInterpreter(CommandType.KICKUSER.Value, OnKickUser);
            RegisterCommandInterpreter(CommandType.SENDUSERLIST.Value, OnUserListRequested);
            RegisterCommandInterpreter(CommandType.UPDATESTATUS.Value, OnStatusUpdate);
            RegisterCommandInterpreter(CommandType.UPDATEUSERNAME.Value, OnUsernameUpdate);
        }


        public static void UnregisterCommandInterpreter(CommandType command)
        {
            RegisteredInterpreters.Remove(command.Value);
        }

        public static void InterpretCommand(dynamic message, SyncService s, SyncRoom room)
        {
            if (RegisteredInterpreters[(string)message.CommandType] != null)
            {
                Action<dynamic, SyncService, SyncRoom> act = RegisteredInterpreters[(string)message.CommandType];

                act.Invoke(message, s, room);
            }
            
        }

        public static string PermissionsChangedNotification(UserPermissionLevel level)
        {
            var res = new
            {
                CommandType = CommandType.PERMISSIONSCHANGEDNOTIFICATION.Value,
                PermissionLevel = level
            };

            return JsonConvert.SerializeObject(res);
        }


        public static void RegisterCommandInterpreter(string command, Action<dynamic, SyncService, SyncRoom> callback)
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
                CommandType = CommandType.UPDATELIKES.Value,
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
                CommandType = CommandType.REGISTERUSER.Value,
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
                    CommandType = CommandType.QUEUEUPDATE.Value,
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
                SyncService target = room.GetServiceByNickname((string)message.TargetNickname);

                if ((bool)message.Upgrade)
                {
                    target.Permissions = UserPermissionLevel.TRUSTED;
                }
                else
                {
                    target.Permissions = UserPermissionLevel.VIEWER;
                }
            }
        }

        public static void OnKickUser(dynamic message, SyncService s, SyncRoom room)
        {
            if(s.Permissions == UserPermissionLevel.OWNER)
            {
                SyncService target = room.GetServiceByNickname((string)message.TargetNickname);

                var res = new
                {
                    CommandType = CommandType.KICKUSER.Value
                };

                target.SendMessage(JsonConvert.SerializeObject(res));

                target.Disconnect();


            }
        }

        public static void OnSyncState(dynamic message, SyncService s, SyncRoom room)
        {
            if (s.Permissions == UserPermissionLevel.OWNER)
            {
                var res = new
                {
                    CommandType = CommandType.SYNCSTATE.Value,
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

        public static void OnUserListRequested(dynamic message, SyncService s, SyncRoom room)
        {
            room.SendUserList();
        }

        public static void OnStatusUpdate(dynamic message, SyncService s, SyncRoom room)
        {
            s.status = (UserStatus)message.Status;
            room.SendUserList();
        }

        public static void OnUsernameUpdate(dynamic message, SyncService s, SyncRoom room) 
        {
            s.Nickname = (string)message.Nickname;

            //notify of nickname change
            var res1 = new
                {
                    CommandType = CommandType.SETUSERNICKNAME.Value,
                    Nickname = s.Nickname
                };

            s.SendMessage(JsonConvert.SerializeObject(res1));

            //notify rest of room of nickname change
            room.SendUserList();
        }


    }

    public class CommandType
    {
        private CommandType(string value) { Value = value; }

        public string Value { get; set; }

        public static CommandType REGISTERUSER { get { return new CommandType("REGISTERUSER"); } }
        public static CommandType LOGINUSER { get { return new CommandType("LOGINUSER"); } }
        public static CommandType UPGRADEUSERPERMISSIONS { get { return new CommandType("UPGRADEUSERPERMISSIONS"); } }
        public static CommandType PERMISSIONSCHANGEDNOTIFICATION { get { return new CommandType("PERMISSIONSCHANGEDNOTIFICATION"); } }
        public static CommandType MODIFYQUEUE { get { return new CommandType("MODIFYQUEUE"); } }
        public static CommandType ADDQUEUE { get { return new CommandType("ADDQUEUE"); } }
        public static CommandType DELETEQUEUE { get { return new CommandType("DELETEQUEUE"); } }
        public static CommandType GETALL { get { return new CommandType("GETALL"); } }
        public static CommandType GETONE { get { return new CommandType("GETONE"); } }
        public static CommandType SYNCSTATE { get { return new CommandType("SYNCSTATE"); } }
        public static CommandType QUEUEUPDATE { get { return new CommandType("QUEUEUPDATE"); } }
        public static CommandType SETUSERNICKNAME { get { return new CommandType("SETUSERNICKNAME"); } }
        public static CommandType SENDUSERLIST { get { return new CommandType("SENDUSERLIST"); } }

        public static CommandType ADDLIKE { get { return new CommandType("ADDLIKE"); } }
        public static CommandType UPDATELIKES { get { return new CommandType("UPDATELIKES"); } }
        public static CommandType CHAT { get { return new CommandType("CHAT"); } }
        public static CommandType KICKUSER { get { return new CommandType("KICKUSER"); } }
        public static CommandType UPDATESTATUS { get { return new CommandType("UPDATESTATUS"); } }
        public static CommandType UPDATEUSERNAME { get { return new CommandType("UPDATEUSERNAME"); } }

    }


}
