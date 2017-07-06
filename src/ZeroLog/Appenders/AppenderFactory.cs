using System;
using Jil;
using ZeroLog.Config;

namespace ZeroLog.Appenders.Builders
{
    public class AppenderFactory
    {
        public static IAppender BuildAppender(AppenderDefinition definition)
        {
            var appenderType = Type.GetType(definition.AppenderTypeName);

            var appenderParameterType = GetAppenderParameterType(appenderType);

            var appenderParameters = GetAppenderParameters(definition, appenderParameterType);

            var appender = Activator.CreateInstance(appenderType);

            var configureMethod = appenderType.GetMethod(nameof(IAppender<object>.Configure), new[] { appenderParameterType });
            configureMethod.Invoke(appender, new[]{appenderParameters});

            return (IAppender)appender;
        }

        private static object GetAppenderParameters(AppenderDefinition definition, Type appenderParameterType)
        {
            var appenderParameterJson = JSON.SerializeDynamic(definition.AppenderJsonConfig);
            var appenderParameters = (object)JSON.Deserialize(appenderParameterJson, appenderParameterType);
            return appenderParameters;
        }

        private static Type GetAppenderParameterType(Type appenderType)
        {
            var type = appenderType;

            var implementedInterfaceTypes = type.GetInterfaces();

            foreach (var interfaceType in implementedInterfaceTypes)
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IAppender<>))
                    return interfaceType.GetGenericArguments()[0];
            }

            throw new InvalidOperationException();
        }
    }
}
