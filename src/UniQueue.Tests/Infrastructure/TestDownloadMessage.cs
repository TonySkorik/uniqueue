using UniQueue.Messaging.Base;

namespace UniQueue.Tests.Infrastructure
{
	internal class TestDownloadMessage<TPayload> : Message
	{
		public TPayload Payload { get; }

		public string Content { set; get; }

		public TestDownloadMessage(TPayload payload)
		{
			Payload = payload;
		}
	}
}
