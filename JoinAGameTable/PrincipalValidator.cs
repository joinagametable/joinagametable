using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JoinAGameTable.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JoinAGameTable
{
    public static class PrincipalValidator
    {
        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var userId = context
                .Principal
                .Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.Authentication)
                ?.Value;

            if (userId == null)
            {
                context.RejectPrincipal();
            }
            else
            {
                using (var appDbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>())
                {
                    var user = await appDbContext.UserAccount
                        .AsNoTracking()
                        .IgnoreQueryFilters()
                        .Select(account => new
                        {
                            account.Id
                        })
                        .FirstOrDefaultAsync(account => account.Id == Guid.Parse(userId));
                    if (user == null)
                    {
                        context.RejectPrincipal();
                    }
                }
            }
        }
    }
}
