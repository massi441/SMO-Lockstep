using System.Net;
using SMOO.Server;

namespace SMOO.Client;

internal class PlayerInfo
{
    public required IPEndPoint Endpoint { get; init; }
    public required string Name { get; init; }
    public required Room Room { get; init; }
}
