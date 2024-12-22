using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Shared.Domain.JsonResolvers;

public sealed class PrivatePropsResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty prop = base.CreateProperty(member, memberSerialization);
        if (prop.Writable)
            return prop;

        PropertyInfo? property = member as PropertyInfo;
        bool hasPrivateSetter = property?.GetSetMethod(true) is not null;
        prop.Writable = hasPrivateSetter;

        return prop;
    }
}
