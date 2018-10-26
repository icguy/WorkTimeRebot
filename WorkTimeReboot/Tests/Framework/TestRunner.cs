using System;
using System.Linq;
using System.Reflection;

namespace WorkTimeReboot.Tests.Framework
{
	static class TestRunner
	{
		public static void RunTests(object testClass)
		{
			var tests = testClass.GetType().GetRuntimeMethods().Where(m => m.GetCustomAttributes<TestAttribute>().Any());
			var allTestsPassed = true;
			foreach( var test in tests )
			{
				bool currentTestPassed = true;
				try
				{
					currentTestPassed = (bool)test.Invoke(testClass, new object[] { });
				}
				catch( Exception ex )
				{
					WriteLineError($"test threw exception: {ex}");
					currentTestPassed = false;
				}

				if( !currentTestPassed )
				{
					WriteLineError($"=| FAILED {test.Name}");
					allTestsPassed = false;
				}
				else
				{
					WriteLineSuccess($"=| OK     {test.Name}");
				}
			}

			Console.WriteLine();
			if( !allTestsPassed )
				WriteLineError("====== TESTS FAILED ======");
			else
				WriteLineSuccess("====== TESTS PASSED ======");
		}

		private static void WriteLineSuccess(string text)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		private static void WriteLineError(string text)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
}
