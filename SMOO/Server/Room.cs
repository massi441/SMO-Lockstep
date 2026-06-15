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
    public IBroadcaster Broadcaster { get; }
    public PlayerList Players => PlayerHolder.Players;

    public Room(ushort roomId, ServerContext conxtext, IPlayerHolder playerHolder, IBroadcaster broadcaster)
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
        await foreach (Packet packet in Packets.Reader.ReadAllAsync())
        {
            try
            {
                while (_commands.TryDequeue(out Action? command))
                {
                    _context.Logger.LogTrace("Processing command in room #{RoomId}", Id);
                    command!.Invoke();
                }

                if (!IsAllowedInRoom(packet.Sender, packet.Header, out Player? player))
                {
                    _context.Logger.LogWarning("{Address}:{Port} illegally tried to access room #{RoomId}", packet.Sender.Address, packet.Sender.Port, Id);
                    packet.RentedBuffer.Return();
                    continue;
                }

                player?.RefreshLastSeen();

                PacketHandler packetHandler = PacketHandlerTable.GetHandler(packet.Header.Type);

                if (packet.PayloadSize < packetHandler.MinPayloadSize)
                {
                    _context.Logger.LogWarning("{PacketType} packet of invalid size ({PacketSize}) was requested. Minimum required: {Minimum}", packet.Header.Type, packet.PayloadSize, packetHandler.MinPayloadSize);
                    packet.RentedBuffer.Return();
                    continue;
                }

                if (packet.PayloadSize > packetHandler.MaxPayloadSize)
                {
                    _context.Logger.LogWarning("{PacketType} packet payload too large ({PacketSize}), maximum allowed: {Maximum}. Error: {Error}", packet.Header.Type, packet.PayloadSize, packetHandler.MaxPayloadSize, Error.PayloadTooLarge);
                    packet.RentedBuffer.Return();
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
            catch (InvalidDataException ex)
            {
                _context.Logger.LogError("Invalid data detected in {PacketType} in Room #{RoomId}: {Message}", packet.Header.Type, Id, ex.Message);
                packet.RentedBuffer.Return();
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Unexpected error in Room #{RoomId}", Id);
                packet.RentedBuffer.Return();
            }
        }

        _context.Logger.LogInformation("Room #{RoomId} was shutdown sucessfully", Id);
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
