namespace ZeroLog.Appenders.Builders
{
    public class DateAndSizeRollingFileAppenderBuilder : IAppenderBuilder
    {
        public string TypeName 
            => nameof(DateAndSizeRollingFileAppender);

        public IAppender BuildAppender(string name, string config)
        {
            string filenameRoot = "todo";
            return new DateAndSizeRollingFileAppender(filenameRoot);
        } 

    }
}