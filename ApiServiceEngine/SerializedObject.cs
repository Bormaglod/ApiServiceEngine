namespace ApiServiceEngine
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using ApiServiceEngine.Configuration;
    using Inflector;

    [DataContract]
    class SerializedObject
    {
        public SerializedObject(ServiceAPI api, Service service, Method method, StringDictionary parameters)
        {
            Type type = GetType();
            foreach (Parameter p in method.In)
            {
                if (!parameters.ContainsKey(p.Name))
                    continue;

                PropertyInfo prop = type.GetProperty(Inflector.Pascalize(p.ApiName));
                if (prop == null)
                    continue;

                if (p.IsList)
                {
                    if (prop.PropertyType.IsArray)
                    {
                        Type t = prop.PropertyType.GetElementType();
                        Array array;

                        if (string.IsNullOrEmpty(p.Recive.Template))
                        {
                            // шаблон не установлен, данные списка берем из параметров командной строки
                            string[] list = parameters[p.Name].Split(new char[] { ',' });
                            array = Array.CreateInstance(t, list.Length);
                            for (int i = 0; i < list.Length; i++)
                            {
                                object obj = Convert.ChangeType(list[i], t);
                                array.SetValue(obj, i);
                            }
                        }
                        else
                        {
                            if (parameters[p.Name] != p.Recive.Template)
                                continue;

                            // если шаблон установлен и равен соответствующему параметру командной строки,
                            // то выполним указанные метод
                            (object Info, HttpStatusCode Status) = api.ExecuteMethod(p.Recive.Method, parameters);

                            if (Info == null)
                                continue;

                            // если метод выполнился нормально и вернул какие-то данные, то...
                            Type infoType = Info.GetType();
                            if (!infoType.IsGenericType)
                                continue;

                            IList list = (IList)Info;
                            if (list.Count == 0)
                                continue;

                            array = Array.CreateInstance(t, list.Count);
                            for (int i = 0; i < array.Length; i++)
                            {
                                object obj = list[i];
                                array.SetValue(obj.GetType().GetProperty(p.Recive.Parameter).GetValue(obj), i);
                            }
                        }

                        prop.SetValue(this, array);
                    }
                }
                else
                {
                    object obj = Convert.ChangeType(parameters[p.Name], prop.PropertyType);
                    prop.SetValue(this, obj);
                }
            }
        }

        public string Json()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(GetType());
            string json = string.Empty;
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, this);
                json = Encoding.Default.GetString(stream.ToArray());
            }

            return json;
        }
    }
}
