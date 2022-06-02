using UniQueue.Messaging.Base;

namespace UniQueue.Tests.Infrastructure
{
	internal class TestMessage<TPayload> : Message
	{
		public TPayload Payload { get; }

		public TestMessage(TPayload payload)
		{
			Payload = payload;
		}
	}
}
