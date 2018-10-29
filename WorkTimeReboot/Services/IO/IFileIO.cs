namespace WorkTimeReboot.Services.IO
{
	public interface IFileIO<T>
	{
		T ReadFromFile();
		void WriteToFile(T events);
	}
}
