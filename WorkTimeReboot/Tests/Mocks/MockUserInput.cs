using System;
using WorkTimeReboot.Services.UserInput;

namespace WorkTimeReboot.Tests.Mocks
{
	class MockUserInput : IUserInput
	{
		public Func<string> OnReadLine { get; set; }

		public string ReadLine()
		{
			return this.OnReadLine();
		}
	}
}
