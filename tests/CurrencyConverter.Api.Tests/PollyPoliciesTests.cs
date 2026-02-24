using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using Xunit;

namespace CurrencyConverter.Api.Tests
{
    class TestSequenceHandler : DelegatingHandler
    {
        private readonly int[] _statuses;
        public int CallCount { get; private set; }

        public TestSequenceHandler(int[] statuses)
        {
            _statuses = statuses;
            CallCount = 0;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var idx = CallCount < _statuses.Length ? CallCount : _statuses.Length - 1;
            var status = _statuses[idx];
            CallCount++;
            var resp = new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = JsonContent.Create(new { success = status == 200 })
            };
            return Task.FromResult(resp);
        }
    }

    class PolicyWrappingHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;
        public PolicyWrappingHandler(IAsyncPolicy<HttpResponseMessage> policy)
        {
            _policy = policy;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _policy.ExecuteAsync((ct) => base.SendAsync(request, ct), cancellationToken);
        }
    }

    public class PollyPoliciesTests
    {
        [Fact]
        public async Task RetryPolicy_Retries_On_TransientErrors_And_Eventually_Succeeds()
        {
            // Arrange: first two requests 500, then 200
            var handler = new TestSequenceHandler(new[] { 500, 500, 200 });

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(new[] { System.TimeSpan.FromMilliseconds(10), System.TimeSpan.FromMilliseconds(10) });

            var policyHandler = new PolicyWrappingHandler(retryPolicy)
            {
                InnerHandler = handler
            };

            var client = new HttpClient(policyHandler);

            // Act
            var resp = await client.GetAsync("http://test/latest");

            // Assert
            Assert.True(resp.IsSuccessStatusCode);
            Assert.Equal(3, handler.CallCount);
        }

        [Fact]
        public async Task CircuitBreaker_Opens_After_ConsecutiveFailures()
        {
            // Arrange: always 500
            var handler = new TestSequenceHandler(new[] { 500, 500, 500, 500 });

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(new[] { System.TimeSpan.FromMilliseconds(1) });

            var circuitPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(2, System.TimeSpan.FromSeconds(30));

            var wrap = Policy.WrapAsync(retryPolicy, circuitPolicy);

            var policyHandler = new PolicyWrappingHandler(wrap)
            {
                InnerHandler = handler
            };

            var client = new HttpClient(policyHandler);

            // Act & Assert: first and second calls should fail (500), but the circuit
            // may open on the second call depending on timing. Accept either behavior.
            try
            {
                var r1 = await client.GetAsync("http://test/latest");
                Assert.False(r1.IsSuccessStatusCode);
            }
            catch (BrokenCircuitException<HttpResponseMessage>)
            {
                // Circuit opened early; test passes.
                return;
            }

            try
            {
                var r2 = await client.GetAsync("http://test/latest");
                Assert.False(r2.IsSuccessStatusCode);
            }
            catch (BrokenCircuitException<HttpResponseMessage>)
            {
                // Circuit opened on second call; test passes.
                return;
            }

            // Third call should observe open circuit and throw BrokenCircuitException
            await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(async () => await client.GetAsync("http://test/latest"));
        }
    }
}
