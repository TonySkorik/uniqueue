using System.Text.Json;
using System.Text.Json.Serialization;

namespace UniQueue.Messaging.Base
{
	public abstract class Message
	{
		public Guid Id { get; } = Guid.NewGuid();
		public string QueueName { set; get; }

		public TimeSpan Ttl { set; get; } = TimeSpan.FromMinutes(15);
		public DateTime? TakenAt { set; get; }
		public DateTime? EnqueuedAt { set; get; }
		public bool IsExpired => TakenAt != null && DateTime.Now >= TakenAt.Value.Add(Ttl);

		public bool IsPoisoned { set; get; }

		public override string ToString() => $"Message Id: {Id}";

		public T As<T>()
			where T : Message =>
			(T)this;

		public virtual string Save() =>
			JsonSerializer.Serialize(
				this,
				new JsonSerializerOptions()
				{
					WriteIndented = true,
					DefaultIgnoreCondition = JsonIgnoreCondition.Never,
					IgnoreReadOnlyProperties = false
				});

		public virtual T Load<T>(string serialized) 
			where T : Message =>
			JsonSerializer.Deserialize<T>(
				serialized,
				new JsonSerializerOptions()
				{
					WriteIndented = true,
					DefaultIgnoreCondition = JsonIgnoreCondition.Never,
					IgnoreReadOnlyProperties = false
				});
	}
}
