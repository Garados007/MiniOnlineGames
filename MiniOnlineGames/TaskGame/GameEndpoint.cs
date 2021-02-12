using MaxLib.WebServer;
using MaxLib.WebServer.WebSocket;
using System.IO;

namespace MiniOnlineGames.TaskGame
{
    public class GameEndpoint : WebSocketEndpoint<GameConnection>
    {
        public override string? Protocol => null;

        protected override GameConnection? CreateConnection(Stream stream, HttpRequestHeader header)
        {
            if (header.Location.DocumentPathTiles.Length != 3)
                return null;
            if (header.Location.StartsUrlWith(new[] { "ws", "task-game" }))
                return null;
            var result = GameController.Get(
                header.Location.DocumentPathTiles[2]
            );
            if (result == null)
                return null;
            return new GameConnection(stream, header.Location.DocumentPathTiles[2], result);
        }
    }
}
