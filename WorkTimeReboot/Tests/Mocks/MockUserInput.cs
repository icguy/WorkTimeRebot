using System;
using WorkTimeReboot.Services.UserIO;

namespace WorkTimeReboot.Tests.Mocks
{
	class MockUserIO : IUserIO
	{
		public Func<string> OnReadLine { get; set; }
		public Action OnWriteLine { get; set; }
		public Action<object> OnWriteLineParam { get; set; }
		public Action<object> OnError { get; set; }

		private UserIO _userIO;

		public MockUserIO()
		{
			_userIO = new UserIO();
			this.OnWriteLine = _userIO.WriteLine;
			this.OnWriteLineParam = _userIO.WriteLine;
			this.OnError = _userIO.WriteError;
		}

		public string ReadLine() => this.OnReadLine();
		public void WriteError(object o) => this.OnError(o);
		public void WriteLine() => this.OnWriteLine();
		public void WriteLine(object o) => this.OnWriteLineParam(o);
		public void Clear() { }
	}
}
