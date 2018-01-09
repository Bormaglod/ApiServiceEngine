namespace ApiServiceEngine
{
    using NLog;

    class LogHelper
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        static public Logger Logger => log; 
    }
}
