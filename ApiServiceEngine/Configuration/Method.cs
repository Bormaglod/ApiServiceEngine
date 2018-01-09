namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(Method), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    class Method : ConfigurationElementCollection
    {
        [ConfigurationProperty("name")]
        public string Name => this["name"] as string;

        [ConfigurationProperty("api_name")]
        public string ApiName
        {
            get
            {
                string api_name = this["api_name"] as string;
                if (string.IsNullOrWhiteSpace(api_name))
                    return Name;

                return api_name;
            }
        }

        [ConfigurationProperty("procedure")]
        public string Procedure => this["procedure"] as string;

        protected override ConfigurationElement CreateNewElement()
        {
            return new Parameter();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Parameter)element).Name;
        }

        public Parameter Get(string name)
        {
            return (Parameter)BaseGet(name);
        }

        public override string ToString()
        {
            return $"Method name = {Name}";
        }
    }
}
