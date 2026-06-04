using System.Net;
using SMOO.Server;

namespace SMOO.Client;

internal readonly struct PlayerInfo
{
    public required IPEndPoint Endpoint { get; init; }
    public required string Name { get; init; }
    public required Room Room { get; init; }
}
