namespace SMOO.Event;

internal enum EventType : ushort
{
    JoinStage,
    LeaveStage,
    ChangeCostume,
    PlayerSync,

    /// <summary>
    /// An reserved EventType for server side validation
    /// </summary>
    Invalid
}
