namespace ApiServiceEngine.Configuration
{
    using System.Configuration;
    using FirebirdSql.Data.FirebirdClient;

    class ParameterMethod : ConfigurationElement
    {
        [ConfigurationProperty("param")]
        public string Name => this["param"] as string;

        [ConfigurationProperty("is_page", DefaultValue = false)]
        public bool IsPage => (bool)this["is_page"];

        [ConfigurationProperty("index", DefaultValue = 0)]
        public int Index => (int)this["index"];

        [ConfigurationProperty("required", DefaultValue = true)]
        public bool Required => (bool)this["required"];

        [ConfigurationProperty("in", DefaultValue = false)]
        public bool In => (bool)this["in"];

        [ConfigurationProperty("field")]
        public string Field => (string)this["field"];

        [ConfigurationProperty("type")]
        public FbDbType FieldType => (FbDbType)this["type"];

        [ConfigurationProperty("len", DefaultValue = 0)]
        public int Length => (int)this["len"];

        public override string ToString()
        {
            return $"Parameter name = {Name}";
        }
    }
}
