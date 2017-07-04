using Jil;

namespace ZeroLog.Appenders.Builders
{
    public static class JSONExtensions
    {
        public static T DeserializeOrDefault<T>(string json, T @default) => string.IsNullOrEmpty(json) ? @default : JSON.Deserialize<T>(json);
    }
}