using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeReboot.Model;
using WorkTimeReboot.Services.Clock;
using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;
using WorkTimeReboot.Services.UserIO;
using WorkTimeReboot.Utils;

namespace WorkTimeReboot
{
	class WorkTimeApp
	{
		private readonly ITimer _timer;
		private readonly IFileIO _fileIO;
		private readonly IEventLogReader _eventLogReader;
		private readonly IUserIO _userIO;
		private readonly IClock _clock;

		public WorkTimeApp(ITimer timer, IFileIO fileIO, IEventLogReader eventLogReader, IUserIO UserIO, IClock clock)
		{
			_timer = timer;
			_fileIO = fileIO;
			_eventLogReader = eventLogReader;
			_userIO = UserIO;
			_clock = clock;
		}

		public void Run()
		{
			_timer.Tick += this.Tick;
			_timer.Start();
			_userIO.WriteLine("App started, type 'help' for available commands");
			while( true )
			{
				var command = _userIO.ReadLine();
				if( this.HandleUserCommand(command) )
					break;
			}
			_timer.Stop();
		}

		protected void Tick()
		{
			try
			{
				var workEvents = this.GetEvents();
				_fileIO.WriteToFile(workEvents);
			}
			catch( Exception ex )
			{
				_userIO.WriteError(ex);
			}
		}

		protected bool HandleUserCommand(string command)
		{
			try
			{
				switch( command )
				{
					case "":
						break;
					case "exit":
						return true;
					case "s":
					case "status":
						this.GetStatus().Print(_userIO);
						break;
					case "help":
						this.ShowHelp();
						break;
					case "cls":
						_userIO.Clear();
						break;
					case "tick":
						this.Tick();
						break;
					default:
						_userIO.WriteLine("Unknown command, type 'help' for available commands.");
						break;
				}
				return false;
			}
			catch( Exception ex )
			{
				_userIO.WriteError(ex);
				return false;
			}
		}

		protected Status GetStatus()
		{
			var status = new Status();
			var now = _clock.Now;
			var nowSeconds = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);
			var events = this.GetEvents().ToList();
			events.Add(new WorkEvent() { Time = nowSeconds, Type = EventType.Departure });
			var workTime = WorkTimesUtils.CreateWorkTimes(events);

			var today = workTime.DailyWorks.OrderByDescending(dw => dw.Events.FirstOrDefault().Time).FirstOrDefault();
			status.Total = workTime.Balance - today.Balance;

			var expectedDeparture = this.GetExpectedDeparture(today);

			status.TodayWork = today;
			status.ExpectedDeparture = expectedDeparture;
			return status;
		}

		protected DateTime GetExpectedDeparture(DailyWork todayWork)
		{
			var lastSignin = todayWork.Events.OrderByDescending(e => e.Time).FirstOrDefault(/*e => e.Type == EventType.Arrival*/);
			var expectedDeparture = lastSignin.Time - todayWork.Balance;
			return expectedDeparture;
		}

		private IEnumerable<WorkEvent> GetEvents()
		{
			var newWorkEvents = _eventLogReader.GetWorkEvents();
			var eventsFromFile = _fileIO.ReadFromFile();
			var workEvents = EventStreamUtils.CleanUpStream(newWorkEvents.Concat(eventsFromFile));
			return workEvents;
		}

		private void ShowHelp()
		{
			_userIO.WriteLine("soon..");
		}
	}
}
