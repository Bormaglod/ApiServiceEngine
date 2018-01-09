namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class Task : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name => this["name"] as string;

        [ConfigurationProperty("methods")]
        public TaskMethodCollection Methods => (TaskMethodCollection)this["methods"];

        [ConfigurationProperty("parameters")]
        public TaskParameterCollection Params => (TaskParameterCollection)this["parameters"];

        public override string ToString()
        {
            return Name;
        }
    }
}
