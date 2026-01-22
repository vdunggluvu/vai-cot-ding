namespace MagicTrackpad.Core.Models;

public class TouchPoint
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public int Pressure { get; set; }
    public bool IsContact { get; set; }
}

public class TouchFrame
{
    public List<TouchPoint> Points { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public int ButtonState { get; set; }
}
