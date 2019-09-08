using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace JoinAGameTable.Models
{
    public class UserAccountModel
    {
        /// <summary>
        /// Unique Id of the user account.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Email address of the user account owner.
        /// </summary>
        [Required,
         StringLength(125)]
        public string Email { get; set; }

        /// <summary>
        /// First name of the user account owner.
        /// </summary>
        [Required,
         StringLength(35)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user account owner.
        /// </summary>
        [Required,
         StringLength(35)]
        public string LastName { get; set; }

        /// <summary>
        /// Display name of the user account owner.
        /// </summary>
        [Required,
         StringLength(70)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Preferred language.
        /// </summary>
        [Required,
         StringLength(2, MinimumLength = 2)]
        public string Language { get; set; }

        /// <summary>
        /// User avatar.
        /// </summary>
        public virtual FileMetaDataModel Avatar { get; set; }

        /// <summary>
        /// Events owned by the user.
        /// </summary>
        public virtual IEnumerable<EventModel> Events { get; set; }

        /// <summary>
        /// Account password.
        /// </summary>
        [Required,
         StringLength(95)]
        public string Password { get; set; }

        /// <summary>
        /// When the user account has been created.
        /// </summary>
        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Encrypts the given password.
        /// </summary>
        /// <param name="password">Password to encrypt</param>
        /// <returns>Encrypted password</returns>
        public static Task<string> EncryptPasswordAsync(string password)
        {
            // Generate a 128-bit salt using a secure PRNG
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive a 256-bit subkey (use HMACSHA256 with 200,000 iterations)
            var encryptedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 200000,
                numBytesRequested: 256 / 8)
            );

            // Return encrypted password
            return Task.FromResult($"pbkdf2_sha256${Convert.ToBase64String(salt)}$200000${encryptedPassword}");
        }

        /// <summary>
        /// Check if the given password is valid.
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <returns>true if password is correct, otherwise, false</returns>
        public Task<bool> CheckPasswordAsync(string password)
        {
            // Use current password data
            var passwordData = Password.Split("$", 5);

            // Derive a 256-bit subkey (use HMACSHA256 with n iterations)
            var encryptedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(passwordData[1]),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Convert.ToInt32(passwordData[2]),
                numBytesRequested: 256 / 8)
            );

            // Return result
            return Task.FromResult(
                string.Compare(encryptedPassword, passwordData[3], StringComparison.Ordinal) == 0
            );
        }
    }
}
