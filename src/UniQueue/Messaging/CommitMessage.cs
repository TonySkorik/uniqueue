using UniQueue.Messaging.Base;

namespace UniQueue.Messaging
{
	public class CommitMessage : Message
	{
		public Message MessageToCommit { get; }

		public CommitMessage(Message messageToCommit)
		{
			MessageToCommit = messageToCommit;
		}
	}
}
