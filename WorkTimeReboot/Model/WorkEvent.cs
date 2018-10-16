using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WorkTimeReboot.Model
{
	public class WorkEvent
	{
		[JsonConverter(typeof(StringEnumConverter))]
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
