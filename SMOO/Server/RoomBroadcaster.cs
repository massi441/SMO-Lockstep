using System.Net;
using Lockstep.Client;
using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Server;

internal class RoomBroadcaster : IRoomBroadcaster
{
    private readonly ServerContext _context;
    private readonly IPacketPendingStore _resendStore;
    private readonly CancellationTokenSource _resendToken;
    private readonly Task _resendTask;

    public IPacketPendingStore AckPacketStore => _resendStore;

    public RoomBroadcaster(ServerContext context, IPacketPendingStore resendStore)
    {
        _context = context;
        _resendStore = resendStore;
        _resendToken = CancellationTokenSource.CreateLinkedTokenSource(_context.CancellationToken);
        _resendTask = Task.Run(ResendLoop);
    }

    private async Task ResendLoop()
    {
        while (!_resendToken.IsCancellationRequested)
        {
            foreach (var pair in _resendStore.PendingPackets)
            {
                ProcessAckPacket(pair.Value);
            }

            await Task.Delay(Config.ResendTick);
        }

        _context.Logger.LogInformation("Room Broadcaster was shutdown successfully");
    }

    private void ProcessAckPacket(PacketPending pendingPacket)
    {
        if (pendingPacket.IsAlive)
        {
            if (!pendingPacket.IsResendTime)
            {
                return;
            }

            Result<Error> sendResult = _context.PacketSender.Send(pendingPacket.Player.Endpoint, pendingPacket.Payload);
            if (sendResult.IsSuccess)
            {
                pendingPacket.DecrementTries();
            }
            else
            {
                _context.Logger.LogError("An error occured while trying to resend the packet");
            }

            pendingPacket.RefreshTime();
        }
        else
        {
            _resendStore.RemovePacket(pendingPacket.SequenceNumber);
            pendingPacket.Player.Room.UploadCommand(() =>
            {
                Result<Error> disconnectResult = _context.PlayerDisconnector.Disconnect(pendingPacket.Player);
                if (disconnectResult.IsSuccess)
                {
                    _context.Logger.LogWarning("Disconnected player {PlayerName} for not Acking packet #{PacketId} in room #{RoomId}", pendingPacket.Player.Name, pendingPacket.SequenceNumber, pendingPacket.Player.Room.Id);
                }
                else
                {
                    _context.Logger.LogError("Failed to disconnect player {PlayerName} for no Acking packet #{PacketId} in room #{RoomId}", pendingPacket.Player.Name, pendingPacket.SequenceNumber, pendingPacket.Player.Room.Id);
                }
            });
        }
    }

    public Result<Error> Broadcast(Room room, ReadOnlySpan<byte> payload)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            _context.PacketSender.Send(player.Endpoint, payload);
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastAck(Room room, in PacketAckBroadcastRequest request)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            SendPlayerAckPacket(player, in request);
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastExcept(Room room, IPEndPoint sender, ReadOnlySpan<byte> payload)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player.Endpoint.Equals(sender))
            {
                continue;
            }

            _context.PacketSender.Send(player.Endpoint, payload);
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastAckExcept(Room room, Player sender, in PacketAckBroadcastRequest request)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player != sender)
            {
                SendPlayerAckPacket(player, in request);
            }
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastExceptWith(Room room, IPEndPoint sender, ReadOnlySpan<byte> senderPayload, ReadOnlySpan<byte> payload)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player.Endpoint.Equals(sender))
            {
                _context.PacketSender.Send(player.Endpoint, senderPayload);
                continue;
            }

            _context.PacketSender.Send(player.Endpoint, payload);
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastAckExceptWith(Room room, Player sender, in PacketAckBroadcastRequest playerRequest, in PacketAckBroadcastRequest request)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player == sender)
            {
                SendPlayerAckPacket(sender, in playerRequest);
            }
            else
            {
                SendPlayerAckPacket(player, in request);
            }
        }

        return Result<Error>.Success();
    }

    private void SendPlayerAckPacket(Player player, in PacketAckBroadcastRequest ackRequest)
    {
        PacketPendingRequest request = new PacketPendingRequest()
        {
            Receiver = player,
            Payload = ackRequest.Payload,
            MaxRetries = ackRequest.MaxRetries
        };

        Result<Error> uploadResult = _resendStore.UploadPacket(in request);
        if (uploadResult.IsSuccess)
        {
            _context.PacketSender.Send(player.Endpoint, request.Payload);
        }
    }

    public Task Shutdown()
    {
        _resendToken.Cancel();
        return _resendTask;
    }
}
