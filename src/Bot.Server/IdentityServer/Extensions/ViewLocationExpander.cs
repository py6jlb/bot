using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Bot.Server.IdentityServer.Extensions
{
    public class ViewLocationExpander : IViewLocationExpander 
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            context.Values["customviewlocation"] = nameof(ViewLocationExpander);
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
           var locations = new []
           {
               "/IdentityServer/Views/{1}/{0}.cshtml", 
               "/IdentityServer/Views/Shared/{0}.cshtml"
           };
           return locations.Union(viewLocations); 
        }
    }
}