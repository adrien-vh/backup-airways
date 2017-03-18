/*
 * Created by SharpDevelop.
 * User: vanhyftea
 * Date: 03/03/2017
 * Time: 14:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;

namespace WebServer
{
	static internal class Logger
    {
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
