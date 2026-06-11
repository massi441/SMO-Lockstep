namespace SMOO.Event;

internal enum EventType : ushort
{
    JoinStage,
    LeaveStage,
    ChangeCostume,
    GameSync,

    /// <summary>
    /// An reserved EventType for server side validation
    /// </summary>
    Invalid
}
