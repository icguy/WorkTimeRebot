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
	class QuitCommandException : Exception
	{
	}

	class WorkTimeApp
	{
		private readonly ITimer _timer;
		private readonly IFileIO<IEnumerable<WorkEvent>> _fileIO;
		private readonly IEventLogReader _eventLogReader;
		private readonly IUserIO _userIO;
		private readonly IClock _clock;
		private readonly IFileIO<WorkModifiers> _modifiersFileIO;

		public WorkTimeApp(
			ITimer timer,
			IFileIO<IEnumerable<WorkEvent>> fileIO,
			IEventLogReader eventLogReader,
			IUserIO UserIO,
			IClock clock,
			IFileIO<WorkModifiers> modifiersFileIO
			)
		{
			_timer = timer;
			_fileIO = fileIO;
			_eventLogReader = eventLogReader;
			_userIO = UserIO;
			_clock = clock;
			_modifiersFileIO = modifiersFileIO;
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
						case "edit":
							this.EditDay();
							break;
						default:
							_userIO.WriteLine("Unknown command, type 'help' for available commands.");
							break;
					}
				}
				catch( QuitCommandException )
				{
				}
				catch( Exception ex )
				{
					_userIO.WriteError(ex);
				}
			}
			return false;
		}

		//todo test this
		protected void EditDay()
		{
			_userIO.WriteLine("type 'q' to quit command");
			_userIO.WriteLine("enter a date");
			DateTime date;
			while( true )
			{
				var input = this.GetUserInputOrQuit();
				if( DateTime.TryParse(input, out date) )
					break;
			}
			var events = _fileIO.ReadFromFile()?.Where(e => e.Time.Date == date.Date)?.ToList();

			if( events == null || events.Count == 0 )
			{
				_userIO.WriteLine("No events on the specified date.");
				return;
			}

			_userIO.WriteLine($" ============== EDITING DAY ({date.ToShortDateString()}) ==============");
			foreach( var e in events )
			{
				_userIO.WriteLine(e);
			}
			_userIO.WriteLine();

			var ignoredEvents = new List<WorkEvent>();
			foreach( var e in events )
			{
				while( true )
				{
					_userIO.WriteLine(e.ToString());
					_userIO.WriteLine("Keep event? (y/n)");
					string resp = this.GetUserInputOrQuit();
					if( resp.ToLower() == "y" )
					{
						break;
					}
					else if( resp.ToLower() == "n" )
					{
						ignoredEvents.Add(e);
						break;
					}
				}
			}

			int hoursToWork;
			while( true )
			{
				_userIO.WriteLine("Hours to work today: ");
				var hoursString = this.GetUserInputOrQuit();
				if( int.TryParse(hoursString, out hoursToWork) )
					break;
				_userIO.WriteLine("Please write a number.");
			}

			var modifiers = _modifiersFileIO.ReadFromFile() ?? new WorkModifiers
			{
				HoursModifiers = new List<HoursToWorkModifier>(),
				IgnoredEventsModifiers = new List<IgnoredEventModifier>()
			};

			var hoursModifiers = modifiers.HoursModifiers.Where(hm => hm.Date.Date != date.Date).ToList();
			hoursModifiers.Add(new HoursToWorkModifier { Date = date.Date, Hours = hoursToWork });

			var ignoredEventsModifiers = modifiers.IgnoredEventsModifiers.Where(ie => ie.Date.Date != date.Date).ToList();
			ignoredEventsModifiers.AddRange(ignoredEvents.Select(ie => new IgnoredEventModifier { Date = ie.Time }));

			_modifiersFileIO.WriteToFile(new WorkModifiers
			{
				HoursModifiers = hoursModifiers,
				IgnoredEventsModifiers = ignoredEventsModifiers
			});
			_userIO.WriteLine("editing finished.");
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
			var events = _fileIO.ReadFromFile()?.Where(e => e.Time.Date != _clock.Now.Date) ?? new WorkEvent[0];
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
			var eventsFromFile = _fileIO.ReadFromFile() ?? new WorkEvent[0];
			workEvents = EventStreamUtils.CleanUpStream(newWorkEvents.Concat(eventsFromFile));

			if( enableLogging ) _userIO.WriteLine("gathered events");
			return workEvents;
		}

		private void ShowHelp()
		{
			_userIO.WriteLine("soon..");
		}

		private string GetUserInputOrQuit()
		{
			var userInput = _userIO.ReadLine();
			if( userInput.Trim().ToLower() == "q" )
				throw new QuitCommandException();
			return userInput;
		}
	}
}
