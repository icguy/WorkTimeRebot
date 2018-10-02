using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.IO
{
	public class FileIO : IFileIO
	{
		private readonly string _filePath;
		private readonly object _lock;

		public FileIO(string filePath)
		{
			_filePath = filePath;
			_lock = new object();
		}

		public IEnumerable<WorkEvent> ReadFromFile()
		{
			lock( _lock )
			{
				using( var fileStream = File.Open(_filePath, FileMode.OpenOrCreate, FileAccess.Read) )
				using( var streamReader = new StreamReader(fileStream) )
				{
					try
					{
						return JsonConvert.DeserializeObject<IEnumerable<WorkEvent>>(streamReader.ReadToEnd());
					}
					catch( Exception )
					{
					}

					return new WorkEvent[0];
				}
			}
		}

		public void WriteToFile(IEnumerable<WorkEvent> events)
		{
			lock( _lock )
			{
				using( var fileStream = File.Open(_filePath, FileMode.Create, FileAccess.Write) )
				using( var streamWriter = new StreamWriter(fileStream) )
				{
					streamWriter.WriteLine(JsonConvert.SerializeObject(events));
				}
			}
		}
	}
}
