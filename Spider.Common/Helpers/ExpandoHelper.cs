namespace SiteMapManager.Helpers
{
    using Microsoft.CSharp.RuntimeBinder;
    using System.Runtime.CompilerServices;

    public static class ExpandoHelper
    {
        public static object GetDynamicMember(object obj, string memberName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, obj.GetType(),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<System.Func<CallSite, object, object>>.Create(binder);
            return callsite.Target(callsite, obj);
        }
    }
}
