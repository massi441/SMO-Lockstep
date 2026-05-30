using System.Net;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Client;

internal interface IPlayerHolder
{
    Result<Player, Error> AddPlayer(PlayerInfo playerInfo);
    Player? FindPlayerByHost(IPEndPoint endpoint);
    IEnumerable<Player> GetPlayers();
}
