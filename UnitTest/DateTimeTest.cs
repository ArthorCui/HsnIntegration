using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class DateTimeTest
    {
        internal Timer timer = new Timer();
        internal Timer2 timer2 = new Timer2();

        [Test]
        public void DateTimeConvertTest()
        {
            timer.Start();
            timer2.Start();
            var str = "000000200154";
            var substr = str.Substring(0, 11);
            timer.End();
            timer2.End();
            Console.WriteLine(timer.Interval.TotalMilliseconds);
            Console.WriteLine(timer2.Interval);
        }
    }

    public class Timer
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Interval { get; set; }

        public void Start()
        {
            this.StartTime = DateTime.Now;
        }

        public void End()
        {
            this.EndTime = DateTime.Now;
            this.Interval = EndTime - StartTime;
        }
    }

    public class Timer2
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Interval { get; set; }

        public void Start()
        {
            this.StartTime = DateTime.Now;
        }

        public void End()
        {
            this.EndTime = DateTime.Now;
            var ts1 = new TimeSpan(StartTime.Ticks);
            var ts2 = new TimeSpan(EndTime.Ticks);
            var ts = ts1.Subtract(ts2).Duration();
            this.Interval = ts.TotalMilliseconds;
        }
    }
}
