using System.Collections.Concurrent;
using SMOO.Client;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IReliablePacketStore
{
    public ConcurrentDictionary<ushort, ReliablePacket> PendingPackets { get; }

    public Result<Error> UploadPacket(RentedBuffer rentedBuffer, RefCounter refCounter, Player receiver, byte maxRetries = Config.MaxRetries);

    /// <summary>
    /// Removes a reliable packet, and returns its rented buffer to the array pool.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the packet to remove</param>
    /// <returns>The removed packed</returns>
    public ReliablePacket? RemovePacket(Player requester, ushort sequenceNumber);
}
