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
    private readonly IReliablePacketStore _resendStore;
    private readonly CancellationTokenSource _resendToken;
    private readonly Task _resendTask;

    public IReliablePacketStore ReliablePacketStore => _resendStore;

    public RoomBroadcaster(ServerContext context, IReliablePacketStore resendStore)
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

            await Task.Delay(Config.ResendThreadTick);
        }

        _context.Logger.LogInformation("Room Broadcaster was shutdown successfully");
    }

    private void ProcessAckPacket(ReliablePacket reliablePacket)
    {
        if (reliablePacket.IsAlive)
        {
            if (!reliablePacket.IsResendTime)
            {
                return;
            }

            _context.Logger.LogTrace("Resending reliable packet #{Id} to {PlayerName} in room {#RoomdId}", reliablePacket.SequenceNumber, reliablePacket.Receiver.Name, reliablePacket.Receiver.Room.Id);

            Result<Error> sendResult = _context.PacketSender.Send(reliablePacket.Receiver.Endpoint, reliablePacket.RentedBuffer.UsedSpan);
            if (!sendResult.IsSuccess)
            {
                _context.Logger.LogError("An error occured while trying to resend the packet");
            }

            reliablePacket.DecrementTries();
            reliablePacket.RefreshLastSent();
        }
        else
        {
            PacketType packetType = reliablePacket.Header.Type; // need to capture here as packet store frees the rented buffer

            ReliablePacket? expiredPacket = _resendStore.RemovePacket(reliablePacket.SequenceNumber);
            if (expiredPacket == null)
            {
                _context.Logger.LogWarning("Expired packet already removed");
                return;
            }

            reliablePacket.Receiver.Room.UploadCommand(() =>
            {
                Result<Error> disconnectResult = _context.PlayerDisconnector.Disconnect(reliablePacket.Receiver);
                if (disconnectResult.IsSuccess)
                {
                    _context.Logger.LogWarning("Disconnected player {PlayerName} for not Acking {PacketType} packet (#{SequenceNumber}) in room #{RoomId}", reliablePacket.Receiver.Name, packetType, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
                }
                else
                {
                    _context.Logger.LogError("Failed to disconnect player {PlayerName} for no Acking packet #{PacketId} in room #{RoomId}", reliablePacket.Receiver.Name, reliablePacket.SequenceNumber, reliablePacket.Receiver.Room.Id);
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

    public Result<Error> BroadcastReliably(Room room, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            UploadAndSendAckPacket(roomPayload, player, maxRetries);
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastExcept(Room room, Player ignoredPlayer, ReadOnlySpan<byte> payload)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player == ignoredPlayer)
            {
                continue;
            }

            _context.PacketSender.Send(player.Endpoint, payload);
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastReliablyExcept(Room room, Player ignoredPlayer, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player != ignoredPlayer)
            {
                UploadAndSendAckPacket(roomPayload, player, maxRetries);
            }
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastExceptWith(Room room, ReadOnlySpan<byte> roomPayload, Player ignoredPlayer, ReadOnlySpan<byte> ignoredPlayerPayload)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player == ignoredPlayer)
            {
                _context.PacketSender.Send(player.Endpoint, ignoredPlayerPayload);
                continue;
            }

            _context.PacketSender.Send(player.Endpoint, roomPayload);
        }

        return Result<Error>.Success();
    }

    public Result<Error> BroadcastReliablyExceptWith(Room room, RentedBuffer roomPayload, Player ignoredPlayer, RentedBuffer ignoredPlayerPayload, byte maxRetries = Config.MaxRetries)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player == ignoredPlayer)
            {
                UploadAndSendAckPacket(ignoredPlayerPayload, player, maxRetries);
            }
            else
            {
                UploadAndSendAckPacket(roomPayload, player, maxRetries);
            }
        }

        return Result<Error>.Success();
    }

    private void UploadAndSendAckPacket(RentedBuffer rentedBuffer, Player receiver, byte maxRetries)
    {
        Result<Error> uploadResult = _resendStore.UploadPacket(rentedBuffer, receiver, maxRetries);
        if (uploadResult.IsSuccess)
        {
            _context.PacketSender.Send(receiver.Endpoint, rentedBuffer.UsedSpan);
        }
    }

    public Task Shutdown()
    {
        _resendToken.Cancel();
        return _resendTask;
    }
}
