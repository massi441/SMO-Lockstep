namespace SMOO.Util;

internal class Config
{
    public const uint Magic = 0x534D4F4F; // "SMOO"
    public const byte Version = 1;
    public const byte MaxRetries = 5;
    public const byte MaxPlayerNameLength = 50;
    public const byte DefaultRoomSize = 4;
    public const byte MaxRoomSize = 10;
    public const ushort MaxChatMessageLength = 512;
    public const ushort MaxCostumeNameLength = 512;
    public const ushort MaxAnimNameLength = 64;
    public const ushort MaxBlendWeights = 6;
    public const ushort MaxBufferSize = 2048;
    public static readonly TimeSpan ResendThreadTick = TimeSpan.FromMilliseconds(1000);
    public static readonly TimeSpan MinimumResendDelay = TimeSpan.FromMilliseconds(400); // ideally store that on the packet itself, based on RTT
    public static readonly string DefaultCostumeName = "Mario";
}
