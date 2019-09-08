using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using JoinAGameTable.ViewModels.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JoinAGameTable.ViewModels
{
    public sealed class UserAccountProfileViewModel
    {
        /// <summary>
        /// Display name. This variable handle a sanitized
        /// version: value has been trimmed and titled.
        /// </summary>
        /// <see cref="DisplayName"/>
        private string _displayName;

        /// <summary>
        /// Email address. This variable handle a sanitized
        /// version: value has been trimmed and lowered.
        /// </summary>
        /// <see cref="Email"/>
        private string _email;

        /// <summary>
        /// First name. This variable handle a sanitized
        /// version: value has been trimmed and titled.
        /// </summary>
        /// <see cref="FirstName"/>
        private string _firstName;

        /// <summary>
        /// Last name. This variable handle a sanitized
        /// version: value has been trimmed and titled.
        /// </summary>
        /// <see cref="LastName"/>
        private string _lastName;

        /// <summary>
        /// Email address.
        /// </summary>
        /// <see cref="_email"/>
        [Required(ErrorMessage = "error.required"),
         StringLength(125, MinimumLength = 5, ErrorMessage = "error.length")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim().ToLower();
        }

        /// <summary>
        /// First name.
        /// </summary>
        /// <see cref="_firstName"/>
        [Required(ErrorMessage = "error.required"),
         StringLength(35, MinimumLength = 2, ErrorMessage = "error.length")]
        public string FirstName
        {
            get => _firstName;
            set => _firstName = value != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim()) : null;
        }

        /// <summary>
        /// Last name.
        /// </summary>
        /// <see cref="_lastName"/>
        [Required(ErrorMessage = "error.required"),
         StringLength(35, MinimumLength = 2, ErrorMessage = "error.length")]
        public string LastName
        {
            get => _lastName;
            set => _lastName = value != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim()) : null;
        }

        /// <summary>
        /// Display name.
        /// </summary>
        /// <see cref="_displayName"/>
        [StringLength(70, MinimumLength = 3, ErrorMessage = "error.length")]
        public string DisplayName
        {
            get => _displayName;
            set => _displayName = value?.Trim();
        }

        /// <summary>
        /// List of available languages.
        /// </summary>
        public List<SelectListItem> LanguageAvailables { get; set; }

        /// <summary>
        /// Current selected language.
        /// </summary>
        [Required(ErrorMessage = "error.required"),
         MustBePresentIn("LanguageAvailables", ErrorMessage = "error.invalid")]
        public string Language { get; set; }

        /// <summary>
        /// Avatar Url.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Avatar.
        /// </summary>
        public IFormFile Avatar { get; set; }
    }
}
