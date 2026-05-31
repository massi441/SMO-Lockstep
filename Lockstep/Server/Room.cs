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
    public IRoomNotifier Notifier { get; }

    public Room(ushort roomId, ServerContext conxtext, IPlayerHolder playerHolder, IRoomNotifier notifier)
    {
        _context = conxtext;

        Id = roomId;
        PlayerHolder = playerHolder;
        Packets = Channel.CreateUnbounded<Packet>();
        Notifier = notifier;

        _processTask = Task.Run(StartWork, _context.CancellationToken);
    }

    private async Task StartWork()
    {
        try
        {
            await ProcessAsync();
        }
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "An Error Occured while processing the room");
        }
    }

    private async Task ProcessAsync()
    {
        await foreach (Packet packet in Packets.Reader.ReadAllAsync())
        {
            IPacketHandler? packetHandler = PacketHandlerFactory.CreateHandler(packet.Header.Type, _context);
            if (packetHandler != null)
            {
                if (packet.Payload.Buffer.Length < packetHandler.MinPayloadSize)
                {
                    _context.Logger.LogWarning("A {PacketType} packet of invalid size ({PacketSize}) was requested. Minimum required: {Minimum}", packet.Header.Type, packet.Payload.Length, packetHandler.MinPayloadSize);
                    continue;
                }

                Result<Error> handlerResult = packetHandler.Handle(packet, this);
                if (handlerResult.IsSuccess)
                {
                    _context.Logger.LogTrace("Successfully handled packet {PacketType}", packet.Header.Type);
                }
                else
                {
                    _context.Logger.LogTrace("Failed to handle packet, Error: {Error}", handlerResult.Error);
                }
            }
            else
            {
                _context.Logger.LogWarning("No handler found for packet type {PacketType}", (int)packet.Header.Type);
            }
        }

        _context.Logger.LogInformation("Room #{RoomId} was shutdown sucessfully", Id);
    }
    public Task Shutdown()
    {
        Packets.Writer.Complete();
        return Task.WhenAll(_processTask, Notifier.Shutdown());
    }
}
