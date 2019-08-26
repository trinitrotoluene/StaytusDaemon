using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StaytusDaemon.Plugins;

namespace StaytusDaemon.Reflection
{
    public class PluginManager
    {
        private readonly ConcurrentDictionary<string, IStatusResolver> _resolverTypes;

        public PluginManager()
        {
            _resolverTypes = new ConcurrentDictionary<string, IStatusResolver>();
        }

        public IStatusResolver GetResolver(string name)
        {
            // Fast path, case-sensitive
            if (_resolverTypes.TryGetValue(name, out var resolver))
            {
                return resolver;
            }
            // Slow path, case-insensitive
            else
            {
                foreach (var kvp in _resolverTypes)
                {
                    if (string.Equals(name, kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Value;
                    }
                }
            }
            
            throw new KeyNotFoundException($"No resolver for name {name} could be found.");
        }
        
        public void AddResolversFrom(Assembly assembly)
        {
            var resolverTypes = assembly.GetExportedTypes()
                .Where(t => typeof(IStatusResolver).IsAssignableFrom(t)
                && t.GetCustomAttribute<ResolverAttribute>() != null)
                .ToArray();

            foreach (var resolverType in resolverTypes)
            {
                var ctor = resolverType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, 
                    CallingConventions.Any, new Type[0] {}, new ParameterModifier[0] {});

                var resolverAttribute = resolverType.GetCustomAttribute<ResolverAttribute>();
                
                if (ctor == null) throw new Exception($"No public empty ctor found for resolver {resolverType}!");

                var createResolver = ActivatorUtilities.GetInstanceCreator<IStatusResolver>(ctor);

                _resolverTypes[resolverAttribute.Name] = createResolver();
            }
        }
    }
}