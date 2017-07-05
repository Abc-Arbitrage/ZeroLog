namespace ZeroLog.Appenders
{
    public interface IAppender<in TAppenderParameters> : IAppender
    {
        void Configure(TAppenderParameters parameters);
    }
}
