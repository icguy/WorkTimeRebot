using System;

namespace WorkTimeReboot.Services.Timer
{
	public interface ITimer
	{
		double Interval { get; set; }
		event Action Tick;
		void Start();
		void Stop();
	}
}
