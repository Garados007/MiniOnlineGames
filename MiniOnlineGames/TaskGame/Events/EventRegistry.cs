using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MiniOnlineGames.TaskGame.Events
{
    public static class EventRegistry
    {
        static readonly Dictionary<string, Func<EventBase>> registry
            = new Dictionary<string, Func<EventBase>>();

        public static void Add<T>()
            where T : EventBase, new()
        {
            Add<T>(new T().TypeName);
        }

        public static void Add<T>(string key)
            where T : EventBase, new()
        {
            registry.Add(key, () => new T());
        }

        public static EventBase? Parse(ReadOnlyMemory<byte> jsonEvent)
        {
            try
            {
                var doc = JsonDocument.Parse(jsonEvent);
                var key = doc.RootElement.GetProperty("$type").GetString();
                if (key == null || !registry.TryGetValue(key, out Func<EventBase>? caller))
                    return null;
                var @event = caller();
                @event.ReadJsonContent(doc.RootElement);
                return @event;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        static EventRegistry()
        {
            Add<SendCard>();
            Add<SendDiceResult>();
            Add<SendGameInfo>();
            Add<SendText>();
            Add<SetName>();
            Add<StartRollDice>();
        }
    }
}
