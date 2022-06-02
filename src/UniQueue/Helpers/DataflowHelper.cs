using System.Threading.Tasks.Dataflow;

namespace UniQueue.Helpers
{
	public static class DataflowHelper
	{
		public static readonly DataflowLinkOptions DefaultLinkOptions = new()
		{
			PropagateCompletion = true
		};
	}
}
