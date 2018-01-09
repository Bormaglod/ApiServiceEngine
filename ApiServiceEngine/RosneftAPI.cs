namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;

    class RosneftAPI : ServiceAPI
    {
        [DataContract]
        class InfoCard
        {
            [DataMember]
            public int SubjID { get; set; }

            [DataMember]
            public string ContractNum { get; set; }

            [DataMember]
            public string ContractDate { get; set; }
        }

        [DataContract]
        class Goods
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string FName { get; set; }
        }

        [DataContract]
        class InfoGoods
        {
            [DataMember]
            public List<Goods> GoodsList { get; set; }
        }

        public RosneftAPI(Service config, FbConnection connection, FbTransaction transaction) : base(config, connection, transaction)
        {
        }

        public HttpStatusCode InfoCardContract(HttpWebResponse response, ParameterList parameters)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(InfoCard));
                InfoCard info = (InfoCard)serializer.ReadObject(response.GetResponseStream());
                FbCommand cmd = Prepare("InfoCardContract", parameters, info);

                FbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    object result = reader["RES"];
                    object message = reader["MSG"];
                    LogHelper.Logger.Debug(message);
                }

                reader.Close();
            }

            return response.StatusCode;
        }

        public HttpStatusCode InfoGoodsList(HttpWebResponse response, ParameterList parameters)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(InfoGoods));
                InfoGoods info = (InfoGoods)serializer.ReadObject(response.GetResponseStream());
                foreach (Goods g in info.GoodsList)
                {
                    FbCommand cmd = Prepare("InfoGoodsList", parameters, g);
                    FbDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object result = reader["RES"];
                        object message = reader["MSG"];
                        LogHelper.Logger.Debug(message);
                    }

                    reader.Close();
                }
            }

            return response.StatusCode;
        }

        override protected string GetAddresWithParameters(string methodName, ParameterList parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Address}/{methodName}/");
            Method method = Config.Methods.Get(methodName);
            if (method != null)
            {
                foreach (Parameter param in method.OfType<Parameter>().Where(x => x.IsPage && x.In).OrderBy(x => x.Index))
                {
                    ParameterValue pv = parameters.Get(method.ApiName, param.Name);
                    if (pv != null)
                    {
                        sb.Append($"{pv.Value}/");
                    }
                }


                IEnumerable<Parameter> p = method.OfType<Parameter>().Where(x => !x.IsPage && x.In);
                string ep = GetExtendedParameteres();
                if (p.Any() || !string.IsNullOrWhiteSpace(ep))
                {
                    sb.Append('?');

                    foreach (Parameter param in p)
                    {
                        ParameterValue pv = parameters.Get(method.ApiName, param.Name);
                        if (pv != null)
                        {
                            sb.Append($"{param.Name}={pv.Value}&");
                        }
                    }

                    if (string.IsNullOrWhiteSpace(ep))
                        sb.Remove(sb.Length - 1, 1);
                    else
                        sb.Append(ep);
                }
            }

            return sb.ToString();
        }

        string GetExtendedParameteres()
        {
            return $"u={Login}&p={Password}&type=JSON";
        }

        FbCommand Prepare(string methodName, ParameterList parameters, object obj)
        {
            Method method = Config.Methods.Get(methodName);

            FbCommand cmd = new FbCommand(method.Procedure, Connection, Transaction);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            foreach (Parameter p in method.OfType<Parameter>())
            {
                if (string.IsNullOrWhiteSpace(p.Field))
                    continue;

                object o;
                if (p.In)
                {
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
