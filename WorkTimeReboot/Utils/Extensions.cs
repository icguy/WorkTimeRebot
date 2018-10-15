using System;
using System.Diagnostics;
using System.Linq;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Utils
{
	public static class Extensions
	{
		public static void Print(this Status status)
		{
			Console.WriteLine("total:");
			Console.WriteLine(status.Total);
			Console.WriteLine();
			Console.WriteLine("today:");
			Console.WriteLine(status.TodayWork);
			Console.WriteLine($"expected departure at: {status.ExpectedDeparture}");
		}

		public static WorkEvent ToWorkEvent(this EventLogEntry entry)
		{
			WorkEvent workEvent = new WorkEvent();
			workEvent.Time = entry.TimeGenerated;
			workEvent.Type = entry.GetEventType();
			return workEvent;
		}

		private static EventType GetEventType(this EventLogEntry logEntry)
		{
			var id = logEntry.InstanceId;
			long[] loginIds = new[] { 4624L, 4648L, 4801L, };
			long[] logoutIds = new[] { 4634L, 4800L, 4647L, };
			if( loginIds.Contains(id) )
				return EventType.Arrival;
			if( logoutIds.Contains(id) )
				return EventType.Departure;
			return EventType.Unknown;
		}
	}
}
