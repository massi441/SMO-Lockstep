using System.Net;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Client;

internal class PlayerHolder : IPlayerHolder
{
    private readonly Player[] _players;

    public byte MaxSize => (byte)_players.Length;
    public Player[] Players => [.. _players.Where(p => p != null)];
    public byte ActivePlayerCount => (byte)Players.Count();

    public PlayerHolder(byte size = Config.DefaultRoomSize)
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
            Id = new PlayerId()
            {
                Endpoint = playerInfo.Endpoint,
                SessionId = Guid.NewGuid(),
            },
            Slot = index,
            Name = playerInfo.Name,
            Room = playerInfo.Room,
            WorldInfo = new PlayerWorldInfo()
            {
                CostumeBody = Config.DefaultCostumeName,
                CostumeCap = Config.DefaultCostumeName
            }
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

    public Player[] InSameStageAs(Player targetPlayer)
    {
        return [.. _players.Where(p => p != targetPlayer && p.WorldInfo.CurrentStage == targetPlayer.WorldInfo.CurrentStage)];
    }

    public Player? FindPlayerById(PlayerId id)
    {
        foreach (Player p in _players)
        {
            if (p == null)
            {
                continue;
            }

            if (p.Id == id)
            {
                return p;
            }
        }

        return null;
    }

    public Player? FindPlayerByHost(IPEndPoint endpoint)
    {
        foreach (Player p in _players)
        {
            if (p == null)
            {
                continue;
            }

            if (p.Endpoint.Equals(endpoint))
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

            if (p.Id == player.Id)
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
        return FindPlayerByHost(playerInfo.Endpoint) != null;
    }
}
