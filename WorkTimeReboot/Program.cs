using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;

namespace WorkTimeReboot
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = new Model.Config
			{
				TimerIntervalInSeconds = 20,
				FilePath = "times.json"
			};

			var timer = new Timer(config.TimerIntervalInSeconds * 1000);
			var fileIO = new FileIO(config.FilePath);
			var eventLogReader = new EventLogReader();

			new WorkTimeApp(
				timer,
				fileIO,
				eventLogReader
				).Run();
		}
	}
}
