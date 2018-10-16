namespace WorkTimeReboot.Services.UserIO
{
	public interface IUserIO
	{
		string ReadLine();
		void WriteLine();
		void WriteLine(object o);
		void WriteError(object o);
		void Clear();
	}
}
