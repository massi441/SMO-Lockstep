using System.Net;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Client;

internal interface IPlayerHolder
{
    PlayerList Players { get; }
    byte MaxSize { get; }
    Result<Player, Error> RegisterPlayer(in PlayerInfo playerInfo);
    Result<Error> UnregisterPlayer(Player player);

    Player? FindPlayerByHost(IPEndPoint endpoint);
    Player? FindPlayerById(PlayerId id);
}
