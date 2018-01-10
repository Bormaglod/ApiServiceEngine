namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(Parameters), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    class Parameters : ConfigurationElementCollection
    {
        public Parameter Get(string name)
        {
            return (Parameter)BaseGet(name);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Parameter();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Parameter)element).Name;
        }
    }
}
