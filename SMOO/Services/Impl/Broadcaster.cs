using Microsoft.Extensions.Logging;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Server;
using SMOO.Services.Interface;
using SMOO.Util;

namespace SMOO.Services.Impl;

internal class Broadcaster : IBroadcaster
{
    private readonly ServerContext _context;
    private readonly IReliablePacketStore _resendStore;
    private readonly CancellationTokenSource _resendToken;
    private readonly Task _resendTask;

    public IReliablePacketStore ReliablePacketStore => _resendStore;

    public Broadcaster(ServerContext context, IReliablePacketStore resendStore)
    {
        _context = context;
        _resendStore = resendStore;
        _resendToken = CancellationTokenSource.CreateLinkedTokenSource(_context.CancellationToken);
        _resendTask = Task.Run(ResendLoop);
    }

    public void Broadcast(Player[] players, RentedBuffer buffer)
    {
        foreach (Player player in players)
        {
            _context.PacketSender.Send(player.Endpoint, buffer);
        }
    }

    public void BroadcastReliably(Player[] players, RentedBuffer roomPayload, byte maxRetries = Config.MaxRetries)
    {
        RefCounter counter = new RefCounter();
        foreach (Player player in players)
        {
            UploadAndSendAckPacket(roomPayload, counter, player, maxRetries);
        }

        if (counter.Count == 0)
        {
            roomPayload.Return();
        }
    }

    // TODO: Check success of upload, keep track of which packets have been successfully uploaded
    private void UploadAndSendAckPacket(RentedBuffer buffer, RefCounter refCounter, Player receiver, byte maxRetries)
    {
        Result<ReliablePacket, Error> uploadResult = _resendStore.UploadPacket(buffer, refCounter, receiver, maxRetries);
        if (uploadResult.IsFailed)
        {
            return;
        }

        _context.PacketSender.Send(receiver.Endpoint, buffer);
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

            Result<Error> sendResult = _context.PacketSender.Send(reliablePacket.Receiver.Endpoint, reliablePacket.RentedBuffer);
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

            ReliablePacket? expiredPacket = _resendStore.RemovePacket(reliablePacket.Receiver, reliablePacket.SequenceNumber);
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

    public Task Shutdown()
    {
        _resendToken.Cancel();
        _resendToken.Dispose();
        return _resendTask;
    }
}
