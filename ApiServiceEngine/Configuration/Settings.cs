namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class Settings : ConfigurationElement
    {
        [ConfigurationProperty("address")]
        public string Address => this["address"] as string;

        [ConfigurationProperty("login")]
        public string Login => this["login"] as string;

        [ConfigurationProperty("password")]
        public string Password => this["password"] as string;
    }
}
