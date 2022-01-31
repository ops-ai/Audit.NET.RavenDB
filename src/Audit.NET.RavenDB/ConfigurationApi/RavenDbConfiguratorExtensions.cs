﻿using Audit.Core.ConfigurationApi;
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
        /// Store the events in a MongoDB database.
        /// </summary>
        /// <param name="configurator">The Audit.NET Configurator</param>
        /// <param name="connectionString">The mongo DB connection string.</param>
        /// <param name="database">The mongo DB database name.</param>
        /// <param name="collection">The mongo DB collection name.</param>
        /// <param name="jsonSerializerSettings">The custom JsonSerializerSettings.</param>
        /// <param name="serializeAsBson">Specifies whether the target object and extra fields should be serialized as Bson. Default is Json.</param>
        public static ICreationPolicyConfigurator UseRavenDB(this IConfigurator configurator, string[] urls, X509Certificate2 certificate, string database = "Audit", JsonSerializerSettings jsonSerializerSettings = null)
        {
            var store = new DocumentStore { Urls = urls, Certificate = certificate, Database = database };
            store.Conventions.Serialization = new NewtonsoftJsonSerializationConventions
            {
                JsonContractResolver = new AuditContractResolver()
            };
            store.Initialize();

            var ravendbDataProvider = new RavenDbDataProvider(store, database, jsonSerializerSettings);
            return configurator.UseCustomProvider(ravendbDataProvider);
        }

        /// <summary>
        /// Store the events in a MongoDB database.
        /// </summary>
        /// <param name="configurator">The Audit.NET Configurator</param>
        /// <param name="config">The mongo DB provider configuration.</param>
        public static ICreationPolicyConfigurator UseRavenDB(this IConfigurator configurator, Action<IRavenDbProviderConfigurator> config)
        {
            var ravenDbConfig = new RavenDbProviderConfigurator();
            config.Invoke(ravenDbConfig);
            return UseRavenDB(configurator, ravenDbConfig._urls, ravenDbConfig._certificate, ravenDbConfig._database, ravenDbConfig._jsonSerializerSettings);
        }
    }
}
