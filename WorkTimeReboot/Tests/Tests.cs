using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using WorkTimeReboot.Model;
using WorkTimeReboot.Services.UserInput;
using WorkTimeReboot.Tests.Mocks;

namespace WorkTimeReboot.Tests
{
	class TestAttribute : Attribute { }

	class Tests
	{
		public void RunTests()
		{
			var tests = this.GetType().GetRuntimeMethods().Where(m => m.GetCustomAttributes<TestAttribute>().Any());
			var allTestsPassed = true;
			foreach( var test in tests )
			{
				bool currentTestPassed = true;
				try
				{
					currentTestPassed = (bool)test.Invoke(this, new object[] { });
				}
				catch( Exception ex )
				{
					Console.WriteLine($"test threw exception: {ex}");
					currentTestPassed = false;
				}

				if( !currentTestPassed )
				{
					Console.WriteLine($"=| FAILED {test.Name}");
					allTestsPassed = false;
				}
				else
				{
					Console.WriteLine($"=| OK     {test.Name}");
				}
			}

			Console.WriteLine();
			if( !allTestsPassed )
				Console.WriteLine("====== TESTS FAILED ======");
			else
				Console.WriteLine("====== TESTS PASSED ======");
		}

		[Test]
		private bool Test1_Tick_MergesFine()
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
		private bool Test2_Tick_CleansUpFine()
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
	}

	class TestContext
	{
		public bool TestPassed { get; private set; } = true;
		public MockTimer Timer { get; private set; }
		public MockFileIO FileIO { get; private set; }
		public MockEventLogReader EventLogReader { get; private set; }
		public MockUserInput UserInput { get; private set; }
		public TestWorkTimeApp App { get; private set; }

		public TestContext()
		{
			this.Timer = new MockTimer();
			this.FileIO = new MockFileIO();
			this.EventLogReader = new MockEventLogReader();
			this.UserInput = new MockUserInput();
			this.App = new TestWorkTimeApp(
				this.Timer,
				this.FileIO,
				this.EventLogReader,
				this.UserInput
			);
		}

		public void ExpectEqual(object a, object b, [CallerLineNumber] int lineNum = 0)
		{
			if( !a.Equals(b) )
			{
				this.TestPassed = false;
				Console.WriteLine($"equality failed: {a} should equal {b} on line {lineNum}");
			}
		}
	}

	static class TestHelper
	{
		public static DateTime GetDateHours(int number)
		{
			return new DateTime(2018, 07, 05, 06, 00, 00).AddHours(number);
		}
	}
}
