using System;
using System.Collections.Generic;
using WorkTimeReboot.IO;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Tests.Mocks
{
	class MockFileIO : IFileIO
	{
		public Action OnWriteToFile { get; set; }
		public Func<IEnumerable<WorkEvent>> OnReadFromFile { get; set; }

		public IEnumerable<WorkEvent> ReadFromFile()
		{
			return this.OnReadFromFile();
		}

		public void WriteToFile(IEnumerable<WorkEvent> events)
		{
			if( this.OnWriteToFile != null )
				this.OnWriteToFile();
		}
	}
}
