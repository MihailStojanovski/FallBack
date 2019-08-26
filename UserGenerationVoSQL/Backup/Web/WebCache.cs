using System;
using System.Web.Caching;

namespace FastReport.Web
{
    public partial class WebReport 
    {
        private void CacheAdd(string Name, Object Obj, CacheItemRemovedCallback DisposeCallBack, int CacheDelay)
        {
            this.Page.Cache.Add(Name, Obj, null,
                Cache.NoAbsoluteExpiration, new TimeSpan(0, CacheDelay, 0),
                System.Web.Caching.CacheItemPriority.Normal, DisposeCallBack);
        }

        private object CacheGet(string Name)
        {
            return this.Page.Cache[Name];
        }

        private object CacheRemove(string Name)
        {
            return this.Page.Cache.Remove(Name);
        }

        private void CacheRefresh(string Name, CacheItemRemovedCallback DisposeCallBack, int CacheDelay)
        {
            object obj = this.Page.Cache.Remove(Name);
            CacheAdd(Name, obj, DisposeCallBack, CacheDelay);
        }
	}
}