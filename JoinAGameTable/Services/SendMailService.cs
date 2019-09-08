using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace JoinAGameTable.Services
{
    /// <summary>
    /// Render and store email on database.
    /// </summary>
    public class SendMailService : ISendMailService
    {
        /// <summary>
        /// Handle to the configuration.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Handle to the Razor view engine.
        /// </summary>
        private readonly IRazorViewEngine _razorViewEngine;

        /// <summary>
        /// Handle to the service provider.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Handle to the temporary data provider.
        /// </summary>
        private readonly ITempDataProvider _tempDataProvider;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="configuration">Handle to the current configuration</param>
        /// <param name="razorViewEngine">Handle to the Razor view engine</param>
        /// <param name="tempDataProvider">Handle to the temporary data provider</param>
        /// <param name="serviceProvider">Handle to the service provider</param>
        public SendMailService(IConfiguration configuration,
                               IRazorViewEngine razorViewEngine,
                               ITempDataProvider tempDataProvider,
                               IServiceProvider serviceProvider)
        {
            _configuration = configuration.GetSection("SendMailService");
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task SendMailAsync<TModel>(string recipient,
                                                string viewName,
                                                TModel model,
                                                CultureInfo cultureInfo)
        {
            if (!viewName.StartsWith("/"))
            {
                viewName = "/Views/Emails/" + viewName;
            }

            if (!viewName.EndsWith(".cshtml"))
            {
                viewName = viewName + ".cshtml";
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Join a Game Table", _configuration.GetValue<string>("FromEmail")));
            message.To.Add(new MailboxAddress(recipient));
            message.Subject = "Mail Title";
            message.Body = new TextPart("html")
            {
                Text = await RenderView(viewName, model, cultureInfo)
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _configuration.GetValue<string>("Host"),
                    _configuration.GetValue<int>("Port")
                );
                await client.AuthenticateAsync(
                    _configuration.GetValue<string>("Username"),
                    _configuration.GetValue<string>("Password")
                );
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Render template view.
        /// </summary>
        /// <param name="viewName">Fullpath of the View to render</param>
        /// <param name="model">View model to use</param>
        /// <param name="cultureInfo">Culture to use</param>
        /// <typeparam name="TModel">Must be a view model</typeparam>
        /// <returns></returns>
        private async Task<string> RenderView<TModel>(string viewName, TModel model, CultureInfo cultureInfo)
        {
            // Fake HTTP Context
            var newCookies = new[] {"Language=c%3Den%7Cuic%3Den%2C"};
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider,
                Request = {Headers = {["Accept-Language"] = "en", ["Cookie"] = newCookies}}
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            actionContext.HttpContext.Request.Headers["Accepted-Language"] = "en";
            actionContext.HttpContext.Request.Headers["Cookie"] = newCookies;

            // Try to retrieve view
            var getViewResult = _razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            IView view = null;
            if (getViewResult.Success)
            {
                view = getViewResult.View;
            }
            else
            {
                var findViewResult = _razorViewEngine.FindView(actionContext, viewName, isMainPage: true);
                if (findViewResult.Success)
                {
                    view = findViewResult.View;
                }
            }

            // Render view
            using (var output = new StringWriter())
            {
                // View context
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                // Render
                var currentCulture = CultureInfo.CurrentCulture;
                var currentUiCulture = CultureInfo.CurrentUICulture;
                try
                {
                    CultureInfo.CurrentCulture = cultureInfo;
                    CultureInfo.CurrentUICulture = cultureInfo;
                    await view.RenderAsync(viewContext);
                }
                finally
                {
                    CultureInfo.CurrentCulture = currentCulture;
                    CultureInfo.CurrentUICulture = currentUiCulture;
                }

                // Return view as string
                return output.ToString();
            }
        }
    }
}
