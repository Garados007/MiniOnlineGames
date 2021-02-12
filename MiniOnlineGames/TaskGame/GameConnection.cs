using MaxLib.WebServer.WebSocket;
using MiniOnlineGames.TaskGame.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MiniOnlineGames.TaskGame
{
    public class GameConnection : WebSocketConnection
    {
        public string Key { get; }

        public GameRoom Room { get; }

        public string PlayerName { get; private set; } = "";

        public GameConnection(Stream networkStream, string key, GameRoom room) : base(networkStream)
        {
            (Key, Room) = (key, room);
            Closed += GameConnection_Closed;
            Room.AddConnection(this);
        }

        private void GameConnection_Closed(object? sender, EventArgs e)
        {
            Room.RemoveConnection(this);
        }

        protected override async Task ReceiveClose(CloseReason? reason, string? info)
        {
            await Task.CompletedTask;
        }

        protected override async Task ReceivedFrame(Frame frame)
        {
            var @event = EventRegistry.Parse(frame.Payload);
            if (@event != null)
                await ReceivedFrame(@event);
        }

        protected virtual async Task ReceivedFrame(EventBase @event)
        {
            switch (@event)
            {
                case SendText sendText:
                    Room.AddCard(sendText.Message);
                    _ = Room.SendRoomInfo();
                    break;
                case SetName setName:
                    PlayerName = setName.Name;
                    _ = Room.SendRoomInfo();
                    break;
                case StartRollDice:
                    _ = Room.StartRoll(this);
                    break;
            }
            await Task.CompletedTask;
        }

        public new async Task SendFrame(Frame frame)
            => await base.SendFrame(frame);
    }
}
