using UniQueue.Messaging;
using UniQueue.Messaging.Base;

namespace UniQueue.MessageHandlers
{
	/// <summary>
	/// Represents a message handler which short circuits on <see cref="MessageHandlerExceptionMessage"/> and just forwards such messages.
	/// This class holds asynchronous logic.
	/// </summary>
	internal class AsyncMessageHandler
	{
		public Func<Message, Task<Message>> LogicAsync { get; }

		private AsyncMessageHandler(Func<Message, Task<Message>> asyncLogic)
		{
			LogicAsync = asyncLogic;
		}

		public static implicit operator AsyncMessageHandler(Func<Message, Task<Message>> asyncHandlerLogic)
		{
			var handler = new AsyncMessageHandler(async m =>
			{
				if (m.IsPoisoned)
				{
					return m;
				}

				return await asyncHandlerLogic(m);
			});

			return handler;
		}
	}
}
