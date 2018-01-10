namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class Parameter : ConfigurationElement
    {
        [ConfigurationProperty("param")]
        public string Name => this["param"] as string;

        [ConfigurationProperty("api_name")]
        public string ApiName => this["api_name"] as string;

        [ConfigurationProperty("method")]
        public string Method => this["method"] as string;
    }
}
