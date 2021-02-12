using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniOnlineGames.TaskGame.Events
{
    public abstract class EventBase
    {
        public virtual string TypeName => GetType().Name;

        public void WriteJson(Utf8JsonWriter writer)
        {
            writer.WriteString("$type", TypeName);
            WriteJsonContent(writer);
        }

        protected abstract void WriteJsonContent(Utf8JsonWriter writer);

        public abstract void ReadJsonContent(JsonElement json);
    }
}
