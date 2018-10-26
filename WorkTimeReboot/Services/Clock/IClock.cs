using System;

namespace WorkTimeReboot.Services.Clock
{
	public interface IClock
	{
		DateTime Now { get; }
	}
}
