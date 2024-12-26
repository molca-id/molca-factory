using System;
using UnityEngine;

namespace Molca
{
    public class LogHandler : ILogHandler, IDisposable
    {
        private LogManager logManager;

        private ILogHandler _defaultLogHandler = Debug.unityLogger.logHandler;

        public LogHandler(LogManager manager)
        {
            logManager = manager;
            Debug.unityLogger.logHandler = this;
        }

        public void Dispose()
        {
            Debug.unityLogger.logHandler = _defaultLogHandler;
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            if (!logManager.IsActive)
                return;

            _defaultLogHandler.LogException(exception, context);
            logManager.onLogError?.Invoke($"Exception: {exception.Message}");
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (!logManager.IsActive)
                return;

            _defaultLogHandler.LogFormat(logType, context, format, args);
            string message = string.Format(format, args);
            logManager._logMessages.Add($"[{DateTime.Now.ToLongTimeString()}] {message}");

            switch (logType)
            {
                case LogType.Error:
                    logManager.onLogError?.Invoke(message);
                    break;
                case LogType.Warning:
                    logManager.onLogWarning?.Invoke(message);
                    break;
                case LogType.Log:
                    logManager.onLogInfo?.Invoke(message);
                    break;
                default:
                    logManager.onLogError?.Invoke(message);
                    break;
            }
        }
    }
}