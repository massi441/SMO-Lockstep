using Lockstep.Server;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep;

class Program
{
    static async Task Main(string[] args)
    {
        int port = 5001;

        ILogger logger = LockstepLogger.Instance();

        try
        {
            UdpServer server = new UdpServer(port);
            CancellationTokenSource ctSource = new CancellationTokenSource();
            await server.RunAsync(ctSource.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while the server was running");
        }
    }
}
