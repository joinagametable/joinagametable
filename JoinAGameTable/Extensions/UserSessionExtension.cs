using System;
using System.Security.Claims;
using System.Security.Principal;

namespace JoinAGameTable.Extensions
{
    /// <summary>
    /// This extension adds new methods to get user information more easily.
    /// </summary>
    /// <seealso cref="IPrincipal"/>
    public static class UserSessionExtension
    {
        /// <summary>
        /// Retrieves the user unique Id.
        /// </summary>
        /// <param name="user">[IMPLICIT] Principal to use</param>
        /// <returns>The user unique Id</returns>
        public static Guid GetUniqueId(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity) user.Identity).FindFirst(ClaimTypes.Authentication);
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }

        /// <summary>
        /// Retrieves the user display name.
        /// </summary>
        /// <param name="user">[IMPLICIT] Principal to use</param>
        /// <returns>The user fullname</returns>
        public static string GetDisplayName(this IPrincipal user)
        {
            var claimGivenName = ((ClaimsIdentity) user.Identity).FindFirst(ClaimTypes.GivenName);
            var claimName = ((ClaimsIdentity) user.Identity).FindFirst(ClaimTypes.Name);
            return (claimGivenName?.Value + " " + claimName?.Value).Trim();
        }

        /// <summary>
        /// Retrieves the user email address.
        /// </summary>
        /// <param name="user">[IMPLICIT] Principal to use</param>
        /// <returns>The user email address</returns>
        public static string GetEmail(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity) user.Identity).FindFirst(ClaimTypes.Email);
            return claim?.Value;
        }
    }
}
