using System;

namespace WorkTimeReboot.Services.UserInput
{
	class UserInput : IUserInput
	{
		public string ReadLine()
		{
			return Console.ReadLine();
		}
	}
}
