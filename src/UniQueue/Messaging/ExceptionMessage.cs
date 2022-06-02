using UniQueue.Messaging.Base;

namespace UniQueue.Messaging
{
	public class ExceptionMessage : Message
	{
		public Exception Exception { get; }


		public const string DefaultHandlerGroupName = "Exceptions";
		
		public ExceptionMessage(Exception exception)
		{
			Exception = exception;
		}
	}
}
