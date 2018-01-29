namespace ApiServiceEngine
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using CommandLine;

    class Options
    {
        [Option('t', "task", Required = true, HelpText = "Имя задачи заданное в конфигурационном файле в секции /configuration/api/tasks")]
        public string Task { get; set; }

        [Option('p', "param", Required = true, HelpText = "Список параметров задачи вида Имя=Значение перечисленные через запятую. Имена параметров задаются в конфигурационном файле в секции /configuration/api/services/methods/method/in", Separator = ',')]
        public IList<string> Parameters { get; set; }

        [Option('d', "database", Default = "default", HelpText = "Имя базы данных (список перечислен в конфигурационном файле в секции /configuration/connectionStrings")]
        public string Database { get; set; }

        public StringDictionary ParamDictionary
        {
            get
            {
                StringDictionary parameters = new StringDictionary();

                //список входных параметров
                foreach (string item in Parameters.Where(x => !string.IsNullOrEmpty(x)))
                {
                    string[] p = item.Split('=');
                    if (p.Length != 2)
                    {
                        LogHelper.Logger.Error($"Параметр {item} имеет неверный формат.");
                        return null;
                    }

                    parameters.Add(p[0].ToLower(), p[1].Trim());
                }

                return parameters;
            }
        }
    }
}
