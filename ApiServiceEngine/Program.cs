namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Net;
    using FirebirdSql.Data.FirebirdClient;
    using ApiServiceEngine.Configuration;
    using CommandLine;

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        static void RunOptionsAndReturnExitCode(Options options)
        {
            string s = new string('=', 30);
            LogHelper.Logger.Info($"{s} Start of ApiServiveEngine execution {s}");

            // задача на выполнение (заодно проверим правильность конфигурационного файла)
            Task task = null;
            try
            {
                task = ApiSection.Instance.Tasks[options.Task];
            }
            catch (ConfigurationErrorsException e)
            {
                LogHelper.Logger.Error($"{e.BareMessage} (AppServiceEngine.exe.config). Строка {e.Line}");
                return;
            }
            catch (Exception e)
            {
                LogHelper.Logger.Error(e.Message);
                return;
            }

            if (task == null)
            {
                LogHelper.Logger.Error($"Задача {options.Task}, заданная в параметрах, не найдена в списке задач (configuration/api/tasks) и не будет выполнена.");
                return;
            }

            FbConnection conn = new FbConnection(GetConnectionString(options.Database));
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
                LogHelper.Logger.Info($"Выполнение задачи {task.Name}.");

                // выполнение всех методов задачи
                foreach (RunMethod runMethod in task.Methods)
                {
                    Service service = ApiSection.Instance.Services.GetService(runMethod.Method);
                    if (service == null)
                    {
                        LogHelper.Logger.Error($"В задаче {task.Name} указан метод {runMethod.Method} который отсутствует во всех сервисах. Метод будет пропущен.");
                        continue;
                    }

                    IEnumerable<ServiceAPI> services = LoadServices(conn, tran);
                    ServiceAPI api = services.FirstOrDefault(x => x.Name == service.Name);

                    Method m = service.GetMethod(runMethod.Method);
                    if (m == null)
                    {
                        LogHelper.Logger.Error($"Заявленный на выполнение метод {service.Name}.{runMethod.Method} отсутствует в списке методов сервиса. Метод не будет выполнен.");
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

                    HttpStatusCode statusCode = api.ExecuteMethod(m, options.ParamDictionary).Status;
                    if (statusCode != HttpStatusCode.OK)
                        LogHelper.Logger.Error($"Вызов метода {m.Name} вернул код ошибки {statusCode}.");
                }
            }
            finally
            {
                tran.Commit();
                conn.Close();
                LogHelper.Logger.Info($"{s} End of ApiServiveEngine execution {s}");
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {

        }

        static string GetConnectionString(string name)
        {
            // Assume failure.
            string returnValue = null;

            // Look for the name in the connectionStrings section.
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];

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
                    try
                    {
                        services.Add((ServiceAPI)Activator.CreateInstance(type, new object[] { service, conn, tran }));
                    }
                    catch (Exception)
                    {
                        LogHelper.Logger.Warn($"Ошибка при создании обработчика сервиса {service.Name}.  Запросы к этому обработчику будут проигнорированы.");
                    }
                }
                else
                {
                    LogHelper.Logger.Warn($"Не найден класс {service.Type} для создания обработчика сервиса { service.Name}. Запросы к нему будут проигнорированы.");
                }
            }

            return services;
        }
    }
}
