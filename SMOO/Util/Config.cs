namespace Lockstep.Util;

internal class Config
{
    public const int ServerBufferSize= 1024;
    public const uint Magic = 0x534D4F4C; // "SMOL"
    public const byte Version = 1;
    public const byte MaxRetries = 3;
    public const byte MaxPlayerNameLength = 30;
    public const byte MaxRoomSize = 5;
    public static readonly TimeSpan ResendTick = TimeSpan.FromMilliseconds(20);
    public static readonly TimeSpan MinimumResendSpan = TimeSpan.FromMilliseconds(200);
}
