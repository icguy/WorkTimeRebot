using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTimeReboot.Timer
{
	public interface ITimer
	{
		double Interval { get; set; }
		event Action Tick;
		void Start();
		void Stop();
	}
}
