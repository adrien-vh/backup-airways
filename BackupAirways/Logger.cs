﻿using System;
using System.Diagnostics;
using Logger;

namespace BackupAirways
{
	public sealed class _logger : SimpleLogger
	{
		private static _logger instance = new _logger("backupAirway.log");
		public static _logger Instance { get { return instance;	} }
		
		private _logger(string fichier) : base(true, fichier) { }
	}
	
	static internal class Logger
    {
		public static void Log(string message, LogLevel ll = LogLevel.TRACE) {
			_logger.Instance.WriteFormattedLog(ll, message);
			Trace.TraceInformation(message.Replace("{","").Replace("}",""));
		}
    }
}
