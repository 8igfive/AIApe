using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Text;
using System;

using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;

using BNBCrypt = BCrypt.Net.BCrypt;

namespace Buaa.AIBot.Services
{
    public interface IUserService
    {
        /// <summary> 
        /// Get a token for login.
        /// </summary>
        /// <returns>an available token if login succes, or string.Empty.</returns>
        Task<string[]> AuthorizeAccountAsync(UserBody userBody);

        /// <summary> 
        /// Check if name, email and password follow the rules.
        /// </summary>
        bool CheckSignUpBody(UserBody userBody, out StatusMessageResponse result);

        bool CheckName(string name);

        bool CheckPassword(string password);

        /// <summary> 
        /// Get UserId from token in request headers.
        /// </summary>
        int GetUidFromToken(HttpRequest request);
        bool TryGetUidFromToken(HttpRequest request, out int uid);

        /// <summary> 
        /// Get UserId from parameters.
        /// </summary>
        /// <returns>uid if uid exists, or -1</returns>
        int GetUidFromParameters(HttpRequest request);

        AuthLevel GetAuthLevelFromToken(HttpRequest request);

        DateTime GetExpirationFromToken(UserBody userBody);

        Task<string> FreshTokenAsync(UserBody userBody);
    }

    public class UserService : IUserService
    {
        private readonly TokenManagement tokenManagement;

        private readonly IUserRepository userRepository;

        private static readonly Dictionary<AuthLevel, string> roleMap = new Dictionary<AuthLevel, string>
        {
            {AuthLevel.None, "Traveler"},
            {AuthLevel.User, "User"},
            {AuthLevel.Admin, "Administrator"}
        };

        private static readonly Dictionary<string, AuthLevel> stringToAuthLevel = new Dictionary<string, AuthLevel>
        {
            {"Traveler", AuthLevel.None},
            {"User", AuthLevel.User},
            {"Administrator", AuthLevel.Admin}
        };

        public UserService(IOptions<TokenManagement> tokenManagement, IUserRepository userRepository)
        {
            this.tokenManagement = tokenManagement.Value;
            this.userRepository = userRepository;
        }

        private async Task<UserInfo> AuthenticateAccountAsync(UserBody userbody)
        {
            UserInfo userInfo = await userRepository.SelectUserByEmailAsync(userbody.Email);
            if (userInfo != null)
            {
                if (BNBCrypt.Verify(userbody.Password, userInfo.Bcrypt))
                {
                    return userInfo;
                }
            }
            return null;
        }
        
        public async Task<string[]> AuthorizeAccountAsync(UserBody userBody)
        {
            string token = string.Empty;
            UserInfo userInfo = await AuthenticateAccountAsync(userBody);
            if(userInfo == null)
            {
                return new string[]
                {
                    token,
                    ""
                };
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Sid, userInfo.UserId.ToString()),
                new Claim(ClaimTypes.Email, userInfo.Email),
                new Claim(ClaimTypes.Role, roleMap[userInfo.Auth]),
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddMinutes(tokenManagement.AccessExpiration).ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(tokenManagement.Issuer, tokenManagement.Audience, claims, 
                expires: DateTime.Now.AddMinutes(tokenManagement.AccessExpiration), 
                signingCredentials: credentials);
            token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return new string[]
            {
                token,
                roleMap[userInfo.Auth]
            };
        }

        private static readonly Regex reName = new Regex(@"^\w+$");
        private static readonly Regex reEmail = new Regex(@"^([ -\?A-~]+)@([ -\?A-~]+)$");
        private static readonly Regex rePassword = new Regex(@"^[a-zA-Z0-9@\$!%\*#_~\?&\^]+$");

        public bool CheckName(string name)
        {
            Match nameMatch = reName.Match(name);
            if (nameMatch.Success)
            {
                if (name.Length <= 2 || name.Length >= 18)
                {
                    return false;
                }
            } else {
                return false;
            }
            return true;
        }

        public bool CheckPassword(string password)
        {
            Match passwordMatch = rePassword.Match(password);
            if (passwordMatch.Success)
            {
                if (password.Length < 8 || password.Length > 16)
                {
                    return false;
                }
            } else {
                return false;
            }
            return true;
        }

        public bool CheckSignUpBody(UserBody userBody, out StatusMessageResponse result)
        {
            Match nameMatch = reName.Match(userBody.Name);
            Match emailMatch = reEmail.Match(userBody.Email);
            Match passwordMatch = rePassword.Match(userBody.Password);
            if (nameMatch.Success)
            {
                if (userBody.Name.Length <= 2 || userBody.Name.Length >= 18)
                {
                    result = new StatusMessageResponse
                    {
                        Status = "nameInvalid",
                        Message = "length of name is not within 2 and 18"
                    };
                    return false;
                }
            } else {
                result = new StatusMessageResponse
                {
                    Status = "nameInvalid",
                    Message = "include illegal characters"
                };
                return false;
            }
            if (emailMatch.Success)
            {
                if (userBody.Email.Length > 254 || emailMatch.Groups[1].Length > 64)
                {
                    result = new StatusMessageResponse
                    {
                        Status = "emailInvalid",
                        Message = "email or username of email is too long"
                    };
                    return false;
                }
            } else {
                result = new StatusMessageResponse
                {
                    Status = "emailInvalid",
                    Message = "include illegal characters"
                };
                return false;
            }
            if (passwordMatch.Success)
            {
                if (userBody.Password.Length < 8 || userBody.Password.Length > 16)
                {
                    result = new StatusMessageResponse
                    {
                        Status = "passwordInvalid",
                        Message = "length of password is not within 8 and 16"
                    };
                    return false;
                }
            } else {
                result = new StatusMessageResponse
                {
                    Status = "passwordInvalid",
                    Message = "include illegal characters"
                };
                return false;
            }
            result = null;
            return true;
        }

        public bool TryGetUidFromToken(HttpRequest request, out int uid)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            string token_content = request.Headers["Authorization"].ToString();
            if (token_content.Length <= 7)
            {
                uid = default;
                return false;
            }
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(token_content.Substring(7));
            if (jwtToken.Payload.TryGetValue(ClaimTypes.Sid, out Object ret))
            {
                uid = int.Parse(ret.ToString());
                return true;
            }
            uid = default;
            return false;
        }

        public int GetUidFromToken(HttpRequest request)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(request.Headers["Authorization"].ToString().Substring(7));
            Object uid;
            jwtToken.Payload.TryGetValue(ClaimTypes.Sid, out uid);
            return int.Parse(uid.ToString());
        }

        public int GetUidFromParameters(HttpRequest request)
        {
            int uid;
            try
            {
                uid = int.Parse(request.Query["uid"]);
            } catch (ArgumentNullException) {
                uid = -1;
            }
            return uid;
        }

        public AuthLevel GetAuthLevelFromToken(HttpRequest request)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(request.Headers["Authorization"].ToString().Substring(7));
            Object authLevel;
            jwtToken.Payload.TryGetValue(ClaimTypes.Role, out authLevel);
            return stringToAuthLevel[authLevel.ToString()];
        }
        
        public DateTime GetExpirationFromToken(UserBody userBody)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(userBody.Token);
            Object expiration;
            jwtToken.Payload.TryGetValue(ClaimTypes.Expiration, out expiration);
            return DateTime.Parse(expiration.ToString());
        }

        public async Task<string> FreshTokenAsync(UserBody userBody)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(userBody.Token);
            Object uid;
            Object email;
            jwtToken.Payload.TryGetValue(ClaimTypes.Sid, out uid);
            jwtToken.Payload.TryGetValue(ClaimTypes.Email, out email);
            UserInfo userInfo = await userRepository.SelectUserByIdAsync(int.Parse(uid.ToString()));
            if (userInfo == null)
            {
                throw new UserNotExistException(int.Parse(uid.ToString()));
            }
            AuthLevel auth = userInfo.Auth;
            var claims = new[]
            {
                new Claim(ClaimTypes.Sid, uid.ToString()),
                new Claim(ClaimTypes.Email, email.ToString()),
                new Claim(ClaimTypes.Role, roleMap[auth]),
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddMinutes(tokenManagement.AccessExpiration).ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            jwtToken = new JwtSecurityToken(tokenManagement.Issuer, tokenManagement.Audience, claims, 
                expires: DateTime.Now.AddMinutes(tokenManagement.AccessExpiration), 
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }

    public static class ConfigureUserServices
    {
        public static IServiceCollection AddUserServices(this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.Configure<TokenManagement>(configuration.GetSection("tokenConfig"));
            var token = configuration.GetSection("tokenConfig").Get<TokenManagement>();
            
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.
                    GetBytes(token.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = token.Issuer,
                    ValidateAudience = true,
                    ValidAudience = token.Audience,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };
            });

            services.AddAuthorization(option =>
            {
                option.AddPolicy("UserAdmin", policy => policy.RequireRole("User", "Administrator").Build());
                option.AddPolicy("Admin", policy => policy.RequireRole("Administrator").Build());
            });

            services.AddTransient<IUserService, UserService>();
            return services;
        }
    }
}