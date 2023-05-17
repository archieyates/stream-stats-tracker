namespace StatTracker
{
    public class Playthrough
    {
        public string Lookup { get; set; }
        public string Game { get; set; }
        public int Deaths { get; set; }
        public int Sessions { get; set; }
        public string Status { get; set; }
        public string VOD { get; set; }
        public string Playtime { get; set; }

        public Playthrough()
        {
            Lookup = String.Empty;
            Game = String.Empty;
            Status = "Scheduled";
            Deaths = 0;
            Sessions = 0;
        }
    }
    public class PlaythroughContainer
    {
        public Playthrough[] Playthroughs { get; set; }
    }
}
