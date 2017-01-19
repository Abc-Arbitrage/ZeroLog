
using System;
using JetBrains.Annotations;

namespace ZeroLog
{
    partial class Log
    {
                
        public bool IsDebugEnabled => Level.Debug >= _logManager.Level;

        public ILogEvent Debug()
        {
            return GetLogEventFor(Level.Debug);
        }
        
        public void Debug(string message)
        {
            GetLogEventFor(Level.Debug).Append(message).Log();
        }

        public void Debug(string message, Exception ex)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.Append(message);
            logEvent.Append(ex.ToString());
            logEvent.Log();
        }

                    
        [StringFormatMethod("format")]	
        public void DebugFormat<T0>(string format, T0 arg0)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void DebugFormat<T0, T1>(string format, T0 arg0, T1 arg1)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void DebugFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void DebugFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void DebugFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void DebugFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void DebugFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void DebugFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var logEvent = GetLogEventFor(Level.Debug);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.AppendGeneric(arg7);
            logEvent.Log();
        }
                    
        public bool IsInfoEnabled => Level.Info >= _logManager.Level;

        public ILogEvent Info()
        {
            return GetLogEventFor(Level.Info);
        }
        
        public void Info(string message)
        {
            GetLogEventFor(Level.Info).Append(message).Log();
        }

        public void Info(string message, Exception ex)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.Append(message);
            logEvent.Append(ex.ToString());
            logEvent.Log();
        }

                    
        [StringFormatMethod("format")]	
        public void InfoFormat<T0>(string format, T0 arg0)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void InfoFormat<T0, T1>(string format, T0 arg0, T1 arg1)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void InfoFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void InfoFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void InfoFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void InfoFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void InfoFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void InfoFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var logEvent = GetLogEventFor(Level.Info);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.AppendGeneric(arg7);
            logEvent.Log();
        }
                    
        public bool IsWarnEnabled => Level.Warn >= _logManager.Level;

        public ILogEvent Warn()
        {
            return GetLogEventFor(Level.Warn);
        }
        
        public void Warn(string message)
        {
            GetLogEventFor(Level.Warn).Append(message).Log();
        }

        public void Warn(string message, Exception ex)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.Append(message);
            logEvent.Append(ex.ToString());
            logEvent.Log();
        }

                    
        [StringFormatMethod("format")]	
        public void WarnFormat<T0>(string format, T0 arg0)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void WarnFormat<T0, T1>(string format, T0 arg0, T1 arg1)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void WarnFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void WarnFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void WarnFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void WarnFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void WarnFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void WarnFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var logEvent = GetLogEventFor(Level.Warn);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.AppendGeneric(arg7);
            logEvent.Log();
        }
                    
        public bool IsErrorEnabled => Level.Error >= _logManager.Level;

        public ILogEvent Error()
        {
            return GetLogEventFor(Level.Error);
        }
        
        public void Error(string message)
        {
            GetLogEventFor(Level.Error).Append(message).Log();
        }

        public void Error(string message, Exception ex)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.Append(message);
            logEvent.Append(ex.ToString());
            logEvent.Log();
        }

                    
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0>(string format, T0 arg0)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0, T1>(string format, T0 arg0, T1 arg1)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var logEvent = GetLogEventFor(Level.Error);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.AppendGeneric(arg7);
            logEvent.Log();
        }
                    
        public bool IsFatalEnabled => Level.Fatal >= _logManager.Level;

        public ILogEvent Fatal()
        {
            return GetLogEventFor(Level.Fatal);
        }
        
        public void Fatal(string message)
        {
            GetLogEventFor(Level.Fatal).Append(message).Log();
        }

        public void Fatal(string message, Exception ex)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.Append(message);
            logEvent.Append(ex.ToString());
            logEvent.Log();
        }

                    
        [StringFormatMethod("format")]	
        public void FatalFormat<T0>(string format, T0 arg0)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void FatalFormat<T0, T1>(string format, T0 arg0, T1 arg1)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void FatalFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void FatalFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void FatalFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void FatalFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void FatalFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.Log();
        }
            
        [StringFormatMethod("format")]	
        public void FatalFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var logEvent = GetLogEventFor(Level.Fatal);
            logEvent.AppendFormat(format);
            logEvent.AppendGeneric(arg0);
            logEvent.AppendGeneric(arg1);
            logEvent.AppendGeneric(arg2);
            logEvent.AppendGeneric(arg3);
            logEvent.AppendGeneric(arg4);
            logEvent.AppendGeneric(arg5);
            logEvent.AppendGeneric(arg6);
            logEvent.AppendGeneric(arg7);
            logEvent.Log();
        }
                    
    }
}