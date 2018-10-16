using System;

namespace WorkTimeReboot.Model
{
	public class WorkEvent
	{
		public EventType Type = EventType.Unknown;
		public DateTime Time = DateTime.MinValue;

		public override string ToString()
		{
			string typeChar;
			switch( this.Type )
			{
				case EventType.Arrival:
					typeChar = "A";
					break;
				case EventType.Departure:
					typeChar = "D";
					break;
				case EventType.Unknown:
				default:
					typeChar = "U";
					break;
			}
			return $"{typeChar} {this.Time}";
		}
	}
}
