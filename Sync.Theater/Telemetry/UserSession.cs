using System;
public class UserSession : TelemetrySession {
    public string RoomCode { get; set; }

    public UserSession(string RoomCode) : base() {
        this.RoomCode = RoomCode;
    }

    
}