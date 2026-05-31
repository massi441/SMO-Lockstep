using Lockstep.Client;
using Lockstep.Net;
using Lockstep.Protocol;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep.Server;

internal class RoomNotifier : IRoomNotifier
{
    private readonly ServerContext _context;
    private readonly IPlayerHolder _playerHolder;
    private readonly IPendingPacketStore _resendStore;
    private readonly CancellationTokenSource _resendToken;
    private readonly Task _resendTask;

    private const int ResendTick = 20;

    public IPendingPacketStore AckPacketStore => _resendStore;

    public RoomNotifier(ServerContext context, IPlayerHolder playerHolder, IPendingPacketStore resendStore)
    {
        _context = context;
        _playerHolder = playerHolder;
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
                ProcessPacket(pair.Value);
            }

            await Task.Delay(ResendTick);
        }

        _context.Logger.LogInformation("Room Notifier was shutdown successfully");
    }

    private void ProcessPacket(PendingPacket packet)
    {
        if (packet.IsAlive)
        {
            Result<Error> sendResult = _context.PacketSender.Send(packet.Payload, packet.Receiver);
            if (sendResult.IsSuccess)
            {
                packet.DecrementTries();
            }
            else
            {
                _context.Logger.LogError("An error occured while trying to resend packet ");
            }
        }
        else
        {
            _resendStore.RemovePacket(packet.SequenceNumber);
            packet.OnDropped?.Invoke(packet.Receiver);
        }
    }

    public Result<Error> NotifyAll(ReadOnlySpan<byte> payload)
    {
        foreach (Player player in _playerHolder.Players)
        {
            _context.PacketSender.Send(payload, player.Info.Endpoint);
        }

        return Result<Error>.Success();
    }

    public Result<Error> NotifyOthers(ReadOnlySpan<byte> payload, Player sender)
    {
        foreach (Player player in _playerHolder.Players)
        {
            if (player == sender)
            {
                continue;
            }

            _context.PacketSender.Send(payload, player.Info.Endpoint);
        }

        return Result<Error>.Success();
    }

    public Result<Error> NotifyOthers(ReadOnlySpan<byte> payload, Player sender, ReadOnlySpan<byte> senderPayload)
    {
        foreach (Player player in _playerHolder.Players)
        {
            if (player == sender)
            {
                _context.PacketSender.Send(senderPayload, player.Info.Endpoint);
                continue;
            }

            _context.PacketSender.Send(payload, player.Info.Endpoint);
        }

        return Result<Error>.Success();
    }

    public Task Shutdown()
    {
        _resendToken.Cancel();
        return _resendTask;
    }
}
