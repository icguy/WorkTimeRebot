using System;
using System.Diagnostics;
using System.Linq;
using WorkTimeReboot.Model;
using WorkTimeReboot.Services.UserIO;

namespace WorkTimeReboot.Utils
{
	public static class Extensions
	{
		public static void Print(this Status status, IUserIO userIO)
		{
			userIO.WriteLine();
			userIO.WriteLine("total:");
			userIO.WriteLine(status.Total);
			userIO.WriteLine("today:");
			userIO.WriteLine(status.TodayWork);
			userIO.WriteLine($"expected departure at: {status.ExpectedDeparture}");
		}

		public static void Print(this QuickStatus status, IUserIO userIO)
		{
			userIO.WriteLine();
			userIO.WriteLine("total:");
			userIO.WriteLine(status.Total);
		}

		public static WorkEvent ToWorkEvent(this EventLogEntry entry)
		{
			WorkEvent workEvent = new WorkEvent();
			workEvent.Time = entry.TimeGenerated;
			workEvent.Type = entry.GetEventType();
			return workEvent;
		}

		public static void ApplyModifiers(this WorkTimes workTimes, WorkModifiers modifiers)
		{
			foreach( var dw in workTimes.DailyWorks )
			{
				dw.Events = dw.Events.Where(e => !modifiers.IgnoredEventsModifiers.Any(ie => ie.Time == e.Time)).ToArray();

				var hoursMod = modifiers.HoursModifiers.FirstOrDefault(hm => hm.Date.Date == dw.Events.First().Time.Date);
				if( hoursMod != null )
				{
					dw.HoursToWorkToday = hoursMod.Hours;
				}
			}

			workTimes.Recalculate();
		}

		// todo test
		public static void Recalculate(this WorkTimes workTimes)
		{
			workTimes.Balance = TimeSpan.Zero;
			var q = workTimes.DailyWorks.GroupBy(w => w.Events.First().Time).Select(g => g.First());
			workTimes.DailyWorks = q.ToArray();

			foreach( var work in q )
			{
				work.Recalculate();
				workTimes.Balance += work.Balance;
			}
		}

		// todo test
		public static void Recalculate(this DailyWork dailyWork)
		{
			var hours = dailyWork.HoursToWorkToday;
			var work = WorkTimesUtils.CreateDailyWork(dailyWork.Events, hours);
			dailyWork.Balance = work.Balance;
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
