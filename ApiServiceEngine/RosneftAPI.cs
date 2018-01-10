namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;

    class RosneftAPI : ServiceAPI
    {
        #region InfoCard

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

        #endregion

        #region Goods

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

        #endregion
        
        
        #region Operation

        [DataContract]
        class Operation
        {
            [DataMember]
            public string OperDate { get; set; }

            [DataMember]
            public decimal CardID { get; set; }

            [DataMember]
            public int OperType { get; set; }

            [DataMember]
            public decimal OperValue { get; set; }

            [DataMember]
            public decimal OperSum { get; set; }

            [DataMember]
            public decimal OperPrice { get; set; }

            [DataMember]
            public int SubjID { get; set; }

            [DataMember]
            public string PosAddress { get; set; }

            [DataMember]
            public int GoodsID { get; set; }

            [DataMember]
            public string GoodsName { get; set; }

            [DataMember]
            public string CardHolder { get; set; }

            [DataMember]
            public string OperGuid { get; set; }

            [DataMember]
            public int Pos { get; set; }
        }

        [DataContract]
        class Operations
        {
            [DataMember]
            public List<Operation> OperationList { get; set; }
        }

        #endregion

        #region OperationDisc

        [DataContract]
        class OperationDisc : Operation
        {
            [DataMember]
            public decimal OperSumDisc { get; set; }

            [DataMember]
            public decimal OperPriceDisc { get; set; }
        }

        [DataContract]
        class OperationsDisc
        {
            [DataMember]
            public List<OperationDisc> OperationList { get; set; }
        }

        #endregion

        #region CardAccount

        [DataContract]
        class CardAccount
        {
            [DataMember]
            public decimal CardID { get; set; }

            [DataMember]
            public decimal SubjSaldo { get; set; }

            [DataMember]
            public decimal CardSaldo { get; set; }

            [DataMember]
            public decimal OperOpSum { get; set; }
        }

        [DataContract]
        class CardAccounts
        {
            [DataMember]
            public List<CardAccount> CardAccountList { get; set; }
        }

        #endregion

        #region Card

        [DataContract]
        class Card
        {
            [DataMember]
            public decimal ID { get; set; }

            [DataMember]
            public string Status { get; set; }

            [DataMember]
            public string Owner { get; set; }

            [DataMember]
            public string Type { get; set; }
        }

        [DataContract]
        class CardList
        {
            [DataMember]
            public List<Card> Cards { get; set; }
        }

        #endregion

        #region ContractGoods

        [DataContract]
        class ContractGoods
        {
            [DataMember]
            public int ID { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public decimal Saldo { get; set; }

            [DataMember]
            public string Unit { get; set; }
        }

        [DataContract]
        class ContractGoodsList
        {
            [DataMember]
            public List<ContractGoods> ContractGoods { get; set; }
        }

        #endregion

        #region Limit

        [DataContract]
        class Limit
        {
            [DataMember]
            public decimal Card { get; set; }

            [DataMember]
            public int Err { get; set; }

            [DataMember]
            public decimal Daily { get; set; }

            [DataMember]
            public decimal Monthly { get; set; }

            [DataMember]
            public decimal DailyCurr { get; set; }

            [DataMember]
            public decimal MonthlyCurr { get; set; }
        }

        [DataContract]
        class LimitList
        {
            [DataMember]
            public List<Limit> Limit { get; set; }
        }

        [DataContract]
        class SerializedObject
        {
            public string Json()
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LimitListRequest));
                string json = string.Empty;
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, this);
                    json = Encoding.Default.GetString(stream.ToArray());
                }

                return json;
            }
        }

        [DataContract]
        class LimitListRequest : SerializedObject
        {
            public LimitListRequest(Service config, ParameterList parameters)
            {
                User = config.Settings.Login;
                Password = config.Settings.Password;
                SubjID = int.Parse(parameters.Get("ContractID").Value);

                string[] cards = parameters.Get("Cards").Value.Split(new char[] { ',' });
                CardList = new decimal[cards.Length];
                for (int i = 0; i < cards.Length; i++)
                {
                    CardList[i] = decimal.Parse(cards[i]);
                }
            }

            [DataMember]
            public string User { get; set; }

            [DataMember]
            public string Password { get; set; }

            [DataMember]
            public int SubjID { get; set; }

            [DataMember]
            public decimal[] CardList { get; set; }
        }

        #endregion

        public RosneftAPI(Service config, FbConnection connection, FbTransaction transaction) : base(config, connection, transaction)
        {
        }

        public HttpStatusCode InfoCardContract(HttpWebResponse response, ParameterList parameters)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(InfoCard));
            InfoCard info = (InfoCard)serializer.ReadObject(response.GetResponseStream());
            ExecuteProcedure("InfoCardContract", parameters, info);

            return response.StatusCode;
        }

        public HttpStatusCode InfoGoodsList(HttpWebResponse response, ParameterList parameters)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(InfoGoods));
            InfoGoods info = (InfoGoods)serializer.ReadObject(response.GetResponseStream());
            foreach (Goods g in info.GoodsList)
            {
                ExecuteProcedure("InfoGoodsList", parameters, g);
            }

            return response.StatusCode;
        }

        public HttpStatusCode InfoCardOperation(HttpWebResponse response, ParameterList parameters)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Operations));
            Operations info = (Operations)serializer.ReadObject(response.GetResponseStream());
            foreach (Operation op in info.OperationList)
            {
                ExecuteProcedure("InfoCardOperation", parameters, op);
            }

            return response.StatusCode;
        }

        public HttpStatusCode InfoCardOperationDisc(HttpWebResponse response, ParameterList parameters)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OperationsDisc));
            OperationsDisc info = (OperationsDisc)serializer.ReadObject(response.GetResponseStream());
            foreach (OperationDisc op in info.OperationList)
            {
                ExecuteProcedure("InfoCardOperationDisc", parameters, op);
            }

            return response.StatusCode;
        }

        public HttpStatusCode InfoCardAccountSaldo(HttpWebResponse response, ParameterList parameters)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CardAccounts));
            CardAccounts info = (CardAccounts)serializer.ReadObject(response.GetResponseStream());
            foreach (CardAccount ca in info.CardAccountList)
            {
                ExecuteProcedure("InfoCardAccountSaldo", parameters, ca);
            }

            return response.StatusCode;
        }

        public HttpStatusCode InfoContractCardList(HttpWebResponse response, ParameterList parameters)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CardList));
            CardList info = (CardList)serializer.ReadObject(response.GetResponseStream());
            foreach (Card c in info.Cards)
            {
                ExecuteProcedure("InfoContractCardList", parameters, c);
            }

            return response.StatusCode;
        }

        public HttpStatusCode InfoContractAccountSaldo(HttpWebResponse response, ParameterList parameters)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ContractGoodsList));
            ContractGoodsList info = (ContractGoodsList)serializer.ReadObject(response.GetResponseStream());
            foreach (ContractGoods c in info.ContractGoods)
            {
                ExecuteProcedure("InfoContractAccountSaldo", parameters, c);
            }

            return response.StatusCode;
        }

        public HttpStatusCode InfoCardLimitSum(ParameterList parameters)
        {
            LimitListRequest r = new LimitListRequest(Config, parameters);
            HttpWebResponse response = GetRequest("InfoCardLimitSum", r);
            if (response == null)
            {
                return HttpStatusCode.BadRequest;
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LimitList));
            LimitList info = (LimitList)serializer.ReadObject(response.GetResponseStream());
            foreach (Limit l in info.Limit)
            {
                ExecuteProcedure("InfoCardLimitSum", parameters, l);
            }

            return response.StatusCode;
        }

        HttpWebResponse GetRequest(string method, SerializedObject obj)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{Address}/{method}");
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

        override protected string GetAddresWithParameters(string methodName, ParameterList parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Address}/{methodName}/");
            Method method = Config.Methods.Get(methodName);
            if (method != null)
            {
                foreach (ParameterMethod param in method.OfType<ParameterMethod>().Where(x => x.IsPage && x.In).OrderBy(x => x.Index))
                {
                    ParameterValue pv = parameters.Get(method.ApiName, param.Name);
                    if (pv != null)
                    {
                        sb.Append($"{pv.Value}/");
                    }
                }


                IEnumerable<ParameterMethod> p = method.OfType<ParameterMethod>().Where(x => !x.IsPage && x.In);
                string ep = GetExtendedParameteres();
                if (p.Any() || !string.IsNullOrWhiteSpace(ep))
                {
                    sb.Append('?');

                    foreach (ParameterMethod param in p)
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
    }
}
