using System.Net;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Util;
using Microsoft.Extensions.Logging;
using SMOO.Server;
using SMOO.Services.Interface;

namespace SMOO.Services.Impl;

internal class RoomBroadcaster : IRoomBroadcaster
{
    private readonly ServerContext _context;
    private readonly IPendingPacketStore _resendStore;
    private readonly CancellationTokenSource _resendToken;
    private readonly Task _resendTask;

    public IPendingPacketStore PendingPacketStore => _resendStore;

    public RoomBroadcaster(ServerContext context, IPendingPacketStore resendStore)
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

    private void ProcessAckPacket(PendingPacket pendingPacket)
    {
        if (pendingPacket.IsAlive)
        {
            if (!pendingPacket.IsResendTime)
            {
                return;
            }

            Result<Error> sendResult = _context.PacketSender.Send(pendingPacket.Player.Endpoint, pendingPacket.RentedPayload.Span);
            if (sendResult.IsSuccess)
            {
                pendingPacket.DecrementTries();
            }
            else
            {
                _context.Logger.LogError("An error occured while trying to resend the packet");
            }

            pendingPacket.RefreshLastSent();
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

    public Result<Error> BroadcastAck(Room room, PacketBroadcastRequest request)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            SendPlayerAckPacket(player, request);
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

    public Result<Error> BroadcastAckExcept(Room room, Player sender, PacketBroadcastRequest request)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player != sender)
            {
                SendPlayerAckPacket(player, request);
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

    public Result<Error> BroadcastAckExceptWith(Room room, Player sender, PacketBroadcastRequest playerRequest, PacketBroadcastRequest request)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player == sender)
            {
                SendPlayerAckPacket(sender, playerRequest);
            }
            else
            {
                SendPlayerAckPacket(player, request);
            }
        }

        return Result<Error>.Success();
    }

    private void SendPlayerAckPacket(Player player, PacketBroadcastRequest ackRequest)
    {
        PendingPacketRequest request = new PendingPacketRequest()
        {
            Receiver = player,
            RentedPayload = ackRequest.RentedPayload,
            MaxRetries = ackRequest.MaxRetries
        };

        Result<Error> uploadResult = _resendStore.UploadPacket(request);
        if (uploadResult.IsSuccess)
        {
            _context.PacketSender.Send(player.Endpoint, request.RentedPayload.Span);
        }
    }

    public Task Shutdown()
    {
        _resendToken.Cancel();
        return _resendTask;
    }
}
