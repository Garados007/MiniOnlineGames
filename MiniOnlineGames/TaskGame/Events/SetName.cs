using System;
using System.Text.Json;

namespace MiniOnlineGames.TaskGame.Events
{
    public class SetName : EventBase
    {
        public string Name { get; set; } = "";

        public override void ReadJsonContent(JsonElement json)
        {
            Name = json.GetProperty("name").GetString() ?? throw new NotSupportedException();
        }

        protected override void WriteJsonContent(Utf8JsonWriter writer)
        {
            writer.WriteString("name", Name);
        }
    }
}
