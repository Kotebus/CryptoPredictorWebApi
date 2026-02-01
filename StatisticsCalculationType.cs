
using System.Text.Json.Serialization;

namespace CryptoPredictorWebApi
{
    public partial class Program
    {
        //We need to add JsonStringEnumConverter to be able to see enum values as strings in Swagger UI
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum StatisticsCalculationType
        {
            Oldest,
            Newest,
            Min,
            Max,
        }
    }
}
