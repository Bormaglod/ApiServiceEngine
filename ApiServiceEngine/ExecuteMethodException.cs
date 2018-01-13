namespace ApiServiceEngine
{
    using System;

    class ExecuteMethodException : Exception
    {
        public ExecuteMethodException(string message) : base(message) { }
    }
}
