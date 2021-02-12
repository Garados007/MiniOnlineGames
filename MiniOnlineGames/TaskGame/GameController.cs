using System;
using System.Collections.Concurrent;

namespace MiniOnlineGames.TaskGame
{
    public static class GameController
    {
        private static readonly ConcurrentDictionary<string, GameRoom> rooms
            = new ConcurrentDictionary<string, GameRoom>();

        public static (string key, GameRoom room) New()
        {
            var rng = new Random();
            var buffer = new byte[8];
            var room = new GameRoom();
            while (true)
            {
                rng.NextBytes(buffer);
                var key = Convert.ToBase64String(buffer).Replace('/', '-').Replace('+', '_');
                if (rooms.TryAdd(key, room))
                    return (key, room);
            }
        }

        public static GameRoom? Get(string key)
        {
            if (rooms.TryGetValue(key, out GameRoom? room))
                return room;
            else return null;
        }
    }
}
