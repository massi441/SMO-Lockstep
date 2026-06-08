using System.Collections.Concurrent;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IReliablePacketStore
{
    public ConcurrentDictionary<ushort, ReliablePacket> PendingPackets { get; }
    public Result<Error> UploadPacket(RentedBuffer rentedBuffer, Player receiver, byte maxRetries = Config.MaxRetries);
    public ReliablePacket? RemovePacket(ushort sequenceNumber);
}
