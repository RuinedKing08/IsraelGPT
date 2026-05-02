public static class PlayerSession
{
    public static string PlayerName { get; private set; } = string.Empty;

    public static void SetPlayerName(string name)
    {
        PlayerName = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
    }
}
