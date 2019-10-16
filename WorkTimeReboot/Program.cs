using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeReboot.Model;
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
				FilePath = "times.json",
				ModifiersFilePath = "modifiers.json"
			};

			var timer = new Timer(config.TimerIntervalInSeconds * 1000);
			var fileIO = new FileIO<IEnumerable<WorkEvent>>(config.FilePath);
			var eventLogReader = new EventLogReader("ayhand");
			var userIO = new UserIO();
			var clock = new AppClock();
			var modifiersFileIO = new FileIO<WorkModifiers>(config.ModifiersFilePath);

			new WorkTimeApp(
				timer,
				fileIO,
				eventLogReader,
				userIO,
				clock,
				modifiersFileIO
			).Run();
		}
	}
}
