namespace ApiServiceEngine
{
    using System;

    /// <summary>
    /// Указывает, что тип сериализуется из данных получаемых с момощью метода Name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class MethodDataAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
