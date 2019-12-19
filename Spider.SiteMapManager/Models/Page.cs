namespace SiteMapManager.Models
{
    using Spider.Common.Model;
    using System.Dynamic;
    using System.Linq;
    using Westwind.Utilities.Dynamic;

    public class Page : Expando
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public Selector[] Selectors { get; set; }

        #region Dynamic Properties

        //private readonly Dictionary<string, object> _dynamicProperties;

        //public Page(Dictionary<string, object> properties)
        //{
        //    _properties = properties;
        //}

        //public override IEnumerable<string> GetDynamicMemberNames()
        //{
        //    return _properties.Keys;
        //}

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var selector = Selectors.FirstOrDefault(s => s.Key == binder.Name);
            result = selector;
            return true;
        }

        //public override bool TrySetMember(SetMemberBinder binder, object value)
        //{
        //    if (_properties.ContainsKey(binder.Name))
        //    {
        //        //_properties[binder.Name] = value;
        //        //return true;
        //    }
        //    //else
        //    //{
        //    //    return false;
        //    //}
        //}

        #endregion

    }
}
