using Newtonsoft.Json;
using Spider.Steps.Abstractions;
using System.Collections.Generic;

namespace Spider.Steps.Helpers
{
    public class StepsDeserializer
    {
        public static IList<IStep> Deserialize(string serializedJson)
        {
            return JsonConvert.DeserializeObject<IList<IStep>>(serializedJson, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new StepSerializationBinder()
            });
        }
    }
}