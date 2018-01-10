namespace ApiServiceEngine.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(Services), AddItemName = "service", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    class Services : ConfigurationElementCollection
    {
        public Service Get(string name)
        {
            return (Service)BaseGet(name);
        }

        public Service GetByMethod(string method)
        {
            foreach (Service service in this)
            {
                Method m = service.Methods.Get(method);
                if (m != null)
                {
                    return service;
                }
            }

            return null;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Service();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Service)element).Name;
        }
    }
}
