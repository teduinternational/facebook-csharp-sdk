namespace Facebook.Web.Mvc
{
    using System.Web.Mvc;

    public class SubscriptionReceivedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.ContentType = "text/plain";
            var request = filterContext.HttpContext.Request;
            var modelState = filterContext.Controller.ViewData.ModelState;
            var appSecret = FacebookContext.Current.AppSecret;

            filterContext.ActionParameters["subscription"] = null;

            string errorMessage;
            if (request.HttpMethod == "POST")
            {
                if (string.IsNullOrEmpty(appSecret))
                {
                    errorMessage = "FacebookContext.Current.AppSecret is null or empty.";
                }
                else
                {
                    var reader = new System.IO.StreamReader(request.InputStream);
                    var jsonString = reader.ReadToEnd();

                    if (FacebookWebUtils.VerifyPostSubscription(request, appSecret, jsonString, out errorMessage))
                    {
                        var jsonObject = JsonSerializer.DeserializeObject(jsonString);
                        filterContext.ActionParameters["subscription"] = jsonObject;

                        return;
                    }
                }
            }
            else
            {
                errorMessage = "Invalid http method.";
            }

            modelState.AddModelError("facebook-c#-sdk", errorMessage);

            filterContext.HttpContext.Response.StatusCode = 401;
        }
    }
}