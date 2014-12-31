using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Utilities.TimeRecord
{
    public class ElapsedTimeRecorder<T>
    {
        public Func<T> Function { get; set; }

        public ElapsedTimeRecorder(Func<T> func)
        {
            this.Function = func;
        }
        public T Execute()
        {
            var sw = Stopwatch.StartNew();
            var obj = this.Function();
            Console.WriteLine(sw.ElapsedMilliseconds);
            return obj;
        }

    }
}
