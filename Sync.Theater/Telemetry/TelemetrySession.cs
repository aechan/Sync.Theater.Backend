using System;

public class TelemetrySession { 

    public DateTime SessionStart { get; set; }
    public DateTime SessionStop { get; set; }
    public double SessionLength { get; set; }

    public TelemetrySession() {
        Start();
    }

    protected void Start() {
        SessionStart = DateTime.Now;
    }

    public void Stop() {
        SessionStart = DateTime.Now;
        SessionLength = SessionStop.Subtract(SessionStart).TotalSeconds;
    }
}