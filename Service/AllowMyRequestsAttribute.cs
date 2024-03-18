using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LapoLoanWebApi.Service
{
   
    public class AllowMyRequestsAttribute : ControllerAttribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // check origin
            var origin = context.HttpContext.Request.Headers["origin"].FirstOrDefault();
            if (origin == LapoLoanAllowSpecificOrigins.OriginNamme1)
            {
                context.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                context.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                context.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                context.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "*");
                // Add whatever CORS Headers you need.
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // empty
        }
    }
}
