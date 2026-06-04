using System.Collections.Concurrent;
using SMOO.Protocol;
using SMOO.Util;

namespace SMOO.Services.Interface;

internal interface IReliablePacketStore
{
    public ConcurrentDictionary<ushort, ReliablePacket> PendingPackets { get; }

    /// <summary>
    /// Uploads a pending packet in the store, and writes the sequence number into the payload if the operation was successful.
    /// The payload request MUST have reserved room for a UInt16, where the sequence number is written
    /// </summary>
    /// <param name="uploadRequest">The packet and its metadata to upload.</param>
    /// <returns>Success, or <see cref="Error.PendingPacketStoreFull"/> if the store is at capacity.</returns>
    public Result<Error> UploadPacket(ReliablePacketRequest uploadRequest);


    /// <summary>
    /// Removes and returns the pending packet with the given sequence number, or null if not found.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the packet to remove.</param>
    /// <returns>The removed <see cref="ReliablePacket"/>, or null if no packet with that sequence number exists.</returns>
    public ReliablePacket? RemovePacket(ushort sequenceNumber);
}
