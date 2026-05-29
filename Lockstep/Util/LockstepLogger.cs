using Lockstep.Protocol;
using Microsoft.Extensions.Logging;

namespace Lockstep.Util;

internal static class LockstepLogger
{
    private static ILogger _logger = null!;
    private static ILoggerFactory _loggerFactory = null!;
    private static readonly Lock _lock = new Lock();

    public static ILogger Instance()
    {
        if (_logger == null)
        {
            lock (_lock)
            {
                if (_logger == null)
                {
                    _loggerFactory = LoggerFactory.Create(builder =>
                    {
                        builder.AddSimpleConsole(options =>
                        {
                            options.SingleLine = true;
                            options.TimestampFormat = "HH:mm:ss ";
                        });

                        builder.SetMinimumLevel(LogLevel.Trace);
                    });

                    _logger = _loggerFactory.CreateLogger("Server");
                }
            }
        }

        return _logger;
    }
}
