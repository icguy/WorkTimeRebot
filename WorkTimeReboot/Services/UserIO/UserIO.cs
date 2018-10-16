using System;
using System.Linq;

namespace WorkTimeReboot.Services.UserIO
{
	class UserIO : IUserIO
	{
		public bool PrintTimeStamp { get; set; } = true;

		public string ReadLine() => Console.ReadLine();
		public void WriteError(object o) => Console.Error.WriteLine(this.Stringify(o));
		public void WriteLine() => Console.WriteLine();
		public void WriteLine(object o) => Console.WriteLine(this.Stringify(o));
		public void Clear() => Console.Clear();

		private string Stringify(object o)
		{
			return this.AddTimeStampIfNotEmpty(o.ToString());
		}

		private string AddTimeStampIfNotEmpty(string text)
		{
			if( this.PrintTimeStamp )
				return string.Join("\r\n", text.Split(new[] { "\r\n" }, StringSplitOptions.None).Select(s => $"[{DateTime.Now.ToString()}] {s}"));
			return text;
		}
	}
}
