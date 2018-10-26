using System;

namespace WorkTimeReboot.Services.Clock
{
	public class AppClock : IClock
	{
		public DateTime Now => DateTime.Now;
	}
}
