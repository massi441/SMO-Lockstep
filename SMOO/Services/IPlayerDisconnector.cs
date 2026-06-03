using SMOO.Client;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services;

internal interface IPlayerDisconnector
{
    Result<Error> Disconnect(Player player);
}
