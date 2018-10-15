using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeReboot.Model;
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
			Console.WriteLine("App started, type 'help' for available commands");
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
			var workEvents = this.GetEvents();
			_fileIO.WriteToFile(workEvents);
		}

		protected WorkTimes Calculate()
		{
			var events = this.GetEvents();
			return WorkTimesUtils.CreateWorkTimes(events);
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
						this.GetStatus().Print();
						break;
					case "help":
						this.ShowHelp();
						break;
					default:
						Console.WriteLine("Unknown command, type 'help' for available commands.");
						break;
				}
				return false;
			}
			catch( Exception )
			{
				return false;
			}
		}

#error todo tests
		protected Status GetStatus()
		{
			var status = new Status();
			var workTime = this.Calculate();

			var today = workTime.DailyWorks.OrderByDescending(dw => dw.Events.FirstOrDefault().Time).FirstOrDefault();
			status.Total = workTime.Balance - today.Balance;

			var now = DateTime.Now;
			var nowSeconds = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind);
			var workEventsToday = today.Events.ToList();
			workEventsToday.Add(new WorkEvent() { Time = nowSeconds, Type = EventType.Departure });
			var todayWork = WorkTimesUtils.CreateDailyWork(workEventsToday, 8);
			var expectedDeparture = this.GetExpectedDeparture(todayWork);


			status.TodayWork = todayWork;
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
			Console.WriteLine("soon..");
		}
	}
}
