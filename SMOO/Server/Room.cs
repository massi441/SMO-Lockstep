using System.Collections.Concurrent;
using System.Net;
using System.Threading.Channels;
using SMOO.Client;
using SMOO.Protocol;
using Microsoft.Extensions.Logging;
using SMOO.Services.Interface;
using SMOO.Handle;

namespace SMOO.Server;

internal class Room
{
    private readonly ServerContext _context;
    private readonly Task _processTask;
    private readonly ConcurrentQueue<Action> _commands = [];

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

        _processTask = Task.Run(ProcessAsync, _context.CancellationToken);
    }

    public Task Shutdown()
    {
        Packets.Writer.Complete();
        return Task.WhenAll(_processTask, Broadcaster.Shutdown());
    }

    public void UploadCommand(Action action)
    {
        _commands.Enqueue(action);
    }

    private async Task ProcessAsync()
    {
        try
        {
            await foreach (Packet packet in Packets.Reader.ReadAllAsync())
            {
                ProcessCommands();

                if (!IsAllowedInRoom(packet.Sender, packet.Header, out Player? player))
                {
                    _context.Logger.LogWarning("{Address}:{Port} illegally tried to access room #{RoomId}", packet.Sender.Address, packet.Sender.Port, Id);
                    continue;
                }

                player?.RefreshLastSeen();

                PacketHandler packetHandler = PacketHandlerProvider.GetHandler(packet.Header.Type);

                if (packet.Payload.Length < packetHandler.MinPayloadSize)
                {
                    _context.Logger.LogWarning("{PacketType} packet of invalid size ({PacketSize}) was requested. Minimum required: {Minimum}", packet.Header.Type, packet.Payload.Length, packetHandler.MinPayloadSize);
                    continue;
                }

                ParsedPacket parsedPacket = new ParsedPacket()
                {
                    SenderPlayer = player,
                    RentedBuffer = packet.RentedBuffer,
                    SenderIp = packet.Sender
                };

                unsafe
                {
                    packetHandler.Handler(parsedPacket, this, _context);
                }
            }
        } 
        catch (Exception ex)
        {
            _context.Logger.LogError(ex, "Error in Room #{RoomId}", Id);
            return;
        }

        _context.Logger.LogInformation("Room #{RoomId} was shutdown sucessfully", Id);
    }

    private void ProcessCommands()
    {
        while (_commands.TryDequeue(out Action? command))
        {
            _context.Logger.LogTrace("Processing command in room #{RoomId}", Id);
            command!.Invoke();
        }
    }

    private bool IsAllowedInRoom(IPEndPoint sender, PacketHeader header, out Player? player)
    {
        if (header.Type == PacketType.Connect)
        {
            player = null;
            return true;
        }

        player = PlayerHolder.FindPlayerByHost(sender);

        return player != null;
    }
}
