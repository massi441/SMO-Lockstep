using System.Net;

namespace Lockstep.Client;

internal class PlayerInfo
{
    public required IPEndPoint Endpoint { get; init; }
    public required string Name { get; init; }
}
