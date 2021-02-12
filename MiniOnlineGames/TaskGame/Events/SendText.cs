using System;
using System.Text.Json;

namespace MiniOnlineGames.TaskGame.Events
{
    public class SendText : EventBase
    {
        public string Message { get; set; } = "";

        public override void ReadJsonContent(JsonElement json)
        {
            Message = json.GetProperty("message").GetString() ?? throw new NotSupportedException();
        }

        protected override void WriteJsonContent(Utf8JsonWriter writer)
        {
            writer.WriteString("message", Message);
        }
    }
}
