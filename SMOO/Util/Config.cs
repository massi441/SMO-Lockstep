namespace SMOO.Util;

internal class Config
{
    public const int ServerReceiveBufferSize = 2048;
    public const int DynamicBufferSize32 = 32;
    public const int DynamicBufferSize64 = 64;
    public const int DynamicBufferSize128 = 128;
    public const int DynamicBufferSize256 = 256;
    public const int DynamicBufferSize512 = 512;
    public const int DynamicBufferSize1024 = 1024;
    public const uint Magic = 0x534D4F4F; // "SMOO"
    public const byte Version = 1;
    public const byte MaxRetries = 5;
    public const byte MaxPlayerNameLength = 50;
    public const byte DefaultRoomSize = 4;
    public const byte MaxRoomSize = 10;
    public static readonly TimeSpan ResendThreadTick = TimeSpan.FromMilliseconds(1000);
    public static readonly TimeSpan MinimumResendDelay = TimeSpan.FromMilliseconds(400); // ideally store that on the packet itself, based on RTT
    public static readonly string DefaultCostumeName = "Mario";
}
