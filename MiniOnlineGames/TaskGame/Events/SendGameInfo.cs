using System;
using System.Text.Json;

namespace MiniOnlineGames.TaskGame.Events
{
    public class SendGameInfo : EventBase
    {
        public int CardCount { get; set; }

        public string[] Player { get; set; } = Array.Empty<string>();

        public override void ReadJsonContent(JsonElement json)
        {
            CardCount = json.GetProperty("card-count").GetInt32();
            var jsonPlayer = json.GetProperty("player");
            Player = new string[jsonPlayer.GetArrayLength()];
            for (int i = 0; i < Player.Length; ++i)
                Player[i] = jsonPlayer[i].GetString() ?? throw new NotSupportedException();
        }

        protected override void WriteJsonContent(Utf8JsonWriter writer)
        {
            writer.WriteNumber("card-count", CardCount);
            writer.WriteStartArray("player");
            foreach (var player in Player)
                writer.WriteStringValue(player);
            writer.WriteEndArray();
        }
    }
}
