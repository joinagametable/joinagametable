using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JoinAGameTable.Extensions;
using JoinAGameTable.Models;
using JoinAGameTable.Services;
using JoinAGameTable.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace JoinAGameTable.Controllers
{
    /// <summary>
    /// This controller handle user account related methods.
    /// </summary>
    [Authorize]
    public class UserAccountController : Controller
    {
        /// <summary>
        /// Handle to the application database context.
        /// </summary>
        private readonly AppDbContext _appDbContext;

        /// <summary>
        /// Handle to the file storage service.
        /// </summary>
        private readonly IFileStorageService _fileStorageService;

        /// <summary>
        /// Handle to image manipulation service.
        /// </summary>
        private readonly IImageManipulationService _imageManipulationService;

        /// <summary>
        /// Handle to the localizer.
        /// </summary>
        private readonly IStringLocalizer<SessionController> _localizer;

        /// <summary>
        /// Handle localization options.
        /// </summary>
        private readonly RequestLocalizationOptions _requestLocalizationOptions;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        /// <param name="requestLocalizationOptions">Handle to the localization options</param>
        /// <param name="localizer">Handle to the localizer</param>
        /// <param name="appDbContext">Handle to the application database context</param>
        /// <param name="fileStorageService">Handle to the file storage service</param>
        /// <param name="imageManipulationService">Handle to the image manipulation service</param>
        public UserAccountController(IOptions<RequestLocalizationOptions> requestLocalizationOptions,
                                     IStringLocalizer<SessionController> localizer,
                                     AppDbContext appDbContext,
                                     IFileStorageService fileStorageService,
                                     IImageManipulationService imageManipulationService)
        {
            _appDbContext = appDbContext;
            _fileStorageService = fileStorageService;
            _requestLocalizationOptions = requestLocalizationOptions.Value;
            _localizer = localizer;
            _imageManipulationService = imageManipulationService;
        }

        /// <summary>
        /// Display authentication form.
        /// </summary>
        /// <returns>An html document</returns>
        [HttpGet("/account/profile")]
        public async Task<IActionResult> GET_ShowUserProfile()
        {
            // Retrieves user profile from database
            var userAccountModel = await _appDbContext.UserAccount
                .AsNoTracking()
                .Include(account => account.Avatar)
                .FirstOrDefaultAsync(account =>
                    account.Id.Equals(User.GetUniqueId())
                );

            // Build view model
            var userAccountViewModel = new UserAccountProfileViewModel
            {
                FirstName = userAccountModel.FirstName,
                LastName = userAccountModel.LastName,
                Email = userAccountModel.Email,
                DisplayName = userAccountModel.DisplayName,
                Language = userAccountModel.Language,
                LanguageAvailables = _requestLocalizationOptions.SupportedCultures.Select(culture => new SelectListItem
                {
                    Value = culture.TwoLetterISOLanguageName.ToLowerInvariant(),
                    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.NativeName)
                }).ToList(),
                AvatarUrl = userAccountModel.Avatar != null
                    ? _fileStorageService.GenerateFilePublicUrl(
                        userAccountModel.Avatar.Directory + "/" + userAccountModel.Avatar.Id)
                    : "/img/no-avatar.png"
            };

            // Show view
            return View("UserProfile", userAccountViewModel);
        }

        [HttpPost("/account/profile"),
         ValidateAntiForgeryToken,
         ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> POST_UpdateUserProfile(UserAccountProfileViewModel model)
        {
            // Retrieves user profile from database
            var userAccountModel = await _appDbContext.UserAccount
                .Include(account => account.Avatar)
                .FirstOrDefaultAsync(account =>
                    account.Id.Equals(User.GetUniqueId())
                );

            // Set missing data
            model.LanguageAvailables = _requestLocalizationOptions.SupportedCultures.Select(culture =>
                new SelectListItem
                {
                    Value = culture.TwoLetterISOLanguageName.ToLowerInvariant(),
                    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.NativeName)
                }
            ).ToList();
            model.AvatarUrl = userAccountModel.Avatar != null
                ? _fileStorageService.GenerateFilePublicUrl(
                    userAccountModel.Avatar.Directory + "/" + userAccountModel.Avatar.Id)
                : "/img/no-avatar.png";

            // Revalidate model
            TryValidateModel(model);

            // If email changed, check if available
            if (!userAccountModel.Email.Equals(model.Email))
            {
                var emailIsAlreadyTaken = await _appDbContext.UserAccount
                    .Select(account => new
                    {
                        account.Email
                    })
                    .Take(1)
                    .AnyAsync(
                        account => account.Email.Equals(model.Email)
                    );
                if (emailIsAlreadyTaken)
                {
                    ModelState.AddModelError("Email", _localizer["error.email-not-available"]);
                }
            }

            // Check if submitted model is valid
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Update avatar
            if (model.Avatar != null)
            {
                // Resize uploaded picture
                using (var resizedAvatar = new MemoryStream())
                {
                    using (var uploadedAvatar = model.Avatar.OpenReadStream())
                    {
                        var image = Image.Load(uploadedAvatar);
                        await _imageManipulationService.ResizeImageAsync(image, new Size(256, 256));
                        await _imageManipulationService.SaveAsPngAsync(image, resizedAvatar);
                    }

                    // Remove old avatar if needed
                    if (userAccountModel.Avatar != null)
                    {
                        await _fileStorageService.DeleteFileAsync(
                            userAccountModel.Avatar.Directory,
                            userAccountModel.Avatar.Id.ToString()
                        );

                        _appDbContext.Remove(userAccountModel.Avatar);
                        userAccountModel.Avatar = null;
                    }

                    // Upload new avatar
                    var uid = Guid.NewGuid();
                    var fileMetaData = new FileMetaDataModel()
                    {
                        Id = uid,
                        Bucket = _fileStorageService.BucketName,
                        ContentType = model.Avatar.ContentType,
                        Directory = "user",
                        FileName = uid.ToString()
                    };
                    await _fileStorageService.StoreFileAsync(
                        fileMetaData.Directory + "/" + fileMetaData.Id,
                        "image/png",
                        resizedAvatar.Length,
                        resizedAvatar
                    );

                    userAccountModel.Avatar = fileMetaData;
                }
            }

            // Update profile
            userAccountModel.Email = model.Email;
            userAccountModel.FirstName = model.FirstName;
            userAccountModel.LastName = model.LastName;
            userAccountModel.DisplayName = model.DisplayName ?? model.FirstName + " " + model.LastName;
            userAccountModel.Language = model.Language;
            _appDbContext.Update(userAccountModel);

            // Commit changes
            await _appDbContext.SaveChangesAsync();

            // Update language
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(userAccountModel.Language);
            HttpContext.Response.Cookies.Append(
                "Language",
                $"c={userAccountModel.Language}|uic={userAccountModel.Language}",
                new CookieOptions()
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(365),
                }
            );

            // Retrieve identity
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                // Can't retrieve identity, force logout
                return RedirectToAction("GET_CloseSession", "Session");
            }

            // Refresh claims
            identity.RemoveClaim(identity.FindFirst(ClaimTypes.Email));
            identity.RemoveClaim(identity.FindFirst(ClaimTypes.Name));
            identity.AddClaim(new Claim(ClaimTypes.Email, userAccountModel.Email));
            identity.AddClaim(new Claim(ClaimTypes.Name, userAccountModel.DisplayName));

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(5),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.Now,
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties
            );

            // Success
            HttpContext.MessageFlash("success", _localizer.GetString("user-account.profile.flash-success"));

            // Redirect
            return RedirectToAction("GET_ShowUserProfile", "UserAccount");
        }
    }
}
