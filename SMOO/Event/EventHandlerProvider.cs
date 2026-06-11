using System.Diagnostics;
using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Event;

internal readonly unsafe struct EventHandler
{
    public readonly ushort MinPayloadSize;
    public readonly delegate*<ParsedPacket, Room, ServerContext, ReadOnlySpan<byte>, void> Handle;

    public EventHandler(ushort minPayloadSize, delegate*<ParsedPacket, Room, ServerContext, ReadOnlySpan<byte>, void> handle)
    {
        MinPayloadSize = minPayloadSize;
        Handle = handle;
    }
}

internal static unsafe class EventHandlerProvider
{
    private static readonly EventHandler DefaultHandler         = new EventHandler(EventDefaultHandler.MinPayloadSize, &EventDefaultHandler.Handle);
    private static readonly EventHandler JoinStage              = DefaultHandler;
    private static readonly EventHandler LeaveStage             = DefaultHandler;
    private static readonly EventHandler ChangeCostume          = DefaultHandler;
    private static readonly EventHandler InfoSync               = DefaultHandler;

    private static readonly EventHandler[] Handlers =
    [
        JoinStage,
        LeaveStage,
        ChangeCostume,
        InfoSync,
    ];

    static EventHandlerProvider()
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
