using System.Net;

namespace Lockstep.Client;

internal class PlayerClient
{
    public IPEndPoint Endpoint { get; set; }
    public string? Name { get; set; }

    public PlayerClient(IPEndPoint endpoint, string? name)
    {
        Endpoint = endpoint;
        Name = name;
    }
}
