using System;
using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;
using WorkTimeReboot.Services.UserInput;

namespace WorkTimeReboot
{
	class Program
	{
		static void Main(string[] args)
		{
			new Tests.Tests().RunTests();
			Console.ReadLine();
			return;

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
