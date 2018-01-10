namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class Task : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name => this["name"] as string;

        [ConfigurationProperty("methods")]
        public TaskMethods Methods => (TaskMethods)this["methods"];

        public override string ToString()
        {
            return Name;
        }
    }
}
