using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lockstep;

class Program
{
    static async Task Main(string[] args)
    {
        ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
        Console.WriteLine($"Worker: {workerThreads}, IO: {completionPortThreads}");
        Console.WriteLine(Process.GetCurrentProcess().Threads.Count);

        int port = 5001;

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);

        socket.Bind(endpoint);
        Console.WriteLine($"Main thread: {Thread.CurrentThread.ManagedThreadId}");
        await Task.Run(() =>
        {
            while (true)
            {
                Console.WriteLine($"Task thread: {Thread.CurrentThread.ManagedThreadId}");
                byte[] buffer = new byte[1024];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderEndpoint = sender;

                Console.WriteLine($"Listening on port {port}...");

                int received = socket.ReceiveFrom(buffer, ref senderEndpoint);
                string message = Encoding.UTF8.GetString(buffer, 0, received);

                Console.WriteLine(message);
            }
        });
    }
}
