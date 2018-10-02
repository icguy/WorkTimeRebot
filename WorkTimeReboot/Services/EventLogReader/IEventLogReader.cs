using System.Collections.Generic;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Services.EventLogReader
{
	interface IEventLogReader
	{
		IEnumerable<WorkEvent> GetWorkEvents();
	}
}
