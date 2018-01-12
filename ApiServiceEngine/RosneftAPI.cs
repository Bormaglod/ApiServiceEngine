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
            public decimal Pos { get; set; }
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
            [DataMember(Name = "ID")]
            public decimal CardID { get; set; }

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
        class LimitListRequest : SerializedObject
        {
            public LimitListRequest(ServiceAPI api, Service service, Method method, StringDictionary parameters) : base(api, service, method, parameters)
            {
                User = service.Settings.Login;
                Password = service.Settings.Password;
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

        [DataContract]
        class GoodsLimitItem
        {
            [DataMember(Name = "ID")]
            public int GoodsID { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Unit { get; set; }

            [DataMember]
            public string IndLimit { get; set; }

            [DataMember]
            public string LimitScheme { get; set; }

            [DataMember]
            public string LimitType { get; set; }

            [DataMember]
            public decimal Limit { get; set; }

            [DataMember]
            public decimal MaxLimit { get; set; }

            [DataMember]
            public string OilStatus { get; set; }

            [DataMember]
            public decimal OilValue { get; set; }

            [DataMember]
            public DateTime OliBegin { get; set; }

            [DataMember]
            public DateTime OliEnd { get; set; }
        }

        [DataContract]
        class GoodsLimitOffline
        {
            [DataMember]
            public List<GoodsLimitItem> Item { get; set; }
        }

        [DataContract]
        class LimitOffline
        {
            [DataMember(Name = "ID")]
            public decimal CardID { get; set; }

            [DataMember]
            public int ErrorCode { get; set; }

            [DataMember]
            public string ErrorMessage { get; set; }

            [DataMember]
            public GoodsLimitOffline Goods { get; set; }
        }

        [DataContract]
        class LimitListOffline
        {
            [DataMember]
            public List<LimitOffline> Card { get; set; }
        }

        #endregion

        public RosneftAPI(Service config, FbConnection connection, FbTransaction transaction) : base(config, connection, transaction)
        {
        }

        public (object Info, HttpStatusCode Status) InfoCardContract(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            HttpWebResponse response = GetResponse(method, parameters);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(InfoCard));
            InfoCard info = (InfoCard)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
                ExecuteProcedure(method.Name, parameters, info);

            return (info, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoGoodsList(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            HttpWebResponse response = GetResponse(method, parameters);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(InfoGoods));
            InfoGoods info = (InfoGoods)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (Goods g in info.GoodsList)
                {
                    ExecuteProcedure(method.Name, parameters, g);
                }
            }

            return (info.GoodsList, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoCardOperation(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            HttpWebResponse response = GetResponse(method, parameters);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Operations));
            Operations info = (Operations)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (Operation op in info.OperationList)
                {
                    ExecuteProcedure(method.Name, parameters, op);
                }
            }

            return (info.OperationList, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoCardOperationDisc(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            HttpWebResponse response = GetResponse(method, parameters);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OperationsDisc));
            OperationsDisc info = (OperationsDisc)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (OperationDisc op in info.OperationList)
                {
                    ExecuteProcedure(method.Name, parameters, op);
                }
            }

            return (info.OperationList, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoCardAccountSaldo(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            HttpWebResponse response = GetResponse(method, parameters);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CardAccounts));
            CardAccounts info = (CardAccounts)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (CardAccount ca in info.CardAccountList)
                {
                    ExecuteProcedure(method.Name, parameters, ca);
                }
            }

            return (info.CardAccountList, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoContractCardList(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            HttpWebResponse response = GetResponse(method, parameters);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CardList));
            CardList info = (CardList)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (Card c in info.Cards)
                {
                    ExecuteProcedure(method.Name, parameters, c);
                }
            }

            return (info.Cards, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoContractAccountSaldo(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            HttpWebResponse response = GetResponse(method, parameters);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ContractGoodsList));
            ContractGoodsList info = (ContractGoodsList)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (ContractGoods c in info.ContractGoods)
                {
                    ExecuteProcedure(method.Name, parameters, c);
                }
            }

            return (info.ContractGoods, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoCardLimitSum(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            LimitListRequest r = new LimitListRequest(this, Service, method, parameters);
            HttpWebResponse response = GetResponse(method, r);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LimitList));
            LimitList info = (LimitList)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (Limit l in info.Limit)
                {
                    ExecuteProcedure(method.Name, parameters, l);
                }
            }

            return (info.Limit, response.StatusCode);
        }

        public (object Info, HttpStatusCode Status) InfoCardLimitOffline(Method method, StringDictionary parameters, bool writeToDatabase)
        {
            LimitListRequest r = new LimitListRequest(this, Service, method, parameters);
            HttpWebResponse response = GetResponse(method, r);
            if (response == null)
                return (null, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                return (null, response.StatusCode);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LimitListOffline));
            LimitListOffline info = (LimitListOffline)serializer.ReadObject(response.GetResponseStream());
            if (writeToDatabase)
            {
                foreach (LimitOffline l in info.Card)
                {
                    foreach (GoodsLimitItem item in l.Goods.Item)
                    {
                        ExecuteProcedure(method.Name, parameters, l, item);
                    }
                }
            }

            return (info.Card, response.StatusCode);
        }

        protected override string GetBaseUrl(Method method)
        {
            int version = method.Version == 0 ? 2 : method.Version;
            string apiName = string.IsNullOrEmpty(method.ApiName) ? method.Name : method.ApiName;
            return $"{Url}/v{version}/{apiName}/";
        }

        override protected string GetParametersUrl(Method method, StringDictionary parameters)
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (Parameter param in method.In.OfType<Parameter>().Where(x => x.IsPage).OrderBy(x => x.Index))
            {
                if (!parameters.ContainsKey(param.Name))
                    continue;

                sb.Append($"{parameters[param.Name]}/");
            }


            IEnumerable<Parameter> p = method.In.OfType<Parameter>().Where(x => !x.IsPage);
            string ep = GetExtendedParameteres();
            if (p.Any() || !string.IsNullOrWhiteSpace(ep))
            {
                sb.Append('?');

                foreach (Parameter param in p)
                {
                    if (!parameters.ContainsKey(param.Name))
                        continue;

                    sb.Append($"{param.ApiName}={parameters[param.Name]}&");
                }

                if (string.IsNullOrWhiteSpace(ep))
                    sb.Remove(sb.Length - 1, 1);
                else
                    sb.Append(ep);
            }

            return sb.ToString();
        }

        string GetExtendedParameteres()
        {
            return $"u={Login}&p={Password}&type=JSON";
        }
    }
}
