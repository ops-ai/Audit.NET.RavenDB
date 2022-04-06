using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Audit.NET.RavenDB.Providers
{
    public class AuditContractResolver : DefaultContractResolver
    {
        public AuditContractResolver()
        {

        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            properties = properties.Where(p => !p.AttributeProvider!.GetAttributes(true).Any(t => t is System.Text.Json.Serialization.JsonExtensionDataAttribute)).ToList();

            return properties;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (contract.ExtensionDataGetter != null || contract.ExtensionDataSetter != null)
                return contract;

            contract.ExtensionDataGetter = (o) =>
            {
                var props = o.GetType().GetProperties();
                foreach (var prop in props)
                {
                    foreach (object attr in prop.GetCustomAttributes(true))
                    {
                        if (attr is System.Text.Json.Serialization.JsonExtensionDataAttribute extData)
                            return ((Dictionary<string, object>?)prop.GetValue(o))?.ToDictionary(t => (object)t.Key, t => t.Value);
                    }
                }
                return null;
            };

            contract.ExtensionDataSetter = (o, key, value) =>
            {
                var extensionProp = objectType.GetProperties().FirstOrDefault(prop =>
                    prop.GetCustomAttributes(true).Any(attr => attr is System.Text.Json.Serialization.JsonExtensionDataAttribute extData));

                if (extensionProp == null || key.StartsWith('@'))
                    return;

                if (extensionProp.PropertyType == typeof(Dictionary<string, object>))
                {
                    if (extensionProp.GetValue(o) is not Dictionary<string, object> currentValue)
                        currentValue = new();
                    currentValue.Add(key, value!);
                    extensionProp.SetValue(o, currentValue);
                }
            };

            var extensionProp = objectType.GetProperties().FirstOrDefault(prop =>
                prop.GetCustomAttributes(true).Any(attr => attr is System.Text.Json.Serialization.JsonExtensionDataAttribute extData));
            contract.ExtensionDataValueType = extensionProp?.PropertyType;

            return contract;
        }
    }
}
