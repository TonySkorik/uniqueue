using System.Net;
using System.Xml.Linq;
using FluentAssertions;

using UniQueue.Messaging;
using UniQueue.Tests.Infrastructure;
using UniQueue.Tests.Resources;

namespace UniQueue.Tests
{
	[TestClass]
	public class UniQueueTests
	{
		private UniQueue _queue;
		private DummyQueueKeeper _queueKeeper;

		[TestInitialize]
		public void Init()
		{
			_queueKeeper = new DummyQueueKeeper();
			var queueTracer = new DummyQueueTracer();
			_queue = new UniQueue(_queueKeeper, CancellationToken.None, queueTracer);
		}

		[TestMethod]
		public async Task OneHandlerTest_ShouldComplete()
		{
			_queue
				.AddHandler<IncomingXmlMessage>(
					"Malformed XML",
					m =>
					{
						Console.WriteLine($"No content! {m.As<IncomingXmlMessage>().IsMalformed}");
					},
					1,
					m => m.Xml == null
				)
				.AddHandler<IncomingXmlMessage>(
					"Content XML",
					m =>
					{
						Console.WriteLine($"Has content! {m.As<IncomingXmlMessage>().NamespaceUri}");
					},
					1,
					m => m.Xml != null
				);

			_queue.Build();

			await _queue.Enqueue(IncomingXmlMessage.Parse(XDocument.Parse(Messages.TestXmlMessage)));

			await _queue.Enqueue(IncomingXmlMessage.Parse(null));

			await _queue.Complete();

			_queueKeeper.CommittedCount.Should().Be(2);
		}

		[TestMethod]
		public async Task SeveralHandlersTest_ShouldComplete()
		{
			TaskCompletionSource<bool> tcs = new();

			_queue
				.AddAsyncHandler<TestMessage<Uri>>(
					"URL Data Download",
					async m =>
					{
						HttpClient cl = new();

						var downloaded = await cl.GetStringAsync(m.Payload);

						await _queue.Enqueue(new TestMessage<string>(downloaded));

						tcs.SetResult(true);
					},
					1,
					m => m.Payload != null
				)
				.AddHandler<TestMessage<string>>(
					"Downloaded content handling",
					m =>
					{
						Console.Out.WriteLine(m.Payload.Substring(0, 100));
					},
					1);

			_queue.Build();

			await _queue.Enqueue(new TestMessage<Uri>(new Uri("http://ya.ru")));

			await _queue.Complete(tcs.Task);

			_queueKeeper.CommittedCount.Should().Be(2);
		}
		
		[TestMethod]
		public async Task SeveralHandlersTest_ShouldHandleException()
		{
			int exceptionCount = 0;

			_queue
				.AddAsyncHandler<TestDownloadMessage<Uri>>(
					"URL Data Download",
					async m =>
					{
						HttpClient cl = new();
						var downloaded = await cl.GetStringAsync(m.Payload);

						m.Content = downloaded;
					},
					1,
					m => m.Payload != null
				)
				.AddHandler<TestDownloadMessage<Uri>>(
					"URL Data Download",
					m =>
					{
						Console.WriteLine(m.Content?.ToString() ?? "No downloaded content!");
					},
					1)
				.AddExceptionHandler<MessageHandlerExceptionMessage>(
					ex =>
					{
						Console.WriteLine($"Exception handled: {Environment.NewLine}{ex.Exception}");
						Interlocked.Increment(ref exceptionCount);
					}, 1);

			_queue.Build();

			await _queue.Enqueue(new TestDownloadMessage<Uri>(new Uri("http://aaaaa.bbb")));

			await _queue.Complete();

			_queueKeeper.EnqueuedCount.Should().Be(2);
			_queueKeeper.CommittedCount.Should().Be(2);
			exceptionCount.Should().Be(1);
		}
	}
}
