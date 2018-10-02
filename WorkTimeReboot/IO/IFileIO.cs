using System.Collections.Generic;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.IO
{
	public interface IFileIO
	{
		IEnumerable<WorkEvent> ReadFromFile();
		void WriteToFile(IEnumerable<WorkEvent> events);
	}
}
