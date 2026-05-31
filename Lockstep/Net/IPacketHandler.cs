using Lockstep.Protocol;
using Lockstep.Server;
using Lockstep.Util;

namespace Lockstep.Net;

internal interface IPacketHandler
{
    /// <summary>
    /// Returns the minimum size in bytes that a packet payload needs to be in order to be processed by the handler
    /// </summary>
    uint MinPayloadSize { get; }

    /// <summary>
    /// Handles an incoming packet for the given room.
    /// </summary>
    /// <param name="packet">The packet to handle.</param>
    /// <param name="room">The room the packet was routed to.</param>
    /// <returns>A result indicating either success, or the error that occurred during handling.</returns>
    Result<Error> Handle(Packet packet, Room room); // TODO: Remove Result as return type
}
