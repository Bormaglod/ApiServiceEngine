namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Net;
    using FirebirdSql.Data.FirebirdClient;
    using ApiServiceEngine.Configuration;

    class Program
    {
        static void Main(string[] args)
        {
            foreach (string item in args)
            {
                LogHelper.Logger.Debug($"Arguments = {item}");
            }
            
            LogHelper.Logger.Info("Starting ApiServiveEngine");

            if (args.Length == 0)
            {
                LogHelper.Logger.Info("Запуск ApiServiceEngine без параметров.");
                return;
            }

            // задача на выполнение
            Task task = ApiSection.Instance.Tasks[args[0]];
            if (task == null)
            {
                LogHelper.Logger.Warn($"Задача {args[0]}, заданная в параметрах, не найдена в списке задач (configuration/api/tasks) и не будет выполнена.");
                return;
            }

            FbConnection conn = new FbConnection(GetConnectionString("default"));
            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                LogHelper.Logger.Error(e.Message);
                return;
            }

            FbTransaction tran = conn.BeginTransaction();
            try
            {
                IEnumerable<ServiceAPI> services = LoadServices(conn, tran);
                StringDictionary parameters = LoadParameters(args);

                LogHelper.Logger.Info($"Выполнение задачи {task.Name}.");

                // выполнение всех методов задачи
                foreach (RunMethod runMethod in task.Methods)
                {
                    Service service = GetService(runMethod.Method);
                    if (service == null)
                    {
                        LogHelper.Logger.Error($"В задаче {task.Name} указан метод {runMethod.Method} который отсутствует во всех сервисах. Метод будет пропущен.");
                        continue;
                    }

                    ServiceAPI api = services.FirstOrDefault(x => x.Name == service.Name);
                    if (api == null)
                    {
                        LogHelper.Logger.Error($"При попытке выполнить метод {service.Name}.{runMethod.Method} обнаружилось, что в списке ранее созданных сервисов неожиданно не найден {service.Name}.");
                        continue;
                    }

                    Method m = service.Methods[runMethod.Method];
                    if (m == null)
                    {
                        LogHelper.Logger.Error($"Заявленный на выполнение метод {service.Name}.{m.Name} отсутствует в списке методов сервиса. Метод не будет выполнен.");
                        continue;
                    }

                    // проверим наличие обязательных параметров
                    /*StringBuilder builder = new StringBuilder();
                    foreach (ParameterMethod p in m.OfType<ParameterMethod>().Where(x => x.Required && x.In))
                    {
                        ParameterValue pv = parameters.Get(m.Name, p.Name);
                        if (pv == null)
                            builder.Append($"{p.Name},");
                    }

                    if (builder.Length > 0)
                    {
                        string paramNeeded = builder.Remove(builder.Length - 1, 1).ToString();

                        LogHelper.Logger.Error($"Для метода {m.Name} обязательно использование параметров [{paramNeeded}]. Метод не будет выполнен.");
                        continue;
                    }*/

                    LogHelper.Logger.Info($"Выполнение метода {m.Name}.");

                    HttpStatusCode statusCode = api.Execute(m, parameters);
                }
            }
            finally
            {
                 tran.Commit();
                 conn.Close();
            }
        }

        static string GetConnectionString(string name)
        {
            // Assume failure.
            string returnValue = null;

            // Look for the name in the connectionStrings section.
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings[name];

            // If found, return the connection string.
            if (settings != null)
                returnValue = settings.ConnectionString;

            return returnValue;
        }

        static IEnumerable<ServiceAPI> LoadServices(FbConnection conn, FbTransaction tran)
        {
            // список всех описанных сервисов
            List<ServiceAPI> services = new List<ServiceAPI>();

            foreach (Service service in ApiSection.Instance.Services)
            {
                Type type = Assembly.GetCallingAssembly().GetTypes().FirstOrDefault(x => x.FullName == service.Type);
                if (type != null)
                {
                    ConstructorInfo ci = type.GetConstructor(new Type[] { typeof(Service), typeof(FbConnection), typeof(FbTransaction) });
                    if (ci != null)
                    {
                        services.Add((ServiceAPI)ci.Invoke(new object[] { service, conn, tran }));
                    }
                    else
                    {
                        LogHelper.Logger.Error($"Ошибка при создании обработчика сервиса {service.Name}.  Запросы к этому обработчику будут проигнорированы.");
                    }
                }
                else
                {
                    LogHelper.Logger.Warn($"Обработчик сервиса {service.Name} (configuration/api/services) не найден. Запросы к нему будут проигнорированы.");
                }
            }

            return services;
        }

        static StringDictionary LoadParameters(string[] args)
        {
            StringDictionary parameters = new StringDictionary();

            //список входных параметров
            for (int i = 1; i < args.Length; i++)
            {
                string[] p = args[i].Split('=');
                parameters.Add(p[0].Substring(2), p[1]);

                LogHelper.Logger.Debug($"Parameter {p[0]} = {p[1]}");
            }

            return parameters;
        }

        static Service GetService(string methodName)
        {
            foreach (Service service in ApiSection.Instance.Services)
            {
                foreach (Method method in service.Methods)
                {
                    if (method.Name == methodName)
                    {
                        return service;
                    }
                }
            }

            return null;
        }
    }
}
