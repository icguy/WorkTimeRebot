using System.Collections.Generic;
using System.Linq;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Utils
{
	static class EventStreamUtils
	{
		public static IEnumerable<WorkEvent> CleanUpStream(IEnumerable<WorkEvent> events)
		{
			var sorted = events.OrderBy(e => e.Time);
			WorkEvent lastEvent = null;
			foreach( var e in sorted )
			{
				if( lastEvent != null && e.Type == lastEvent.Type )
					continue;

				lastEvent = e;
				yield return e;
			}
		}
	}
}
