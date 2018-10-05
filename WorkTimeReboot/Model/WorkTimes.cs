using System;

namespace WorkTimeReboot.Model
{
	public class WorkTimes
	{
		public TimeSpan Balance { get; set; }
		public DailyWork[] DailyWorks { get; set; }
	}
}
