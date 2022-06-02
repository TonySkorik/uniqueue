namespace UniQueue.Errors
{
	public abstract class ErrorBase
	{
		public string Message { get; }
		public bool IsWarning { get; }
		public bool IsException => Exception != null;

		public Exception Exception { get; }

		protected ErrorBase(string message, bool isWarning = false, Exception exception = null)
		{
			Message = message;
			IsWarning = isWarning;
			Exception = exception;
		}

		public override string ToString()
		{
			return $"{(IsWarning ? "[Warning] " : "[Error] ")} Message :{Environment.NewLine}{Message}";
		}
	}
}
