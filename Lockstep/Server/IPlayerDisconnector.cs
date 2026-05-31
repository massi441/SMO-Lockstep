using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Server;

internal interface IPlayerDisconnector
{
    Result<Error> Disconnect(Player player);
}
