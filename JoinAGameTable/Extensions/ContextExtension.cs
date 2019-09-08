using JoinAGameTable.Enumerations;
using JoinAGameTable.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace JoinAGameTable.Extensions
{
    /// <summary>
    /// This extension adds new methods to the current context.
    /// </summary>
    /// <seealso cref="HttpContext"/>
    public static class ContextExtension
    {
        /// <summary>
        /// Options of the Cookie use to store flash message.
        /// </summary>
        private static readonly CookieOptions FlashMessageCookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict
        };

        /// <summary>
        /// Add new flash message.
        /// </summary>
        /// <param name="httpContext">Current http context</param>
        /// <param name="severity">Message severity</param>
        /// <param name="message">Message content</param>
        public static void MessageFlash(this HttpContext httpContext,
                                        string severity,
                                        string message)
        {
            httpContext.Response.Cookies.Append(
                "FlashMessage",
                JsonConvert.SerializeObject(
                    new MessageFlashViewModel
                    {
                        Severity = severity,
                        Message = message
                    }
                ),
                FlashMessageCookieOptions
            );
        }

        /// <summary>
        /// Retrieve flash message(s).
        /// </summary>
        /// <param name="httpContext">Current http context</param>
        /// <returns>Flash message(s) or null</returns>
        public static MessageFlashViewModel MessageFlash(this HttpContext httpContext)
        {
            httpContext.Request.Cookies.TryGetValue("FlashMessage", out var flashMessage);
            if (flashMessage != null)
            {
                httpContext.Response.Cookies.Delete("FlashMessage");
                return JsonConvert.DeserializeObject<MessageFlashViewModel>(flashMessage);
            }

            return null;
        }

        public static string ToTo(this GameClassificationContentEnum g)
        {
            return null;
        }
    }
}
