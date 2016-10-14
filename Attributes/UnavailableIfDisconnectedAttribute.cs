using System;
using Crunch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crunch.Attributes
{
    public class UnavailableIfDisconnectedFilter : IActionFilter, IFilterContainer {
        public IDBConnector DatabaseConnector { get; set; }

        IFilterMetadata IFilterContainer.FilterDefinition { get; set; }

        public UnavailableIfDisconnectedFilter(IDBConnector databaseConnector) {
            Console.Out.WriteLine(string.Format("Has Object: {0}", databaseConnector.IsConnected));
            DatabaseConnector = databaseConnector;
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            if(!DatabaseConnector.IsConnected) {
                context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(503);
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext context) {}
    }

    public class UnavailableIfDisconnectedAttribute : TypeFilterAttribute
    {
        public UnavailableIfDisconnectedAttribute() 
        : base(typeof(UnavailableIfDisconnectedFilter))
        {
        }
    }
}