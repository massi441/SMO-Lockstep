using System.Net;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Client;

internal class PlayerHolder : IPlayerHolder
{
    private readonly Player[] _players;

    public byte MaxSize => (byte)_players.Length;
    public IEnumerable<Player> Players => _players.Where(p => p != null);
    public byte PlayerCount => (byte)Players.Count();

    public PlayerHolder(byte size = 4)
    {
        _players = new Player[Math.Min(size, Config.MaxRoomSize)];
    }

    public Result<Player, Error> RegisterPlayer(in PlayerInfo playerInfo)
    {
        if (ContainsPlayer(playerInfo))
        {
            return Result<Player, Error>.Failure(Error.PlayerAlreadyInRoom);
        }

        if (!TryFindSlot(out byte index))
        {
            return Result<Player, Error>.Failure(Error.RoomFull);
        }

        Player player = new Player()
        {
            Endpoint = playerInfo.Endpoint,
            Name = playerInfo.Name,
            Room = playerInfo.Room,
        };

        _players[index] = player;

        return Result<Player, Error>.Success(player);
    }

    public Result<Error> UnregisterPlayer(Player player)
    {
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i] == player)
            {
                _players[i] = null!;
                return Result<Error>.Success();
            }
        }

        return Result<Error>.Failure(Error.OperationFailed);
    }

    public Player? FindPlayerByIp(IPEndPoint endpoint)
    {
        foreach (Player p in _players)
        {
            if (p == null)
            {
                continue;
            }

            if (p.Endpoint.Address.Equals(endpoint.Address))
            {
                return p;
            }
        }

        return null;
    }

    public void RemovePlayer(Player player)
    {
        for (int i = 0; i < _players.Length; i++)
        {
            Player p = _players[i];
            if (p == null)
            {
                continue;
            }

            if (p.Endpoint.Equals(player.Endpoint))
            {
                _players[i] = null!;
            }
        }
    }

    private bool TryFindSlot(out byte index)
    {
        index = 0;

        while (index < _players.Length)
        {
            if (_players[index] == null)
            {
                return true;
            }

            index++;
        }

        return false;
    }

    private bool ContainsPlayer(PlayerInfo playerInfo)
    {
        return FindPlayerByIp(playerInfo.Endpoint) != null;
    }
}
