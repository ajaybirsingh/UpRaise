using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace UpRaise.Helpers
{
    public class WrappedLogger : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _memberName;

        private readonly LogLevel _executionLogLevel;
        private readonly DateTimeOffset _startDateTimeOffset;
        private bool disposed = false;

        public WrappedLogger(ILogger logger,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int line = 0,
            LogLevel executionLogLevel = LogLevel.Trace)
        {
            _executionLogLevel = executionLogLevel;
            _logger = logger;
            _startDateTimeOffset = DateTimeOffset.UtcNow;

            _memberName = memberName;
            _logger.Log(_executionLogLevel, $"{_memberName}::Entering [{System.IO.Path.GetFileName(filePath)}::{line}]");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                _logger.Log(_executionLogLevel, $"{_memberName}::Exiting (Duration {(DateTimeOffset.UtcNow - _startDateTimeOffset).TotalSeconds.ToString("F4")}s)");
            }
        }

        public void LogTrace(string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogTrace($"{_memberName}::{message}", args);
        }
        public void LogTrace(Exception exception, string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogTrace(exception, $"{_memberName}::{message}", args);
        }
        public void LogTrace(Exception exception, int? rowNumber = null)
        {
            _logger.LogTrace(exception, $"{_memberName}");
        }


        public void LogDebug(string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogDebug($"{_memberName}::{message}", args);
        }
        public void LogDebug(Exception exception, string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogDebug(exception, $"{_memberName}::{message}", args);
        }

        public void LogDebug(Exception exception, int? rowNumber = null)
        {
            _logger.LogDebug(exception, $"{_memberName}");
        }


        public void LogInformation(string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogInformation($"{_memberName}::{message}", args);
        }
        public void LogInformation(Exception exception, string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogInformation(exception, $"{_memberName}::{message}", args);
        }
        public void LogInformation(Exception exception, int? rowNumber = null)
        {
            _logger.LogInformation(exception, $"{_memberName}");
        }


        public void LogWarning(string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogWarning($"{_memberName}::{message}", args);
        }
        public void LogWarning(Exception exception, string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogWarning(exception, $"{_memberName}::{message}", args);
        }
        public void LogWarning(Exception exception, int? rowNumber = null)
        {
            _logger.LogWarning(exception, $"{_memberName}");
        }


        public void LogError(string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogError($"{_memberName}::{message}", args);
        }
        public void LogError(Exception exception, string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogError(exception, $"{_memberName}::{message} -- Exception ({exception.Message})", args);
        }
        public void LogError(Exception exception, int? rowNumber = null)
        {
            _logger.LogError(exception, $"{_memberName} -- Exception ({exception.Message})");
        }


        public void LogCritical(string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogCritical($"{_memberName}::{message}", args);
        }
        public void LogCritical(Exception exception, string message, int? rowNumber = null, params object[] args)
        {
            _logger.LogCritical(exception, $"{_memberName}::{message}", args);
        }
        public void LogCritical(Exception exception, int? rowNumber = null)
        {
            _logger.LogCritical(exception, $"{_memberName}");
        }
    }
}
