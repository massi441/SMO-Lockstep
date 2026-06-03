using Lockstep.Client;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Services;

internal interface IPlayerDisconnector
{
    Result<Error> Disconnect(Player player);
}
