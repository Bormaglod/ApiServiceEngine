namespace ApiServiceEngine
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Linq;
    using System.Net;
    using System.Text;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;
    using Inflector;

    abstract class ServiceAPI
    {
        Service service;
        FbConnection connection;
        FbTransaction transaction;

        public ServiceAPI(Service config, FbConnection connection, FbTransaction transaction)
        {
            this.service = config;
            this.connection = connection;
            this.transaction = transaction;
        }

        public string Password => service.Settings.Password;

        public string Url => service.Settings.Url;

        public string Login => service.Settings.Login;

        public string Name => service.Name;

        public string Comment => service.Comment;

        protected FbConnection Connection => connection;

        protected FbTransaction Transaction => transaction;

        protected Service Service => service;

        public (object Info, HttpStatusCode Status) ExecuteMethod(string methodName, StringDictionary parameters)
        {
            return ExecuteMethod(service.Methods[methodName], parameters);
        }

        public (object Info, HttpStatusCode Status) ExecuteMethod(Method method, StringDictionary parameters)
        {
            Type type = GetType();
            MethodInfo methodInfo = type.GetMethod(method.Name, new Type[] { typeof(Method), typeof(StringDictionary), typeof(bool) });
            if (methodInfo != null)
            {
                return (ValueTuple<object, HttpStatusCode>)methodInfo.Invoke(this, new object[] { method, parameters, true });
            }

            return (null, HttpStatusCode.NotImplemented);
        }

        public HttpStatusCode Execute(Method method, StringDictionary parameters)
        {
            (object Info, HttpStatusCode Status) = ExecuteMethod(method, parameters);
            if (Status == HttpStatusCode.NotImplemented)
                LogHelper.Logger.Error($"Метод {method.Name} еще не реализован (ApiServiceEngine.ServiceAPI.Execute).");

            return Status;
        }

        protected HttpWebResponse GetResponse(Method method, StringDictionary parameters)
        {
            string address = GetUrl(method, parameters);
            LogHelper.Logger.Info(address);

            HttpWebRequest http = (HttpWebRequest)WebRequest.Create(address);
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

        protected HttpWebResponse GetResponse(Method method, SerializedObject obj)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetBaseUrl(method));
            byte[] data = Encoding.ASCII.GetBytes(obj.Json());

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            return (HttpWebResponse)request.GetResponse();
        }

        abstract protected string GetBaseUrl(Method method);

        abstract protected string GetParametersUrl(Method method, StringDictionary parameters);

        protected string GetUrl(Method method, StringDictionary parameters)
        {
            return GetBaseUrl(method) + GetParametersUrl(method, parameters);
        }

        protected string Sql(FbCommand cmd)
        {
            string cmd_text = cmd.CommandText;
            StringBuilder builder = new StringBuilder();
            foreach (FbParameter p in cmd.Parameters)
            {
                builder.Append($"{p.ParameterName}: {p.Value}, ");
            }
            
            if (builder.Length > 0)
                builder.Remove(builder.Length - 2, 2);

            return $"{cmd_text}({builder.ToString()})";
        }

        protected (int code, string message) ExecuteProcedure(string name, StringDictionary parameters, params object[] obj)
        {
            (int code, string message) result = (0, string.Empty);

            FbCommand cmd = Prepare(name, parameters, obj);
            LogHelper.Logger.Info(Sql(cmd));
            try
            {
                FbDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    result.code = (int)reader["RES"];
                    result.message = reader["MSG"].ToString();

                    if (result.code == 0)
                        LogHelper.Logger.Debug(result.message);
                    else
                        LogHelper.Logger.Error(result.message);
                }

                reader.Close();
            }
            catch (Exception e)
            {
                LogHelper.Logger.Error(e.Message);
            }

            return result;
        }

        FbCommand Prepare(string methodName, StringDictionary parameters, params object[] obj)
        {
            Method method = service.Methods[methodName];

            FbCommand cmd = new FbCommand(method.Procedure, Connection, Transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            foreach (Parameter p in method.In.OfType<Parameter>().Where(x => !string.IsNullOrEmpty(x.Db.Field)))
            {
                if (!parameters.ContainsKey(p.Name))
                    continue;

                cmd.Parameters.Add(p.Db.Field, p.Db.Type, p.Db.Length).Value = parameters[p.Name];
            }

            foreach (Parameter p in method.Out.OfType<Parameter>().Where(x => !string.IsNullOrEmpty(x.Db.Field)))
            {
                object o = null;
                foreach (object item in obj)
                {
                    PropertyInfo prop = item.GetType().GetProperty(Inflector.Pascalize(p.ApiName));
                    if (prop == null)
                    {
                        prop = item.GetType().GetProperty(Inflector.Pascalize(p.Name));
                    }

                    if (prop != null)
                    {
                        o = prop.GetValue(item);
                        break;
                    }
                }

                if (o != null)
                    cmd.Parameters.Add(p.Db.Field, p.Db.Type, p.Db.Length).Value = o;
            }

            return cmd;
        }
    }
}
