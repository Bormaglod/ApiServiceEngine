namespace ApiServiceEngine
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
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

        public static Parameter GetParameter(this In parameters, string parameterName)
        {
            return parameters.OfType<Parameter>().FirstOrDefault(x => string.Compare(x.Name, parameterName, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        public static Parameter GetParameter(this Out parameters, string parameterName)
        {
            return parameters.OfType<Parameter>().FirstOrDefault(x => string.Compare(x.Name, parameterName, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        public static Parameter GetApiParameter(this Out parameters, string parameterName)
        {
            string pName = parameterName;

            int idx = pName.LastIndexOf('/');
            if (idx >= 0)
            {
                pName = pName.Substring(idx + 1);
            }

            foreach (Parameter p in parameters)
            {
                if (string.Compare(string.IsNullOrEmpty(p.Path) ? pName : parameterName, p.GetApiName(), StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return p;
                }
            }

            return null;
        }

        public static string GetApiName(this Parameter parameter)
        {
            string name = parameter.ApiName;
            if (string.IsNullOrEmpty(name))
                name = parameter.Name;

            if (!string.IsNullOrEmpty(parameter.Path))
            {
                string path = parameter.Path;
                if (path[0] != '/')
                {
                    path = $"/{path}";
                }

                name = $"{path}/{name}";
            }

            return name;
        }

        public static Account GetAccount(this Accounts accounts, string name)
        {
            foreach (Account account in accounts)
            {
                string n = string.IsNullOrEmpty(account.Name) ? account.Login : account.Name;
                if (string.Compare(n, name, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return account;
                }
            }

            return null;
        }

        public static string GetParameterValue(this Account account, string parameter_name)
        {
            Type type = account.GetType();
            PropertyInfo property = null;
            foreach (PropertyInfo info in type.GetProperties())
            {
                ConfigurationPropertyAttribute attr = info.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (attr != null)
                {
                    if (string.Compare(attr.Name, parameter_name, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        property = info;
                        break;
                    }
                }
            }

            return property?.GetValue(account).ToString();
        }
    }
}
