using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Sync.Theater
{
    class SyncTheater
    {
        public static List<SyncRoom> rooms;
        private static HttpServer httpsv;

        public static void Start()
        {
            httpsv = new HttpServer(Config.LoadJson().Port);

            // Set the document root path.
            httpsv.RootPath = "../../Public";

            var timer = new System.Threading.Timer((e) =>
            {
                CleanSocketServices(httpsv);
            }, null, 0, (int)TimeSpan.FromMinutes(1).TotalMilliseconds);


            // Set the HTTP GET request event.
            httpsv.OnGet += (sender, e) =>
            {
                var req = e.Request;
                var res = e.Response;

                var path = req.RawUrl;

                if(path.Length==6 && !path.Contains("."))
                {
                    var room = GetRoomByCode(path.Remove(0, 1));
                    if ( room != null )
                    {
                        path = "/index.min.html";
                    }
                    else
                    {
                        var code = SyncRoom.RandomString(6);
                        rooms.Add(new SyncRoom(code));

                        httpsv.AddWebSocketService("/" + code, () => GetRoomByCode(code).Service = new SyncService()
                        {
                            IgnoreExtensions = true
                        });

                    }
                }

                if (path == "/")
                {
                    path = "/index.min.html";
                    var code = SyncRoom.RandomString(6);
                    rooms.Add(new SyncRoom(code));

                    httpsv.AddWebSocketService("/" + code, () => GetRoomByCode(code).Service = new SyncService()
                    {
                        IgnoreExtensions = true
                    });

                    res.Redirect(req.Url + code);
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
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", httpsv.Port);
            }
        }

        public static void Stop()
        {
            httpsv.Stop();
        }

        public static void CreateRoom(string code = "")
        {
            SyncRoom room;
            if (string.IsNullOrWhiteSpace(code)) { room = new SyncRoom(); }
            else { room = new SyncRoom(code); }

            rooms.Add(room);
            
        }

        public static SyncRoom GetRoomByCode(string code)
        {
            return rooms.Where(r => { return r.RoomCode == code; }).ToList()[0];
        }

        public static void DeleteRoom(string code)
        {
            rooms.RemoveAt(rooms.FindIndex(r => { return r.RoomCode == code; }));
        }


    }
}
