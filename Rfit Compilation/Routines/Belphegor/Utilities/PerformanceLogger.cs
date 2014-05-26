using System;
using System.Diagnostics;
using log4net;
using Zeta.Common;

namespace Belphegor.Utilities
{
    public class PerformanceLogger : IDisposable
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private readonly string _blockName;
        private readonly bool _isEnabled;
        private readonly Stopwatch _stopwatch;
        private bool _isDisposed;

        public PerformanceLogger(bool isEnabled, string blockName)
        {
            _isEnabled = isEnabled;
            _blockName = blockName;
            if (!_isEnabled) return;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            if (_isEnabled)
            {
                _stopwatch.Stop();
                Log.DebugFormat("Execution of the block {0} took {1}ms.", _blockName,
                    _stopwatch.ElapsedMilliseconds);
            }
            GC.SuppressFinalize(this);
        }

        #endregion

        ~PerformanceLogger()
        {
            Dispose();
        }
    }
}