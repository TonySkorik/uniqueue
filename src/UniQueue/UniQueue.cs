using System.Threading.Tasks.Dataflow;

using UniQueue.Abstractions;
using UniQueue.Helpers;
using UniQueue.MessageTracing;
using UniQueue.Messaging;
using UniQueue.Messaging.Base;

namespace UniQueue
{
	/// <summary>
	/// Represents Producer - Consumer queue built on top of TPL Dataflow.
	/// </summary>
	public class UniQueue
	{
		#region Private

		private readonly BufferBlock<Message> _messagesBuffer = new();
		private readonly List<ActionBlock<Message>> _messagesCommitBlocks = new();

		private readonly BufferBlock<Message> _exceptionBuffer = new();
		private readonly List<ActionBlock<Message>> _exceptionCommitBlocks = new();

		private readonly IQueueKeeper _queueKeeper;
		private readonly CancellationToken _cancellationToken;
		private readonly IQueueTracer _tracer;
		private readonly HashSet<Type> _handledMessageTypes = new();
		
		private bool _isBuilt;
		private Dictionary<string, HandlerChainBuilder> _handlerBuiders = new();
		private Dictionary<string, HandlerChainBuilder> _exceptionHandlerBuiders = new();
		private int _totalMessagesInQueue = 0;


		#endregion

		#region props

		public int ToalMessageInQueue => _totalMessagesInQueue;

		#endregion

		#region Ctor

		public UniQueue(IQueueKeeper queueKeeper, CancellationToken cancellationToken, IQueueTracer tracer)
		{
			_queueKeeper = queueKeeper;
			_cancellationToken = cancellationToken;
			_tracer = tracer;
		}

		#endregion

		#region Builder pattern

		public void Build()
		{
			if (_handlerBuiders.Count == 0)
			{
				throw new InvalidOperationException("This UniQueue instance has no handlers.");
			}

			foreach (var builder in _handlerBuiders)
			{
				(TransformBlock<Message, Message> head,
					Predicate<Message> headPredicate,
					TransformBlock<Message, Message> tail) = builder.Value.Build();

				var commitBlock = CreateCommitBlock<Message>();

				tail.LinkTo(commitBlock, DataflowHelper.DefaultLinkOptions);
				_messagesCommitBlocks.Add(commitBlock);

				_messagesBuffer.LinkTo(head, DataflowHelper.DefaultLinkOptions, headPredicate);
			}

			//build exception handling chains
			foreach (var exceptionHandlerBuilder in _exceptionHandlerBuiders)
			{
				(TransformBlock<Message, Message> head,
					Predicate<Message> headPredicate,
					TransformBlock<Message, Message> tail) = exceptionHandlerBuilder.Value.Build();

				var commitBlock = CreateCommitBlock<Message>();

				tail.LinkTo(commitBlock, DataflowHelper.DefaultLinkOptions);
				_exceptionCommitBlocks.Add(commitBlock);

				_exceptionBuffer.LinkTo(head, DataflowHelper.DefaultLinkOptions, headPredicate);
			}

			_isBuilt = true;
			_exceptionHandlerBuiders = null;
			_handlerBuiders = null;
		}

		#region Methods for adding synchronous message handlers

		public UniQueue AddHandler(
			string handlerGroupName,
			Func<Message, Message> handler,
			int maxDegreeOfParallelism,
			Predicate<Message> handlePredicate = null)
		{
			EnsureIsNotBuilt();
			EnsureBuilderExists(_handlerBuiders, handlerGroupName, handlePredicate);

			_handlerBuiders[handlerGroupName].AddHandler(handler, maxDegreeOfParallelism, handlePredicate);

			return this;
		}

		public UniQueue AddHandler<TMessage>(
			string handlerGroupName,
			Func<TMessage, TMessage> handler,
			int maxDegreeOfParallelism,
			Predicate<TMessage> handlePredicate = null)
			where TMessage : Message
		{
			EnsureIsNotBuilt();

			Predicate<Message> combinedPredicate = CreateTypedMessagePredicate(handlePredicate);

			EnsureBuilderExists(_handlerBuiders, handlerGroupName, combinedPredicate);

			Func<Message, Message> typedHandler = CombineTyped(handler);

			_handlerBuiders[handlerGroupName].AddHandler(typedHandler, maxDegreeOfParallelism, combinedPredicate);

			_handledMessageTypes.Add(typeof(TMessage));

			return this;
		}

		public UniQueue AddHandler<TMessage>(
			string handlerGroupName,
			Action<TMessage> handler,
			int maxDegreeOfParallelism,
			Predicate<TMessage> handlePredicate = null)
			where TMessage : Message
		{
			EnsureIsNotBuilt();

			Predicate<Message> combinedPredicate = CreateTypedMessagePredicate(handlePredicate);

			EnsureBuilderExists(_handlerBuiders, handlerGroupName, combinedPredicate);

			Func<Message, Message> typedHandler = CombineTyped(handler);

			_handlerBuiders[handlerGroupName].AddHandler(typedHandler, maxDegreeOfParallelism, combinedPredicate);

			_handledMessageTypes.Add(typeof(TMessage));

			return this;
		}

		#endregion

		#region Methods for adding asynchronous message handlers

		public UniQueue AddAsyncHandler(
			string handlerGroupName,
			Func<Message, Task<Message>> handler,
			int maxDegreeOfParallelism,
			Predicate<Message> handlePredicate = null)
		{
			EnsureIsNotBuilt();
			EnsureBuilderExists(_handlerBuiders, handlerGroupName, handlePredicate);

			_handlerBuiders[handlerGroupName].AddHandler(handler, maxDegreeOfParallelism, handlePredicate);

			return this;
		}

		public UniQueue AddAsyncHandler<TMessage>(
			string handlerGroupName,
			Func<TMessage, Task<TMessage>> handler,
			int maxDegreeOfParallelism,
			Predicate<TMessage> handlePredicate = null)
			where TMessage : Message
		{
			EnsureIsNotBuilt();

			Predicate<Message> combinedPredicate = CreateTypedMessagePredicate(handlePredicate);

			EnsureBuilderExists(_handlerBuiders, handlerGroupName, combinedPredicate);

			Func<Message, Task<Message>> typedHandler = CombineAsyncTyped(handler);

			_handlerBuiders[handlerGroupName].AddHandler(typedHandler, maxDegreeOfParallelism, combinedPredicate);

			_handledMessageTypes.Add(typeof(TMessage));

			return this;
		}

		public UniQueue AddAsyncHandler<TMessage>(
			string handlerGroupName,
			Func<TMessage, Task> handler,
			int maxDegreeOfParallelism,
			Predicate<TMessage> handlePredicate = null)
			where TMessage : Message
		{
			EnsureIsNotBuilt();

			Predicate<Message> combinedPredicate = CreateTypedMessagePredicate(handlePredicate);

			EnsureBuilderExists(_handlerBuiders, handlerGroupName, combinedPredicate);

			Func<Message, Task<Message>> typedHandler = CombineAsyncTyped(handler);

			_handlerBuiders[handlerGroupName].AddHandler(typedHandler, maxDegreeOfParallelism, combinedPredicate);

			_handledMessageTypes.Add(typeof(TMessage));

			return this;
		}

		#endregion

		#region Methods for adding exception handlers

		public UniQueue AddAsyncExceptionHandler<TExceptionMessage>(
			Func<TExceptionMessage, Task> handler,
			int maxDegreeOfParallelism)
			where TExceptionMessage : ExceptionMessage
		{
			EnsureIsNotBuilt();
			EnsureBuilderExists(_exceptionHandlerBuiders, typeof(TExceptionMessage).Name, m => m is TExceptionMessage);

			Func<Message, Task<Message>> typedHandler = CombineAsyncTyped(handler);

			_exceptionHandlerBuiders[typeof(TExceptionMessage).Name].AddHandler(typedHandler, maxDegreeOfParallelism);

			_handledMessageTypes.Add(typeof(TExceptionMessage));

			return this;
		}

		public UniQueue AddExceptionHandler<TExceptionMessage>(
			Action<TExceptionMessage> handler,
			int maxDegreeOfParallelism)
			where TExceptionMessage : ExceptionMessage
		{
			EnsureIsNotBuilt();
			EnsureBuilderExists(_exceptionHandlerBuiders, typeof(TExceptionMessage).Name, m => m is TExceptionMessage);

			Func<Message, Message> typedHandler = CombineTyped(handler);

			_exceptionHandlerBuiders[typeof(TExceptionMessage).Name].AddHandler(typedHandler, maxDegreeOfParallelism);

			_handledMessageTypes.Add(typeof(TExceptionMessage));

			return this;
		} 

		#endregion

		#endregion

		#region Queue methods

		private async Task CommitMessage<T>(T message)
			where T : Message
		{
			EnsureIsBuilt();

			var commitSuccess = await _queueKeeper.Commit(message.QueueName, message.Id);
			if (commitSuccess)
			{
				Interlocked.Decrement(ref _totalMessagesInQueue);
				await WriteTrace(TraceEventType.Info, "Committed a message.", message);
			}
			else
			{
				await WriteTrace(TraceEventType.Error, "Failed to commit a message.", message);
			}
		}

		public async Task<bool> Enqueue(Message message)
		{
			EnsureIsBuilt();

			var messageType = message.GetType();
			if (!_handledMessageTypes.Contains(messageType))
			{
				await WriteTrace(
					TraceEventType.Warning,
					$"No handler registered for messages of type {messageType}. Queue may never complete.",
					message);
			}

			message.EnqueuedAt = DateTime.Now;

			var keeperEnqueueSuccess = await _queueKeeper.Enqueue(message.QueueName, message);
			var bufferSendSuccess = await _messagesBuffer.SendAsync(message, _cancellationToken);
			var success = keeperEnqueueSuccess && bufferSendSuccess;

			if (success)
			{
				Interlocked.Increment(ref _totalMessagesInQueue);
				await WriteTrace(TraceEventType.Info, "Enqueued a message.", message);
			}
			else
			{
				await WriteTrace(TraceEventType.Error, $"Failed to enqueue a message. Buffer send success : {bufferSendSuccess}, QueueKeeper send success : {keeperEnqueueSuccess}", message);
			}

			return success;
		}

		public async Task<bool> EnqueueException(ExceptionMessage message)
		{
			EnsureIsBuilt();

			var messageType = message.GetType();
			if (!_handledMessageTypes.Contains(messageType))
			{
				await WriteTrace(
					TraceEventType.Warning,
					$"No handler registered for exception messages of type {messageType}. Queue may never complete.",
					message);
			}

			message.EnqueuedAt = DateTime.Now;

			var keeperEnqueueSuccess = await _queueKeeper.Enqueue(message.QueueName, message);
			var bufferSendSuccess = await _exceptionBuffer.SendAsync(message, _cancellationToken);
			var success = keeperEnqueueSuccess && bufferSendSuccess;

			if (success)
			{
				Interlocked.Increment(ref _totalMessagesInQueue);
				await WriteTrace(TraceEventType.Info, "Enqueued an exception message.", message);
			}
			else
			{
				await WriteTrace(TraceEventType.Error, $"Failed to enqueue an exception message. Buffer send success : {bufferSendSuccess}, QueueKeeper send success : {keeperEnqueueSuccess}", message);
			}

			return success;
		}

		public async Task Complete(Task externalCompletion = null)
		{
			EnsureIsBuilt();

			await WriteInfoMessageTrace(TraceEventType.Info, "Started queue completion procedure.");

			if (externalCompletion != null)
			{
				await WriteInfoMessageTrace(TraceEventType.Info, "Started waiting for external completion task.");
				await externalCompletion;
				await WriteInfoMessageTrace(TraceEventType.Info, "Finished waiting for external completion task.");
			}

			await WriteInfoMessageTrace(TraceEventType.Info, "Started completing messages buffer.");
			_messagesBuffer.Complete();
			await Task.WhenAll(_messagesCommitBlocks.Select(b => b.Completion));
			await WriteInfoMessageTrace(TraceEventType.Info, "Completed messages buffer.");

			await WriteInfoMessageTrace(TraceEventType.Info, "Started completing exceptions buffer.");
			_exceptionBuffer.Complete();
			await Task.WhenAll(_exceptionCommitBlocks.Select(b => b.Completion));
			await WriteInfoMessageTrace(TraceEventType.Info, "Completed exceptions buffer.");

			await WriteInfoMessageTrace(TraceEventType.Info, "Finished queue completion procedure.");
		}

		#endregion

		#region Methods for typed predicates combination

		private Predicate<Message> CreateTypedMessagePredicate<TMessage>(Predicate<TMessage> originalPredicate)
			where TMessage : Message
			=> (m) => m is TMessage tm && (originalPredicate?.Invoke(tm) ?? true);

		private Func<Message, Task<Message>> CombineAsyncTyped<TTyped>(Func<TTyped, Task<TTyped>> typedProcessor)
			where TTyped : Message
		{
			Func<Message, Task<Message>> combinedTyped = async (m) =>
			{
				var typedMessage = m.As<TTyped>();
				var typedResult = await typedProcessor(typedMessage);
				return typedResult;
			};

			return combinedTyped;
		}

		private Func<Message, Task<Message>> CombineAsyncTyped<TTyped>(Func<TTyped, Task> typedProcessor)
			where TTyped : Message
		{
			Func<Message, Task<Message>> combinedTyped = async (m) =>
			{
				var typedMessage = m.As<TTyped>();
				await typedProcessor(typedMessage);
				return typedMessage;
			};

			return combinedTyped;
		}

		private Func<Message, Message> CombineTyped<TTyped>(Func<TTyped, TTyped> typedProcessor)
			where TTyped : Message
		{
			Func<Message, Message> combinedTyped = (m) =>
			{
				var typedMessage = m.As<TTyped>();
				var typedResult = typedProcessor(typedMessage);
				return typedResult;
			};

			return combinedTyped;
		}

		private Func<Message, Message> CombineTyped<TTyped>(Action<TTyped> typedProcessor)
			where TTyped : Message
		{
			Func<Message, Message> combinedTyped = (m) =>
			{
				var typedMessage = m.As<TTyped>();
				typedProcessor(typedMessage);
				return typedMessage;
			};

			return combinedTyped;
		}

		#endregion

		#region Methods for ensuring building process states

		private void EnsureBuilderExists(
			Dictionary<string, HandlerChainBuilder> buildersCollection,
			string handlerGroupName,
			Predicate<Message> handlePredicate = null)
		{
			if (!buildersCollection.ContainsKey(handlerGroupName))
			{
				buildersCollection.Add(
					handlerGroupName,
					new HandlerChainBuilder(handlerGroupName, _cancellationToken, handlePredicate, this));
			}
		}

		private bool EnsureIsBuilt() // this method returns any value at all only to use switch expression inside
			=> _isBuilt switch
			{
				true => true,
				false => throw new InvalidOperationException(
					$"This UniQueue instance is not yet built. Use {nameof(Build)} method to build queue.")

			};

		private bool EnsureIsNotBuilt() // this method returns any value at all only to use switch expression inside
			=> _isBuilt switch
			{
				false => true,
				true => throw new InvalidOperationException(
					$"This UniQueue instance is already built. Can't modify handling chains.")

			};

		#endregion

		#region Methods for tracing messages

		private async Task WriteTrace(TraceEventType eventType, string infoText, Message message, bool isIncludeMessageContent = false, Exception exception = default)
		{
			if (_tracer != null && _tracer.IsEnabled)
			{
				await _tracer.WriteAsync(eventType, infoText, message, isIncludeMessageContent, exception);
			}
		}

		private async Task WriteInfoMessageTrace(TraceEventType eventType, string infoText)
		{
			if (_tracer != null && _tracer.IsEnabled)
			{
				await _tracer.WriteAsync(eventType, infoText);
			}
		}

		#endregion

		#region Service methods

		private ActionBlock<T> CreateCommitBlock<T>()
			where T : Message
			=> new(
				CommitMessage,
				new ExecutionDataflowBlockOptions()
				{
					CancellationToken = _cancellationToken,
					MaxDegreeOfParallelism = Environment.ProcessorCount
				});
		
		#endregion
	}
}
