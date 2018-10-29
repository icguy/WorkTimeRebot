using System;
using System.Collections.Generic;
using WorkTimeReboot.Model;
using WorkTimeReboot.Services.IO;

namespace WorkTimeReboot.Tests.Mocks
{
	class MockFileIO : IFileIO<IEnumerable<WorkEvent>>
	{
		public Action<IEnumerable<WorkEvent>> OnWriteToFile { get; set; }
		public Func<IEnumerable<WorkEvent>> OnReadFromFile { get; set; }

		public IEnumerable<WorkEvent> ReadFromFile()
		{
			return this.OnReadFromFile();
		}

		public void WriteToFile(IEnumerable<WorkEvent> events)
		{
			this.OnWriteToFile?.Invoke(events);
		}
	}
}
