using System;
using MaxLib.WebServer;
using MaxLib.WebServer.Services;
using MaxLib.WebServer.WebSocket;

namespace MiniOnlineGames
{
    class Program
    {
        static void Main()
        {
            var server = new Server(new WebServerSettings(8002, 5000));
            server.AddWebService(new HttpRequestParser());
            server.AddWebService(new HttpHeaderSpecialAction());
            server.AddWebService(new Http404Service());
            server.AddWebService(new HttpResponseCreator());
            server.AddWebService(new HttpSender());
            server.AddWebService(new TaskGame.SpecialFileService());

            var searcher = new HttpDocumentFinder();
            if (System.Diagnostics.Debugger.IsAttached)
                searcher.Add(new HttpDocumentFinder.Rule("/content/", "../../../content/", false, true));
            else searcher.Add(new HttpDocumentFinder.Rule("/content/", "content/", false, true));
            server.AddWebService(searcher);
            server.AddWebService(new HttpDirectoryMapper(false));

            using var webSocket = new WebSocketService();
            webSocket.Add(new TaskGame.GameEndpoint());
            server.AddWebService(webSocket);

            server.Start();

            while (Console.ReadKey().Key != ConsoleKey.Q) ;

            server.Stop();
        }
    }
}
