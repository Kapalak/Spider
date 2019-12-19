namespace Spider.Common.Model
{
    using System.Collections.Generic;
    using System.Dynamic;
    using Westwind.Utilities.Dynamic;

    public class Context : Expando
    {

        private readonly Dictionary<string, object> _dynamicProperties = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dynamicProperties.ContainsKey(binder.Name))
            {
                result = _dynamicProperties[binder.Name];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dynamicProperties.ContainsKey(binder.Name))
            {
                _dynamicProperties[binder.Name] = value;
            }
            else
            {
                _dynamicProperties.Add(binder.Name, value);
            }
            return true;
        }
    }
}
