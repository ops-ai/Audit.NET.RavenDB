using Audit.Core.ConfigurationApi;
using Audit.NET.RavenDB.Providers;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using System.Security.Cryptography.X509Certificates;

namespace Audit.NET.RavenDB.ConfigurationApi
{
    public static class RavenDbConfiguratorExtensions
    {
        /// <summary>
        /// Store the events in a RavenDB database.
        /// </summary>
        /// <param name="configurator">The Audit.NET Configurator</param>
        /// <param name="connectionString">The RavenDB connection string.</param>
        /// <param name="database">The RavenDB database name.</param>
        /// <param name="collection">The RavenDB collection name.</param>
        /// <param name="jsonSerializerSettings">The custom JsonSerializerSettings.</param>
        public static ICreationPolicyConfigurator UseRavenDB(this IConfigurator configurator, string[] urls, X509Certificate2? certificate, string database = "Audit", JsonSerializerSettings? jsonSerializerSettings = null, bool? storeDiffOnly = true)
        {
            var store = new DocumentStore { Urls = urls, Certificate = certificate, Database = database };
            var serializer = new NewtonsoftJsonSerializationConventions
            {
                JsonContractResolver = new AuditContractResolver()
            };
            serializer.CustomizeJsonSerializer += (JsonSerializer serializer) =>
            {
                serializer.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
                serializer.NullValueHandling = NullValueHandling.Ignore;
            };
            store.Conventions.Serialization = serializer;
            store.Initialize();

            var ravendbDataProvider = new RavenDbDataProvider(store, database, jsonSerializerSettings, storeDiffOnly);
            return configurator.UseCustomProvider(ravendbDataProvider);
        }

        /// <summary>
        /// Store the events in a RavenDB database.
        /// </summary>
        /// <param name="configurator">The Audit.NET Configurator</param>
        /// <param name="config">The RavenDB provider configuration.</param>
        public static ICreationPolicyConfigurator UseRavenDB(this IConfigurator configurator, Action<IRavenDbProviderConfigurator> config)
        {
            var ravenDbConfig = new RavenDbProviderConfigurator();
            config.Invoke(ravenDbConfig);
            return UseRavenDB(configurator, ravenDbConfig._urls!, ravenDbConfig._certificate, ravenDbConfig._database, ravenDbConfig._jsonSerializerSettings);
        }
    }
}
