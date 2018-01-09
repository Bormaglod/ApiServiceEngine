namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(TaskParameter), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    class TaskParameterCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TaskParameter();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TaskParameter)element).Name;
        }
    }
}
