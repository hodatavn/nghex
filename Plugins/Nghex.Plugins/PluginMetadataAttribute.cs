namespace Nghex.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginMetadataAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; }
        public string Description { get; }

        public PluginMetadataAttribute(string name, string version, string description)
        {
            Name = name;
            Version = version;
            Description = description;
        }
    }
}