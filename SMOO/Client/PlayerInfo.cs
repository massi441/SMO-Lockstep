using System.Net;
using Lockstep.Server;

namespace Lockstep.Client;

internal class PlayerInfo
{
    public required IPEndPoint Endpoint { get; init; }
    public required string Name { get; init; }
    public required Room Room { get; init; }
}
