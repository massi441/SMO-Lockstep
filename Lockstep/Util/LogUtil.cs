using Microsoft.Extensions.Logging;

namespace Lockstep.Util;

internal static class LogUtil
{
    public static ILogger CreateLogger(string name)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });

            builder.SetMinimumLevel(LogLevel.Debug);
        });

        return loggerFactory.CreateLogger(name);
    }
}
