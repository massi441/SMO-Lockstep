using Microsoft.Extensions.Logging;
using SMOO.Services.Interface;

namespace SMOO.Server;

internal class ServerContext
{
    /// <summary>
    /// The logger used across the server
    /// </summary>
    public required ILogger Logger { get; init; }

    /// <summary>
    /// The room holder used across the server
    /// </summary>
    public required IRoomHolder RoomHolder { get; init; }

    /// <summary>
    /// The packet sender used across the server
    /// </summary>
    public required IPacketSender PacketSender { get; init; }

    /// <summary>
    /// The player disconnector used across the server
    /// </summary>
    public required IPlayerDisconnector PlayerDisconnector { get; init; }

    /// <summary>
    /// The packet handler provider used across the server
    /// </summary>
    public required IPacketHandlerProvider PacketHandlerProvider { get; init; }

    /// <summary>
    /// The cancellation used to signal a server shutdown
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }
}
