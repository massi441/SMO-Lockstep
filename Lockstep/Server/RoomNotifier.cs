using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Server;

internal class RoomNotifier : IRoomNotifier
{
    private readonly ServerContext _context;
    private readonly IPlayerHolder _playerHolder;

    public RoomNotifier(ServerContext context, IPlayerHolder playerHolder)
    {
        _context = context;
        _playerHolder = playerHolder;
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
}
