using Lockstep.Client;
using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Lockstep.Server;

internal class Room
{
    private readonly ServerContext _context;
    private readonly IClientHolder _clientHolder;

    private ILogger Logger => _context.Logger;

    public Task Task { get; }
    public Channel<Packet> Packets { get; }

    public Room(ServerContext conxtext, IClientHolder clientHolder)
    {
        _context = conxtext;
        _clientHolder = clientHolder;

        Packets = Channel.CreateUnbounded<Packet>();
        Task = Task.Run(() => ProcessAsync());
    }

    public async Task ProcessAsync()
    {
        await foreach (Packet packet in Packets.Reader.ReadAllAsync())
        {
            IPacketHandler? packetHandler = PacketHandlerFactory.CreateHandler(packet.Header.Type, _context);
            if (packetHandler != null)
            {
                Result<Error> handlerResult = packetHandler.Handle(this, packet);
                if (handlerResult.IsSuccess)
                {
                    Logger.LogTrace("Successfully handled packet {PacketType}", packet.Header.Type);
                }
                else
                {
                    Logger.LogTrace("Failed to handle packet, Error: {Error}", handlerResult.Error);
                }
            }
            else
            {
                Logger.LogError("No handler found for packet type");
            }
        }
    }

    public void Shutdown()
    {
        Packets.Writer.Complete();
    }
}
