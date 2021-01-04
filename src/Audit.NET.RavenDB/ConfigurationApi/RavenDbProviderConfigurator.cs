using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace Audit.NET.RavenDB.ConfigurationApi
{
    public class RavenDbProviderConfigurator : IRavenDbProviderConfigurator
    {
        internal string[] _urls = null;
        internal string _database = "Audit";
        internal X509Certificate2 _certificate;
        internal JsonSerializerSettings _jsonSerializerSettings = null;

        public IRavenDbProviderConfigurator Urls(string[] urls)
        {
            _urls = urls;
            return this;
        }

        public IRavenDbProviderConfigurator Database(string database)
        {
            _database = database;
            return this;
        }

        public IRavenDbProviderConfigurator Certificate(X509Certificate2 certificate)
        {
            _certificate = certificate;
            return this;
        }

        public IRavenDbProviderConfigurator CustomSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            return this;
        }
    }
}