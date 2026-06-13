using System.Net;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Client;

internal interface IPlayerHolder
{
    Player[] Players { get; }
    byte MaxSize { get; }
    byte ActivePlayerCount { get; }

    Result<Player, Error> RegisterPlayer(in PlayerInfo playerInfo);
    Result<Error> UnregisterPlayer(Player player);

    Player[] InSameStageAs(Player targetPlayer);

    Player? FindPlayerByHost(IPEndPoint endpoint);
    Player? FindPlayerById(PlayerId id);
}
