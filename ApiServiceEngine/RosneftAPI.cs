namespace ApiServiceEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using ApiServiceEngine.Configuration;
    using FirebirdSql.Data.FirebirdClient;

    class RosneftAPI : ServiceAPI
    {
        #region InfoCardContract

        [DataContract]
        [MethodData(Name = "InfoCardContract")]
        class InfoCard
        {
            [DataMember(Name = "SubjID")]
            public int ContractID { get; set; }

            [DataMember]
            public string ContractNum { get; set; }

            [DataMember]
            public string ContractDate { get; set; }
        }

        #endregion

        #region InfoGoodsList

        [DataContract]
        class Goods
        {
            [DataMember(Name = "Id")]
            public int GoodsId { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember(Name = "FName")]
            public string FullName { get; set; }
        }

        [DataContract]
        [MethodData(Name = "InfoGoodsList")]
        class InfoGoods
        {
            [DataMember]
            [ReturningValue]
            public List<Goods> GoodsList { get; set; }
        }

        #endregion

        #region InfoCardOperation

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

            [DataMember(Name = "SubjID")]
            public int ContractID { get; set; }

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
        [MethodData(Name = "InfoCardOperation")]
        class Operations
        {
            [DataMember]
            [ReturningValue]
            public List<Operation> OperationList { get; set; }
        }

        #endregion

        #region InfoCardOperationDisc

        [DataContract]
        class OperationDisc : Operation
        {
            [DataMember]
            public decimal OperSumDisc { get; set; }

            [DataMember]
            public decimal OperPriceDisc { get; set; }
        }

        [DataContract]
        [MethodData(Name = "InfoCardOperationDisc")]
        class OperationsDisc
        {
            [DataMember]
            [ReturningValue]
            public List<OperationDisc> OperationList { get; set; }
        }

        #endregion

        #region CardAccount

        [DataContract]
        class CardAccount
        {
            [DataMember]
            public decimal CardID { get; set; }

            [DataMember(Name = "SubjSaldo")]
            public decimal ContractSaldo { get; set; }

            [DataMember]
            public decimal CardSaldo { get; set; }

            [DataMember]
            public decimal OperOpSum { get; set; }
        }

        [DataContract]
        [MethodData(Name = "InfoCardAccountSaldo")]
        class CardAccounts
        {
            [DataMember]
            [ReturningValue]
            public List<CardAccount> CardAccountList { get; set; }
        }

        #endregion

        #region InfoContractCardList

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
        [MethodData(Name = "InfoContractCardList")]
        class CardList
        {
            [DataMember]
            [ReturningValue]
            public List<Card> Cards { get; set; }
        }

        #endregion

        #region InfoContractAccountSaldo

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
        [MethodData(Name = "InfoContractAccountSaldo")]
        class ContractGoodsList
        {
            [DataMember]
            [ReturningValue]
            public List<ContractGoods> ContractGoods { get; set; }
        }

        #endregion

        #region InfoCardLimitSum

        [DataContract]
        class Limit
        {
            [DataMember(Name = "ID")]
            public decimal CardID { get; set; }

            [DataMember(Name = "Err")]
            public int ErrorCode { get; set; }

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
        [MethodData(Name = "InfoCardLimitSum")]
        class LimitList
        {
            [DataMember]
            [ReturningValue]
            public List<Limit> Limit { get; set; }
        }

        #endregion

        #region InfoCardLimitOffline

        [DataContract]
        class GoodsLimitItem
        {
            [DataMember(Name = "ID")]
            public int GoodsID { get; set; }

            [DataMember(Name = "Name")]
            public string GoodsName { get; set; }

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
        [MethodData(Name = "InfoCardLimitOffline")]
        class LimitListOffline
        {
            [DataMember]
            [ReturningValue]
            public List<LimitOffline> Card { get; set; }
        }

        #endregion

        [DataContract]
        [MethodData(Name = "InfoCardLimitSum")]
        [MethodData(Name = "InfoCardLimitOffline")]
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

            [DataMember(Name = "SubjID")]
            public int? ContractID { get; set; }

            [DataMember]
            public decimal[] CardList { get; set; }
        }

        public RosneftAPI(Service config, FbConnection connection, FbTransaction transaction) : base(config, connection, transaction)
        {
        }

        public object InfoCardContract(Method method, StringDictionary parameters, object info)
        {
            ExecuteProcedure(method.Name, parameters, info);
            return info;
        }

        public object InfoGoodsList(Method method, StringDictionary parameters, object info)
        {
            List<Goods> infoGoods = (List<Goods>)info;
            foreach (Goods goods in infoGoods)
            {
                ExecuteProcedure(method.Name, parameters, goods);
            }

            return infoGoods;
        }

        public object InfoCardOperation(Method method, StringDictionary parameters, object info)
        {
            List<Operation> infoOperations = (List<Operation>)info;
            foreach (Operation op in infoOperations)
            {
                ExecuteProcedure(method.Name, parameters, op);
            }

            return infoOperations;
        }

        public object InfoCardOperationDisc(Method method, StringDictionary parameters, object info)
        {
            List<OperationDisc> infoOperations = (List<OperationDisc>)info;
            foreach (OperationDisc op in infoOperations)
            {
                ExecuteProcedure(method.Name, parameters, op);
            }

            return infoOperations;
        }

        public object InfoCardAccountSaldo(Method method, StringDictionary parameters, object info)
        {
            List<CardAccount> infoCard = (List<CardAccount>)info;
            foreach (CardAccount ca in infoCard)
            {
                ExecuteProcedure(method.Name, parameters, ca);
            }

            return infoCard;
        }

        public object InfoContractCardList(Method method, StringDictionary parameters, object info)
        {
            List<Card> infoCards = (List<Card>)info;
            foreach (Card card in infoCards)
            {
                ExecuteProcedure(method.Name, parameters, card);
            }

            return infoCards;
        }

        public object InfoContractAccountSaldo(Method method, StringDictionary parameters, object info)
        {
            List<ContractGoods> infoGoods = (List<ContractGoods>)info;
            foreach (ContractGoods c in infoGoods)
            {
                ExecuteProcedure(method.Name, parameters, c);
            }

            return infoGoods;
        }

        public object InfoCardLimitSum(Method method, StringDictionary parameters, object info)
        {
            List<Limit> infoLimit = (List<Limit>)info;
            foreach (Limit limit in infoLimit)
            {
                ExecuteProcedure(method.Name, parameters, limit);
            }

            return infoLimit;
        }

        public object InfoCardLimitOffline(Method method, StringDictionary parameters, object info)
        {
            List<LimitOffline> infoLimit = (List<LimitOffline>)info;
            foreach (LimitOffline limit in infoLimit)
            {
                foreach (GoodsLimitItem item in limit.Goods.Item)
                {
                    ExecuteProcedure(method.Name, parameters, limit, item);
                }
            }

            return infoLimit;
        }

        protected override string GetBaseUrl(Method method)
        {
            int version = method.Version == 0 ? 2 : method.Version;
            string apiName = string.IsNullOrEmpty(method.ApiName) ? method.Name : method.ApiName;
            return $"{Url}/v{version}/{apiName}/";
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
                    object obj = GetPropertyFromMethod(param.Recive.Method, param.Recive.Parameter, parameters);
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

        override protected string GetParametersUrl(Method method, StringDictionary parameters)
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (Parameter param in method.In.OfType<Parameter>().Where(x => x.IsPage).OrderBy(x => x.Index))
            {
                string pName = param.Name.ToLower();
                if (!UpdateParameters(param, parameters))
                    continue;

                sb.Append($"{parameters[pName]}/");
            }


            IEnumerable<Parameter> p = method.In.OfType<Parameter>().Where(x => !x.IsPage);
            string ep = GetExtendedParameteres();
            if (p.Any() || !string.IsNullOrWhiteSpace(ep))
            {
                sb.Append('?');

                foreach (Parameter param in p)
                {
                    string pName = param.Name.ToLower();
                    if (!UpdateParameters(param, parameters))
                        continue;

                    sb.Append($"{param.ApiName}={parameters[pName]}&");
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
