using System.Web.Mvc;

namespace Pronto.Controllers
{
    public class HandleErrorForAjaxRequestAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 400;
                filterContext.Result = new ContentResult
                {
                    Content = filterContext.Exception.Message,
                    ContentType = "text/plain"
                };
            }
        }
    }
}
