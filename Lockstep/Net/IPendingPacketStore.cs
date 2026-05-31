using System.Collections.Concurrent;
using Lockstep.Protocol;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPendingPacketStore
{
    public ConcurrentDictionary<ushort, PendingPacket> PendingPackets { get; }
    public Result<Error> UploadPacket(in PendingPacketRequest request);
    public PendingPacket? RemovePacket(ushort sequenceNumber);
}
