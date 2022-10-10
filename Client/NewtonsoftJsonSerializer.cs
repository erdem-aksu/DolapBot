using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serialization;

namespace DolapBot.Client
{
    public class NewtonsoftJsonSerializer : IRestSerializer
    {
        public static readonly DefaultContractResolver ContractResolver =
            new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

        public static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = ContractResolver,
            };

        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content, JsonSerializerSettings);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        }

        public string ContentType { get; set; } = RestSharp.Serialization.ContentType.Json;

        public string[] SupportedContentTypes { get; } =
            RestSharp.Serialization.ContentType.JsonAccept;

        public DataFormat DataFormat { get; } = DataFormat.Json;

        public string Serialize(Parameter parameter) => Serialize(parameter.Value);
    }
}