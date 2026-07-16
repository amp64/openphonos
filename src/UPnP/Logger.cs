using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenPhonos.UPnP
{
    public class NetLogger
    {
        static public ILoggerFactory LoggerFactory;
        public const string LoggerName = "Network";

        static List<string> _Logging = new List<string>();
        static string _LikelyProblem;

        static private ILogger _Logger;
        static private ILogger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    if (LoggerFactory == null)
                    {
                        _Logger = NullLogger.Instance;
                    }
                    else
                    {
                        _Logger = LoggerFactory.CreateLogger(LoggerName);
                    }
                }
                return _Logger;
            }
        }

        public static string LikelyProblem
        {
            get
            {
                return _LikelyProblem;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != _LikelyProblem)
                {
                    WriteLine("Possible problem: {0}", value);
                }
                _LikelyProblem = value;
            }
        }

        public static bool PerfMarkers { get; set; }

        public enum LogType
        {
            LogAll = 1,
            LogVerbose = 2,
        }

        public static void WriteLine(string msg, params object[] args)
        {
            var when = DateTime.Now.TimeOfDay.ToString("t") + " " + msg;
            string item = string.Format(when, args);
            lock (_Logging)
            {
                _Logging.Add(item);
            }
            Logger.LogInformation(msg, args);
        }

        public static void WriteLine(LogType log, string msg, params object[] args)
        {
            switch (log)
            {
                case LogType.LogAll:
                    WriteLine(msg, args);
                    break;
                case LogType.LogVerbose:
                    Logger.LogDebug(msg, args);
                    break;
            }
        }

        public static void WriteCRITICAL(string msg, params object[] args)
        {
            Logger.LogCritical(msg, args);
            Debug.Assert(false);
        }
    }
}
