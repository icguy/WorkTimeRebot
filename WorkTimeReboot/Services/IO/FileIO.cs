using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WorkTimeReboot.Model;

namespace WorkTimeReboot.Services.IO
{
	public class FileIO<T> : IFileIO<T>
	{
		private readonly string _filePath;
		private readonly object _lock;

		public FileIO(string filePath)
		{
			_filePath = filePath;
			_lock = new object();
		}

		public T ReadFromFile()
		{
			lock( _lock )
			{
				using( var fileStream = File.Open(_filePath, FileMode.OpenOrCreate, FileAccess.Read) )
				using( var streamReader = new StreamReader(fileStream) )
				{
					try
					{
						var json = streamReader.ReadToEnd();
						return JsonConvert.DeserializeObject<T>(json);
					}
					catch( Exception )
					{
					}

					return default(T);
				}
			}
		}

		public void WriteToFile(T data)
		{
			var json = JsonConvert.SerializeObject(data, Formatting.Indented);
			lock( _lock )
			{
				using( var fileStream = File.Open(_filePath, FileMode.Create, FileAccess.Write) )
				using( var streamWriter = new StreamWriter(fileStream) )
				{
					streamWriter.WriteLine(json);
				}
			}
		}
	}
}
