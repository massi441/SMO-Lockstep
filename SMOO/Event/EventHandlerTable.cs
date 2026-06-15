using System.Diagnostics;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal readonly unsafe struct EventHandler
{
    public readonly ushort MinDataSize;
    public readonly ushort MaxDataSize;
    public readonly delegate*<ParsedEventPacket, Room, ServerContext, void> Handle;

    public EventHandler(ushort minPayloadSize, ushort maxPayloadSize, delegate*<ParsedEventPacket, Room, ServerContext, void> handle)
    {
        MinDataSize = minPayloadSize;
        MaxDataSize = maxPayloadSize;
        Handle = handle;
    }
}

internal static unsafe class EventHandlerTable
{
    private static readonly EventHandler DefaultHandler         = new EventHandler(EventDefaultHandler.MinDataSize, EventDefaultHandler.MaxDataSize, &EventDefaultHandler.Handle);
    private static readonly EventHandler ChangeStage            = new EventHandler(EventChangeStageHandler.MinDataSize, EventChangeStageHandler.MaxDataSize, &EventChangeStageHandler.Handle);
    private static readonly EventHandler ChangeCostume          = new EventHandler(EventChangeCostumeHandler.MinDataSize, EventChangeCostumeHandler.MaxDataSize, &EventChangeCostumeHandler.Handle);
    private static readonly EventHandler PlayerSync             = new EventHandler(EventPlayerSyncHandler.MinDataSize, EventPlayerSyncHandler.MaxDataSize, &EventPlayerSyncHandler.Handle);

    private static readonly EventHandler[] Handlers =
    [
        ChangeStage,
        ChangeCostume,
        PlayerSync,
    ];

    static EventHandlerTable()
    {
        Debug.Assert(Handlers.Length == (ushort)EventType.Invalid, "Handlers table is out of sync with EventType enum");
    }

    public static EventHandler GetHandler(EventType type)
    {
        ushort index = (ushort)type;
        if (index < Handlers.Length)
        {
            return Handlers[index];
        }
        return DefaultHandler;
    }
}
