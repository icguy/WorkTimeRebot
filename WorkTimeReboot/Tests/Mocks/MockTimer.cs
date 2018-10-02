using System;
using WorkTimeReboot.Services.Timer;

namespace WorkTimeReboot.Tests.Mocks
{
	class MockTimer : ITimer
	{
		public double Interval { get; set; }

		public void DoTick() => this.Tick();
		public Action OnStart { get; set; }
		public Action OnStop { get; set; }

		public event Action Tick;

		public void Start()
		{
			if( this.OnStart != null )
				this.OnStart();
		}

		public void Stop()
		{
			if( this.OnStop != null )
				this.OnStop();
		}
	}
}
