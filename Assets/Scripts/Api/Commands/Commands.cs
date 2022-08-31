using System.Runtime.Serialization;

public enum Commands
{
    [EnumMember(Value = "o")] LaserOn,
    [EnumMember(Value = "p")] LaserOff,
    [EnumMember(Value = "g")] Distance,
    [EnumMember(Value = "o")] StartTracking,
    [EnumMember(Value = "o")] StopTracking,
}