using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Principal;
using System.Security.Cryptography;
using Sync.Theater.Models;

namespace Sync.Theater
{
    class UserAuth
    {
        /// <summary>
        /// The secret key for JWT
        /// </summary>
        public static string Secret = Config.LoadJson().JWTSecret;

        /// <summary>
        /// Validates user info against the info stored in the DB and returns a JWT token if successful or null if not.
        /// </summary>
        /// <param name="RawPassword"></param>
        /// <param name="Username"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
        public static string UserLogin( string RawPassword, string Username="", string Email="")
        {
            if ((string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email)) || (string.IsNullOrWhiteSpace(RawPassword))) { return null; }

            SyncUser user = DatabaseConnector.ValidateAndGetUser(RawPassword, Username, Email);

            if (user != null)
                return GenerateToken(user.Username);
            else
                return null;
        }

        /// <summary>
        /// Attempt to register user into DB. Returns false if error occurs.
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Email"></param>
        /// <param name="RawPassword"></param>
        /// <returns></returns>
        public static bool RegisterUser( string Username, string Email, string RawPassword )
        {
            if ((string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email)) || (string.IsNullOrWhiteSpace(RawPassword))) { return false; }


            // if nobody else has registered with this info then register.
            if (DatabaseConnector.ValidateAndGetUser(RawPassword, Username, Email) == null)
            {
                return DatabaseConnector.AddUserToDB(Username, Email, HashPassword(RawPassword));
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// returns a hashed version of the raw password.
        /// </summary>
        /// <param name="RawPassword"></param>
        /// <returns></returns>
        public static string HashPassword(string RawPassword)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(RawPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }


        /// <summary>
        /// Compare a raw password that a user entered to a hashed password stored in the DB
        /// returns true if valid and false if not.
        /// </summary>
        /// <param name="RawPassword"></param>
        /// <param name="PasswordHash"></param>
        /// <returns></returns>
        public static bool VerifyPassword(string RawPassword, string PasswordHash)
        {
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(PasswordHash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(RawPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Given the username, generates a JSON Web Token for authentication so username and password don't have to be constantly sent.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="expireMinutes"></param>
        /// <returns></returns>
        public static string GenerateToken(string username, int expireMinutes = 1440)
        {
            var symmetricKey = Convert.FromBase64String(Secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                        {
                        new Claim(ClaimTypes.Name, username)
                    }),

                Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }



        private static bool ValidateToken(string token, out string username)
        {
            username = null;

            var simplePrinciple = UserAuth.GetPrincipal(token);
            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim?.Value;

            if (string.IsNullOrEmpty(username))
                return false;

            // More validate to check whether username exists in system

            return true;
        }

        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            string username;

            if (ValidateToken(token, out username))
            {
                // based on username to get more information from database in order to build local identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                    // Add more claims if needed: Roles, ...
                };

                var identity = new ClaimsIdentity(claims, "Jwt");
                IPrincipal user = new ClaimsPrincipal(identity);

                return Task.FromResult(user);
            }

            return Task.FromResult<IPrincipal>(null);
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = Convert.FromBase64String(Secret);

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }

            catch (Exception)
            {
                //should write log
                return null;
            }
        }
    }
}
