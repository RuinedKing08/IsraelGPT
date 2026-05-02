public static class PlayerSession
{
    public static string PlayerName { get; private set; } = string.Empty;
    public static int InitialHealth { get; private set; } = 20;
    public static int InitialDamage { get; private set; } = 4;
    public static int InitialBravery { get; private set; }
    public static bool HasCustomStats { get; private set; }

    public static void SetPlayerName(string name)
    {
        PlayerName = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
    }

    public static void SetPlayerStats(int health, int damage, int bravery)
    {
        InitialHealth = health < 1 ? 1 : health;
        InitialDamage = damage < 1 ? 1 : damage;
        InitialBravery = bravery;
        HasCustomStats = true;
    }

    public static void ClearCustomStats()
    {
        InitialHealth = 20;
        InitialDamage = 4;
        InitialBravery = 0;
        HasCustomStats = false;
    }
}
