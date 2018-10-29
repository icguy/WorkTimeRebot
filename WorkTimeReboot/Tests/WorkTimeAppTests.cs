﻿using System;
using System.Collections.Generic;
using System.Linq;
using WorkTimeReboot.Model;
using WorkTimeReboot.Services.Clock;
using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;
using WorkTimeReboot.Services.UserIO;
using WorkTimeReboot.Tests.Framework;
using WorkTimeReboot.Tests.Mocks;
using WorkTimeReboot.Utils;

namespace WorkTimeReboot.Tests
{
	class WorkTimeAppTests
	{
		[Test]
		private bool Tick_MergesFine()
		{
			var context = new TestContext();

			context.EventLogReader.WorkEvents = new WorkEvent[]
			{
				new WorkEvent() {Time = TestHelper.GetDateHours(1), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(2), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(6), Type = EventType.Departure }
			};

			context.FileIO.OnReadFromFile = () => new WorkEvent[]
			{
				new WorkEvent() {Time = TestHelper.GetDateHours(3), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(4), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(5), Type = EventType.Arrival }
			};

			List<WorkEvent> eventsWritten = null;
			context.FileIO.OnWriteToFile = events => eventsWritten = events.ToList();

			context.App.InvokeTick();

			context.ExpectEqual(eventsWritten.Count, 6);
			context.ExpectEqual(eventsWritten[0].Type, EventType.Arrival);
			context.ExpectEqual(eventsWritten[2].Type, EventType.Arrival);
			context.ExpectEqual(eventsWritten[4].Type, EventType.Arrival);
			context.ExpectEqual(eventsWritten[1].Type, EventType.Departure);
			context.ExpectEqual(eventsWritten[3].Type, EventType.Departure);
			context.ExpectEqual(eventsWritten[5].Type, EventType.Departure);
			context.ExpectEqual(true, eventsWritten[0].Time < eventsWritten[1].Time);
			context.ExpectEqual(true, eventsWritten[1].Time < eventsWritten[2].Time);
			context.ExpectEqual(true, eventsWritten[2].Time < eventsWritten[3].Time);
			context.ExpectEqual(true, eventsWritten[3].Time < eventsWritten[4].Time);
			context.ExpectEqual(true, eventsWritten[4].Time < eventsWritten[5].Time);

			return context.TestPassed;
		}
		[Test]
		private bool Tick_CleansUpFine()
		{
			var context = new TestContext();

			context.EventLogReader.WorkEvents = new WorkEvent[]
			{
				new WorkEvent() {Time = TestHelper.GetDateHours(0), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(1), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(2), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(3), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(4), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(5), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(6), Type = EventType.Departure }
			};

			context.FileIO.OnReadFromFile = () => new WorkEvent[0];

			List<WorkEvent> eventsWritten = null;
			context.FileIO.OnWriteToFile = events => eventsWritten = events.ToList();

			context.App.InvokeTick();

			context.ExpectEqual(eventsWritten.Count, 2);
			context.ExpectEqual(eventsWritten[0].Type, EventType.Arrival);
			context.ExpectEqual(eventsWritten[1].Type, EventType.Departure);
			context.ExpectEqual(TestHelper.GetDateHours(0), eventsWritten[0].Time);
			context.ExpectEqual(TestHelper.GetDateHours(4), eventsWritten[1].Time);

			return context.TestPassed;
		}
		[Test]
		bool T001_DailyWork_FromEvents()
		{
			WorkEvent[] events = new WorkEvent[] {
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 09, 12, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 12, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 52, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 17, 52, 00),
					Type = EventType.Departure
				}
			};

			var dailywork = WorkTimesUtils.CreateDailyWork(events, 8);
			var context = new TestContextBase();
			context.ExpectEqual(dailywork.Balance, new TimeSpan(0, 0, 0));
			return context.TestPassed;
		}
		[Test]
		bool T002_DailyWork_FromEvents()
		{
			WorkEvent[] events = new WorkEvent[] {
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 09, 12, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 12, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 52, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 17, 52, 00),
					Type = EventType.Departure
				}
			};

			var dailywork = WorkTimesUtils.CreateDailyWork(events, 8);
			var context = new TestContextBase();
			context.ExpectEqual(dailywork.Balance, new TimeSpan(-8, 0, 0));
			return context.TestPassed;
		}
		[Test]
		bool T003_DailyWork_FromEvents()
		{
			WorkEvent[] events = new WorkEvent[] {
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 09, 12, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 12, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 52, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 17, 52, 00),
					Type = EventType.Arrival
				}
			};

			var dailywork = WorkTimesUtils.CreateDailyWork(events, 8);
			var context = new TestContextBase();
			context.ExpectEqual(dailywork.Balance, new TimeSpan(-8, 0, 0));
			return context.TestPassed;
		}
		[Test]
		bool T004_DailyWork_FromEvents()
		{
			WorkEvent[] events = new WorkEvent[] {
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 09, 12, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 12, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 52, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 17, 52, 00),
					Type = EventType.Arrival
				}
			};

			var dailywork = WorkTimesUtils.CreateDailyWork(events, 8);
			var context = new TestContextBase();
			context.ExpectEqual(dailywork.Balance, new TimeSpan(-5, 0, 0));
			return context.TestPassed;
		}
		[Test]
		bool T005_DailyWork_FromEvents()
		{
			WorkEvent[] events = new WorkEvent[] {
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 09, 12, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 12, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 42, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 17, 42, 00),
					Type = EventType.Departure
				}
			};

			var dailywork = WorkTimesUtils.CreateDailyWork(events, 8);
			var context = new TestContextBase();
			context.ExpectEqual(dailywork.Balance, new TimeSpan(-2, -30, 0));
			return context.TestPassed;
		}
		[Test]
		bool T006_DailyWork_FromEvents()
		{
			WorkEvent[] events = new WorkEvent[] {
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 09, 12, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 12, 00),
					Type = EventType.Arrival
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 12, 42, 00),
					Type = EventType.Departure
				},
				new WorkEvent()
				{
					Time = new DateTime(2017, 06, 01, 17, 52, 00),
					Type = EventType.Arrival
				}
			};

			var dailywork = WorkTimesUtils.CreateDailyWork(events, 8);
			var context = new TestContextBase();
			context.ExpectEqual(dailywork.Balance, new TimeSpan(-7, -30, 0));
			return context.TestPassed;
		}
		[Test]
		bool T011_GetExpectedDeparture()
		{
			var dailyWork = new DailyWork()
			{
				Events = new WorkEvent[]
				{
					new WorkEvent() { Time = new DateTime(2017, 06, 21, 9, 00, 00), Type = EventType.Arrival },
					new WorkEvent() { Time = new DateTime(2017, 06, 21, 10, 00, 00), Type = EventType.Departure },
					new WorkEvent() { Time = new DateTime(2017, 06, 21, 11, 00, 00), Type = EventType.Arrival },
					new WorkEvent() { Time = new DateTime(2017, 06, 21, 12, 00, 00), Type = EventType.Departure }
				},
				Balance = TimeSpan.FromHours(-6)
			};

			var context = new TestContext();
			var expectedDeparture = context.App.InvokeGetExpectedDeparture(dailyWork);

			context.ExpectEqual(expectedDeparture, new DateTime(2017, 06, 21, 18, 00, 00));
			return context.TestPassed;
		}
		[Test]
		bool GetStatus_SomeEvents_CalculatesCorrectly()
		{
			var context = new TestContext();

			context.EventLogReader.WorkEvents = new WorkEvent[]
			{
				new WorkEvent() {Time = TestHelper.GetDateHours(0), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(1), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(2), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(3), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(4), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(5), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(6), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(8), Type = EventType.Arrival }
			};
			context.Clock.Now = TestHelper.GetDateHours(9);

			context.FileIO.OnReadFromFile = () => new WorkEvent[]
			{
				new WorkEvent() {Time = TestHelper.GetDateHours(-24 + 0), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(-24 + 1), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(-24 + 2), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(-24 + 3), Type = EventType.Arrival },
				new WorkEvent() {Time = TestHelper.GetDateHours(-24 + 4), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(-24 + 5), Type = EventType.Departure },
				new WorkEvent() {Time = TestHelper.GetDateHours(-24 + 6), Type = EventType.Departure }
			};

			context.ModifiersFileIO.OnReadFromFile = () => new WorkModifiers();

			var status = context.App.InvokeGetStatus();

			context.ExpectEqual(status.Total, TimeSpan.FromHours(-4));
			context.ExpectEqual(status.ExpectedDeparture, TestHelper.GetDateHours(12));
			context.ExpectEqual(status.TodayWork.Balance, TimeSpan.FromHours(-3));

			return context.TestPassed;
		}
		[Test]
		bool T009_Recalculate()
		{
			var workTimes = new WorkTimes()
			{
				DailyWorks = new DailyWork[] {
					new DailyWork() {
						 Events = new WorkEvent[] {
							new WorkEvent() { Time = new DateTime(2017, 06, 20, 9, 00, 00), Type = EventType.Arrival },
							new WorkEvent() { Time = new DateTime(2017, 06, 20, 18, 00, 00), Type = EventType.Departure }
						 }
					},
					new DailyWork() {
						 Events = new WorkEvent[] {
							new WorkEvent() { Time = new DateTime(2017, 06, 21, 9, 00, 00), Type = EventType.Arrival },
							new WorkEvent() { Time = new DateTime(2017, 06, 21, 18, 00, 00), Type = EventType.Departure }
						 }

					},
					new DailyWork() {
						 Events = new WorkEvent[] {
							new WorkEvent() { Time = new DateTime(2017, 06, 22, 9, 00, 00), Type = EventType.Arrival },
							new WorkEvent() { Time = new DateTime(2017, 06, 22, 18, 00, 00), Type = EventType.Departure }
						 }

					},
					new DailyWork() {
						 Events = new WorkEvent[] {
							new WorkEvent() { Time = new DateTime(2017, 06, 23, 9, 00, 00), Type = EventType.Arrival },
							new WorkEvent() { Time = new DateTime(2017, 06, 23, 18, 00, 00), Type = EventType.Departure }
						 }

					},
				}
			};

			workTimes.Recalculate();

			var context = new TestContextBase();
			context.ExpectEqual(workTimes.Balance, new TimeSpan(4, 0, 0));

			foreach( var dailyWork in workTimes.DailyWorks )
			{
				context.ExpectEqual(dailyWork.Balance, new TimeSpan(1, 0, 0));
			}
			return context.TestPassed;
		}
	}

	class TestContext : TestContextBase
	{
		public MockTimer Timer { get; private set; }
		public MockFileIO<IEnumerable<WorkEvent>> FileIO { get; private set; }
		public MockEventLogReader EventLogReader { get; private set; }
		public MockUserIO UserIO { get; private set; }
		public MockClock Clock { get; private set; }
		public TestWorkTimeApp App { get; private set; }
		public MockFileIO<WorkModifiers> ModifiersFileIO { get; private set; }

		public TestContext()
		{
			this.Timer = new MockTimer();
			this.FileIO = new MockFileIO<IEnumerable<WorkEvent>>();
			this.EventLogReader = new MockEventLogReader();
			this.UserIO = new MockUserIO();
			this.Clock = new MockClock();
			this.ModifiersFileIO = new MockFileIO<WorkModifiers>();

			this.App = new TestWorkTimeApp(
				this.Timer,
				this.FileIO,
				this.EventLogReader,
				this.UserIO,
				this.Clock,
				this.ModifiersFileIO
			);
		}
	}

	class TestWorkTimeApp : WorkTimeApp
	{
		public TestWorkTimeApp(
			ITimer timer,
			IFileIO<IEnumerable<WorkEvent>> fileIO,
			IEventLogReader eventLogReader,
			IUserIO userIO,
			IClock clock,
			IFileIO<WorkModifiers> modifiersFileIO
			) : base(timer, fileIO, eventLogReader, userIO, clock, modifiersFileIO)
		{
		}

		public void InvokeTick() => this.Tick();
		public DateTime InvokeGetExpectedDeparture(DailyWork todayWork) => this.GetExpectedDeparture(todayWork);
		public Status InvokeGetStatus() => this.GetStatus();
	}
}
