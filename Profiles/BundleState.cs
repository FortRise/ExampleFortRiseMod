using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Teuria.Profiles;

public class BundleState
{
    private Dictionary<string, object> bundles = [];

    public bool TryGet<T>(string bundleName, [NotNullWhen(true)] out T? value)
    {
        if (bundles.TryGetValue(bundleName, out var val))
        {
            value = (T)val;
            return true;
        }

        value = default;
        return false;
    }

    public T Get<T>(string bundleName)
    {
        if (bundles.TryGetValue(bundleName, out var val))
        {
            return (T)val;
        }

        throw new Exception($"Cannot find a field: {bundleName} from a bundle.");
    }

    public void Set(string name, object value)
    {
        bundles[name] = value;
    }
}