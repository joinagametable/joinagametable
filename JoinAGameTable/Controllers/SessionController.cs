using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JoinAGameTable.Extensions;
using JoinAGameTable.Models;
using JoinAGameTable.Services;
using JoinAGameTable.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace JoinAGameTable.Controllers
{
    /// <summary>
    /// This controllers is in charge of the user's session.
    /// </summary>
    public class SessionController : Controller
    {
        /// <summary>
        /// Handle to the application database context.
        /// </summary>
        private readonly AppDbContext _appDbContext;

        /// <summary>
        /// Handle to the localizer.
        /// </summary>
        private readonly IStringLocalizer<SessionController> _localizer;

        /// <summary>
        /// Handle to the sendmail service.
        /// </summary>
        private readonly ISendMailService _sendMailService;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="appDbContext">Handle to the application database context</param>
        /// <param name="localizer">Handle to the localizer</param>
        /// <param name="sendMailService">Handle to the mail service</param>
        public SessionController(AppDbContext appDbContext,
                                 IStringLocalizer<SessionController> localizer,
                                 ISendMailService sendMailService)
        {
            _appDbContext = appDbContext;
            _localizer = localizer;
            _sendMailService = sendMailService;
        }

        /// <summary>
        /// Display the sign up form.
        /// </summary>
        /// <returns>An html document</returns>
        [HttpGet("/signup")]
        public IActionResult GET_ShowSignUpForm()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("SignUp", null);
        }

        [HttpPost("/signup"),
         ValidateAntiForgeryToken,
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> POST_SignUp(SignUpViewModel model)
        {
            // Check if submitted model is valid
            if (!ModelState.IsValid)
            {
                return View("SignUp", model);
            }

            // Check email is available
            var userAccountModel = await _appDbContext.UserAccount
                .Select(account => new
                {
                    account.Email
                })
                .Take(1)
                .FirstOrDefaultAsync(
                    account => account.Email.Equals(model.Email)
                );
            if (userAccountModel != null)
            {
                ModelState.AddModelError("Email", _localizer["error.email-not-available"]);
                return View("SignUp", model);
            }


            // Create user
            var newUserAccountModel = new UserAccountModel
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DisplayName = model.FirstName + " " + model.LastName,
                Password = await UserAccountModel.EncryptPasswordAsync(model.Password),
                Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow
            };

            // Save on database
            _appDbContext.Add(newUserAccountModel);
            await _appDbContext.SaveChangesAsync();

            // Send Welcome email
            await _sendMailService.SendMailAsync(
                model.Email,
                "Welcome",
                new { },
                CultureInfo.CurrentCulture
            );

            // Redirect
            HttpContext.MessageFlash("success", _localizer["flash-message.account-created-with-success"]);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Display authentication form.
        /// </summary>
        /// <param name="returnUrl">Where redirect user after authentication</param>
        /// <returns>An html document</returns>
        [HttpGet("/authentication")]
        public IActionResult GET_ShowAuthenticationForm(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("Login", new AuthenticationViewModel
            {
                ReturnUrl = returnUrl ?? "/"
            });
        }

        /// <summary>
        /// Try to open a session from the provided information.
        /// </summary>
        /// <param name="model">Bind authentication view model</param>
        /// <returns>A redirection</returns>
        [HttpPost("/authentication"),
         ValidateAntiForgeryToken,
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> POST_AuthenticateUser(AuthenticationViewModel model)
        {
            // Check if submitted model is valid
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            // Try to retrieve user from database
            var userAccount = await _appDbContext.UserAccount.FirstOrDefaultAsync(account =>
                account.Email.Equals(model.Email)
            );

            if (userAccount == null || await userAccount.CheckPasswordAsync(model.Password) == false)
            {
                HttpContext.MessageFlash("alert", "Can't open session");
                return RedirectToAction("GET_ShowAuthenticationForm", "Session");
            }

            // Create session
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Authentication, userAccount.Id.ToString()),
                new Claim(ClaimTypes.Email, userAccount.Email),
                new Claim(ClaimTypes.Name, userAccount.DisplayName)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(5),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.Now,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Update language cookie
            HttpContext.Response.Cookies.Append(
                "Language",
                $"c={userAccount.Language}|uic={userAccount.Language}",
                new CookieOptions()
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(365),
                }
            );

            // Redirect to the right page
            if (string.IsNullOrEmpty(model.ReturnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(model.ReturnUrl);
        }

        /// <summary>
        /// Close the current active session.
        /// </summary>
        /// <returns>A redirection</returns>
        [HttpGet("/logout"),
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GET_CloseSession()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
