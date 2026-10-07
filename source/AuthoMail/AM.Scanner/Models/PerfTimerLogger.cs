using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AM.Scanner.Models
{
    public class PerfTimerLogger : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public PerfTimerLogger(string message)
        {
            this._message = message;
            this._timer = new Stopwatch();
            this._timer.Start();
        }

        string _message;
        Stopwatch _timer;

        public void Dispose()
        {
            this._timer.Stop();
            var ms = this._timer.ElapsedMilliseconds;
             log.Info(
                 string.Format("{0} - Elapsed Milliseconds: {1}", this._message, ms)
             );
        }
    }
}