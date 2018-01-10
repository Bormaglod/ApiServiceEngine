namespace ApiServiceEngine
{
    using System;
    using System.Reflection;
    using System.Linq;
    using System.Net;
    using System.Text;
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
            MethodInfo method = type.GetMethod(methodName, new Type[] { typeof(HttpWebResponse), typeof(ParameterList) });
            if (method != null)
            {
                string address = GetAddresWithParameters(methodName, parameters);
                LogHelper.Logger.Info(address);

                HttpWebResponse response = GetResonse(address);
                if (response == null)
                {
                    return HttpStatusCode.BadRequest;
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return response.StatusCode;
                }

                return (HttpStatusCode)method.Invoke(this, new object[] { response, parameters });
            }

            method = type.GetMethod(methodName, new Type[] { typeof(ParameterList) });
            if (method != null)
            {
                return (HttpStatusCode)method.Invoke(this, new object[] { parameters });
            }
            
            LogHelper.Logger.Error($"Метод {methodName} еще не реализован (ApiServiceEngine.ServiceAPI.Execute).");
            return HttpStatusCode.NotImplemented;
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

        protected (int code, string message) ExecuteProcedure(string name, ParameterList parameters, object obj)
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

        FbCommand Prepare(string methodName, ParameterList parameters, object obj)
        {
            Method method = Config.Methods.Get(methodName);

            FbCommand cmd = new FbCommand(method.Procedure, Connection, Transaction);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            foreach (ParameterMethod p in method.OfType<ParameterMethod>())
            {
                if (string.IsNullOrWhiteSpace(p.Field))
                    continue;

                object o;
                if (p.In)
                {
                    ParameterValue pv = parameters.Get(method.Name, p.Name);
                    if (pv == null)
                        continue;

                    o = parameters.Get(method.Name, p.Name).Value;
                }
                else
                {
                    string name = p.Name.Substring(0, 1).ToUpper() + p.Name.Substring(1);
                    o = obj.GetType().GetProperty(name).GetValue(obj);
                }

                cmd.Parameters.Add(p.Field, p.FieldType, p.Length).Value = o;
            }

            return cmd;
        }
    }
}
