namespace UniQueue.Helpers
{
	public static class DateTimeHelper
	{
		/// <summary>
		/// Converts passed <see cref="DateTime"/> to <c>yyyy-mm-dd hh_mm_ss</c> format
		/// </summary>
		/// <param name="dt"><see cref="DateTime"/> to convert</param>
		/// <param name="dateAndTimeSeparator">String used to separate dete amd time parts of the name</param>
		/// <returns></returns>
		public static string ToFilenameString(this DateTime dt, string dateAndTimeSeparator = " ")
		{
			return dt.ToString("s").Replace("T", dateAndTimeSeparator).Replace(":", "_");
		}

		/// <summary>
		/// Gets current <see cref="DateTime"/> in <c>yyyy-mm-dd hh_mm_ss</c> format
		/// </summary>
		/// <returns></returns>
		public static string GetFilenameString(string dateAndTimeSeparator = " ")
		{
			return DateTime.Now.ToFilenameString(dateAndTimeSeparator);
		}

		/// <summary>
		/// Converts passed <see cref="DateTime"/> to '<c>yyyy-mm-dd hh-mm-ss</c>' format
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <param name="dateAndTimeSeparator">The date and time separator.</param>
		/// <param name="timeSegmentsSeparator">The time segments separator. Default is "-" hh-mm-ss</param>
		/// <param name="dateSegmentsSeparator">The date segments separator. Default is "-" yyyy-mm-dd</param>
		/// <returns></returns>
		public static string ToDateTimeString(
			this DateTime dt,
			string dateAndTimeSeparator = " ",
			string timeSegmentsSeparator = "-",
			string dateSegmentsSeparator = "-")
		{
			return dt.ToString(
				$"yyyy{dateSegmentsSeparator}MM{dateSegmentsSeparator}dd{dateAndTimeSeparator}HH{timeSegmentsSeparator}mm{timeSegmentsSeparator}ss");
		}

		/// <summary>
		/// Converts passed <see cref="DateTime"/> to '<c>hh-mm-ss</c>' format
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <param name="timeSegmentsSeparator">The time segments separator. Default is "-" hh-mm-ss</param>
		/// <returns></returns>
		public static string ToTimeString(
			this DateTime dt,
			string timeSegmentsSeparator = "-")
		{
			return dt.ToString(
				$"HH{timeSegmentsSeparator}mm{timeSegmentsSeparator}ss");
		}

		/// <summary>
		/// Converts passed <see cref="DateTime"/> to '<c>hh-mm-ss</c>' format
		/// </summary>
		/// <param name="dt">The dt.</param>
		/// <param name="timeSegmentsSeparator">The time segments separator. Default is "-" : hh-mm-ss</param>
		/// <param name="milisecondsSegmantSeparator">The miliseconds time segment separator. Default is "." hh-mm-ss.fff </param>
		/// <returns></returns>
		public static string ToLongTimeString(
			this DateTime dt,
			string timeSegmentsSeparator = "-",
			string milisecondsSegmantSeparator = ".")
		{
			return dt.ToString(
				$"HH{timeSegmentsSeparator}mm{timeSegmentsSeparator}ss{milisecondsSegmantSeparator}fff");
		}

		public static string ToLogEventDateTimeString(this DateTime dt)
		{
			return dt.ToString("O").Replace("T", " ");
		}

		public static string GetXmlTimestamp()
		{
			//"2015-12-01T14:33:09+03:00"
			return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"); // zzz stands for +03:00
		}

	}
}
