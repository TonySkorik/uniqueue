using System.Threading.Tasks.Dataflow;
using UniQueue.Helpers;
using UniQueue.MessageHandlers;
using UniQueue.Messaging;
using UniQueue.Messaging.Base;

namespace UniQueue
{
	/// <summary>
	/// A builder class for crating <see cref="Message"/> handling chains.
	/// </summary>
	internal class HandlerChainBuilder
	{
		#region Private
		
		private TransformBlock<Message, Message> _head;
		private TransformBlock<Message, Message> _tail;
		private readonly CancellationToken _cancellationToken;
		private readonly Predicate<Message> _startPredicate;
		private readonly UniQueue _queueInstance;
		private readonly string _handlerGroupName;

		#endregion

		#region Ctor
		
		public HandlerChainBuilder(string handlerGroupName, CancellationToken cancellationToken, Predicate<Message> startPredicate, UniQueue queueInstance)
		{
			_cancellationToken = cancellationToken;
			_startPredicate = startPredicate;
			_queueInstance = queueInstance;
			_handlerGroupName = handlerGroupName;
		}

		#endregion

		#region Methods for adding message handlers
		
		public void AddHandler(Func<Message, Task<Message>> handler, int maxDegreeOfParallelism, Predicate<Message> predicate = null)
		{
			var block = CreateBlock(handler, maxDegreeOfParallelism);
			AddHandlerCore(block, predicate);
		}

		public void AddHandler(Func<Message, Message> handler, int maxDegreeOfParallelism, Predicate<Message> predicate = null)
		{
			var block = CreateBlock(handler, maxDegreeOfParallelism);
			AddHandlerCore(block, predicate);
		}

		private void AddHandlerCore(
			TransformBlock<Message, Message> block,
			Predicate<Message> predicate = null)
		{
			
			if (_head == null)
			{
				_head = block;
				_tail = _head;
			}
			else
			{
				var newBlock = block;
				if (predicate != null)
				{
					_head.LinkTo(newBlock, DataflowHelper.DefaultLinkOptions, predicate);
				}
				else
				{
					_head.LinkTo(newBlock, DataflowHelper.DefaultLinkOptions);
				}

				_tail = newBlock;
			}
		}

		#endregion

		#region Methods for building handler chain
		
		public (TransformBlock<Message, Message> head, Predicate<Message> headPredicate,
			TransformBlock<Message, Message> tail) Build()
		{
			return (_head, _startPredicate, _tail);
		}

		#endregion

		#region Methods for actual block creation

		private TransformBlock<Message, Message> CreateBlock(Func<Message, Message> handler, int maxDegreeOfParallelism)
			=> new(
				WrapInTry(handler),
				new ExecutionDataflowBlockOptions()
				{
					CancellationToken = _cancellationToken,
					MaxDegreeOfParallelism = maxDegreeOfParallelism,
					EnsureOrdered = false,
					TaskScheduler = TaskScheduler.Default
				});
		
		private TransformBlock<Message, Message> CreateBlock(
			Func<Message, Task<Message>> handler,
			int maxDegreeOfParallelism)
			=> new(
				WrapInTry(handler),
				new ExecutionDataflowBlockOptions()
				{
					CancellationToken = _cancellationToken,
					MaxDegreeOfParallelism = maxDegreeOfParallelism,
					EnsureOrdered = false,
					TaskScheduler = TaskScheduler.Default
				});

		#endregion

		#region Service methods
		
		private Func<Message, Task<Message>> WrapInTry(AsyncMessageHandler handler)
		{
			return async m =>
			{
				try
				{
					await handler.LogicAsync.Invoke(m);
				}
				catch (Exception ex)
				{
					var success = await _queueInstance.EnqueueException(new MessageHandlerExceptionMessage(m, ex, _handlerGroupName));
				}

				return m;
			};
		}

		private Func<Message, Message> WrapInTry(MessageHandler handler)
		{
			return m =>
			{
				try
				{
					handler.Logic.Invoke(m);
				}
				catch (Exception ex)
				{
					m.IsPoisoned = true;
					var success = _queueInstance.EnqueueException(new MessageHandlerExceptionMessage(m, ex, _handlerGroupName)).GetAwaiter().GetResult();
				}

				return m;
			};
		}

		#endregion
	}
}
