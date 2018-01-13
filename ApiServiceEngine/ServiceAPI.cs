namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;
    
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

        public (object Info, HttpStatusCode Status) ExecuteMethod(Method method, StringDictionary parameters)
        {
            (object Info, HttpStatusCode Status) = ExecuteWebMethod(method, parameters);
            if (Status == HttpStatusCode.OK)
            {
                Type type = GetType();
                MethodInfo methodInfo = type.GetMethod(method.Name, StringComparison.CurrentCultureIgnoreCase);
                if (methodInfo != null)
                    return (methodInfo.Invoke(this, new object[] { method, parameters, Info }), Status);
            }

            return (Info, Status);
        }

        public (object Info, HttpStatusCode Status) ExecuteWebMethod(string methodName, StringDictionary parameters)
        {
            return ExecuteWebMethod(service.Methods[methodName], parameters);
        }

        public (object Info, HttpStatusCode Status) ExecuteWebMethod(Method method, StringDictionary parameters)
        {
            HttpWebResponse response;
            Type type = GetType();
            Type typeData = null;

            foreach (Type t in type.GetNestedTypes(BindingFlags.NonPublic))
            {
                IEnumerable<MethodDataAttribute> attrs = t.GetCustomAttributes<MethodDataAttribute>();
                if (attrs.FirstOrDefault(x => string.Compare(x.Name, method.Name, StringComparison.CurrentCultureIgnoreCase) == 0) != null)
                {
                    typeData = t;
                    break;
                }
            }

            if (typeData == null)
                return (null, HttpStatusCode.NotImplemented);

            Type typeRequest = null;
            if (method.Request == RequestMethod.Post)
            {
                foreach (Type t in type.GetNestedTypes(BindingFlags.NonPublic).Where(x => x.BaseType == typeof(SerializedObject)))
                {
                    IEnumerable<MethodDataAttribute> attrs = t.GetCustomAttributes<MethodDataAttribute>();
                    if (attrs.FirstOrDefault(x => string.Compare(x.Name, method.Name, StringComparison.CurrentCultureIgnoreCase) == 0) != null)
                    {
                        typeRequest = t;
                        break;
                    }
                }

                if (typeRequest == null)
                    return (null, HttpStatusCode.NotImplemented);

                SerializedObject p = (SerializedObject)Activator.CreateInstance(typeRequest, new object[] { this, Service, method, parameters });
                response = GetResponse(method, p);
            }
            else
            {
                response = GetResponse(method, parameters);
            }

            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeData);
            object info = serializer.ReadObject(response.GetResponseStream());
            foreach (PropertyInfo prop in info.GetType().GetProperties())
            {
                if (prop.GetCustomAttribute<ReturningValueAttribute>() != null)
                {
                    info = prop.GetValue(info);
                    break;
                }
            }

            return (info, response.StatusCode);
        }

        public object GetPropertyFromMethod(string method, string param, StringDictionary parameters)
        {
            (object Info, HttpStatusCode Status) = ExecuteWebMethod(method, parameters);
            if (Info == null)
            {
                throw new ExecuteMethodException($"Вызов метода {method} произведен неудачно.");
            }

            PropertyInfo pInfo = Info.GetType().GetProperty(param);
            if (pInfo == null)
            {
                throw new UnknownPropertyException($"Неизвестный параметр {param} метода {method}");
            }

            return pInfo.GetValue(Info);
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
            string address = GetBaseUrl(method);
            string json = obj.Json();
            LogHelper.Logger.Info($"{address} {json}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            byte[] data = Encoding.ASCII.GetBytes(json);

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
            Method method = service.GetMethod(methodName);

            FbCommand cmd = new FbCommand(method.Procedure, Connection, Transaction)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            foreach (Parameter p in method.In.OfType<Parameter>().Where(x => !string.IsNullOrEmpty(x.Db.Field)))
            {
                string pName = p.Name.ToLower();
                if (!parameters.ContainsKey(pName))
                    continue;

                cmd.Parameters.Add(p.Db.Field, p.Db.Type, p.Db.Length).Value = parameters[pName];
            }

            foreach (Parameter p in method.Out.OfType<Parameter>().Where(x => !string.IsNullOrEmpty(x.Db.Field)))
            {
                object o = null;
                foreach (object item in obj)
                {
                    PropertyInfo prop = item.GetType().GetProperty(p.Name, StringComparison.CurrentCultureIgnoreCase);
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
