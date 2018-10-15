﻿using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Utils
{
	class WorkTimesUtils
	{
		public static WorkTimes CreateWorkTimes(IEnumerable<WorkEvent> events)
		{
			var workTimes = new WorkTimes()
			{
				Balance = TimeSpan.Zero,
				DailyWorks = new DailyWork[0]
			};
			var lastWorkTime = workTimes.DailyWorks.LastOrDefault()?.Events?.LastOrDefault()?.Time ?? DateTime.MinValue;
			var filteredEvents = events
				.OrderBy(e => e.Time)
				.ToList();

			var newDailyWorks = new List<DailyWork>();
			newDailyWorks.AddRange(workTimes.DailyWorks);

			while( filteredEvents.Count != 0 )
			{
				var firstEventTime = filteredEvents[0].Time;
				var currentDayEvents = filteredEvents.Where(e => (e.Time.Date == firstEventTime.Date)).ToList();
				var currentDailyWork = CreateDailyWork(currentDayEvents, 8);
				newDailyWorks.Add(currentDailyWork);
				workTimes.Balance += currentDailyWork.Balance;
				foreach( var e in currentDayEvents )
				{
					filteredEvents.Remove(e);
				}
			}
			workTimes.DailyWorks = newDailyWorks.ToArray();
			return workTimes;
		}


		public static DailyWork CreateDailyWork(IEnumerable<WorkEvent> events, int hoursToWorkToday)
		{
			var work = new DailyWork()
			{
				Balance = TimeSpan.Zero,
				Events = new WorkEvent[0],
				HoursToWorkToday = 8
			};
			var balance = TimeSpan.FromHours(-hoursToWorkToday);
			var filteredEvents = new List<WorkEvent>();

			DateTime? lastSignin = null;
			foreach( var e in events )
			{
				if( lastSignin == null && e.Type == EventType.Arrival )
				{
					lastSignin = e.Time;
					filteredEvents.Add(e);
				}
				else if( e.Type == EventType.Departure && lastSignin != null )
				{
					balance += (e.Time - lastSignin.Value);
					lastSignin = null;
					filteredEvents.Add(e);
				}
			}
			if( lastSignin != null )
			{
				filteredEvents.Remove(filteredEvents.Last());
			}

			return new DailyWork()
			{
				Balance = balance,
				Events = filteredEvents.ToArray()
			};
		}
	}
}
