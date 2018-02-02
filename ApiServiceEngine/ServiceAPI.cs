namespace ApiServiceEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ServiceAPI
    {
        Service service;
        Account account;
        FbConnection connection;
        FbTransaction transaction;

        public ServiceAPI(Service config, Account account, FbConnection connection, FbTransaction transaction)
        {
            this.service = config;
            this.account = account;
            this.connection = connection;
            this.transaction = transaction;
        }

        public string Name => service.Name;

        protected FbConnection Connection => connection;

        protected FbTransaction Transaction => transaction;

        public (object Info, HttpStatusCode Status) ExecuteMethod(Method method, StringDictionary parameters)
        {
            (List<Dictionary<Parameter, object>> Info, HttpStatusCode Status) = ExecuteWebMethod(method, parameters);
            if (Status == HttpStatusCode.OK)
            {
                foreach (Dictionary<Parameter, object> d in Info)
                {
                    ExecuteProcedure(method.Name, parameters, d);
                }
            }

            return (Info, Status);
        }

        public (List<Dictionary<Parameter, object>> Info, HttpStatusCode Status) ExecuteWebMethod(Method method, StringDictionary parameters)
        {
            HttpWebResponse response = method.Request == RequestMethod.Post ? 
                PostResponse(method, parameters) : 
                GetResponse(method, parameters);

            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            StreamReader stream = new StreamReader(response.GetResponseStream());
            Dictionary<string, object> d = DeserializeToDictionary(string.Empty, stream.ReadLine());

            return (DictionaryFromApiResult(method, d), response.StatusCode);
        }

        private object ExecuteMethod(string methodName, string parameterName, StringDictionary parameters)
        {
            Method method = service.GetMethod(methodName);
            Parameter parameter = method.Out.GetApiParameter(parameterName);
            if (parameter == null)
            {
                throw new UnknownPropertyException($"Неизвестный параметр {parameterName} при вызове метода {methodName}");
            }

            (List<Dictionary<Parameter, object>> Info, HttpStatusCode Status) = ExecuteWebMethod(method, parameters);
            if (Info == null)
            {
                throw new ExecuteMethodException($"Вызов метода {method} произведен неудачно.");
            }

            if (Status == HttpStatusCode.OK)
            {
                StringBuilder builder = new StringBuilder();
                foreach (Dictionary<Parameter, object> item in Info)
                {
                    if (item.ContainsKey(parameter))
                    {
                        if (builder.Length > 0)
                            builder.Append(",");

                        builder.Append(item[parameter]);
                    }
                }

                return builder.ToString();
            }

            return string.Empty;
        }

        private HttpWebResponse GetResponse(Method method, StringDictionary parameters)
        {
            try
            {
                string address = MacroResult(method, parameters, method.Request == RequestMethod.Get ? service.Settings.Get.Url : service.Settings.Post.Url);
            
                LogHelper.Logger.Info(address);

                HttpWebRequest http = (HttpWebRequest)WebRequest.Create(address);
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

        private HttpWebResponse PostResponse(Method method, StringDictionary parameters)
        {
            Dictionary<string, object> content = new Dictionary<string, object>();
            foreach (Content item in service.Settings.Post.Contents)
            {
                try
                {
                    string value = MacroResult(method, parameters, item.Value);
                    content.Add(item.Name, value);
                }
                catch (Exception e)
                {
                    LogHelper.Logger.Error(e.Message);
                }
            }

            foreach (Parameter item in method.In)
            {
                if (parameters.ContainsKey(item.Name.ToLower()))
                {
                    if (item.IsList)
                    {
                        content.Add(item.GetApiName(), parameters[item.Name].Split(new string[] { item.Separator }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        content.Add(item.GetApiName(), parameters[item.Name]);
                    }
                }
                else
                {
                    if (item.Required)
                    {
                        string res = string.Empty;
                        if (!string.IsNullOrEmpty(item.Default))
                        {
                            res = item.Default;
                        }
                        else if (!string.IsNullOrEmpty(item.AccountParameter))
                        {
                            res = account.GetParameterValue(item.AccountParameter);
                        }
                        else if (!string.IsNullOrEmpty(item.Recive.Method))
                        {
                            res = ExecuteMethod(item.Recive.Method, item.Recive.Parameter, parameters).ToString();
                        }

                        if (!string.IsNullOrEmpty(res))
                            content.Add(item.GetApiName(), res);
                        else
                            LogHelper.Logger.Error($"Отсутствует параметр {item.Name}");
                    }
                }
            }

            try
            {
                string address = MacroResult(method, parameters, method.Request == RequestMethod.Get ? service.Settings.Get.Url : service.Settings.Post.Url);
                string json = JsonConvert.SerializeObject(content);
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
            catch (Exception e)
            {
                LogHelper.Logger.Error(e.Message);
            }

            return null;
        }

        private string Sql(FbCommand cmd)
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

        private (int code, string message) ExecuteProcedure(string name, StringDictionary parameters, Dictionary<Parameter, object> dictionary)
        {
            (int code, string message) result = (0, string.Empty);

            FbCommand cmd = Prepare(name, parameters, dictionary);
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

        private List<Dictionary<Parameter, object>> DictionaryFromApiResult(Method method, Dictionary<string, object> source)
        {
            List<Dictionary<Parameter, object>> dest = new List<Dictionary<Parameter, object>>();
            Dictionary<Parameter, object> dictionary = new Dictionary<Parameter, object>();
            foreach (KeyValuePair<string, object> d in source)
            {
                if (d.Value is IList)
                {
                    foreach (Dictionary<string, object> item in (IList)d.Value)
                    {
                        int cnt = dest.Count;
                        Dictionary<Parameter, object> dt = DictionaryFromApiResult(method, item, dictionary, dest);
                        if (dt.Count > 0 && cnt == dest.Count)
                            dest.Add(dt);
                    }
                }
                else
                {
                    Parameter parameter = method.Out.GetApiParameter(d.Key);
                    if (parameter != null)
                    {
                        dictionary.Add(parameter, d.Value);
                    }
                }
            }

            if (dest.Count == 0)
                dest.Add(dictionary);

            return dest;
        }

        private Dictionary<Parameter, object> DictionaryFromApiResult(Method method, Dictionary<string, object> source, Dictionary<Parameter, object> prev, List<Dictionary<Parameter, object>> dest)
        {
            Dictionary<Parameter, object> dictionary = new Dictionary<Parameter, object>();

            foreach (KeyValuePair<string, object> d in source)
            {
                if (d.Value is IList)
                {
                    foreach (Dictionary<string, object> item in (IList)d.Value)
                    {
                        Dictionary<Parameter, object> dt = DictionaryFromApiResult(method, item, prev, dest);
                        
                        foreach (KeyValuePair<Parameter, object> p in prev)
                        {
                            dt.Add(p.Key, p.Value);
                        }

                        dest.Add(dt);
                    }
                }
                else if (d.Value is IDictionary)
                {
                    Dictionary<Parameter, object> dt = DictionaryFromApiResult(method, (Dictionary<string, object>)d.Value, dictionary, dest);
                }
                else
                {
                    Parameter parameter = method.Out.GetApiParameter(d.Key);
                    if (parameter != null)
                    {
                        dictionary.Add(parameter, d.Value);
                    }
                }
            }

            return dictionary;
        }

        private Dictionary<string, object> DeserializeToDictionary(string key, string jo)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
            var values2 = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> d in values)
            {
                string full_key = $"{key}/{d.Key}";
                if (d.Value == null)
                {
                    values2.Add(full_key, string.Empty);
                }
                else if (d.Value is JArray)
                {
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                    foreach (var item in (JArray)d.Value)
                    {
                        Dictionary<string, object> dictionary = DeserializeToDictionary(full_key, item.ToString());
                        list.Add(dictionary);
                    }

                    values2.Add(d.Key, list);
                }
                else if (d.Value is JObject)
                {
                    values2.Add(full_key, DeserializeToDictionary(full_key, d.Value.ToString()));
                }
                else
                {
                    values2.Add(full_key, d.Value);
                }
            }

            return values2;
        }

        private FbCommand Prepare(string methodName, StringDictionary parameters, Dictionary<Parameter, object> dictionary)
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
                object obj = string.Empty;
                if (dictionary.ContainsKey(p))
                    obj = dictionary[p];

                // TODO: Тип параметра (если указан)?
                if (obj != null)
                    //cmd.Parameters.Add(p.Db.Field, p.Db.Type, p.Db.Length).Value = obj;
                    cmd.Parameters.Add(p.Db.Field, obj);
            }

            return cmd;
        }

        private string GetPagesUrl(Method method, StringDictionary parameters)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<Parameter> p = method.In.OfType<Parameter>().Where(x => x.IsPage).OrderBy(x => x.Index);
            foreach (Parameter param in p)
            {
                string pName = param.Name.ToLower();
                if (!UpdateParameters(param, parameters))
                    continue;

                if (!parameters.ContainsKey(pName))
                {
                    if (param.Required)
                    {
                        if (!string.IsNullOrEmpty(param.Default))
                        {
                            sb.Append($"{param.Default}/");
                        }
                        else if (!string.IsNullOrEmpty(param.AccountParameter))
                        {
                            sb.Append($"{account.GetParameterValue(param.AccountParameter)}/");
                        }
                        else
                            LogHelper.Logger.Error($"Не указан параметр {pName}. Укажите его в аргументах командной строки.");
                    }
                }
                else
                  sb.Append($"{parameters[pName]}/");
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private string GetParametersUrl(Method method, StringDictionary parameters)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<Parameter> p = method.In.OfType<Parameter>().Where(x => !x.IsPage);
            foreach (Parameter param in p)
            {
                string pName = param.Name.ToLower();
                if (!UpdateParameters(param, parameters))
                    continue;

                if (!parameters.ContainsKey(pName))
                {
                    if (param.Required)
                    {
                        if (!string.IsNullOrEmpty(param.Default))
                        {
                            sb.Append($"{param.GetApiName()}={param.Default}&");
                        }
                        else if (!string.IsNullOrEmpty(param.AccountParameter))
                        {
                            sb.Append($"{param.GetApiName()}={account.GetParameterValue(param.AccountParameter)}&");
                        }
                        else
                            LogHelper.Logger.Error($"Не указан параметр {pName}. Укажите его в аргументах командной строки.");
                    }
                }
                else
                    sb.Append($"{param.GetApiName()}={parameters[pName]}&");
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private string MacroResult(Method method, StringDictionary parameters, string inputText)
        {
            var macro = new Dictionary<string, Func<object>>()
            {
                { "version", () => method.Version == 0 ? 2 : method.Version },
                { "method_name",    () => string.IsNullOrEmpty(method.ApiName) ? method.Name : method.ApiName },
                { "pages", () => GetPagesUrl(method, parameters) },
                { "parameters", () => GetParametersUrl(method, parameters) },
                { "login", () => account.Login },
                { "password", () => account.Password }
            };

            string pattern = @"\[(?<p1>.*?(?<p2>{(?<m>(?<={).+?(?=}))}).*?)\]|{(?<m>(?<={).+?(?=}))}";
            string text = Regex.Replace(inputText, pattern,
                (m) =>
                {
                    string name = m.Groups["m"].Value.ToLower();
                    if (macro.ContainsKey(name))
                    {
                        string res = macro[name]().ToString();
                        if (!(string.IsNullOrEmpty(m.Groups["p1"].Value) || string.IsNullOrEmpty(res)))
                        {
                            res = m.Groups["p1"].Value.Replace(m.Groups["p2"].Value, res);
                        }

                        return res;
                    }

                    throw new Exception("Макрос {" + name + "} не найден");
                });

            return text;
        }

        bool UpdateParameters(Parameter param, StringDictionary parameters)
        {
            string pName = param.Name.ToLower();
            if (!parameters.ContainsKey(pName))
            {
                if (string.IsNullOrEmpty(param.Recive.Method))
                    return false;

                try
                {
                    object obj = ExecuteMethod(param.Recive.Method, param.Recive.Parameter, parameters);
                    parameters.Add(pName, obj.ToString());
                    return true;
                }
                catch (Exception e)
                {
                    LogHelper.Logger.Error(e.Message);
                }
            }

            return true;
        }
    }
}
