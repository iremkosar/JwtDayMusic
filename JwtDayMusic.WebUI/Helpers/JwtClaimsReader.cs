using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JwtDayMusic.WebUI.Helpers
{
    // Session'daki JWT token'ı (imza doğrulamadan, sadece içeriğini okumak için)
    // çözümleyip görüntüleme amaçlı isim ve rol bilgisi çıkarır.
    public static class JwtClaimsReader
    {
        private static readonly Dictionary<string, int> RoleLevels = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Basic", 1 },
            { "Gold", 2 },
            { "Premium", 3 },
            { "Elit", 4 },
            { "Admin", 99 }
        };

        public static (string Name, List<string> Roles) Read(string? rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                return ("Misafir", new List<string>());

            var token = rawToken.Trim().Replace("\"", "");

            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                    return ("Misafir", new List<string>());

                var jwt = handler.ReadJwtToken(token);

                var name = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                           ?? jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                           ?? "Kullanıcı";

                var roles = jwt.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();

                return (name, roles);
            }
            catch
            {
                return ("Kullanıcı", new List<string>());
            }
        }

        // Kullanıcının birden fazla rolü olabilir; görüntülemede en yüksek olanı gösteriyoruz.
        public static string GetHighestRole(List<string> roles)
        {
            if (roles == null || roles.Count == 0) return "Free";

            string highest = "Free";
            int highestLevel = 0;

            foreach (var role in roles)
            {
                if (RoleLevels.TryGetValue(role, out var level) && level > highestLevel)
                {
                    highestLevel = level;
                    highest = role;
                }
            }

            return highest;
        }

        public static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";

            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }
    }
}
