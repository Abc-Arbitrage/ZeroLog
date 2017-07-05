using Jil;

namespace ZeroLog.Config
{
    public class AppenderDefinition
    {
        public string Name { get; set; }

        public string AppenderTypeName { get; set; }
        
        public dynamic AppenderJsonConfig { get; set; }
    }
}