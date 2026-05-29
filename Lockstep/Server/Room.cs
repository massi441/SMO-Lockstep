using System.Threading.Channels;
using Lockstep.Client;
using Lockstep.Protocol;

namespace Lockstep.Server;

internal class Room
{
    private IClientHolder _clientHolder;

    public Channel<Payload> Payloads { get; }

    public Room(IClientHolder clientHolder)
    {
        _clientHolder = clientHolder;
        Payloads = Channel.CreateUnbounded<Payload>();
    }
}
