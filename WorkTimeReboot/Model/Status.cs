using System;

namespace WorkTimeReboot.Model
{
	public class Status
	{
		public DateTime ExpectedDeparture { get; set; }
		public DailyWork TodayWork { get; set; }
		public TimeSpan Total { get; set; }
	}
}
