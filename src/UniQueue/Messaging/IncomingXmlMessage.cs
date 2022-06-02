using System.Xml.Linq;
using UniQueue.Helpers;
using UniQueue.Messaging.Base;

namespace UniQueue.Messaging
{
	public class IncomingXmlMessage : Message
	{
		#region Props

		public XDocument Xml { get; private set; }

		public bool IsMalformed { get; private set; }
		
		public string NamespaceUri { get; private set; }

		#endregion

		#region Ctor - private

		protected IncomingXmlMessage()
		{ }

		#endregion

		#region Factory methods

		public static IncomingXmlMessage Parse(XDocument payload)
		{
			IncomingXmlMessage ret = new()
			{
				Xml = payload
			};

			return ret;
		}

		#endregion
	}
}
