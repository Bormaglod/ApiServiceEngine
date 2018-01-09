namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class TaskParameter : ConfigurationElement
    {
        [ConfigurationProperty("param")]
        public string Name => this["param"] as string;

        public override string ToString()
        {
            return $"TaskParemeter name = {Name}";
        }
    }
}
