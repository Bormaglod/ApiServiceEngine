namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using FirebirdSql.Data.FirebirdClient;
    using ApiServiceEngine.Configuration;
    using CommandLine;

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed((errs) => HandleParseError(errs));
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

            string connectionString = ConfigurationManager.ConnectionStrings[options.Database]?.ConnectionString;
            if (connectionString == null)
            {
                LogHelper.Logger.Error($"Неизвестное имя базы данных {options.Database}");
                return;
            }

            FbConnection conn = new FbConnection(connectionString);
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

                    IEnumerable<Account> accounts;
                    string account_name = string.IsNullOrEmpty(options.Account) ? service.Settings.Accounts.Default : options.Account;
                    if (account_name == "*")
                    {
                        accounts = service.Settings.Accounts.OfType<Account>();
                    }
                    else
                    {
                        Account account = service.Settings.Accounts.GetAccount(account_name);
                        if (account == null)
                        {
                            LogHelper.Logger.Error($"Неверно указано имя регистрационных данных. Метод {runMethod.Method} для задачи {task.Name} не будет выполнен.");
                            continue;
                        }

                        accounts = new Account[] { account };
                    }

                    foreach (Account account in accounts)
                    {
                        List<ServiceAPI> services = new List<ServiceAPI>();

                        ApiSection.Instance.Services
                            .OfType<Service>()
                            .ToList<Service>()
                            .ForEach(x => services.Add(new ServiceAPI(x, account, conn, tran)));

                        ServiceAPI api = services.FirstOrDefault(x => x.Name == service.Name);

                        Method m = service.GetMethod(runMethod.Method);
                        if (m == null)
                        {
                            LogHelper.Logger.Error($"Заявленный на выполнение метод {service.Name}.{runMethod.Method} отсутствует в списке методов сервиса. Метод не будет выполнен.");
                            continue;
                        }

                        LogHelper.Logger.Info($"Выполнение метода {m.Name}.");

                        HttpStatusCode statusCode = api.ExecuteMethod(m, options.GetParamDictionary()).Status;
                        if (statusCode != HttpStatusCode.OK)
                            LogHelper.Logger.Error($"Вызов метода {m.Name} вернул код ошибки {statusCode}.");
                    }
                }
            }
            finally
            {
                tran.Commit();
                conn.Close();
                LogHelper.Logger.Info($"{s}= End of ApiServiveEngine execution ={s}");
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {

        }
    }
}
