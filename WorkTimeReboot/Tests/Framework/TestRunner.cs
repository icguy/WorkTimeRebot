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
					Console.WriteLine($"test threw exception: {ex}");
					currentTestPassed = false;
				}

				if( !currentTestPassed )
				{
					Console.WriteLine($"=| FAILED {test.Name}");
					allTestsPassed = false;
				}
				else
				{
					Console.WriteLine($"=| OK     {test.Name}");
				}
			}

			Console.WriteLine();
			if( !allTestsPassed )
				Console.WriteLine("====== TESTS FAILED ======");
			else
				Console.WriteLine("====== TESTS PASSED ======");
		}
	}
}
