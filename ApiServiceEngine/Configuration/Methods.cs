namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(Methods), AddItemName = "method", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class Methods : ConfigurationElementCollection
    {
        public Method Get(string name)
        {
            return (Method)BaseGet(name);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Method();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Method)element).Name;
        }
    }
}
