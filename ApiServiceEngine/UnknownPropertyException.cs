namespace ApiServiceEngine
{
    using System;

    class UnknownPropertyException : Exception
    {
        public UnknownPropertyException(string message) : base(message) { }
    }
}
