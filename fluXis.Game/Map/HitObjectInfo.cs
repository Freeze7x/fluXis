using System.Text.Json.Serialization;

namespace fluXis.Game.Map;

public class HitObjectInfo
{
    public float Time;
    public int Lane;
    public float HoldTime;

    [JsonIgnore]
    public float HoldEndTime => Time + HoldTime;

    public bool IsLongNote()
    {
        return HoldTime > 0;
    }

    public override string ToString()
    {
        return $"Time: {Time}, Lane: {Lane}, HoldTime: {HoldTime}";
    }
}
