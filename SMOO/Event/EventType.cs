namespace SMOO.Event;

internal enum EventType : ushort
{
    ChangeStage,
    ChangeCostume,
    PlayerSync,

    /// <summary>
    /// An reserved EventType for server side validation
    /// </summary>
    Invalid
}
