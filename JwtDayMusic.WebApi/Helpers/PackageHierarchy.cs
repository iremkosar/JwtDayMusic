namespace JwtDayMusic.WebApi.Helpers
{
    public static class PackageHierarchy
    {
        private static readonly Dictionary<string, int> Levels = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Basic", 1 },
            { "Gold", 2 },
            { "Premium", 3 },
            { "Elit", 4 },
            { "Admin", 99 }
        };

        public static bool HasAccess(string? userRole, string requiredPackage)
        {
            if (string.IsNullOrEmpty(userRole)) return false;
            if (!Levels.TryGetValue(userRole, out var userLevel)) return false;
            if (!Levels.TryGetValue(requiredPackage, out var requiredLevel)) return false;

            return userLevel >= requiredLevel;
        }

        // Kullanıcının birden fazla rolü olabilir (ör. hem Gold hem Premium).
        // Erişim kontrolünde her zaman EN YÜKSEK paketi esas alıyoruz.
        public static string? GetHighestRole(IEnumerable<string> userRoles)
        {
            string? highest = null;
            int highestLevel = 0;

            foreach (var role in userRoles)
            {
                if (Levels.TryGetValue(role, out var level) && level > highestLevel)
                {
                    highestLevel = level;
                    highest = role;
                }
            }

            return highest;
        }
    }
}