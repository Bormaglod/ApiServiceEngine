namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Net;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;

    abstract class ServiceAPI
    {
        Service config;
        FbConnection connection;
        FbTransaction transaction;

        public ServiceAPI(Service config, FbConnection connection, FbTransaction transaction)
        {
            this.config = config;
            this.connection = connection;
            this.transaction = transaction;
        }

        public string Password => config.Settings.Password;

        public string Name => config.Name;

        public string Comment => config.Comment;

        public string Address => config.Settings.Address;

        public string Login => config.Settings.Login;

        protected FbConnection Connection => connection;

        protected FbTransaction Transaction => transaction;

        protected Service Config => config;

        public HttpStatusCode Execute(string methodName, ParameterList parameters)
        {
            Type type = GetType();
            MethodInfo method = type.GetMethod(methodName);
            string address = GetAddresWithParameters(methodName, parameters);
            LogHelper.Logger.Info(address);

            HttpWebResponse response = GetResonse(address);
            if (response == null)
            {
                return HttpStatusCode.BadRequest;
            }

            return (HttpStatusCode)method.Invoke(this, new object[] { response, parameters });
        }

        protected HttpWebResponse GetResonse(string request)
        {
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create(request);
            try
            {
                HttpWebResponse response = (HttpWebResponse)http.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    LogHelper.Logger.Error(response.StatusDescription);
                }

                return response;
            }
            catch (Exception e)
            {
                LogHelper.Logger.Error(e.Message);
            }

            return null;
        }

        abstract protected string GetAddresWithParameters(string methodName, ParameterList parameters);
    }
}
