using UniQueue.Messaging.Base;

namespace UniQueue.MessageTracing
{
	public interface IQueueTracer
	{
		public bool IsEnabled { get; }

		public Task WriteAsync(
			TraceEventType traceEventType,
			string infoToWrite,
			Message message,
			bool isIncludeMessageContent,
			Exception exception = default);

		public Task WriteAsync(
			TraceEventType traceEventType,
			string infoToWrite,
			Exception exception = default);
	}
}
