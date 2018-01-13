namespace ApiServiceEngine
{
    using System;
    using System.Linq;
    using ApiServiceEngine.Configuration;

    public static class ConfigurationHelper
    {
        /// <summary>
        /// Метод возвращает сервис, содержащий метод с именем methodName.
        /// </summary>
        /// <param name="services">Список сервисов.</param>
        /// <param name="methodName">Имя метода, который должен поддерживаться сервисом.</param>
        /// <returns>Сервис, содержащий метод с именем methodName.</returns>
        public static Service GetService(this Services services, string methodName)
        {
            foreach (Service service in services)
            {
                foreach (Method method in service.Methods)
                {
                    if (string.Compare(method.Name, methodName, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        return service;
                    }
                }
            }

            return null;
        }

        public static Method GetMethod(this Service service, string methodNme)
        {
            return service.Methods.OfType<Method>().FirstOrDefault(x => string.Compare(x.Name, methodNme, StringComparison.CurrentCultureIgnoreCase) == 0);
        }
    }
}
