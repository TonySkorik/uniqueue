using UniQueue.Messaging.Base;

namespace UniQueue.Messaging
{
	public class MessageHandlerExceptionMessage : ExceptionMessage
	{
		public Message MessageWithError { get; }
		public string HandlerQueueName { get; }

		public new const string DefaultHandlerGroupName = "HandlerExceptions";

		public MessageHandlerExceptionMessage(
			Message messageWithError,
			Exception exception,
			string handlerQueueName) : base(exception)
		{
			MessageWithError = messageWithError;
			HandlerQueueName = handlerQueueName;
			QueueName = DefaultHandlerGroupName;
		}

		public override string ToString()
		{
			return
				$"An exception happened during {HandlerQueueName} message handler invocation.{Environment.NewLine}{Exception}";
		}
	}
}
