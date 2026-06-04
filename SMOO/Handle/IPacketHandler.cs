using SMOO.Client;
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
    /// </summary>
    /// <param name="packet">The incoming packet to handle</param>
    /// <param name="room">The room the packet was for</param>
    /// <param name="player">The player that sent the packet</param>
    void Handle(Packet packet, Room room, Player? player);
}
