namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class TaskMethod : ConfigurationElement
    {
        [ConfigurationProperty("method")]
        public string Method => this["method"] as string;

        public override string ToString()
        {
            return Method;
        }
    }
}
