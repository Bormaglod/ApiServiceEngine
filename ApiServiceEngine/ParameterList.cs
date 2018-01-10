namespace ApiServiceEngine
{
    using System.Collections.Generic;
    using System.Linq;
    using ApiServiceEngine.Configuration;

    class ParameterList
    {
        List<ParameterValue> list;

        public ParameterList()
        {
            list = new List<ParameterValue>();
        }

        public IEnumerable<ParameterValue> Get()
        {
            return list.Where(x => x.IsActive);
        }

        public ParameterValue Add(Parameter item)
        {
            ParameterValue pv = new ParameterValue(item);
            list.Add(pv);
            return pv;
        }

        public ParameterValue Get(string name)
        {
            return list.FirstOrDefault(x => x.Name == name);
        }

        public ParameterValue Get(string method, string api_param)
        {
            IEnumerable<ParameterValue> p = list.Where(x => x.IsActive && x.ApiName == api_param);
            ParameterValue v = p.FirstOrDefault(x => x.Method == method);
            if (v == null)
                v = p.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Method));

            return v;
        }
    }
}
