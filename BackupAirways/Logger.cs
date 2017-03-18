using System;
using System.Diagnostics;

namespace Saw
{
	static internal class Logger
    {
		public static void Log(string message, int type = 1)
		{
			Logger.Information(message);
		}
		
        public static void Information(Type type, string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public static void Information(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }
        
        public static void Warning(Type type, string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }
        
        public static void Warning(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }

        public static void Error(Type type, string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }
        
        
        public static void Error(string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }

        public static void Error(Type type, Exception exception)
        {
            Error(type, "{0}", exception);
        }
    }
}
