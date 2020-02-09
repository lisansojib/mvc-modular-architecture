using System;
using System.IO;
using log4net;
using log4net.Core;
using log4net.Config;

namespace MvcModules.Utils
{
	/// <summary>
	/// Provides functionality for loggers.
	/// </summary>
    public class Logger
    {
        private static readonly Level MessageLevel = new Level(35000, "MESSAGE");

		/// <summary>
		/// Initializes a new instance of the Logger class.
		/// </summary>
        static Logger()
        {
            LogManager.GetRepository().LevelMap.Add(MessageLevel);

            var config = new FileInfo(FileUtils.GetFullPath("log.config"));

            if (config.Exists)
                XmlConfigurator.ConfigureAndWatch(config);
            else
                XmlConfigurator.Configure();
        }

		/// <summary>
		/// Initializes a new instance of the Logger class. Gets logger from LogerManager.
		/// </summary>
		/// <param name="name"></param>
        public Logger(string name)
        {
            _log = LogManager.GetLogger(name);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax.
		/// </summary>
		/// <param name="message">A message string.</param>
		/// <param name="args">Parameters for message.</param>
        public void Message(string message, params object[] args)
        {
            _log.Logger.Log(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                MessageLevel, String.Format(message, args), null
            );
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Debug (1 - highest) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
		public void Debug(string message, params object[] args)
		{
			_log.DebugFormat(message, args);
		}

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Info (2) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public void Info(string message, params object[] args)
        {
            _log.InfoFormat(message, args);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Warn (3) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public void Warn(string message, params object[] args)
        {
            _log.WarnFormat(message, args);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Error (4) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public void Error(string message, params object[] args)
        {
            _log.ErrorFormat(message, args);
        }

		/// <summary>
		/// Logs a message string with exception using the System.String.Format syntax with Error (4) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public void Error(string message, Exception e)
        {
            _log.Error(message, e);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Fatal (5 - lowest) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public void Fatal(string message, params object[] args)
        {
            _log.FatalFormat(message, args);
        }

		/// <summary>
		/// Logs a message string with exception using the System.String.Format syntax with Fatal (5 - lowest) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public void Fatal(string message, Exception e)
        {
            _log.Fatal(message, e);
        }

		/// <summary>
		/// Flushes the currently buffered events.
		/// </summary>
        public void Flush()
        {
            log4net.Repository.ILoggerRepository repo = _log.Logger.Repository;

            if (repo != null)
            {
                foreach (log4net.Appender.IAppender appender in repo.GetAppenders())
                {
                    var buffered = appender as log4net.Appender.BufferingAppenderSkeleton;

                    if (buffered != null)
                        buffered.Flush();
                }
            }
        }

        private readonly ILog _log;
    }

    public static class Log
	{
		/// <summary>
		/// Logs a message string using the System.String.Format syntax.
		/// </summary>
		/// <param name="message">A message string.</param>
		/// <param name="args">Parameters for message.</param>
        public static void Message(string message, params object[] args)
        {
            Default.Message(message, args);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Debug (1 - highest) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
		public static void Debug(string message, params object[] args)
		{
			Default.Debug(message, args);
		}

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Info (2) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public static void Info(string message, params object[] args)
        {
            Default.Info(message, args);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Warn (3) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public static void Warn(string message, params object[] args)
        {
            Default.Warn(message, args);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Error (4) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public static void Error(string message, params object[] args)
        {
            Default.Error(message, args);
        }

		/// <summary>
		/// Logs a message string with exception using the System.String.Format syntax with Error (4) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public static void Error(string message, Exception e)
        {
            Default.Error(message, e);
        }

		/// <summary>
		/// Logs a message string using the System.String.Format syntax with Fatal (5) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public static void Fatal(string message, params object[] args)
        {
            Default.Fatal(message, args);
        }

		/// <summary>
		/// Logs a message string with exception using the System.String.Format syntax with Error (5) priority.
		/// </summary>
		/// <param name="message">A message to log.</param>
		/// <param name="args">Parameters for message.</param>
        public static void Fatal(string message, Exception e)
        {
            Default.Fatal(message, e);
        }

		/// <summary>
		/// Flushes the currently buffered events.
		/// </summary>
        public static void Flush()
        {
            Default.Flush();
        }

		/// <summary>
		/// Gets the default logger if it is null.
		/// </summary>
        private static Logger Default
        {
            get
            {
                //TODO: make it thread safe
                if (_logger == null)
                    _logger = new Logger("Default");

                return _logger;
            }
        }

        private static Logger _logger;
    }
}
