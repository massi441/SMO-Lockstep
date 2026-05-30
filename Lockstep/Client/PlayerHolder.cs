using System.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Client;

internal class PlayerHolder : IPlayerHolder
{
    private readonly Player[] _players;

    public PlayerHolder(int size = 4)
    {
        _players = new Player[size];
    }

    public Result<Player, Error> RegisterPlayer(PlayerInfo playerInfo)
    {
        if (ContainsPlayer(playerInfo))
        {
            return Result<Player, Error>.Failure(Error.PlayerAlreadyInRoom);
        }

        if (!TryFindSlot(out int index, out int playerPort))
        {
            return Result<Player, Error>.Failure(Error.RoomFull);
        }

        Player player = new Player()
        {
            Info = playerInfo,
            PortNumber = playerPort
        };

        _players[index] = player;
        player.LastSeen = DateTime.UtcNow;
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

    public Player? FindPlayerByHost(IPEndPoint endpoint)
    {
        foreach (Player p in _players)
        {
            if (p == null)
            {
                continue;
            }

            if (p.Info.Endpoint.Equals(endpoint))
            {
                return p;
            }
        }

        return null;
    }

    public IEnumerable<Player> GetPlayers()
    {
        return _players.Where(p => p != null);
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

            if (p.Info.Endpoint.Equals(player.Info.Endpoint))
            {
                _players[i] = null!;
            }
        }
    }

    private bool TryFindSlot(out int index, out int playerPort)
    {
        index = 0;
        playerPort = 0;

        while (index < _players.Length)
        {
            if (IsReservedPort(playerPort))
            {
                playerPort++;
                continue;
            }

            if (_players[index] == null)
            {
                return true;
            }

            index++;
            playerPort++;
        }

        return false;
    }

    private bool ContainsPlayer(PlayerInfo playerInfo)
    {
        return FindPlayerByHost(playerInfo.Endpoint) != null;
    }

    private static bool IsReservedPort(int port)
    {
        // 0 is for the debug controller, 2 is for cappy
        return port == 0 || port == 2;
    }
}
