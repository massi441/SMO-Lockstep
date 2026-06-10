namespace SMOO.Event;

internal enum EventType : ushort
{
    JoinStage,
    LeaveStage,
    ChangeCostume,
    InfoSync,

    /// <summary>
    /// An reserved EventType for server side validation
    /// </summary>
    Invalid
}
