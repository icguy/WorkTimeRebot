using System.Collections.Generic;

namespace WorkTimeReboot.Model
{
	public class WorkModifiers
	{
		public IEnumerable<HoursToWorkModifier> HoursModifiers { get; set; }
		public IEnumerable<IgnoredEventModifier> IgnoredEventsModifiers { get; set; }

		public WorkModifiers()
		{
			this.HoursModifiers = new List<HoursToWorkModifier>();
			this.IgnoredEventsModifiers = new List<IgnoredEventModifier>();
		}
	}
}
