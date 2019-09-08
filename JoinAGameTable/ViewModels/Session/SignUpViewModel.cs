using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace JoinAGameTable.ViewModels
{
    public sealed class SignUpViewModel
    {
        /// <summary>
        /// Email address is used to find user on database. This variable handle a
        /// sanitized version: value has been trimmed and lowered.
        /// </summary>
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
        /// Email address is used to find user on database.
        /// </summary>
        /// <see cref="_email"/>
        [Required(ErrorMessage = "error.required"),
         StringLength(125, MinimumLength = 5, ErrorMessage = "error.length"),
         EmailAddress(ErrorMessage = "error.email")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim().ToLower();
        }

        /// <summary>
        /// Password to use against the authentication process.
        /// </summary>
        [Required(ErrorMessage = "error.required"),
         StringLength(125, MinimumLength = 5, ErrorMessage = "error.length")]
        public string Password { get; set; }

        /// <summary>
        /// If provided, redirect used to the right page after the authentication
        /// process was successfully completed.
        /// </summary>
        [StringLength(50)]
        public string ReturnUrl { get; set; }
    }
}
