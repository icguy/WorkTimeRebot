using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
			_userIO.WriteLine("tick start");
			try
			{
				var workEvents = this.GetEvents(false);
				_fileIO.WriteToFile(workEvents);
			}
			catch( Exception ex )
			{
				_userIO.WriteError(ex);
			}
			_userIO.WriteLine("tick end");
		}

		protected bool HandleUserCommand(string command)
		{
			var tokens = command.Split(' ');
			if( tokens.Length > 0 )
			{
				try
				{
					switch( tokens[0] )
					{
						case "":
							break;
						case "exit":
							return true;
						case "s":
						case "status":
							bool quick = tokens.Contains("--quick") || tokens.Contains("-q");
							if( quick )
								this.GetQuickStatus().Print(_userIO);
							else
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
				}
				catch( Exception ex )
				{
					_userIO.WriteError(ex);
				}
			}
			return false;
		}

		//todo test this
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

		//todo test this
		protected QuickStatus GetQuickStatus()
		{
			var events = _fileIO.ReadFromFile().Where(e => e.Time.Date != _clock.Now.Date);
			var workTime = WorkTimesUtils.CreateWorkTimes(events);
			return new QuickStatus
			{
				Total = workTime.Balance
			};
		}

		protected DateTime GetExpectedDeparture(DailyWork todayWork)
		{
			var lastSignin = todayWork.Events.OrderByDescending(e => e.Time).FirstOrDefault(/*e => e.Type == EventType.Arrival*/);
			var expectedDeparture = lastSignin.Time - todayWork.Balance;
			return expectedDeparture;
		}

		//todo test this
		protected IEnumerable<WorkEvent> GetEvents(bool enableLogging = true)
		{
			IEnumerable<WorkEvent> workEvents;
			if( enableLogging ) _userIO.WriteLine("gathering events...");

			var newWorkEvents = _eventLogReader.GetWorkEvents();
			var eventsFromFile = _fileIO.ReadFromFile();
			workEvents = EventStreamUtils.CleanUpStream(newWorkEvents.Concat(eventsFromFile));

			if( enableLogging ) _userIO.WriteLine("gathered events");
			return workEvents;
		}

		private void ShowHelp()
		{
			_userIO.WriteLine("soon..");
		}
	}
}
