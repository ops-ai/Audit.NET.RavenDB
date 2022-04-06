using Audit.Core;
using Audit.NET.RavenDB.Providers;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using Raven.TestDriver;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Audit.NET.RavenDB.Tests
{
    public class TestRavenProvider : RavenTestDriver
    {
        private readonly ITestOutputHelper _output;
        private IDocumentStore _store;

        public TestRavenProvider(ITestOutputHelper output)
        {
            _output = output;
            _store = GetDocumentStore();

        }

        protected override void PreInitialize(IDocumentStore documentStore)
        {
            var serializer = new NewtonsoftJsonSerializationConventions
            {
                JsonContractResolver = new AuditContractResolver()
            };
            serializer.CustomizeJsonSerializer += (JsonSerializer serializer) =>
            {
                serializer.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
                serializer.NullValueHandling = NullValueHandling.Ignore;
            };
            documentStore.Conventions.Serialization = serializer;
        }

        [Fact]
        public async Task TestStoreSimpleEvent()
        {
            var ravendbDataProvider = new RavenDbDataProvider(_store, _store.Database, null, true);
            Configuration.Setup()
                .UseCustomProvider(ravendbDataProvider);

            using (var scope = await AuditScope.CreateAsync(new AuditScopeOptions() { EventType = "test" }))
            {
                scope.SetCustomField("Test", "test");
                scope.SetCustomField("Test2", "test2");
            }

            using (var session = _store.OpenAsyncSession())
            {
                var events = await session.Query<AuditEvent>().ToListAsync();

                Assert.Single(events);
                var ev = events[0];
                Assert.Equal("test", ev.EventType);
            }
        }
    }
}
