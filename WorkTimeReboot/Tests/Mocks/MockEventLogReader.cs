using System.Collections.Generic;
using WorkTimeReboot.Model;
using WorkTimeReboot.Services.EventLogReader;

namespace WorkTimeReboot.Tests.Mocks
{
	class MockEventLogReader : IEventLogReader
	{
		public IEnumerable<WorkEvent> WorkEvents { get; set; }

		public IEnumerable<WorkEvent> GetWorkEvents()
		{
			return this.WorkEvents;
		}
	}
}
