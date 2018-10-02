using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WorkTimeReboot.Model;
using WorkTimeReboot.Utils;

namespace WorkTimeReboot.Services.EventLogReader
{
	class EventLogReader : IEventLogReader
	{
		private readonly long[] EventIds = new[] { 4647L, 4648L, 4800L, 4801L, /*4624L,*/ /*4634L*/ };

		public virtual IEnumerable<WorkEvent> GetWorkEvents()
		{
			var securityLog = GetSecurityLog();
			return securityLog.Entries.Cast<EventLogEntry>()
				.Where(e => EventIds.Contains(e.InstanceId))
				.Select(e => e.ToWorkEvent());
		}

		private EventLog GetSecurityLog()
		{
			var logs = EventLog.GetEventLogs();
			var securityLog = logs.Where(l => l.Log == "Security").FirstOrDefault();
			if( securityLog == null )
				throw new Exception("securitylog null");
			return securityLog;
		}
	}
}
