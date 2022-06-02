using UniQueue.Events;
using UniQueue.Messaging.Base;

namespace UniQueue.Messaging
{
	public class EventMessage<TPayload> : Message
	{
		public SystemEvent<TPayload> Event { get; }

		public bool IsDisplayInUi { get; }

		public const string DefaultHandlerGroupName = "SystemEvents";

		public EventMessage(SystemEvent<TPayload> payload, bool isDisplayInUi)
		{
			Event = payload;
			IsDisplayInUi = isDisplayInUi;
			QueueName = DefaultHandlerGroupName;
		}
	}
}
