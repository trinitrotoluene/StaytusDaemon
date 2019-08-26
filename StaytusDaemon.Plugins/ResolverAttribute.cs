using System;

namespace StaytusDaemon.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResolverAttribute : Attribute
    {
        public string Name { get; }

        public ResolverAttribute(string name)
        {
            Name = name;
        }
    }
}