using System.Collections.Generic;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Utils
{
	static class EventStreamUtils
	{
		public static IEnumerable<WorkEvent> CleanUpStream(IEnumerable<WorkEvent> events)
		{
			WorkEvent lastEvent = null;
			foreach( var e in events )
			{
				if( lastEvent != null && e.Type == lastEvent.Type )
					continue;

				lastEvent = e;
				yield return e;
			}
		}
	}
}
