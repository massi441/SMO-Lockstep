using System.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Client;

internal interface IPlayerHolder
{
    Result<Player, Error> RegisterPlayer(PlayerInfo playerInfo);
    Result<Error> UnregisterPlayer(Player player);
    Player? FindPlayerByHost(IPEndPoint endpoint);
    IEnumerable<Player> GetPlayers();
}
