namespace UniQueue.Abstractions
{
	public interface IQueueKeeper
	{
		Task<bool> Create(string queueName);

		Task<bool> Enqueue<TItem>(string queueName, TItem item);

		Task<bool> Commit(string queueName, Guid messageId);
	}
}
