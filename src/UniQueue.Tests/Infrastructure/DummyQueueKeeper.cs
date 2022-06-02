using ConcurrentCollections;

using UniQueue.Abstractions;

namespace UniQueue.Tests.Infrastructure
{
	internal class DummyQueueKeeper : IQueueKeeper
	{
		private int _committedCount;
		private int _enqueuedCount;
		private readonly ConcurrentHashSet<Guid> _alreadyCommitted = new();

		public int CommittedCount => _committedCount;
		public int EnqueuedCount => _enqueuedCount;

		public Task<bool> Create(string queueName)
		{
			return Task.FromResult(true);
		}

		public Task<bool> Enqueue<TItem>(string queueName, TItem item)
		{
			Interlocked.Increment(ref _enqueuedCount);
			return Task.FromResult(true);
		}

		public Task<bool> Commit(string queueName, Guid messageId)
		{
			if (_alreadyCommitted.Contains(messageId))
			{
				return Task.FromResult(true);
			}

			Interlocked.Increment(ref _committedCount);

			_alreadyCommitted.Add(messageId);

			return Task.FromResult(true);
		}
	}
}
