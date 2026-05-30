using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Client;

internal interface IPlayerHolder
{
    Result<Player, Error> AddPlayer(PlayerInfo playerInfo);
    ReadOnlySpan<Player> GetPlayers();
}
