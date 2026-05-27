using Microsoft.Extensions.Logging;

namespace Lockstep.Util;

internal static class Logger
{
    private static ILogger _logger = null!;
    private static readonly Lock _lock = new Lock();

    public static ILogger Get()
    {
        if (_logger == null)
        {
            lock (_lock)
            {
                if (_logger == null)
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

                    _logger = loggerFactory.CreateLogger("Server");
                }
            }
        }

        return _logger;
    }
}
