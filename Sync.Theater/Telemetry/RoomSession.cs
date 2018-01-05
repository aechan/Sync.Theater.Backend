using System;
using System.Collections.Generic;

public class RoomSession : TelemetrySession {
    private List<UserSession> UserSessions { get; set; }
    public DateTime RoomStartTime { get; set; }
    public DateTime RoomEndTime { get; set; }
    public int TotalUsers { get; set; }
    public double AverageTimePerUser { get; set; }
    public string RoomCode { get; set; }
    
    public RoomSession(string RoomCode) : base() {
        this.RoomCode = RoomCode;
    }

    public void AddUser(UserSession u) {
        UserSessions.Add(u);
    }

    public void Stop() {
        base.Stop();

        TotalUsers = UserSessions.Count;
        double totalTime = 0.0;

        foreach(UserSession u in UserSessions) {
            totalTime += u.SessionLength;
        }

        AverageTimePerUser = totalTime / TotalUsers;

        TelemetrySerializer.SerializeRoomSession(this);
    }
}