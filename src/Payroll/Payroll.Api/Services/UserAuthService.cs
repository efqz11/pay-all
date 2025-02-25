using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Payroll.Api.Models;
using Payroll.Models;
using Payroll.Services;

namespace Payroll.Api.Services
{
    public interface IUserAuthService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string ipAddress);
        Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress);
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task<AppUser> GetByIdAsync(string id);
        Task<AppUser> GetRefreshTokensAsync(string id);
    }

    public class UserAuthService : IUserAuthService
    {
        private Payroll.Database.PayrollDbContext context;
        private readonly Database.AccountDbContext accountDbContext;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly AppSettings _appSettings;

        public UserAuthService(
            Database.PayrollDbContext context,
            Database.AccountDbContext accountDbContext,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IOptions<AppSettings> appSettings)
        {
            this.context = context;
            this.accountDbContext = accountDbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _appSettings = appSettings.Value;
        }

        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string ipAddress)
        {
            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, lockoutOnFailure: true);
            if (!result.Succeeded)
                return null;
            var user = await userManager.FindByNameAsync(model.Username);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = await generateJwtTokenAsync(user);
            var refreshToken = generateRefreshToken(ipAddress);

            // save refresh token
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);
            context.SaveChanges();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public async Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            // return null if no user found with token
            if (user == null) return null;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            context.Update(user);
            context.SaveChanges();

            // generate new jwt
            var jwtToken = await generateJwtTokenAsync(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            context.Update(user);
            context.SaveChanges();

            return true;
        }

        public async Task<IEnumerable<AppUser>> GetAllAsync()
        {
            return await userManager.Users.ToListAsync();
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            return await userManager.FindByIdAsync(id);
        }

        public async Task<AppUser> GetRefreshTokensAsync(string id)
        {
            return await userManager.Users.Include(a=> a.RefreshTokens).FirstOrDefaultAsync(x=> x.Id == id);
        }

        // helper methods

        private async Task<string> generateJwtTokenAsync(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            

            var claimPrincial = await GetClaimsForUserAsync(user);
            var customClaims = claimPrincial.ToDictionary(a => a.Type, a => (object)a.Value);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimPrincial),
                //= new ClaimsIdentity(new Claim[]
                //{
                //    new Claim(ClaimTypes.Name, user.Id.ToString()),
                //    new Claim(CustomClaimTypes.accessgrant_companyId, "1".ToString())
                //}),
                //Claims = customClaims,
                Issuer = "www.payall.com",
                Expires = DateTime.UtcNow.AddHours(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }


        private async Task<Claim[]> GetClaimsForUserAsync(AppUser user)
        {
            var claims = new List<Claim>();
            // access grants for company access
            var userAccessGrant = await accountDbContext.AccessGrants
                .Include(x => x.CompanyAccount).FirstOrDefaultAsync(x => x.UserId == user.Id && x.IsDefault);
            if (userAccessGrant != null)
            {
                claims.AddRange(new[] {
                 new Claim(CustomClaimTypes.accessgrant_companyName, userAccessGrant.CompanyAccount.Name),
                 new Claim(CustomClaimTypes.accessgrant_companyId, userAccessGrant.CompanyAccountId.ToString()),
                       //new Claim(ClaimTypes.GivenName, user.NickName.ToString())
                   });

                claims.AddRange(new[] {
                        new Claim(CustomClaimTypes.accessgrant_companyTimeZone, userAccessGrant.CompanyAccount?.TimeZone ?? "UTC"),
                });

                //claims.AddRange(new[] {
                //        new Claim(CustomClaimTypes.accessgrant_companyLogo, !string.IsNullOrWhiteSpace(userAccessGrant.CompanyAccount.LogoUrl) ? userAccessGrant.CompanyAccount.LogoUrl : DefaultPictures.default_company),
                //       });

                if (!string.IsNullOrWhiteSpace(userAccessGrant.Roles))
                    claims.AddRange(new[] {
                        new Claim(CustomClaimTypes.accessgrant_roles, userAccessGrant.Roles),
                       });


                // add formatters
                claims.AddRange(new[] {
                        new Claim(CustomClaimTypes.formatter_date, userAccessGrant.CompanyAccount?.DateFormat ?? "dd-MMM-yyyy"),
                });
                claims.AddRange(new[] {
                        new Claim(CustomClaimTypes.formatter_time, userAccessGrant.CompanyAccount?.TimeFormat ?? "hh:mm:ss"),
                });
                claims.AddRange(new[] {
                        new Claim(CustomClaimTypes.formatter_number, userAccessGrant.CompanyAccount?.CurrencyFormat ?? "N2"),
                });
                claims.AddRange(new[] {
                        new Claim(CustomClaimTypes.formatter_name, userAccessGrant.CompanyAccount?.NameFormat ?? "{FirstName} {lastName}"),
                });
            }


            //claims.AddRange(new[] {
            //     new Claim(CustomClaimTypes.Profile, !string.IsNullOrWhiteSpace(user.Avatar) ? user.Avatar : DefaultPictures.default_user)
            //   });

            //if (!string.IsNullOrWhiteSpace(user.NickName))
            //    claims.AddRange(new[] {
            //     new Claim(CustomClaimTypes.Nickname, user.NickName),
            //});




            // add employee access grants
            // (if user access NOT NULL, then get employee belonging to current company access grant)
            var emp = await context.Employees.AsQueryable()
                    .Where(x => x.HasUserAccount && x.UserId == user.Id
                    && (userAccessGrant == null || userAccessGrant.CompanyAccountId == x.Department.CompanyId))
                    .Select(x => new
                    {
                        companyLogo = x.Department.Company.LogoUrl,
                        companyName = x.Department.Company.Name,
                        employeeId = x.Id,
                        //employeeName = x.GetSystemName(principal),
                        departmentId = x.DepartmentId,
                        departmentName = x.Department.Name
                    })
                    .FirstOrDefaultAsync();

            if (emp != null)
            {
                claims.AddRange(new[] {
                 new Claim(CustomClaimTypes.EmployeeId, emp.employeeId.ToString()),
                 //new Claim(CustomClaimTypes.EmployeeName, emp.employeeName.ToString()),
                 //new Claim(CustomClaimTypes.DepartmentId, emp.departmentId.ToString()),
                 //new Claim(CustomClaimTypes.DepartmentName, emp.departmentName.ToString()),
                 //new Claim(CustomClaimTypes.EmployeeCompanyLogo, emp.companyLogo.ToString()),
                 //new Claim(CustomClaimTypes.EmployeeCompanyName, emp.companyName.ToString()),
                });
            }

            return claims.ToArray();
        }
    }
}
