using System;
using System.Text.Json;

namespace MiniOnlineGames.TaskGame.Events
{
    public class SendCard : EventBase
    {
        public string Initiator { get; set; } = "";

        public string Message { get; set; } = "";

        public override void ReadJsonContent(JsonElement json)
        {
            Initiator = json.GetProperty("initiator").GetString() ?? throw new NotSupportedException();
            Message = json.GetProperty("message").GetString() ?? throw new NotSupportedException();
        }

        protected override void WriteJsonContent(Utf8JsonWriter writer)
        {
            writer.WriteString("initiator", Initiator);
            writer.WriteString("message", Message);
        }
    }
}
