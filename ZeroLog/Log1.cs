using JetBrains.Annotations;

namespace ZeroLog
{
	partial class Log
	{
				
		public LogEvent Fatal()
        {
            return GetLogEventFor(Level.Fatal);
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
					
		public LogEvent Error()
        {
            return GetLogEventFor(Level.Error);
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
					
		public LogEvent Warn()
        {
            return GetLogEventFor(Level.Warn);
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
					
		public LogEvent Info()
        {
            return GetLogEventFor(Level.Info);
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
					
		public LogEvent Debug()
        {
            return GetLogEventFor(Level.Debug);
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
					
		public LogEvent Verbose()
        {
            return GetLogEventFor(Level.Verbose);
        }
			
		[StringFormatMethod("format")]	
		public void VerboseFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void VerboseFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void VerboseFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.AppendGeneric(arg2);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void VerboseFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.AppendGeneric(arg2);
			logEvent.AppendGeneric(arg3);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void VerboseFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.AppendGeneric(arg2);
			logEvent.AppendGeneric(arg3);
			logEvent.AppendGeneric(arg4);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void VerboseFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
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
		public void VerboseFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
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
		public void VerboseFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
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
					
		public LogEvent Finest()
        {
            return GetLogEventFor(Level.Finest);
        }
			
		[StringFormatMethod("format")]	
		public void FinestFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void FinestFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void FinestFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.AppendGeneric(arg2);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void FinestFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.AppendGeneric(arg2);
			logEvent.AppendGeneric(arg3);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void FinestFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGeneric(arg0);
			logEvent.AppendGeneric(arg1);
			logEvent.AppendGeneric(arg2);
			logEvent.AppendGeneric(arg3);
			logEvent.AppendGeneric(arg4);
			logEvent.Log();
        }
			
		[StringFormatMethod("format")]	
		public void FinestFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Finest);
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
		public void FinestFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Finest);
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
		public void FinestFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Finest);
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