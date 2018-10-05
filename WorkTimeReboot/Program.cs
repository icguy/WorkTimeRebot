using System;
using System.Linq;
using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;
using WorkTimeReboot.Services.UserInput;
using WorkTimeReboot.Tests;
using WorkTimeReboot.Tests.Framework;

namespace WorkTimeReboot
{
	class Program
	{
		static void Main(string[] args)
		{
			if( args.FirstOrDefault() == "/test" )
			{
				TestRunner.RunTests(new WorkTimeAppTests());
				Console.ReadLine();
				return;
			}

			var config = new Model.Config
			{
				TimerIntervalInSeconds = 20,
				FilePath = "times.json"
			};

			var timer = new Timer(config.TimerIntervalInSeconds * 1000);
			var fileIO = new FileIO(config.FilePath);
			var eventLogReader = new EventLogReader();
			var userInput = new UserInput();

			new WorkTimeApp(
				timer,
				fileIO,
				eventLogReader,
				userInput
			).Run();
		}
	}
}
