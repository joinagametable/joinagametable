using System.ComponentModel.DataAnnotations;

namespace JoinAGameTable.ViewModels
{
    public sealed class AuthenticationViewModel
    {
        /// <summary>
        /// Email address is used to find user on database. This variable handle a
        /// sanitized version: value has been trimmed and lowered.
        /// </summary>
        private string _email;

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
