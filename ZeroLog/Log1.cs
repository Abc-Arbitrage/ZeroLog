
namespace ZeroLog
{
	partial class Log
	{
			
		public void FatalFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0>(string format, T0 arg0)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
						
			logEvent.Log();
        }
	
		public void FatalFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0, T1>(string format, T0 arg0, T1 arg1)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
						
			logEvent.Log();
        }
	
		public void FatalFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
						
			logEvent.Log();
        }
	
		public void FatalFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
						
			logEvent.Log();
        }
	
		public void FatalFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
						
			logEvent.Log();
        }
	
		public void FatalFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
						
			logEvent.Log();
        }
	
		public void FatalFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
						
			logEvent.Log();
        }
	
		public void FatalFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Fatal);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
			logEvent.AppendGenericInlined(arg7);
						
			logEvent.Log();
        }
	
		public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Error);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
			logEvent.AppendGenericInlined(arg7);
						
			logEvent.Log();
        }
	
		public void WarningFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Warning);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
			logEvent.AppendGenericInlined(arg7);
						
			logEvent.Log();
        }
	
		public void InfoFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Info);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
			logEvent.AppendGenericInlined(arg7);
						
			logEvent.Log();
        }
	
		public void DebugFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Debug);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
			logEvent.AppendGenericInlined(arg7);
						
			logEvent.Log();
        }
	
		public void VerboseFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Verbose);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
			logEvent.AppendGenericInlined(arg7);
						
			logEvent.Log();
        }
	
		public void FinestFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			var logEvent = GetLogEventFor(Level.Finest);
			logEvent.AppendFormat(format);
			logEvent.AppendGenericInlined(arg0);
			logEvent.AppendGenericInlined(arg1);
			logEvent.AppendGenericInlined(arg2);
			logEvent.AppendGenericInlined(arg3);
			logEvent.AppendGenericInlined(arg4);
			logEvent.AppendGenericInlined(arg5);
			logEvent.AppendGenericInlined(arg6);
			logEvent.AppendGenericInlined(arg7);
						
			logEvent.Log();
        }
		
	}
}