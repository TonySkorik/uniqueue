using UniQueue.Errors;
using UniQueue.Messaging.Base;

namespace UniQueue.Messaging
{
	public class ErrorMessage : Message
	{
		public ErrorBase Error { get; }

		public const string DefaultHandlerGroupName = "Errors";

		public ErrorMessage(ErrorBase payload)
		{
			Error = payload;
			QueueName = DefaultHandlerGroupName;
		}
	}
}
