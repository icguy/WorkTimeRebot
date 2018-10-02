using System;
using System.Linq;
using WorkTimeReboot.IO;
using WorkTimeReboot.Utils;

namespace WorkTimeReboot
{
	class WorkTimeApp
	{
		private readonly Timer.ITimer _timer;
		private readonly FileIO _fileIO;

		public WorkTimeApp(Timer.ITimer timer, FileIO fileIO)
		{
			_timer = timer;
			_fileIO = fileIO;
		}

		public void Run()
		{
			_timer.Tick += this.Tick;
			_timer.Start();
			while( true )
			{
				var command = Console.ReadLine();
				if( this.HandleUserCommand(command) )
					break;
			}
			_timer.Stop();
		}

		private void Tick()
		{
			var newWorkEvents = EventReader.GetWorkEvents();
			var eventsFromFile = _fileIO.ReadFromFile();
			var workEvents = EventStreamUtils.CleanUpStream(newWorkEvents.Concat(eventsFromFile));
			_fileIO.WriteToFile(workEvents);
		}

		private bool HandleUserCommand(string command)
		{
			switch( command )
			{
				case "exit":
					return true;
				default:
					this.ShowHelp();
					break;
			}
			return false;
		}

		private void ShowHelp()
		{

		}
	}
}
