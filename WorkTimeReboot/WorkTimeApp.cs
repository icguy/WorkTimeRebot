using System;
using System.Timers;
using WorkTimeReboot.Model;
using WorkTimeReboot.Utils;

namespace WorkTimeReboot
{
	class WorkTimeApp
	{
		private Timer _timer;

		public WorkTimeApp(Config config)
		{
			_timer = new Timer(config.TimerIntervalInSeconds * 1000);
		}

		public void Run()
		{
			_timer.Elapsed += this.Tick;
			_timer.Start();
			while( true )
			{
				var command = Console.ReadLine();
				if( this.HandleUserCommand(command) )
					break;
			}
			_timer.Stop();
		}

		private void Tick(object sender, EventArgs e)
		{
			var workEvents = EventReader.GetWorkEvents();
			workEvents = EventStreamUtils.CleanUpStream(workEvents);

		}

		private bool HandleUserCommand(string command)
		{
			switch( command )
			{
				case "exit":
					return true;
				default:
					return false;
			}
		}
	}
}
