using System;

namespace WorkTimeReboot.Tests.Framework
{
	static class TestHelper
	{
		public static DateTime GetDateHours(int number)
		{
			return new DateTime(2018, 07, 05, 06, 00, 00).AddHours(number);
		}
	}
}
