using System;

namespace WorkTimeReboot
{
	class Program
	{
		static void Main(string[] args)
		{
			new WorkTimeApp(new Model.Config {
				TimerIntervalInSeconds = 20
			}).Run();
		}
	}
}
