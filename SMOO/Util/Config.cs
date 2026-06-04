namespace SMOO.Util;

internal class Config
{
    public const int ServerBufferSize= 1024;
    public const uint Magic = 0x534D4F4F; // "SMOO"
    public const byte Version = 1;
    public const byte MaxRetries = 5;
    public const byte MaxPlayerNameLength = 30;
    public const byte DefaultRoomSize = 4;
    public const byte MaxRoomSize = 10;
    public static readonly TimeSpan ResendTick = TimeSpan.FromMilliseconds(30);
    public static readonly TimeSpan MinimumResendSpan = TimeSpan.FromMilliseconds(350);
}
