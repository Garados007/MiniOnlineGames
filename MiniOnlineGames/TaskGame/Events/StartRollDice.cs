using System.Text.Json;

namespace MiniOnlineGames.TaskGame.Events
{
    public class StartRollDice : EventBase
    {

        public override void ReadJsonContent(JsonElement json)
        {
        }

        protected override void WriteJsonContent(Utf8JsonWriter writer)
        {
        }
    }
}
