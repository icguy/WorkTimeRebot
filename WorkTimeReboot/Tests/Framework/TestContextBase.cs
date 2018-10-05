using System;
using System.Runtime.CompilerServices;

namespace WorkTimeReboot.Tests.Framework
{
	public class TestContextBase
	{
		public bool TestPassed { get; private set; } = true;

		public void ExpectEqual(object a, object b, [CallerLineNumber] int lineNum = 0)
		{
			if( !a.Equals(b) )
			{
				this.TestPassed = false;
				Console.WriteLine($"equality failed: {a} should equal {b} on line {lineNum}");
			}
		}
	}
}
