
using System.Net;
using SMOO.Server;

namespace SMOO.Client;

internal class Player
{
    public required PlayerId Id { get; init; }
    public required string Name { get; init; }
    public required Room Room { get; init; }
    public required byte Slot { get; init; }
    public DateTime LastSeen { get; private set; } = DateTime.UtcNow;

    public IPEndPoint Endpoint => Id.Endpoint;

    public void RefreshLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }
}
