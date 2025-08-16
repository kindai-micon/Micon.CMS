using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Reflection;

namespace Micon.CMS.Library.Services
{
    public static class AssemblyService
    {
        static ImmutableDictionary<Guid, Assembly> _assemblies = (new Dictionary<Guid,Assembly>()).ToImmutableDictionary();
        public static void AddAssemblies(List<Assembly> assemblies)
        {
            var directory =_assemblies.ToDictionary();
            foreach(var assembly in assemblies)
            {
                var types = assembly.DefinedTypes;
                var type = types.Where(x => x.IsPublic && x.IsClass && x.Name == "MiconCmsSettings").FirstOrDefault();
                if (type != null)
                {
                    var field = type.GetField("PackageId");
                    if (field != null && field.IsInitOnly && field.IsStatic)
                    {
                        var guid = field.GetValue(null) as Guid?;
                        if(guid != null)
                        {
                            directory.Add(guid.Value, assembly);
                        }
                    }
                }
            }
            _assemblies = directory.ToImmutableDictionary();
        }

        public static Assembly? GetAssembly(Guid packageId)
        {
            if (_assemblies.TryGetValue(packageId, out var assembly))
            {
                return assembly;
            }
            return null;
        }

        public static Type? GetType(Guid packageId, string typeName)
        {
            if (_assemblies.TryGetValue(packageId, out var assembly))
            {
                return assembly.GetType(typeName);
            }
            return null;
        }
    }
}
