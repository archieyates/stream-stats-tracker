namespace StatTracker
{
  public class Boss
  {
    public string Lookup { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public int Deaths { get; set; }

    public Boss()
    {
      Lookup = String.Empty;
      Name = String.Empty;
      Status = "Undefeated";
      Deaths = 0;
    }
  }
  public class LegacyBossContainer
  {
    public List<Boss> Bosses { get; set; } = new List<Boss>();
  }
}
