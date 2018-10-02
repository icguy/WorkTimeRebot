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

			new WorkTimeApp(
				timer,
				fileIO
				).Run();
		}
	}
}
