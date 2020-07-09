using Newtonsoft.Json;

namespace ZeroLog.Utils
{
    public static class JsonExtensions
    {
        public static T DeserializeOrDefault<T>(string? json, T @default)
            => string.IsNullOrEmpty(json) ? @default : JsonConvert.DeserializeObject<T>(json!);
    }
}
