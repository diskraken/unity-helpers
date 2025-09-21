using NUnit.Framework;

namespace Tests.Messaging {
    public struct NumMessageA {
        public int Value;
    }

    public struct TextMessageB {
        public string Text;
    }

    [TestFixture]
    public class ZeroMessengerFactoryTests {
        private ZeroMessengerFactory _factory;

        [SetUp]
        public void SetUp() {
            _factory = new ZeroMessengerFactory();
        }

        [TearDown]
        public void TearDown() {
            _factory = null;
        }

        [Test]
        public void PublishA_ShouldDeliverToSubscriberA() {
            // Arrange
            var received = false;
            NumMessageA receivedMessage = default;
            var subscription = _factory.Subscribe<NumMessageA>(msg => {
                received = true;
                receivedMessage = msg;
            });

            var testMessage = new NumMessageA { Value = 123 };

            // Act
            _factory.Publish(testMessage);

            // Assert
            Assert.IsTrue(received, "TestMessageA should be delivered");
            Assert.AreEqual(123, receivedMessage.Value, "Value should match");

            subscription.Dispose();
        }

        [Test]
        public void MultipleSubscribersA_ShouldAllReceive() {
            // Arrange
            var count1 = 0;
            var count2 = 0;
            var sub1 = _factory.Subscribe<NumMessageA>(_ => count1++);
            var sub2 = _factory.Subscribe<NumMessageA>(_ => count2++);

            // Act
            _factory.Publish(new NumMessageA { Value = 1 });

            // Assert
            Assert.AreEqual(1, count1, "First subscriber should receive");
            Assert.AreEqual(1, count2, "Second subscriber should receive");

            sub1.Dispose();
            sub2.Dispose();
        }

        [Test]
        public void PublishB_NoSubscribers_ShouldNotThrow() {
            // Arrange
            var testMessage = new TextMessageB { Text = "hello" };

            // Act & Assert
            Assert.DoesNotThrow(() => _factory.Publish(testMessage),
                "Publishing with no subscribers must not throw");
        }

        [Test]
        public void SubscribeAndDisposeA_ShouldNotReceiveAfterDispose() {
            // Arrange
            var received = false;
            var subscription = _factory.Subscribe<NumMessageA>(_ => received = true);
            subscription.Dispose();

            // Act
            _factory.Publish(new NumMessageA { Value = 0 });

            // Assert
            Assert.IsFalse(received, "Disposed subscription must not receive");
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void PublishMultipleA_ShouldDeliverAll(int count) {
            // Arrange
            var receivedCount = 0;
            var subscription = _factory.Subscribe<NumMessageA>(_ => receivedCount++);

            // Act
            for (int i = 0; i < count; i++)
                _factory.Publish(new NumMessageA { Value = i });

            // Assert
            Assert.AreEqual(count, receivedCount, $"Should receive {count} messages");

            subscription.Dispose();
        }

        [Test]
        public void GetPublisher_SameType_ReturnsSameInstance() {
            // Act
            var pub1 = _factory.GetPublisher<NumMessageA>();
            var pub2 = _factory.GetPublisher<NumMessageA>();

            // Assert
            Assert.AreSame(pub1, pub2, "Same generic type must return same broker");
        }

        [Test]
        public void GetPublisher_DifferentTypes_ReturnsDifferentInstances() {
            // Act
            var pubA = _factory.GetPublisher<NumMessageA>();
            var pubB = _factory.GetPublisher<TextMessageB>();

            // Assert
            Assert.AreNotSame(pubA, pubB, "Different message types must have different brokers");
        }

        [Test]
        public void GetSubscriber_EqualsPublisherInstance() {
            // Act
            var publisher = _factory.GetPublisher<TextMessageB>();
            var subscriber = _factory.GetSubscriber<TextMessageB>();

            // Assert
            Assert.AreSame(publisher, subscriber, "Subscriber must be the same broker as publisher");
        }

        [Test]
        public void Factory_MixedTypes_DoNotInterfere() {
            // Arrange
            var receivedA = false;
            var receivedB = false;
            var subA = _factory.Subscribe<NumMessageA>(_ => receivedA = true);
            var subB = _factory.Subscribe<TextMessageB>(_ => receivedB = true);

            // Act
            _factory.Publish(new NumMessageA { Value = 5 });
            // Intentionally omit publishing B

            // Assert
            Assert.IsTrue(receivedA, "TestMessageA should be received");
            Assert.IsFalse(receivedB, "TestMessageB should not be received");

            subA.Dispose();
            subB.Dispose();
        }

        [Test, Timeout(500)]
        public void HighVolumeTestMessageA_PerformsQuickly() {
            // Arrange
            const int total = 5000;
            var received = 0;
            var subscription = _factory.Subscribe<NumMessageA>(_ => received++);

            // Act
            for (int i = 0; i < total; i++)
                _factory.Publish(new NumMessageA { Value = i });

            // Assert
            Assert.AreEqual(total, received, "All messages must be received");
            subscription.Dispose();
        }
    }
}