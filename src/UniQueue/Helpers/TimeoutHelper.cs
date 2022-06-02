namespace UniQueue.Helpers
{
	/// <summary>
	/// Helper calss for working with timeout
	/// </summary>
	public static class TimeoutHelper
	{
		/// <summary>
		/// Converts specified TimeSpan to <c>Task.Delay</c>.
		/// </summary>
		/// <param name="timeout">The timeout.</param>
		/// <returns></returns>
		public static Task ToDelay(this TimeSpan timeout)
		{
			return Task.Delay(timeout);
		}

		/// <summary>
		/// Executes the target task with timeout.
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <param name="successAction">The success action.</param>
		/// <param name="timeoutAction">The timeout action.</param>
		/// <returns></returns>
		public static async Task ExecuteWithTimeout<TResult>(
			this Task<TResult> task,
			TimeSpan timeout,
			Action<TResult> successAction,
			Action timeoutAction)
		{
			if (task == await Task.WhenAny(timeout.ToDelay(), task))
			{
				successAction(task.Result);
			}
			else
			{
				timeoutAction();
			}
		}

		/// <summary>
		/// Executes the target task with specified timeout. If timeout passes prior to the target task completion throws <see cref="TimeoutException"/>
		/// </summary>
		/// <typeparam name="TResult">The type of the target task result.</typeparam>
		/// <param name="task">The task.</param>
		/// <param name="timeout">The timeout.</param>
		/// <returns></returns>
		/// <exception cref="System.TimeoutException">Thrown when timeout passes prior to the target task completion.</exception>
		public static async Task<TResult> ExecuteWithTimeout<TResult>(
			this Task<TResult> task,
			TimeSpan timeout)
		{
			if (task == await Task.WhenAny(timeout.ToDelay(), task))
			{
				return await task; // use second await instead of .Result to unwrap AggreagteException
			}

			throw new TimeoutException();
		}
	}
}
