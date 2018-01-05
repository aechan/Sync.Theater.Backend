using Newtonsoft.Json;
using System;

public class TelemetrySerializer {
    
    public static void SerializeRoomSession(RoomSession room) {
        string json = JsonConvert.SerializeObject(room);
        System.IO.File.WriteAllText("RoomSession_"+room.RoomCode+".json", json);
    }
}