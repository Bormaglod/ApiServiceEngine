﻿namespace ApiServiceEngine
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using ApiServiceEngine.Configuration;

    public static class TypeHelper
    {
        public static PropertyInfo GetProperty(this Type type, string name, StringComparison comparison)
        {
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (string.Compare(prop.Name, name, comparison) == 0)
                {
                    return prop;
                }
            }

            return null;
        }

        public static PropertyInfo GetProperty(this Type type, Parameter parameter)
        {
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (string.Compare(prop.Name, parameter.Name, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return prop;
                }

                DataMemberAttribute attr = prop.GetCustomAttribute<DataMemberAttribute>();
                if (attr != null && string.Compare(attr.Name, parameter.ApiName, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    return prop;
                }
            }

            return null;
        }

        public static MethodInfo GetMethod(this Type type, string name, StringComparison comparison)
        {
            foreach (MethodInfo method in type.GetMethods())
            {
                if (string.Compare(method.Name, name, comparison) == 0)
                {
                    return method;
                }
            }

            return null;
        }
    }
}
