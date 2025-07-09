using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace StatTracker
{
  class TwitchBot
  {
    // Credit to Bradley Saunders and his article (https://medium.com/swlh/writing-a-twitch-bot-from-scratch-in-c-f59d9fed10f3) 
    // for providing most of the initial setup here
    const string ip = "irc.chat.twitch.tv";
    const int port = 6667;

    // Core stuff
    private string ChannelName;
    private string BotName;
    private string OAuth;
    private StreamReader StreamReader;
    private StreamWriter StreamWriter;

    // Only 1 thread allowed at a time
    private static Thread TwitchThread;

    public static void InitTwitchListener()
    {
      if (TwitchThread != null && TwitchThread.IsAlive)
      {
        Program.WriteLine(ConsoleColor.Red, "Twitch Bot already running. Only 1 concurrent bot is currently supported");
        return;
      }

      // Thread for managing the Twitch integration
      TwitchThread = new Thread(RunTwitchChatbot);
      TwitchThread.Start();
    }

    private static void RunTwitchChatbot()
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      TwitchBot bot = new TwitchBot();
      bot.Start();
    }

    public void Start()
    {
      Program.WriteLine(ConsoleColor.Green, "Twitch Bot Running");

      ChannelName = Program.Settings.ChannelName;
      BotName = Program.Settings.BotName;
      OAuth = Environment.GetEnvironmentVariable("TWITCH_BOT_OAUTH", EnvironmentVariableTarget.User);

      // Set up the connection to Twitch
      var tcpClient = new TcpClient();
      tcpClient.Connect(ip, port);
      StreamReader = new StreamReader(tcpClient.GetStream());
      StreamWriter = new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };

      // Connect to channel
      StreamWriter.WriteLine($"PASS {OAuth}");
      StreamWriter.WriteLine($"NICK {BotName}");
      StreamWriter.WriteLine($"JOIN #{ChannelName}");
      SendMessage("Stat Tracker Connected");

      while (true)
      {
        string line = StreamReader.ReadLine();

        string[] split = line.Split(" ");
        //PING :tmi.twitch.tv
        //Respond with PONG :tmi.twitch.tv
        if (line.StartsWith("PING"))
        {
          StreamWriter.WriteLine($"PONG {split[1]}");
        }

        if (split.Length > 1 && split[1] == "PRIVMSG")
        {
          //:mytwitchchannel!mytwitchchannel@mytwitchchannel.tmi.twitch.tv 
          // ^^^^^^^^
          //Grab this name here
          int exclamationPointPosition = split[0].IndexOf("!");
          string username = split[0].Substring(1, exclamationPointPosition - 1);
          //Skip the first character, the first colon, then find the next colon
          int secondColonPosition = line.IndexOf(':', 1);//the 1 here is what skips the first character
          string message = line.Substring(secondColonPosition + 1);//Everything past the second colon

          // The bot and any specified banned players cannot use commands
          if (message.StartsWith("!deaths") && username != BotName && !Program.Settings.BannedPlayers.Contains(username.ToLower()))
          {
            //ParseDeathsCommand(username, message);
            Parse(username, message);
          }
        }
      }
    }

    private void Parse(string Username, string Message)
    {
      string[] args = Message.Split(" ");
      string search = String.Empty;
      foreach (string arg in args)
      {
        if (arg == "!deaths")
        {
          continue;
        }
        else
        {
          search = search + arg;
        }
      }

      if (search == "top")
      {
        int topDeaths = 0;
        string topPlaythrough = String.Empty;
        int topBossDeaths = 0;
        string topBoss = String.Empty;
        string topBossPlaythrough = String.Empty;
        foreach (Playthrough playthrough in Program.StatsManager.Playthroughs)
        {
          if (playthrough.Deaths > topDeaths)
          {
            topDeaths = playthrough.Deaths;
            topPlaythrough = playthrough.Game;
          }

          foreach (Boss boss in playthrough.Bosses)
          {
            if (boss.Deaths > topBossDeaths)
            {
              topBoss = boss.Name;
              topBossDeaths = boss.Deaths;
              topBossPlaythrough = playthrough.Game;
            }
          }
        }

        SendMessage($"{Username} The highest death count for a game was {topPlaythrough} with {topDeaths} deaths and the highest death count for a boss was {topBoss} in {topBossPlaythrough} with {topBossDeaths} deaths");

      }
      else if (search != String.Empty)
      {
        Dictionary<Playthrough, int> potentialPlaythroughs = new Dictionary<Playthrough, int>();
        Dictionary<Tuple<Boss, string>, int> potentialBosses = new Dictionary<Tuple<Boss, string>, int>();

        // simplify the search command to remove spaces and special characters
        string concat = Regex.Replace(search, "[^0-9a-zA-Z]+", "").ToLower();

        // Go through every playthrough and boss
        foreach (Playthrough playthrough in Program.StatsManager.Playthroughs)
        {
          string concatLookup = Regex.Replace(playthrough.Lookup, "[^0-9a-zA-Z]+", "").ToLower();
          string concatGame = Regex.Replace(playthrough.Game, "[^0-9a-zA-Z]+", "").ToLower();

          // Check if there is an exact match or the game at least wholly contains the search string 
          // before performing the Levenshtein distance
          if (concatLookup == concat || concatGame == concat)
          {
            potentialPlaythroughs.Add(playthrough, 0);
          }
          else if (concatLookup.Contains(concat) || concatGame.Contains(concat))
          {
            potentialPlaythroughs.Add(playthrough, 1);
          }
          else
          {
            // Levenshtein Distance algorithm
            int levDistance = LevenshteinDistance.Compute(concatGame, concat) + 1;
            int lookupLevDistance = LevenshteinDistance.Compute(concatLookup, concat) + 1;

            // Threshold for accepting an entry
            if (levDistance < Program.Settings.GameLevDistance || lookupLevDistance < Program.Settings.GameLevDistance)
            {
              potentialPlaythroughs.Add(playthrough, Math.Min(levDistance, lookupLevDistance));
            }
          }

          // Now find all the bosses
          foreach (Boss boss in playthrough.Bosses)
          {
            concatLookup = Regex.Replace(boss.Lookup, "[^0-9a-zA-Z]+", "").ToLower();
            string concatBoss = Regex.Replace(boss.Name, "[^0-9a-zA-Z]+", "").ToLower();

            // Check if there is an exact match or the boss at least wholly contains the search string 
            // before performing the Levenshtein distance
            if (concatLookup == concat || concatBoss == concat)
            {
              potentialBosses.Add(Tuple.Create(boss, playthrough.Game), 0);
            }
            else if (concatLookup.Contains(concat) || concatBoss.Contains(concat))
            {
              potentialBosses.Add(Tuple.Create(boss, playthrough.Game), 1);
            }
            else
            {
              // Levenshtein Distance algorithm
              int levDistance = LevenshteinDistance.Compute(concatBoss, concat) + 1;
              int lookupLevDistance = LevenshteinDistance.Compute(concatLookup, concat) + 1;

              // Threshold for accepting an entry
              if (levDistance < Program.Settings.BossLevDistance || lookupLevDistance < Program.Settings.BossLevDistance)
              {
                potentialBosses.Add(Tuple.Create(boss, playthrough.Game), Math.Min(levDistance, lookupLevDistance));
              }
            }
          }
        }

        // Twitch has a character limit of 500 so we may need to send multiple messages
        List<string> messageList = new List<string>();
        // Change the match type message based on accuracy
        List<string> matchTypes = new List<string>() { "match", "close match", "partial match" };

        // Prioritise Games over bosses
        if (potentialPlaythroughs.Count > 0)
        {
          // Only select the entries that had the lowest score
          int minimumModifications = potentialPlaythroughs.Min(c => c.Value);
          List<KeyValuePair<Playthrough, int>> closestPlaythroughlist = potentialPlaythroughs.Where(c => c.Value == minimumModifications).ToList();

          if (closestPlaythroughlist.Count > Program.Settings.BotGameLimit)
          {
            messageList.Add($"Sorry {Username} but there were too many entries matching that. Try a more specific lookup");
          }
          else
          {
            // String formatting
            string matchType = minimumModifications == 0 ? matchTypes[0] : minimumModifications == 1 ? matchTypes[1] : matchTypes[2];
            if (closestPlaythroughlist.Count > 1)
            {
              matchType = matchType + "es";
            }

            string messageToSend = $"{Username} Found game {matchType}: ";
            bool first = true;
            foreach (KeyValuePair<Playthrough, int> entry in closestPlaythroughlist)
            {
              if (!first)
              {
                messageToSend = messageToSend + ", ";
              }
              else
              {
                first = false;
              }

              // Add each game to the message, splitting into multiple messages if character limit hit
              string deathString = entry.Key.Deaths == 1 ? "death" : "deaths";
              string playthroughData = $"{entry.Key.Game} with {entry.Key.Deaths} {deathString}";
              if ((messageToSend + playthroughData).Length >= 500)
              {
                messageList.Add(messageToSend);
                messageToSend = playthroughData;
              }
              else
              {
                messageToSend = messageToSend + playthroughData;
              }
            }
            messageList.Add(messageToSend);
          }
        }

        if (potentialBosses.Count > 0)
        {
          // Only select the entries that had the lowest score
          int minimumModifications = potentialBosses.Min(c => c.Value);
          List<KeyValuePair<Tuple<Boss, string>, int>> closestBosslist = potentialBosses.Where(c => c.Value == minimumModifications).ToList();

          if (closestBosslist.Count > Program.Settings.BotBossLimit)
          {
            messageList.Add($"Sorry {Username} but there were too many entries matching that. Try a more specific lookup");
          }
          else
          {
            // String formatting
            string matchType = minimumModifications == 0 ? matchTypes[0] : minimumModifications == 1 ? matchTypes[1] : matchTypes[2];
            if (closestBosslist.Count > 1)
            {
              matchType = matchType + "es";
            }

            string messageToSend = $"{Username} Found boss {matchType}: ";
            bool first = true;
            foreach (KeyValuePair<Tuple<Boss, string>, int> entry in closestBosslist)
            {
              if (!first)
              {
                messageToSend = messageToSend + ", ";
              }
              else
              {
                first = false;
              }

              // Add each boss to the message, splitting into multiple messages if character limit hit
              string deathString = entry.Key.Item1.Deaths == 1 ? "death" : "deaths";
              string bossData = $"{entry.Key.Item1.Name} in game {entry.Key.Item2} with {entry.Key.Item1.Deaths} {deathString}";
              if ((messageToSend + bossData).Length >= 500)
              {
                messageList.Add(messageToSend);
                messageToSend = bossData;
              }
              else
              {
                messageToSend = messageToSend + bossData;
              }
            }
            messageList.Add(messageToSend);
          }
        }

        // No matches found at all
        if (potentialBosses.Count() == 0 && potentialPlaythroughs.Count == 0)
        {
          messageList.Add($"Sorry {Username} but I couldn't find any matches. Consider a refined search");
        }

        // Send all the messages
        foreach (string message in messageList)
        {
          SendMessage(message);
        }
      }
      else
      {
        SendMessage($"{Username} Total Deaths: {Program.StatsManager.TotalDeaths}. Enter a search term for specific game or boss e.g. \"dark souls\" or \"demon of hatred\" or \"top\" for the highest counts");
      }
    }

    private void SendMessage(string message)
    {
      StreamWriter.WriteLine($"PRIVMSG #{ChannelName} :{message}");
    }

  }
}
