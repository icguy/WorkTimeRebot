using System;
using System.Linq;
using System.Text;

namespace WorkTimeReboot.Model
{
	public class DailyWork
	{
		public int HoursToWorkToday { get; set; } = 8;
		public TimeSpan Balance { get; set; }
		public WorkEvent[] Events { get; set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"{this.Events.FirstOrDefault()?.Time.Date.ToShortDateString()} | {this.Balance}");
			sb.AppendLine($"hours to work: {this.HoursToWorkToday}");
			foreach( var workEvent in this.Events )
			{
				sb.AppendLine($"  {workEvent.ToString()}");
			}
			return sb.ToString();
		}
	}
}
