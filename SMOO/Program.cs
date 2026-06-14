using SMOO.Server;
using SMOO.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep;

class Program
{
    static async Task Main(string[] args)
    {
        //int port = 5001;
        ILogger logger = LockstepLogger.Instance();

        try
        {
            var config = ServerConfig.Load("./Server/config.json"); 
            UdpServer server = new UdpServer(config.Port);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                cancellationTokenSource.Cancel();
            };

            await server.RunAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while the server was running");
        }
    }
}
