using Jil;

namespace ZeroLog.Utils
{
    public static class JsonExtensions
    {
        public static T DeserializeOrDefault<T>(string json, T @default) => string.IsNullOrEmpty(json) ? @default : JSON.Deserialize<T>(json);
    }
}