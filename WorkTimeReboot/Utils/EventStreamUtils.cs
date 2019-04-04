using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Utils
{
	static class EventStreamUtils
	{
		public static IEnumerable<WorkEvent> OrderAndRemoveDuplicate(IEnumerable<WorkEvent> events)
		{
			return events
				.OrderBy(e => e.Time)
				.GroupBy(e => new { e.Time, e.Type })
				.Select(e => e.First());
		}

		public static IEnumerable<WorkEvent> CleanUpStream(IEnumerable<WorkEvent> events, DateTime now)
		{
			var eventGroups = events.GroupBy(e => e.Time.Date);
			return eventGroups
				.Select(g => CleanUpGroup(g, now))
				.Aggregate(new List<WorkEvent>(), (agg, g) => { agg.AddRange(g); return agg; })
				.OrderBy(e => e.Time);
		}

		private static IEnumerable<WorkEvent> CleanUpGroup(IGrouping<DateTime, WorkEvent> group, DateTime now)
		{
			var eventList = group.ToList();
			if( eventList.Count == 0 )
				return eventList;

			eventList = eventList.OrderBy(e => e.Time).ToList();
			eventList = CleanUpRepeating(eventList).ToList();
			eventList = RemoveLeadingDepartures(eventList).ToList();
			if( eventList[0].Time.Date == now.Date )
				return eventList;

			eventList = RemoveTrailingArrivals(eventList).Reverse().ToList();
			return eventList;
		}

		private static IEnumerable<WorkEvent> CleanUpRepeating(IList<WorkEvent> events)
		{
			WorkEvent lastEvent = null;
			for( int i = 0; i < events.Count; i++ )
			{
				var e = events[i];
				if( lastEvent != null && e.Type == lastEvent.Type )
					continue;

				lastEvent = e;
				yield return e;
			}
		}

		private static IEnumerable<WorkEvent> RemoveLeadingDepartures(IList<WorkEvent> noRepetition)
		{
			if( noRepetition.Count == 0 )
				yield break;

			var currentDay = noRepetition[0].Time;
			var passedFirstEventOfDay = false;

			for( int i = 0; i < noRepetition.Count; i++ )
			{
				var e = noRepetition[i];

				if( e.Time.Date != currentDay )
				{
					currentDay = e.Time.Date;
					passedFirstEventOfDay = false;
				}

				if( !passedFirstEventOfDay && e.Type == EventType.Departure )
					continue;

				passedFirstEventOfDay = true;
				yield return e;
			}
		}

		private static IEnumerable<WorkEvent> RemoveTrailingArrivals(IList<WorkEvent> noRepetition)
		{
			if( noRepetition.Count == 0 )
				yield break;

			var currentDay = noRepetition.Last().Time;
			var passedLastEventOfDay = false;

			for( int i = noRepetition.Count - 1; i >= 0; i-- )
			{
				var e = noRepetition[i];

				if( e.Time.Date != currentDay )
				{
					currentDay = e.Time.Date;
					passedLastEventOfDay = false;
				}

				if( !passedLastEventOfDay && e.Type == EventType.Arrival )
					continue;

				passedLastEventOfDay = true;
				yield return e;
			}
		}
	}
}
