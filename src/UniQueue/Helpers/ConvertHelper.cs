namespace UniQueue.Helpers
{
	public static class ConvertHelper
	{
		public static TParsed Parse<TParsed, TActual>(string value, Func<string, TActual> parser)
		{
			var parsed = parser(value);
			return (TParsed) Convert.ChangeType(parsed, typeof(TParsed));
		}

		public static T ParseAs<T>(this string value, T defaultValue = default)
		{
			if (value == null)
			{
				return defaultValue;
			}

			if (typeof(T) == typeof(int))
			{
				return Parse<T, int>(value, int.Parse);
			}

			if (typeof(T) == typeof(double))
			{
				return Parse<T, double>(value, double.Parse);
			}

			if (typeof(T) == typeof(string))
			{
				return Parse<T, string>(value, s => s);
			}

			if (typeof(T) == typeof(bool))
			{
				return Parse<T, bool>(value, bool.Parse);
			}

			if (typeof(T) == typeof(DateTime))
			{
				return Parse<T, DateTime>(value, DateTime.Parse);
			}

			if (typeof(T) == typeof(TimeSpan))
			{
				return Parse<T, TimeSpan>(value, TimeSpan.Parse);
			}

			if (typeof(T) == typeof(DateTimeOffset))
			{
				return Parse<T, DateTimeOffset>(value, DateTimeOffset.Parse);
			}

			throw new InvalidOperationException($"Setting type {typeof(T)} is not supported.");
		}
	}
}
