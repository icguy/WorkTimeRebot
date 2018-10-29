using System;
using WorkTimeReboot.Services.IO;

namespace WorkTimeReboot.Tests.Mocks
{
	class MockFileIO<T> : IFileIO<T>
	{
		public Action<T> OnWriteToFile { get; set; }
		public Func<T> OnReadFromFile { get; set; }

		public T ReadFromFile()
		{
			return this.OnReadFromFile();
		}

		public void WriteToFile(T data)
		{
			this.OnWriteToFile?.Invoke(data);
		}
	}
}
