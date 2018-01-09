namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class Service : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name => this["name"] as string;

        [ConfigurationProperty("comment")]
        public string Comment => this["comment"] as string;

        [ConfigurationProperty("type")]
        public string Type => this["type"] as string;

        [ConfigurationProperty("settings")]
        public Settings Settings => (Settings)this["settings"];

        [ConfigurationProperty("methods")]
        public MethodCollection Methods => (MethodCollection)this["methods"];

        public override string ToString()
        {
            return Name;
        }
    }
}
