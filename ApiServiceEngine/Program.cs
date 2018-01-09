namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Net;
    using System.Text;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;

    class Program
    {
        static readonly ApiConfigSection config = ApiConfigSection.GetSection(ConfigurationUserLevel.None);

        static void Main(string[] args)
        {
            LogHelper.Logger.Info("Starting ApiServiveEngine");

            if (args.Length == 0)
            {
                LogHelper.Logger.Info("Запуск ApiServiceEngine без параметров.");
                return;
            }

            // задача на выполнение
            Task task = config.Tasks.Get(args[0]);
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
                ParameterList parameters = LoadParameters(args);

                LogHelper.Logger.Info($"Выполнение задачи {task.Name}.");

                // выполнение всех методов задачи
                foreach (TaskMethod tm in task.Methods)
                {
                    Service service = config.Services.GetByMethod(tm.Method);
                    if (service == null)
                    {
                        LogHelper.Logger.Error($"В задаче {task.Name} указан метод {tm.Method} который отсутствует во всех сервисах. Метод будет пропущен.");
                        continue;
                    }

                    ServiceAPI api = services.FirstOrDefault(x => x.Name == service.Name);
                    if (api == null)
                    {
                        LogHelper.Logger.Error($"При попытке выполнить метод {service.Name}.{tm.Method} обнаружилось, что в списке ранее созданных сервисов неожиданно не найден {service.Name}.");
                        continue;
                    }

                    Method m = service.Methods.Get(tm.Method);
                    if (m == null)
                    {
                        LogHelper.Logger.Error($"Заявленный на выполнение метод {service.Name}.{m.Name} отсутствует в списке методов сервиса. Метод не будет выполнен.");
                        continue;
                    }

                    // проверим наличие обязательных параметров
                    StringBuilder builder = new StringBuilder();
                    foreach (Parameter p in m.OfType<Parameter>().Where(x => x.Required && x.In))
                    {
                        ParameterValue pv = parameters.Get(m.Name, p.Name);
                        if (pv == null)
                            builder.Append($"{p.Name},");
                    }

                    if (builder.Length > 0)
                    {
                        string paramNeeded = builder.Remove(builder.Length - 1, 1).ToString();

                        LogHelper.Logger.Error($"Для метода {m.Name} обязательно использование параметров {paramNeeded}. Метод не будет выполнен.");
                    }

                    LogHelper.Logger.Info($"Выполнение метода {m.Name}.");

                    HttpStatusCode statusCode = api.Execute(tm.Method, parameters);
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

            foreach (Service service in config.Services)
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

        static ParameterList LoadParameters(string[] args)
        {
            ParameterList parameters = new ParameterList();
            foreach (ParameterMapItem item in config.Parameters)
            {
                parameters.Add(item);
            }

            //список входных параметров
            for (int i = 1; i < args.Length; i++)
            {
                string[] p = args[i].Split('=');
                ParameterValue pv = parameters.Get(p[0].Substring(2));
                if (pv != null)
                    pv.Activate(p[1]);
            }

            return parameters;
        }
    }
}
