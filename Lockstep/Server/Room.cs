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

    public uint Id { get; }
    public Task Task { get; private set; } = null!;
    public Channel<Packet> Packets { get; }
    public IPlayerHolder PlayerHolder { get; }

    public Room(uint roomId, ServerContext conxtext, IPlayerHolder playerHolder)
    {
        _context = conxtext;

        Id = roomId;
        PlayerHolder = playerHolder;
        Packets = Channel.CreateUnbounded<Packet>();
    }

    public void Start()
    {
        if (Task != null)
        {
            return;
        }

        Task = Task.Run(async () =>
        {
            try
            {
                await ProcessAsync();
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "An Error Occured while processing the room");
            }
        });
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
                    _context.Logger.LogError("A {PacketType} packet of invalid size ({PacketSize}) was requested", packet.Header.Type, packet.Payload.Length);
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
                _context.Logger.LogError("No handler found for packet type");
            }
        }
    }

    public void Shutdown()
    {
        Packets.Writer.Complete();
    }
}
