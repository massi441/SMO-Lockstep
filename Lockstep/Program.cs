using Lockstep.Net;
using Lockstep.Util;
using Microsoft.Extensions.Logging;

namespace Lockstep;

class Program
{
    static void Main(string[] args)
    {
        int port = 5001;

        ILogger logger = LockstepLogger.Instance();

        try
        {
            UdpServer server = new UdpServer(port);
            server.Run();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while the server was running");
        }
    }
}
