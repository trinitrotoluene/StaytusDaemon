using System;

namespace StaytusDaemon.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResolverAttribute : Attribute
    {
        public ResolverAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}