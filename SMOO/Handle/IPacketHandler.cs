using SMOO.Protocol;
using SMOO.Server;

namespace SMOO.Handle;

internal interface IPacketHandler
{
    /// <summary>
    /// Returns the minimum size in bytes that a packet payload needs to be in order to be processed by the handler
    /// </summary>
    uint MinPayloadSize { get; }

    /// <summary>
    /// Handles an incoming packet for the given room.
    /// The packet handler is responsible for managing the lifetime of the rented buffer.
    /// </summary>
    /// <param name="packet">The incoming packet to handle</param>
    /// <param name="room">The room the packet was for</param>
    void Handle(ParsedPacket packet, Room room);
}
