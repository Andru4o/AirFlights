using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AirFlights.Infrastructure;

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // издатель токена
    public const string AUDIENCE = "MyAuthClient"; // потребитель токена
    const string KEY = "SecretKey_091824!";   // ключ для шифрации
    public const int LIFETIME = 5; // время жизни токена - 1 минута
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
    }
}