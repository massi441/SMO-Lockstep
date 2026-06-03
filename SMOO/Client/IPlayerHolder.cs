using System.Net;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Client;

internal interface IPlayerHolder
{
    Result<Player, Error> RegisterPlayer(PlayerInfo playerInfo);
    Result<Error> UnregisterPlayer(Player player);
    Player? FindPlayerByHost(IPEndPoint endpoint);
    IEnumerable<Player> Players { get; }
    byte PlayerCount { get; }
    byte OtherPlayerCount => (byte)(PlayerCount - 1);
}
