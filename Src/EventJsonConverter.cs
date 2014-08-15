using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kaazing.Robot.Control.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kaazing.Robot.Control
{
    public class EventJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(CommandEvent).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject eventRawObject = JObject.Load(reader);
            string kind = eventRawObject["kind"].Value<string>();
            if (String.IsNullOrEmpty(kind))
            {
                // TODO: throw exception
            }
            eventRawObject.Remove("kind");
            try
            {
                CommandEvent.Kind eventKind = (CommandEvent.Kind)Enum.Parse(typeof(CommandEvent.Kind), kind, true);

                switch (eventKind)
                {
                    case CommandEvent.Kind.PREPARED:
                        return eventRawObject.ToObject<PreparedEvent>();
                    case CommandEvent.Kind.STARTED:
                        return eventRawObject.ToObject<StartedEvent>();
                    case CommandEvent.Kind.FINISHED:
                        return eventRawObject.ToObject<FinishedEvent>();
                    case CommandEvent.Kind.ERROR:
                        return eventRawObject.ToObject<ErrorEvent>();
                    default:
                        throw new InvalidOperationException(String.Format("Invalid event kind: {0}", kind));
                }
            }
            catch (ArgumentException argumentException)
            {
                throw new InvalidOperationException(String.Format("Invalid event kind: {0}", kind), argumentException);
            }
            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
