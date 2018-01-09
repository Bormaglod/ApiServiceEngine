namespace ApiServiceEngine
{
    using ApiServiceEngine.Configuration;

    class ParameterValue
    {
        ParameterMapItem parameter;
        string value;
        bool isActive;

        public ParameterValue(ParameterMapItem parameter)
        {
            this.parameter = parameter;
            this.value = string.Empty;
        }

        public string Name => parameter.Name;

        public string ApiName => parameter.ApiName;

        public string Method => parameter.Method;

        public bool IsActive => isActive;

        public string Value => value;

        public void Activate(string parameterValue)
        {
            value = parameterValue;
            isActive = true;
        }
    }
}
