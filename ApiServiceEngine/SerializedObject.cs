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
    
    [DataContract]
    class SerializedObject
    {
        public SerializedObject(ServiceAPI api, Service service, Method method, StringDictionary parameters)
        {
            Type type = GetType();
            foreach (Parameter p in method.In)
            {
                PropertyInfo prop = type.GetProperty(p);
                string pName = p.Name.ToLower();

                if (prop == null)
                {
                    continue;
                }

                if (p.IsList)
                {
                    if (prop.PropertyType.IsArray)
                    {
                        Type t = prop.PropertyType.GetElementType();
                        Array array;

                        if (parameters.ContainsKey(pName))
                        {
                            string[] list = parameters[pName].Split(new char[] { ',' });
                            array = Array.CreateInstance(t, list.Length);
                            for (int i = 0; i < list.Length; i++)
                            {
                                try
                                {
                                    object obj = Convert.ChangeType(list[i].Trim(), t);
                                    array.SetValue(obj, i);
                                }
                                catch (Exception)
                                {
                                    LogHelper.Logger.Error($"В параметре командной строки {pName} указаны некорректные данные.");
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(p.Recive.Method))
                            {
                                Method reciveMethod = service.GetMethod(p.Recive.Method);
                                if (reciveMethod == null)
                                {
                                    LogHelper.Logger.Error($"При попытке получить значение параметра {p.Name} в методе {method.Name} был вызван отсутствующий метод {p.Recive.Method}.");
                                    continue;
                                }

                                (object Info, HttpStatusCode Status) = api.ExecuteWebMethod(reciveMethod, parameters);
                                if (Info == null)
                                {
                                    LogHelper.Logger.Error($"При попытке получить значение параметра {p.Name} в метод {method.Name} вернул некорректное значение.");
                                    continue;
                                }

                                // если метод выполнился нормально и вернул какие-то данные, то...
                                Type infoType = Info.GetType();
                                if (!infoType.IsGenericType)
                                {
                                    LogHelper.Logger.Error($"При получении параметра {p.Name} в методе {method.Name} был вызван метод {p.Recive.Method} который вернул некорректное значение.");
                                    continue;
                                }

                                IList list = (IList)Info;
                                if (list.Count == 0)
                                    continue;

                                Parameter reciveParam = reciveMethod.In.GetParameter(p.Recive.Parameter);
                                if (reciveParam == null)
                                {
                                    LogHelper.Logger.Error($"При попытке получить значение параметра {p.Name} в методе {method.Name} был вызван метод {p.Recive.Method} с отсутствующим параметром {p.Recive.Parameter}.");
                                    continue;
                                }

                                PropertyInfo pInfo = list[0].GetType().GetProperty(reciveParam);
                                if (pInfo == null)
                                {
                                    LogHelper.Logger.Error($"При попытке получить значение параметра {p.Name} в методе {method.Name} был вызван метод {p.Recive.Method} с неизвестным параметром {p.Recive.Parameter}.");
                                    continue;
                                }

                                array = Array.CreateInstance(t, list.Count);
                                for (int i = 0; i < array.Length; i++)
                                {
                                    object obj = list[i];
                                    array.SetValue(pInfo.GetValue(obj), i);
                                }

                                prop.SetValue(this, array);
                            }
                        }
                    }
                }
                else
                {
                    object obj = null;
                    if (parameters.ContainsKey(pName))
                    {
                        obj = Convert.ChangeType(parameters[pName], prop.PropertyType);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(p.Recive.Method))
                        {
                            try
                            {
                                obj = api.GetPropertyFromMethod(p.Recive.Method, p.Recive.Parameter, parameters);
                            }
                            catch (ExecuteMethodException)
                            {
                                LogHelper.Logger.Error($"При попытке получить значение параметра {p.Name} в методе {method.Name} был вызван отсутствующий метод {p.Recive.Method}.");
                                continue;
                            }
                            catch (UnknownPropertyException)
                            {
                                LogHelper.Logger.Error($"При попытке получить значение параметра {p.Name} в методе {method.Name} был вызван метод {p.Recive.Method} с отсутствующим параметром {p.Recive.Parameter}.");
                                continue;
                            }

                            parameters.Add(pName, obj.ToString());
                        }
                    }

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
