using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Clifton.WebServer
{
    public static class Server
    {
        private static HttpListener listener;
        private static SemaphoreSlim semaphore;
        private static readonly int maxSimultaneousConnections = 20;
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        public static void Start(string[] prefixes)
        {
            semaphore = new SemaphoreSlim(maxSimultaneousConnections, maxSimultaneousConnections);
            listener = InitializeListener(prefixes);
            listener.Start();
            Task.Run(() => RunServer(listener, cts.Token));
        }

        public static void Stop()
        {
            cts.Cancel();
            listener.Stop();
            listener.Close();
            semaphore.Dispose();
        }

        private static HttpListener InitializeListener(string[] prefixes)
        {
            HttpListener listener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }
            return listener;
        }

        private static async Task RunServer(HttpListener listener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await semaphore.WaitAsync(cancellationToken);
                _ = HandleConnectionAsync(listener, cancellationToken);
            }
        }

        private static async Task HandleConnectionAsync(HttpListener listener, CancellationToken cancellationToken)
        {
            try
            {
                HttpListenerContext context = await listener.GetContextAsync();
                semaphore.Release();

                string response = "Hello Browser!";
                byte[] encoded = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = encoded.Length;
                await context.Response.OutputStream.WriteAsync(encoded, 0, encoded.Length, cancellationToken);
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                Console.WriteLine($"Error handling connection: {ex.Message}");
            }
        }
    }
}