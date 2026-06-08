using System.Runtime.InteropServices;

namespace SMOO.Client;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct ClientInfo
{
    public required Guid SessionId { get; init; }
    public required byte RoomSize { get; init; }
    public required byte ActivePlayerCount { get; init; }
}
