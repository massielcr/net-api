using NUnit.Framework;
using PluralsightKDStreams.Interfaces;
using PluralsightKDStreams.Services;

namespace PluralsightKDStreams.Tests
{
    [TestFixture]
    public class CancellationServiceTests
    {
        private ICancellationService _service = null!;

        [SetUp]
        public void Setup()
        {
            _service = new CancellationService();
        }

        #region GetToken Tests

        [Test]
        public void GetToken_WithUniqueKey_ReturnsCancellationToken()
        {
            // Arrange
            string key = "test-key-1";

            // Act
            CancellationToken token = _service.GetToken(key);

            // Assert
            Assert.That(token, Is.Not.EqualTo(CancellationToken.None));
            Assert.That(token.IsCancellationRequested, Is.False);
        }

        [Test]
        public void GetToken_WithSameKeyTwice_ReturnsSameToken()
        {
            // Arrange
            string key = "test-key-same";

            // Act
            CancellationToken token1 = _service.GetToken(key);
            CancellationToken token2 = _service.GetToken(key);

            // Assert
            Assert.That(token1, Is.EqualTo(token2));
        }

        [Test]
        public void GetToken_WithDifferentKeys_ReturnsDifferentTokens()
        {
            // Arrange
            string key1 = "test-key-diff-1";
            string key2 = "test-key-diff-2";

            // Act
            CancellationToken token1 = _service.GetToken(key1);
            CancellationToken token2 = _service.GetToken(key2);

            // Assert
            Assert.That(token1, Is.Not.EqualTo(token2));
        }

        [Test]
        public void GetToken_WithSeconds_CancelsAfterSpecifiedTime()
        {
            // Arrange
            string key = "test-key-timeout";
            int seconds = 1;

            // Act
            CancellationToken token = _service.GetToken(key, seconds);

            // Wait for cancellation to occur
            bool cancelled = token.WaitHandle.WaitOne(TimeSpan.FromSeconds(2));

            // Assert
            Assert.That(cancelled, Is.True);
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        [Test]
        public void GetToken_WithNegativeSeconds_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            string key = "test-key-negative";
            int seconds = -1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetToken(key, seconds));
        }

        [Test]
        public void GetToken_WithNullSeconds_DoesNotSetTimeout()
        {
            // Arrange
            string key = "test-key-null-seconds";

            // Act
            CancellationToken token = _service.GetToken(key, null);

            // Assert
            Assert.That(token.IsCancellationRequested, Is.False);
        }

        [Test]
        public void GetToken_AfterCancellation_CreatesNewToken()
        {
            // Arrange
            string key = "test-key-refresh";

            // Act
            CancellationToken token1 = _service.GetToken(key, 1);
            token1.WaitHandle.WaitOne(TimeSpan.FromSeconds(2)); // Wait for cancellation

            CancellationToken token2 = _service.GetToken(key);

            // Assert
            Assert.That(token1.IsCancellationRequested, Is.True);
            Assert.That(token2.IsCancellationRequested, Is.False);
            Assert.That(token1, Is.Not.EqualTo(token2));
        }

        [Test]
        public void GetToken_MultipleCallsBeforeCancellation_ReturnsSameToken()
        {
            // Arrange
            string key = "test-key-multiple";

            // Act
            CancellationToken token1 = _service.GetToken(key);
            CancellationToken token2 = _service.GetToken(key);
            CancellationToken token3 = _service.GetToken(key);

            // Assert
            Assert.That(token1, Is.EqualTo(token2));
            Assert.That(token2, Is.EqualTo(token3));
        }

        #endregion

        #region Cancel Tests

        [Test]
        public void Cancel_WithValidKey_CancelsToken()
        {
            // Arrange
            string key = "test-key-cancel";
            CancellationToken token = _service.GetToken(key);

            // Act
            _service.Cancel(key);

            // Assert
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        [Test]
        public void Cancel_WithInvalidKey_DoesNotThrowException()
        {
            // Arrange
            string key = "non-existent-key";

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Cancel(key));
        }

        [Test]
        public void Cancel_AfterCancelling_SubscriberIsNotified()
        {
            // Arrange
            string key = "test-key-notify";
            CancellationToken token = _service.GetToken(key);
            bool callbackCalled = false;

            token.Register(() => callbackCalled = true);

            // Act
            _service.Cancel(key);

            // Assert
            Assert.That(callbackCalled, Is.True);
        }

        [Test]
        public void Cancel_WithAlreadyCancelledToken_DoesNotThrowException()
        {
            // Arrange
            string key = "test-key-already-cancelled";
            CancellationToken token = _service.GetToken(key);
            _service.Cancel(key);

            // Act & Assert
            Assert.DoesNotThrow(() => _service.Cancel(key));
        }

        [Test]
        public void Cancel_RemovesTokenFromStorage()
        {
            // Arrange
            string key = "test-key-remove";
            CancellationToken token1 = _service.GetToken(key);

            // Act
            _service.Cancel(key);
            CancellationToken token2 = _service.GetToken(key);

            // Assert
            Assert.That(token1, Is.Not.EqualTo(token2));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_GetToken_Cancel_GetNewToken_Sequence()
        {
            // Arrange
            string key = "integration-key";

            // Act
            CancellationToken token1 = _service.GetToken(key);
            Assert.That(token1.IsCancellationRequested, Is.False);

            _service.Cancel(key);
            Assert.That(token1.IsCancellationRequested, Is.True);

            CancellationToken token2 = _service.GetToken(key);
            Assert.That(token2.IsCancellationRequested, Is.False);
            Assert.That(token1, Is.Not.EqualTo(token2));

            // Assert - all checks passed in sequence
        }

        [Test]
        public void Integration_MultipleKeysIndependent()
        {
            // Arrange
            string key1 = "integration-key-1";
            string key2 = "integration-key-2";

            // Act
            CancellationToken token1 = _service.GetToken(key1);
            CancellationToken token2 = _service.GetToken(key2);

            _service.Cancel(key1);

            // Assert
            Assert.That(token1.IsCancellationRequested, Is.True);
            Assert.That(token2.IsCancellationRequested, Is.False);
        }

        [Test]
        public async Task Integration_TokenWithTimeoutCancelsCorrectly()
        {
            // Arrange
            string key = "integration-timeout-key";
            int seconds = 1;

            // Act
            CancellationToken token = _service.GetToken(key, seconds);
            await Task.Delay(TimeSpan.FromSeconds(1.5));

            // Assert
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        [Test]
        public void Integration_ConcurrentAccessToMultipleKeys()
        {
            // Arrange
            string[] keys = new[] { "key1", "key2", "key3", "key4", "key5" };
            CancellationToken[] tokens = new CancellationToken[keys.Length];

            // Act
            Parallel.For(0, keys.Length, i =>
            {
                tokens[i] = _service.GetToken(keys[i]);
            });

            // Assert
            for (int i = 0; i < keys.Length; i++)
            {
                Assert.That(tokens[i].IsCancellationRequested, Is.False);
            }
        }

        [Test]
        public void Integration_ConcurrentCancelOperations()
        {
            // Arrange
            string key = "concurrent-cancel-key";
            CancellationToken token = _service.GetToken(key);

            // Act
            Parallel.For(0, 5, i =>
            {
                _service.Cancel(key);
            });

            // Assert
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        #endregion
    }
}
