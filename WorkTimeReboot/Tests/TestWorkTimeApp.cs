using WorkTimeReboot.Services.EventLogReader;
using WorkTimeReboot.Services.IO;
using WorkTimeReboot.Services.Timer;
using WorkTimeReboot.Services.UserInput;

namespace WorkTimeReboot.Tests
{
	class TestWorkTimeApp : WorkTimeApp
	{
		public TestWorkTimeApp(ITimer timer, IFileIO fileIO, IEventLogReader eventLogReader, IUserInput userInput) : base(timer, fileIO, eventLogReader, userInput)
		{
		}

		public void InvokeTick()
		{
			this.Tick();
		}
	}
}
