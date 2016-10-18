using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crunch.Attributes
{
    public class NotAvailableInProductionFilter : IActionFilter, IFilterContainer {
        public IHostingEnvironment _hostingEnvironment { get; set; }

        IFilterMetadata IFilterContainer.FilterDefinition { get; set; }

        public NotAvailableInProductionFilter(IHostingEnvironment hostingEnvironment) {
            _hostingEnvironment = hostingEnvironment;
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            if(_hostingEnvironment.IsProduction()) {
                context.Result = new Microsoft.AspNetCore.Mvc.NotFoundResult();
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext context) {}
    }

    public class NotAvailableInProductionAttribute : TypeFilterAttribute
    {
        public NotAvailableInProductionAttribute() 
        : base(typeof(NotAvailableInProductionFilter))
        {
        }
    }
}