namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ParameterMap), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    class ParameterMap : ConfigurationElementCollection
    {
        public ParameterMapItem Get(string name)
        {
            return (ParameterMapItem)BaseGet(name);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ParameterMapItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ParameterMapItem)element).Name;
        }
    }
}
