namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(Tasks), AddItemName = "task", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class Tasks : ConfigurationElementCollection
    {
        public Task Get(string name)
        {
            return (Task)BaseGet(name);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Task();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Task)element).Name;
        }
    }
}
