using System;
using WorkTimeReboot.Services.Clock;

namespace WorkTimeReboot.Tests.Mocks
{
	public class MockClock : IClock
	{
		public DateTime Now { get; set; } = new DateTime(2018, 10, 10, 10, 10, 10);
	}
}
