namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    class ApiConfigSection : ConfigurationSection
    {
        private Configuration config;

        [ConfigurationProperty("parameters")]
        public ParameterMap Parameters => (ParameterMap)this["parameters"];

        [ConfigurationProperty("tasks")]
        public TasksCollection Tasks => (TasksCollection)this["tasks"];

        [ConfigurationProperty("services")]
        public ServicesCollection Services => (ServicesCollection)this["services"];

        ApiConfigSection()
        {
            // Allow this section to be stored in user.app. By default this is forbidden.
            SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
        }

        public void Save() => config.Save();

        public static ApiConfigSection GetSection(ConfigurationUserLevel ConfigLevel)
        {
            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigLevel);
            ApiConfigSection apiSettings;

            apiSettings = (ApiConfigSection)Config.GetSection("api");
            if (apiSettings == null)
            {
                apiSettings = new ApiConfigSection();
                Config.Sections.Add("api", apiSettings);
            }

            apiSettings.config = Config;

            return apiSettings;
        }
    }
}
