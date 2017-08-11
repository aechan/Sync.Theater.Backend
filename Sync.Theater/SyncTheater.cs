using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using Sync.Theater.Utils;

namespace Sync.Theater
{
    class SyncTheater
    {
        public static List<SyncRoom> rooms;
        private static HttpServer httpsv;
        private static SyncLogger Logger;

        public static void Start()
        {
            Logger = new SyncLogger("Server", ConsoleColor.Red);
            rooms = new List<SyncRoom>();

            httpsv = new HttpServer(ConfigManager.Config.Port);

            // Set the document root path.
            httpsv.RootPath = System.IO.Path.GetFullPath(ConfigManager.Config.HTTPRelativeBasePath);

            var timer = new System.Threading.Timer((e) =>
            {
                for (int i = 0; i < rooms.Count; ++i)
                {
                    if (rooms[i].ActiveUsers == 0)
                    {
                        Logger.Log("[Cleanup] Deleting inactive room {0}.", rooms[i].RoomCode);
                        DeleteRoom(rooms[i].RoomCode);
                    }
                }
            }, null, 0, (int)TimeSpan.FromMinutes(1).TotalMilliseconds);


            // Set the HTTP GET request event.
            httpsv.OnGet += (sender, e) =>
            {
                var req = e.Request;
                var res = e.Response;

                var path = req.RawUrl;

                // if we might have a match for a valid room code
                if(path.Length==7 && !path.Contains("."))
                {
                    // attempt to get the room by the code given
                    var room = GetRoomByCode(path.Remove(0, 1));

                    // set the path to the html file so we don't request something like "/ABC123"
                    path = "htmlm/index.min.html";
                    
                    if(room==null)
                    {
                        

                        var sr = CreateRoom();
                        
                        httpsv.AddWebSocketService("/" + sr.RoomCode, () => new SyncService(sr));

                        res.Redirect((req.Url.GetLeftPart(UriPartial.Authority) + "/" + sr.RoomCode));
                    }
                }
                else if (path == "/")
                {
                    
                    path = "htmlm/index.min.html";
                    
                    var room = CreateRoom();
                    
                    httpsv.AddWebSocketService("/" + room.RoomCode, () => new SyncService(room));

                    res.Redirect(req.Url + room.RoomCode);
                }
                
                

                var content = httpsv.GetFile(path);

                if (content == null)
                {
                    res.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                if (path.EndsWith(".html"))
                {
                    res.ContentType = "text/html";
                    res.ContentEncoding = Encoding.UTF8;
                }
                else if (path.EndsWith(".js"))
                {
                    res.ContentType = "application/javascript";
                    res.ContentEncoding = Encoding.UTF8;
                }
                else if (path.EndsWith(".css"))
                {
                    res.ContentType = "text/css";
                    res.ContentEncoding = Encoding.UTF8;
                }

                res.WriteContent(content);

            };

            httpsv.Start();

            if (httpsv.IsListening)
            {
                Logger.Log("Listening on port {0}, and providing WebSocket services", httpsv.Port);
            }

            Console.ReadLine();
        }

        public static void Stop()
        {
            httpsv.Stop();
        }

        public static SyncRoom CreateRoom( string code = "")
        {
            SyncRoom room;
            if (string.IsNullOrWhiteSpace(code)) { room = new SyncRoom(); }
            else { room = new SyncRoom(code); }

            Logger.Log("Room {0} created successfully.", room.RoomCode);
            rooms.Add(room);

            return room;
        }

        public static SyncRoom GetRoomByCode(string code)
        {
            var room = rooms.FirstOrDefault(x => x.RoomCode == code);
            if (room != null)
            {
                Logger.Log("Got room {0}", room.RoomCode);
            }
            else
            {
                Logger.Log("Could not find room {0}.", code);
            }
            return room;
        }

        public static void DeleteRoom(string code)
        {
            rooms.RemoveAt(rooms.FindIndex(r => { return r.RoomCode == code; }));
        }


    }
}
