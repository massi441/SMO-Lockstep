namespace Lockstep.Util;

internal class Config
{
    public static byte Version = 1;
    public static byte MaxRetries = 3;
    public static TimeSpan ResendTick = TimeSpan.FromMilliseconds(20);
    public static TimeSpan MinimumResendSpan = TimeSpan.FromMilliseconds(200);
}
