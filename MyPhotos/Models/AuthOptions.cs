using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MyPhotos.Models
{
    internal static class AuthOptions
    {
        public static readonly string ISSUER = "https://localhost:7164";
        public static readonly string AUDIENCE = "My Photos";
        private static readonly string KEY = ISSUER + "_" + AUDIENCE;

        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
