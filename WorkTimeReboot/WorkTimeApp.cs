using System;
using System.Linq;
using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;
using WorkTimeReboot.Services.UserInput;
using WorkTimeReboot.Utils;

namespace WorkTimeReboot
{
	class WorkTimeApp
	{
		private readonly ITimer _timer;
		private readonly IFileIO _fileIO;
		private readonly IEventLogReader _eventLogReader;
		private readonly IUserInput _userInput;

		public WorkTimeApp(ITimer timer, IFileIO fileIO, IEventLogReader eventLogReader, IUserInput userInput)
		{
			_timer = timer;
			_fileIO = fileIO;
			_eventLogReader = eventLogReader;
			_userInput = userInput;
		}

		public void Run()
		{
			_timer.Tick += this.Tick;
			_timer.Start();
			while( true )
			{
				var command = _userInput.ReadLine();
				if( this.HandleUserCommand(command) )
					break;
			}
			_timer.Stop();
		}

		protected void Tick()
		{
			var newWorkEvents = _eventLogReader.GetWorkEvents();
			var eventsFromFile = _fileIO.ReadFromFile();
			var workEvents = EventStreamUtils.CleanUpStream(newWorkEvents.Concat(eventsFromFile));
			_fileIO.WriteToFile(workEvents);
		}

		protected bool HandleUserCommand(string command)
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
