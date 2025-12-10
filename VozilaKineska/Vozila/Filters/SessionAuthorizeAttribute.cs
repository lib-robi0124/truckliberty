using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vozila.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public string? RequiredRole { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            
            // Check if user is logged in
            var userId = session.GetInt32("UserId");
            var transporterId = session.GetInt32("TransporterId");
            
            if (userId == null && transporterId == null)
            {
                // User is not logged in, redirect to login
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            // Check role if required
            if (!string.IsNullOrEmpty(RequiredRole))
            {
                var userRole = session.GetString("Role");
                if (userRole != RequiredRole)
                {
                    // User doesn't have required role, redirect or show access denied
                    context.Result = new RedirectToActionResult("Login", "Home", new { error = "Access denied" });
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
