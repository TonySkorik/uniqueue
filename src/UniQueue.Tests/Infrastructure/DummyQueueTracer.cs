using System.Text;

using UniQueue.Helpers;
using UniQueue.MessageTracing;
using UniQueue.Messaging.Base;

namespace UniQueue.Tests.Infrastructure
{
	internal class DummyQueueTracer : IQueueTracer
	{
		public bool IsEnabled => true;

		public Task WriteAsync(
			TraceEventType traceEventType,
			string infoToWrite,
			Message message,
			bool isIncludeMessageContent,
			Exception exception = default)
		{
			var stringMessage = BuildMessage(
				traceEventType,
				infoToWrite,
				message.GetType(),
				message.Id,
				exception: exception,
				message: isIncludeMessageContent
					? message
					: null);

			Console.WriteLine(stringMessage);
			return Task.CompletedTask;
		}

		public Task WriteAsync(TraceEventType traceEventType, string infoToWrite, Type messageType, Guid messageId, Exception exception = default)
		{
			var stringMessage = BuildMessage(traceEventType, infoToWrite, messageType, messageId, exception: exception);
			Console.WriteLine(stringMessage);
			return Task.CompletedTask;
		}

		public Task WriteAsync(TraceEventType traceEventType, string infoToWrite, Exception exception = default)
		{
			var stringMessage = BuildMessage(traceEventType, infoToWrite);
			Console.WriteLine(stringMessage);
			return Task.CompletedTask;
		}

		private string BuildMessage(
			TraceEventType traceEventType,
			string infoToWrite,
			Type messageType,
			Guid messageId,
			Message message = default,
			Exception exception = default)
		{
			var messageBuilder = new StringBuilder($"({DateTime.Now.ToLogEventDateTimeString()}) [{traceEventType}] [Message type: {messageType?.ToString() ?? "unknown"}; id: {messageId}] {infoToWrite}");
			if (message != null)
			{
				messageBuilder.AppendLine(message.ToString());
			}

			if (exception != null)
			{
				messageBuilder.AppendLine("Exception:");
				messageBuilder.AppendLine(exception.ToString());
			}

			return messageBuilder.ToString();
		}

		private string BuildMessage(TraceEventType traceEventType, string infoToWrite)
			=> $"({DateTime.Now.ToLogEventDateTimeString()}) [{traceEventType}] {infoToWrite}";
	}
}
