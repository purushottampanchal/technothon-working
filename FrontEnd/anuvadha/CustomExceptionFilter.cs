using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;


namespace anuvadha
{
    public class CustomExceptionFilter : System.Web.Mvc.FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filtercontext)
        {
            if (filtercontext.Exception is NotImplementedException)
            {



            }
            else
            {
                filtercontext.Result = new ViewResult()
                {
                    ViewName = "Index"
                };
                filtercontext.ExceptionHandled = true;
            }
        }
    }
}
