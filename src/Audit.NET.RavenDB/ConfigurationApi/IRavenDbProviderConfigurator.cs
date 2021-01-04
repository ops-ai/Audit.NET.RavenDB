using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace Audit.NET.RavenDB.ConfigurationApi
{
    /// <summary>
    /// Provides a configuration for the RavenDB data provider
    /// </summary>
    public interface IRavenDbProviderConfigurator
    {
        /// <summary>
        /// Database urls
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        IRavenDbProviderConfigurator Urls(string[] urls);

        /// <summary>
        /// Authentication certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        IRavenDbProviderConfigurator Certificate(X509Certificate2 certificate);

        /// <summary>
        /// Specifies the RavenDB database name.
        /// </summary>
        /// <param name="database">The database name.</param>
        IRavenDbProviderConfigurator Database(string database);

        /// <summary>
        /// Specifies a custom JSON serializer settings
        /// </summary>
        /// <param name="jsonSerializerSettings">The serializer settings.</param>
        IRavenDbProviderConfigurator CustomSerializerSettings(JsonSerializerSettings jsonSerializerSettings);
    }
}
