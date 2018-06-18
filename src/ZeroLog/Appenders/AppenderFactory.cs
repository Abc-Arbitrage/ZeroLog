using System;
using System.Linq;
using Jil;
using ZeroLog.Config;

namespace ZeroLog.Appenders
{
    public class AppenderFactory
    {
        public static IAppender CreateAppender(AppenderDefinition definition)
        {
            var appenderType = GetAppenderType(definition);

            var appender = (IAppender)Activator.CreateInstance(appenderType);
            appender.Name = definition.Name;

            var appenderParameterType = GetAppenderParameterType(appenderType);
            if (appenderParameterType != null)
            {
                var appenderParameters = GetAppenderParameters(definition, appenderParameterType);

                var configureMethod = appenderType.GetMethod(nameof(IAppender<object>.Configure), new[] { appenderParameterType });
                configureMethod?.Invoke(appender, new[] { appenderParameters });
            }

            return appender;
        }

        private static Type GetAppenderType(AppenderDefinition definition)
        {
            var appenderType = AppDomain.CurrentDomain.GetAssemblies()
                                        .Select(x => x.GetType(definition.AppenderTypeName))
                                        .FirstOrDefault(x => x != null);

            return appenderType;
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

            return null;
        }
    }
}
