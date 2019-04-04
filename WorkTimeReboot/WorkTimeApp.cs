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
				var workEvents = this.GetEvents(false).ToList();
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
							this.GetStatus(quick).Print(_userIO);
							break;
						case "log":
							DateTime? dateArg = null;
							try
							{
								dateArg = DateTime.Parse(tokens[1]);
							}
							catch( Exception ) { }
							this.PrintLog(dateArg);
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
							this.EditDay(tokens);
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

		protected void PrintLog(DateTime? dateArg = null)
		{
			var events = this.ReadEventsFromFile().Where(e => e.Time.Date != _clock.Now.Date);
			var workTime = WorkTimesUtils.CreateWorkTimes(events);
			var modifiers = this.ReadModifiersFromFile();
			workTime.ApplyModifiers(modifiers);
			if( dateArg == null )
			{
				foreach( var dw in workTime.DailyWorks )
				{
					var padding = dw.Balance.TotalMinutes > 0 ? " " : "";
					_userIO.WriteLine($"{dw.Events.First().Time.ToShortDateString()}: {padding}{dw.Balance}");
				}
			}
			else
			{
				var work = workTime.DailyWorks.FirstOrDefault(dw => dw.Events.First().Time.Date == dateArg.Value.Date);
				if( work != null )
					_userIO.WriteLine(work);
				else
					_userIO.WriteLine("date not found");
			}
		}

		//todo test this
		protected void EditDay(string[] tokens)
		{
			_userIO.WriteLine("type 'q' to quit command");
			DateTime date = default(DateTime);
			if( tokens.Length > 1 )
			{
				var args = tokens.ToList();
				args.RemoveAt(0);
				var input = string.Join(" ", args);
				DateTime.TryParse(input, out date);
			}

			_userIO.WriteLine("enter a date");
			while( date == default(DateTime) )
			{
				var input = this.GetUserInputOrQuit();
				if( DateTime.TryParse(input, out date) )
					break;
			}
			var events = this.ReadEventsFromFile().Where(e => e.Time.Date == date.Date).ToList();

			if( events.Count == 0 )
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

			var modifiers = this.ReadModifiersFromFile();

			var hoursModifiers = modifiers.HoursModifiers.Where(hm => hm.Date.Date != date.Date).ToList();
			hoursModifiers.Add(new HoursToWorkModifier { Date = date.Date, Hours = hoursToWork });

			var ignoredEventsModifiers = modifiers.IgnoredEventsModifiers.Where(ie => ie.Time.Date != date.Date).ToList();
			ignoredEventsModifiers.AddRange(ignoredEvents.Select(ie => new IgnoredEventModifier { Time = ie.Time }));

			_modifiersFileIO.WriteToFile(new WorkModifiers
			{
				HoursModifiers = hoursModifiers,
				IgnoredEventsModifiers = ignoredEventsModifiers
			});
			_userIO.WriteLine("editing finished.");
		}

		//todo test this
		protected Status GetStatus(bool quick = false)
		{
			var now = _clock.Now;
			var nowSeconds = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);
			var events = this.GetEvents(true, quick).ToList();
			events = EventStreamUtils.CleanUpStream(events, now).ToList();
			events.Add(new WorkEvent() { Time = nowSeconds, Type = EventType.Departure });
			var workTime = WorkTimesUtils.CreateWorkTimes(events);
			var modifiers = this.ReadModifiersFromFile();
			workTime.ApplyModifiers(modifiers);

			var today = workTime.DailyWorks.OrderByDescending(dw => dw.Events.FirstOrDefault().Time).FirstOrDefault();
			var expectedDeparture = this.GetExpectedDeparture(today);

			return new Status
			{
				Total = workTime.Balance - today.Balance,
				TodayWork = today,
				ExpectedDeparture = expectedDeparture
			};
		}

		protected DateTime GetExpectedDeparture(DailyWork todayWork)
		{
			var lastSignin = todayWork.Events.OrderByDescending(e => e.Time).FirstOrDefault(/*e => e.Type == EventType.Arrival*/);
			var expectedDeparture = lastSignin.Time - todayWork.Balance;
			return expectedDeparture;
		}

		//todo test this
		protected IEnumerable<WorkEvent> GetEvents(bool enableLogging = true, bool quick = false)
		{
			var eventsFromFile = this.ReadEventsFromFile();
			var events = eventsFromFile;
			if( !quick )
			{
				var eventsFromLog = this.ReadFromLog(enableLogging);
				events = eventsFromFile.Concat(eventsFromLog);
			}
			return EventStreamUtils.OrderAndRemoveDuplicate(events);
		}

		private IEnumerable<WorkEvent> ReadFromLog(bool enableLogging = true)
		{
			if( enableLogging ) _userIO.WriteLine("reading eventlog...");
			var newWorkEvents = _eventLogReader.GetWorkEvents().ToList();
			if( enableLogging ) _userIO.WriteLine("finished reading eventlog");
			return newWorkEvents;
		}

		private IEnumerable<WorkEvent> ReadEventsFromFile() => _fileIO.ReadFromFile() ?? new WorkEvent[0];

		private WorkModifiers ReadModifiersFromFile()
		{
			return _modifiersFileIO.ReadFromFile() ?? new WorkModifiers();
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
