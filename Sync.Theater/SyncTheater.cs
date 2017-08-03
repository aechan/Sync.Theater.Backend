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

            rooms = new List<SyncRoom>();

            httpsv = new HttpServer(8080);

            // Set the document root path.
            httpsv.RootPath = "../../Public";

            var timer = new System.Threading.Timer((e) =>
            {
                
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
                        path = "/index.min.html";
                        var sr = CreateRoom();


                        httpsv.AddWebSocketService("/" + sr.RoomCode, () => GetRoomByCode(sr.RoomCode).Service);
                        res.Redirect((req.Url.GetLeftPart(UriPartial.Authority) + "/" + sr.RoomCode));
                    }
                }

                if (path == "/")
                {
                    path = "/index.min.html";
                    var room = CreateRoom();

                    httpsv.AddWebSocketService("/" + room.RoomCode, () => room.Service);

                    res.Redirect(req.Url + room.RoomCode);
                }

                var content = httpsv.GetFile(path);

                if (content == null)
                {
                    Console.WriteLine("Couldn't find file {0}.", path);
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

            Console.ReadLine();
        }

        public static void Stop()
        {
            httpsv.Stop();
        }

        public static SyncRoom CreateRoom(string code = "")
        {
            SyncRoom room;
            if (string.IsNullOrWhiteSpace(code)) { room = new SyncRoom(); }
            else { room = new SyncRoom(code); }

            rooms.Add(room);

            return room;
        }

        public static SyncRoom GetRoomByCode(string code)
        {
            var room = rooms.FirstOrDefault(x => x.RoomCode == code);
            Console.WriteLine("Got room {0}", room.RoomCode);
            return room;
        }

        public static void DeleteRoom(string code)
        {
            rooms.RemoveAt(rooms.FindIndex(r => { return r.RoomCode == code; }));
        }


    }
}
