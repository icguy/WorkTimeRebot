using System;

namespace WorkTimeReboot.Timer
{
	public class Timer : ITimer
	{
		private readonly System.Timers.Timer _timer;

		public Timer(double interval)
		{
			this.Interval = interval;
			_timer = new System.Timers.Timer(interval);
			_timer.Elapsed += (s, e) => Tick();
		}

		public double Interval
		{
			get { return _timer.Interval; }
			set { _timer.Interval = value; }
		}

		public event Action Tick;

		public void Start() => _timer.Start();
		public void Stop() => _timer.Stop();
	}
}
