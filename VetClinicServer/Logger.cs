using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace VetClinicServer
{
    internal class Logger
    {
        internal static bool LogToConsole = true;

        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        internal static void Log(string message, LogType logType = LogType.Info)
        {
            switch (logType)
            {
                case LogType.Debug: _logger.Debug(message); break;
                case LogType.Info: _logger.Info(message); break;
                case LogType.Warn: _logger.Warn(message); break;
                case LogType.Error: _logger.Error(message); break;
                case LogType.Fatal: _logger.Fatal(message); break;
            }
            if (LogToConsole)
                WriteToConsole(message);
        }

        private static void WriteToConsole(string message) => Console.WriteLine(message);
    }
    internal enum LogType
    {
        Debug = 0,
        Info,
        Warn,
        Error,
        Fatal
    }
}
