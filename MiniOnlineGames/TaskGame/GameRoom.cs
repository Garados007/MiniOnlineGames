using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiniOnlineGames.TaskGame
{
    public class GameRoom : IDisposable
    {
        public DateTime Created { get; } = DateTime.UtcNow;

        private readonly List<GameConnection> connections = new List<GameConnection>();
        private readonly ReaderWriterLockSlim lockConnections = new ReaderWriterLockSlim();

        private readonly ConcurrentBag<string> Cards = new ConcurrentBag<string>();

        public void AddConnection(GameConnection connection)
        {
            lockConnections.EnterWriteLock();
            connections.Add(connection);
            lockConnections.ExitWriteLock();
            _ = SendRoomInfo();
        }

        public void RemoveConnection(GameConnection connection)
        {
            lockConnections.EnterWriteLock();
            connections.Remove(connection);
            lockConnections.ExitWriteLock();
            _ = SendRoomInfo();
        }

        public void Dispose()
        {
            lockConnections.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task SendRoomInfo()
        {
            var player = new List<string>();
            lockConnections.EnterReadLock();
            foreach (var connection in connections)
                player.Add(connection.PlayerName);
            lockConnections.ExitReadLock();
            var gameInfo = new Events.SendGameInfo
            {
                CardCount = Cards.Count,
                Player = player.ToArray()
            };
            await SendEvent(gameInfo);
        }

        public async Task SendEvent(Events.EventBase @event)
        {
            using var m = new System.IO.MemoryStream();
            using var writer = new Utf8JsonWriter(m);
            @event.WriteJson(writer);
            await writer.FlushAsync();

            var frame = new MaxLib.WebServer.WebSocket.Frame()
            {
                OpCode = MaxLib.WebServer.WebSocket.OpCode.Text,
                Payload = m.ToArray(),
                FinalFrame = true,
            };

            var sendQueue = new List<Task>(connections.Count);
            lockConnections.EnterReadLock();
            foreach (var connection in connections)
                sendQueue.Add(Task.Run(async () => await connection.SendFrame(frame)));
            lockConnections.ExitReadLock();

            await Task.WhenAll(sendQueue);
        }

        public void AddCard(string message)
        {
            Cards.Add(message);
        }

        int rollstate = 0;
        public async Task StartRoll(GameConnection connection)
        {
            if (Interlocked.Exchange(ref rollstate, 1) != 0)
                return;
            await SendEvent(new Events.StartRollDice());
            var rng = new Random();

            await Task.Delay(TimeSpan.FromMilliseconds(rng.Next(2500, 6000)));

            var number = rng.Next(6) + 1;

            await SendEvent(new Events.SendDiceResult { Result = number });

            if (number == 1 || number == 6)
            {

                var pickCount = rng.Next(Cards.Count >> 1);
                var stack = new Stack<string>(pickCount);
                for (int i = 0; i < pickCount; ++i)
                    if (Cards.TryTake(out string? card))
                        stack.Push(card);
                if (!Cards.TryTake(out string? result))
                    result = null;
                while (stack.Count > 0)
                    Cards.Add(stack.Pop());

                if (result != null)
                {
                    await SendEvent(new Events.SendCard
                    {
                        Initiator = connection.PlayerName,
                        Message = result
                    });
                    await SendRoomInfo();
                }
            }

            Interlocked.Exchange(ref rollstate, 0);
        }
    }
}
