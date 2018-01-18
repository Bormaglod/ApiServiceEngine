namespace ApiServiceEngine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    class Options
    {
        [Option('t', "task", Required = true, HelpText = "Имя задачи заданное в конфигурационном файле в секции /configuration/api/tasks")]
        public string Task { get; set; }

        [Option('p', "param", Required = true, HelpText = "Список параметров задачи вида Имя=Значение перечисленные через запятую. Имена параметров задаются в конфигурационном файле в секции /configuration/api/services/methods/method/in", Separator = ',')]
        public IList<string> Parameters { get; set; }

        [Option('d', "database", Default = "default", HelpText = "Имя базы данных (список перечислен в конфигурационном файле в секции /configuration/connectionStrings")]
        public string Database { get; set; }

        /*[HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }*/
    }
}
