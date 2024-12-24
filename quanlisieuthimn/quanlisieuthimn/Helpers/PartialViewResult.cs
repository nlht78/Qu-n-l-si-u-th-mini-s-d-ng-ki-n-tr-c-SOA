using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;

namespace quanlisieuthimn.Helpers
{
    public class PartialViewResult : ActionResult
    {
        public string ViewName { get; set; }
        public ViewDataDictionary ViewData { get; set; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var viewResult = new ViewResult
            {
                ViewName = ViewName,
                ViewData = ViewData,
                TempData = context.HttpContext.RequestServices.GetService(typeof(ITempDataDictionary)) as ITempDataDictionary
            };

            await viewResult.ExecuteResultAsync(context);
        }
    }
}
