namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(TaskMethod), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    class TaskMethodCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TaskMethod();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TaskMethod)element).Method;
        }
    }
}
