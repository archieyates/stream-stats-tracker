namespace StatTracker
{
  internal class Settings
  {
    public bool AutoGenerateLookup { get; set; } = false;
    public bool UseTimeStamps { get; set; } = false;
    public string ChannelName { get; set; } = String.Empty;
    public string BotName { get; set; } = String.Empty;
    public int BotGameLimit { get; set; } = 3;
    public int BotBossLimit { get; set; } = 5;
    public int GameLevDistance { get; set; } = 6;
    public int BossLevDistance { get; set; } = 6;
    public List<string> BannedPlayers { get; set; } = new List<string>();
    public bool AutoConnectToTwitch { get; set; } = false;
    public object GetPropertyValue(string PropName) { return GetType().GetProperty(PropName)?.GetValue(this, null); }
    public void SetPropertyValue(string PropName, Object PropValue) { GetType().GetProperty(PropName)?.SetValue(this, PropValue, null); }
  }
}
