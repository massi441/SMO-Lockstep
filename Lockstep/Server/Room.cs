using Lockstep.Net;
using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Lockstep.Server;

internal class Room
{
    private readonly ServerContext _context;
    private readonly Task _processTask;

    public ushort Id { get; }
    public Channel<Packet> Packets { get; }
    public IPlayerHolder PlayerHolder { get; }
    public IRoomBroadcaster Broadcaster { get; }

    public Room(ushort roomId, ServerContext conxtext, IPlayerHolder playerHolder, IRoomBroadcaster broadcaster)
    {
        _context = conxtext;

        Id = roomId;
        PlayerHolder = playerHolder;
        Packets = Channel.CreateUnbounded<Packet>();
        Broadcaster = broadcaster;

        _processTask = Task.Run(StartWork, _context.CancellationToken);
    }

    private Task StartWork()
    {
        try
        {
            return ProcessAsync();
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "An Error Occured while processing the room");
            return Task.CompletedTask;
        }
    }

    private async Task ProcessAsync()
    {
        await foreach (Packet packet in Packets.Reader.ReadAllAsync())
        {
            ProcessCommands();

            IPacketHandler? packetHandler = PacketHandlerFactory.CreateHandler(packet.Header.Type, _context);
            if (packetHandler == null)
            {
                _context.Logger.LogWarning("No handler found for packet type {PacketType}", (int)packet.Header.Type);
                continue;
            }

            if (packet.Payload.Buffer.Length < packetHandler.MinPayloadSize)
            {
                _context.Logger.LogWarning("A {PacketType} packet of invalid size ({PacketSize}) was requested. Minimum required: {Minimum}", packet.Header.Type, packet.Payload.Length, packetHandler.MinPayloadSize);
                continue;
            }

            packetHandler.Handle(packet, this);
        }

        _context.Logger.LogInformation("Room #{RoomId} was shutdown sucessfully", Id);
    }

    private void ProcessCommands()
    {
        while (Broadcaster.TryGetPendingCommand(out Action? command))
        {
            _context.Logger.LogTrace("Processing command in room #{RoomId}", Id);
            command!.Invoke();
        }
    }

    public Task Shutdown()
    {
        Packets.Writer.Complete();
        return Task.WhenAll(_processTask, Broadcaster.Shutdown());
    }
}
