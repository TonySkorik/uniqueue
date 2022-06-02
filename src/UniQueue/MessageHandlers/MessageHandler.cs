using UniQueue.Messaging;
using UniQueue.Messaging.Base;

namespace UniQueue.MessageHandlers
{
	/// <summary>
	/// Represents a message handler which short circuits on <see cref="MessageHandlerExceptionMessage"/> and just forwards such messages.
	/// This class holds synchronous logic.
	/// </summary>
	internal class MessageHandler
	{
		public Func<Message, Message> Logic { get; }

		private MessageHandler(Func<Message, Message> logic)
		{
			Logic = logic;
		}

		public static implicit operator MessageHandler(Func<Message, Message> handlerLogic)
		{
			var handler = new MessageHandler(m =>
			{
				if (m.IsPoisoned)
				{
					return m;
				}

				return handlerLogic(m);
			});
			return handler;
		}
	}
}
