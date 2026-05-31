using System.Collections.Concurrent;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPendingPacketStore
{
    public Result<Error> UploadPacket(in PendingPacketRequest request);
    public Result<Error> RemovePacket(ushort sequenceNumber);
    public ConcurrentDictionary<ushort, PendingPacket> PendingPackets { get; }
}
