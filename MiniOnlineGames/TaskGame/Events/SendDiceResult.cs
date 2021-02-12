using System.Text.Json;

namespace MiniOnlineGames.TaskGame.Events
{
    public class SendDiceResult : EventBase
    {
        public int Result { get; set; }

        public override void ReadJsonContent(JsonElement json)
        {
            Result = json.GetProperty("result").GetInt32();
        }

        protected override void WriteJsonContent(Utf8JsonWriter writer)
        {
            writer.WriteNumber("result", Result);
        }
    }
}
