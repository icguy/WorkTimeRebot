using System;
using System.Linq;
using WorkTimeReboot.Services.Clock;
using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;
using WorkTimeReboot.Services.UserIO;
using WorkTimeReboot.Tests;
using WorkTimeReboot.Tests.Framework;

namespace WorkTimeReboot
{
	class Program
	{
		static void Main(string[] args)
		{
			if( args.FirstOrDefault() == "/test" )
				TestRunner.RunTests(new WorkTimeAppTests());

			var config = new Model.Config
			{
				TimerIntervalInSeconds = 120,
				FilePath = "times.json"
			};

			var timer = new Timer(config.TimerIntervalInSeconds * 1000);
			var fileIO = new FileIO(config.FilePath);
			var eventLogReader = new EventLogReader();
			var userIO = new UserIO();
			var clock = new AppClock();

			new WorkTimeApp(
				timer,
				fileIO,
				eventLogReader,
				userIO,
				clock
			).Run();
		}
	}
}
