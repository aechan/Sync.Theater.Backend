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

                Console.WriteLine("Path was {0}", path);
                // if we might have a match for a valid room code
                if(path.Length==7 && !path.Contains("."))
                {
                    Console.WriteLine("Path was 6 chars and did not have a .");
                    // attempt to get the room by the code given
                    var room = GetRoomByCode(path.Remove(0, 1));

                    // set the path to the html file so we don't request something like "/ABC123"
                    path = "/index.min.html";
                    
                    if(room==null)
                    {
                        
                        var sr = CreateRoom();

                        httpsv.AddWebSocketService("/" + sr.RoomCode, () => GetRoomByCode(sr.RoomCode).Service);
                        res.Redirect((req.Url.GetLeftPart(UriPartial.Authority) + "/" + sr.RoomCode));
                    }
                }
                else if (path == "/")
                {
                    Console.WriteLine("Path was '/'");
                    path = "/index.min.html";
                    var room = CreateRoom();

                    httpsv.AddWebSocketService("/" + room.RoomCode, () => room.Service);

                    res.Redirect(req.Url + room.RoomCode);
                }
                else
                {
                    Console.WriteLine("Client tried to request resource {0} but it does not exist.", path);
                    res.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
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
            if (room != null)
            {
                Console.WriteLine("Got room {0}", room.RoomCode);
            }
            else
            {
                Console.WriteLine("Could not find room {0}.", code);
            }
            return room;
        }

        public static void DeleteRoom(string code)
        {
            rooms.RemoveAt(rooms.FindIndex(r => { return r.RoomCode == code; }));
        }


    }
}
