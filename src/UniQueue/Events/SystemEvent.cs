namespace UniQueue.Events
{
	public class SystemEvent<TPayload>
	{
		public EventType Type { get; }
		public string EventDescription { get; }
		public TPayload Payload { get; }

		public SystemEvent(EventType type, string eventDescription, TPayload payload = default)
		{
			Type = type;
			EventDescription = eventDescription;
			Payload = payload;
		}
	}
}
