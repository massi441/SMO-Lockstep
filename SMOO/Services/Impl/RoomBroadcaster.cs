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
        if (reliablePacket.HasTriesLeft)
        {
            if (!reliablePacket.IsResendTime)
            {
                return;
            }

            _context.Logger.LogTrace("Resending {Type} packet #{Id} to {PlayerName} in room {#RoomdId}", reliablePacket.Header.Type, reliablePacket.SequenceNumber, reliablePacket.Receiver.Name, reliablePacket.Receiver.Room.Id);

            reliablePacket.WriteSequenceNumber(); // write the packet's sequence number into the payload in case the buffer is shared

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

    public void Broadcast(Room room, ReadOnlySpan<byte> payload)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            _context.PacketSender.Send(player.Endpoint, payload);
        }
    }

    public void BroadcastReliably(Room room, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries)
    {
        AtomicCounter counter = new AtomicCounter();
        foreach (Player player in room.PlayerHolder.Players)
        {
            UploadAndSendAckPacket(roomPayload, counter, player, maxRetries);
        }
    }

    public void BroadcastExcept(Room room, Player ignoredPlayer, ReadOnlySpan<byte> payload)
    {
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player == ignoredPlayer)
            {
                continue;
            }

            _context.PacketSender.Send(player.Endpoint, payload);
        }
    }

    public void BroadcastReliablyExcept(Room room, Player ignoredPlayer, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries)
    {
        AtomicCounter counter = new AtomicCounter();
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player != ignoredPlayer)
            {
                UploadAndSendAckPacket(roomPayload, counter, player, maxRetries);
            }
        }
    }

    public void BroadcastExceptWith(Room room, ReadOnlySpan<byte> roomPayload, Player ignoredPlayer, ReadOnlySpan<byte> ignoredPlayerPayload)
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
    }

    public void BroadcastReliablyExceptWith(Room room, RentedBuffer roomPayload, Player ignoredPlayer, RentedBuffer ignoredPlayerPayload, byte maxRetries = Config.MaxRetries)
    {
        AtomicCounter roomCounter = new AtomicCounter();
        AtomicCounter playerCounter = new AtomicCounter();
        foreach (Player player in room.PlayerHolder.Players)
        {
            if (player == ignoredPlayer)
            {
                UploadAndSendAckPacket(ignoredPlayerPayload, playerCounter, player, maxRetries);
            }
            else
            {
                UploadAndSendAckPacket(roomPayload, roomCounter, player, maxRetries);
            }
        }
    }

    // TODO: Check success of upload, keep track of which packets have been successfully uploaded
    private void UploadAndSendAckPacket(RentedBuffer rentedBuffer, AtomicCounter refCounter, Player receiver, byte maxRetries)
    {
        Result<Error> uploadResult = _resendStore.UploadPacket(rentedBuffer, refCounter, receiver, maxRetries);
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
