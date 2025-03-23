using JsonSubTypes;
#if (NET461)
using Valve.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace SteamQueries.Models
{
    [JsonConverter(typeof(JsonSubtypes), "MessageType")]
    public interface IMessage
    {
        string MessageType { get; set; }
    }
}

