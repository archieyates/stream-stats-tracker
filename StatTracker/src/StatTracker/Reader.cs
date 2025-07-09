﻿using System.Text.RegularExpressions;

namespace StatTracker
{
  class Reader
  {
    // Link commands with functions
    Dictionary<string, Tuple<List<string>, string, Action>> TopFunctions;
    Dictionary<string, Tuple<List<string>, string, Action>> GameFunctions;
    Dictionary<string, Tuple<List<string>, string, Action>> BossFunctions;
    Dictionary<string, Tuple<List<string>, string, Action>> DeathFunctions;
    Dictionary<string, Tuple<List<string>, string, Action>> SettingsFunctions;
    Dictionary<string, Tuple<List<string>, string, Action>> TwitchFunctions;

    public void Run()
    {
      // Set up the Function links
      InitFunctionData();

      // Program just continues to run parsing inputs
      while (true)
      {
        Program.Write(ConsoleColor.White, "Please enter base command or 'help' for command list: ");
        // Read the input
        string input = Console.ReadLine().ToLower();
        // Parse and run the command
        ExecuteFunction(input, TopFunctions);
      }
    }
    private void Help()
    {
      // Loop all the dictionaries and spit out the command info
      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.WriteLine("Top Level Commands");
      WriteFunctionData(TopFunctions);

      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.WriteLine("\nGame Commands");
      WriteFunctionData(GameFunctions);

      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.WriteLine("\nBoss Commands");
      WriteFunctionData(BossFunctions);

      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.WriteLine("\nDeath Commands");
      WriteFunctionData(DeathFunctions);

      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.WriteLine("\nSettings Commands");
      WriteFunctionData(SettingsFunctions);
    }
    private void InitFunctionData()
    {
      // Functions that can be called at the top level
      TopFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"game", Tuple.Create(new List<string>(){"game", "playthrough"},"Perform actions on the playthrough data (using Game Commands)", Game) },
                {"boss", Tuple.Create(new List<string>(){"boss", "bosses"},"Perform actions on the boss data for the current playthrough (using Boss Commands)", Boss) },
                {"death", Tuple.Create(new List<string>(){"death", "deaths"},"Perform actions on the death counts (using Death Commands)", Death) },
                {"settings", Tuple.Create(new List<string>(){"settings"},"Modify the settings file", Settings) },
                {"twitch", Tuple.Create(new List<string>(){"twitch"},"Manage Twitch Integration", Twitch) },
                {"++", Tuple.Create(new List<string>(){ "++"},"Increment the death count shortcut", AddDeath) },
                {"--", Tuple.Create(new List<string>(){ "--"},"Decrement the death count shortcut", SubtractDeath) },
                {"++br", Tuple.Create(new List<string>(){"++br"},"Increment the death count without counting the boss shortcut", AddBossRunDeath) },
                {"--br", Tuple.Create(new List<string>(){"--br"},"Decrement the death count without counting the boss shortcut", SubtractBossRunDeath) },
                {"help", Tuple.Create(new List<string>(){ "help", "commands"},"List help", Help) }
            };

      // Functions that can be called when game is input at the top level
      GameFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"newgame", Tuple.Create(new List<string>(){"new"},"Create a new playthrough", NewGame) },
                {"listgame", Tuple.Create(new List<string>(){"list"},"List all the playthroughs", ListGame) },
                {"currentgame", Tuple.Create(new List<string>(){"current"},"Set the current playthrough", SetCurrentGame) },
                {"completegame", Tuple.Create(new List<string>(){"complete"},"Complete the current playthrough", CompleteGame) },
                {"delete", Tuple.Create(new List<string>(){ "delete"},"Delete a specified playthrough", DeleteGame) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };

      // Functions that can be called when boss is input at the top level
      BossFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"new", Tuple.Create(new List<string>(){"new"},"Create a new boss (sets to current)", NewBoss) },
                {"list", Tuple.Create(new List<string>(){"list"},"List all the bosses for this playthrough", ListBoss) },
                {"current", Tuple.Create(new List<string>(){"current"},"Set the current boss", SetCurrentBoss) },
                {"unset", Tuple.Create(new List<string>(){"unset"},"Unset the current boss", UnsetCurrentBoss) },
                {"defeat", Tuple.Create(new List<string>(){"defeat"},"Mark current boss as defeated", DefeatBoss) },
                {"delete", Tuple.Create(new List<string>(){ "delete"},"Delete a specified boss", DeleteBoss) },
                {"rename", Tuple.Create(new List<string>(){ "rename"},"Rename a specified boss", RenameBoss) },
                {"next", Tuple.Create(new List<string>(){ "next"},"Set the next undefeated boss as current", NextBoss) },
                {"previous", Tuple.Create(new List<string>(){ "prev"},"Set the previous undefeated boss as current", PreviousBoss) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };

      // Functions that can be called when death is input at the top level
      DeathFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"add", Tuple.Create(new List<string>(){"add", "++"},"Increment the death count", AddDeath) },
                {"subtract", Tuple.Create(new List<string>(){ "subtract", "--"},"Decrement the death count", SubtractDeath) },
                {"bradd", Tuple.Create(new List<string>(){"bradd", "++br"},"Increment the death count without counting the boss", AddBossRunDeath) },
                {"brsubtract", Tuple.Create(new List<string>(){ "brsubtract", "--br"},"Decrement the death count without counting the boss", SubtractBossRunDeath) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };

      // Functions that can be called when settings is input at the top level
      SettingsFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"change", Tuple.Create(new List<string>(){"edit", "change"},"Edit the value of a setting", EditSetting) },
                {"list", Tuple.Create(new List<string>(){ "list"},"List all settings and their values", ListSettings) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };

      // Functions that can be called when twitch is input at the top level
      TwitchFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"connect", Tuple.Create(new List<string>(){"connect"},"Connect to Twitch", TwitchConnect) },
                {"ban", Tuple.Create(new List<string>(){"ban"},"Ban a player from accessing the bot commands", TwitchBan) },
                {"unban", Tuple.Create(new List<string>(){"unban"},"Unbans a player from accessing the bot commands", TwitchUnban) },
                {"unbanall", Tuple.Create(new List<string>(){"unbanall"},"Clear the ban list", TwitchUnbanAll) },
                {"banlist", Tuple.Create(new List<string>(){"list", "banlist"},"List all the banned accounts", TwitchBanList) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };
    }

    private void WriteFunctionData(Dictionary<string, Tuple<List<string>, string, Action>> Functions)
    {
      // Loop through all the dictionaries
      foreach (KeyValuePair<string, Tuple<List<string>, string, Action>> entry in Functions)
      {
        // List all the accepted commands
        string outputString = "\t- [";
        int count = 0;
        foreach (string input in entry.Value.Item1)
        {
          ++count;
          outputString += input;

          // Little dressing so that the list of commands terminates nicely
          if (count < entry.Value.Item1.Count)
          {
            outputString += ", ";
          }

        }
        // Close off the list and also print the description
        outputString += "]: " + entry.Value.Item2;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(outputString);
      }
    }
    private bool ExecuteFunction(string Command, Dictionary<string, Tuple<List<string>, string, Action>> Functions)
    {
      // Search the dictionary for a matching command
      foreach (KeyValuePair<string, Tuple<List<string>, string, Action>> entry in Functions)
      {
        // A single entry can have multiple accepted commands
        foreach (string parsed in entry.Value.Item1)
        {
          if (Command == parsed)
          {
            // Call the function associated with the command
            entry.Value.Item3();
            return true;
          }
        }
      }

      // Invalid command: restart the loop
      Program.WriteLine(ConsoleColor.Red, "Command '{0}' not found", Command);
      return false;
    }
    private void Game()
    {
      Program.Write(ConsoleColor.Yellow, "Please enter game command: ");
      // Read the input
      string input = Console.ReadLine().ToLower();
      // Parse and run the command
      if (!ExecuteFunction(input, GameFunctions))
      {
        Game();
      }
    }
    private void Boss()
    {
      if (!Program.StatsManager.CheckCurrentPlaythrough())
      {
        return;
      }

      Program.Write(ConsoleColor.Yellow, "Please enter boss command: ");
      // Read the input
      string input = Console.ReadLine().ToLower();
      // Parse and run the command
      if (!ExecuteFunction(input, BossFunctions))
      {
        Boss();
      }
    }
    private void Death()
    {
      Program.Write(ConsoleColor.Yellow, "Please enter death command: ");
      // Read the input
      string input = Console.ReadLine().ToLower();
      // Parse and run the command
      if (!ExecuteFunction(input, DeathFunctions))
      {
        Death();
      }
    }
    private void Settings()
    {
      Program.Write(ConsoleColor.Yellow, "Please enter setting command: ");
      // Read the input
      string input = Console.ReadLine();

      // Parse and run the command
      if (!ExecuteFunction(input, SettingsFunctions))
      {
        Settings();
      }

    }
    private void Twitch()
    {
      Program.Write(ConsoleColor.Yellow, "Please enter Twitch command: ");
      // Read the input
      string input = Console.ReadLine();

      // Parse and run the command
      if (!ExecuteFunction(input, TwitchFunctions))
      {
        Twitch();
      }
    }
    private void NewGame()
    {
      // Game Name is the actual name of the game and doesn't need to be unique
      Program.Write(ConsoleColor.Blue, "Enter Game Name: ");
      string gameName = Console.ReadLine();

      // Lookup is used as the unique identifier for each playthrough
      string lookup = String.Empty;

      // If we're auto-generating the lookup then remove all the spaces from the game name
      if (Program.Settings.AutoGenerateLookup)
      {
        lookup = Program.StatsManager.GeneratePlaythroughLookup(gameName);
      }
      else
      {
        Program.Write(ConsoleColor.Blue, "Enter Lookup: ");
        lookup = Console.ReadLine().ToLower();
        // Make sure it's in a format that won't mess with text files
        lookup = Regex.Replace(lookup, "[^0-9a-zA-Z]+", "");

        // Check this playthrough doesn't already exist
        if (Program.StatsManager.Playthroughs.Find(p => p.Lookup == lookup) != null)
        {
          Program.WriteLine(ConsoleColor.Red, "{0} already in use", lookup);
          return;
        }
      }

      // Manager handles the actual data-side
      Program.StatsManager.AddNewPlaythrough(lookup, gameName);
    }
    private void ListGame()
    {
      // Prints out some game data
      Console.ForegroundColor = ConsoleColor.Green;
      foreach (Playthrough playthrough in Program.StatsManager.Playthroughs)
      {
        Console.WriteLine($"{playthrough.Lookup} | {playthrough.Game} | {playthrough.Status} | {playthrough.Deaths}");
      }
    }
    private void SetCurrentGame()
    {
      // The unique ID of the playthrough
      Program.Write(ConsoleColor.Blue, "Enter Lookup: ");
      string lookup = Console.ReadLine().ToLower();

      // Manager handles the actual data
      Program.StatsManager.SetCurrentPlaythrough(lookup);
    }
    private void CompleteGame()
    {
      // Manager handles actual data
      Program.StatsManager.CompleteCurrentPlaythrough();
    }
    private void DeleteGame()
    {
      // The unique ID of the playthrough
      Program.Write(ConsoleColor.Blue, "Enter Lookup: ");
      string lookup = Console.ReadLine().ToLower();

      // Manager handles actual data
      Program.StatsManager.DeletePlaythrough(lookup);
    }
    private void NewBoss()
    {
      // Boss Name is the actual name of the game and doesn't need to be unique
      Program.Write(ConsoleColor.Blue, "Enter Boss Name: ");
      string bossName = Console.ReadLine();

      // Lookup is used as the unique identifier for each boss
      string lookup = String.Empty;

      // If we're auto-generating the lookup then remove all the spaces from the game name
      if (Program.Settings.AutoGenerateLookup)
      {
        lookup = Program.StatsManager.GenerateBossLookup(bossName);
      }
      else
      {
        Program.Write(ConsoleColor.Blue, "Enter Lookup: ");
        lookup = Console.ReadLine().ToLower();
        // Make sure it's in a format that won't mess with text files
        lookup = Regex.Replace(lookup, "[^0-9a-zA-Z]+", "");

        // Check this playthrough doesn't already exist
        if (Program.StatsManager.GetCurrentPlaythrough().Bosses.Find(b => b.Lookup == lookup) != null)
        {
          Program.WriteLine(ConsoleColor.Red, "{0} already in use", lookup);
          return;
        }
      }

      // Manager handles the actual data-side
      Program.StatsManager.AddNewBoss(lookup, bossName);
    }
    private void ListBoss()
    {
      // List some boss data
      Console.ForegroundColor = ConsoleColor.Green;
      foreach (Boss boss in Program.StatsManager.GetCurrentPlaythrough().Bosses)
      {
        Console.WriteLine($"{boss.Lookup} | {boss.Name} | {boss.Status} | {boss.Deaths}");
      }
    }
    private void SetCurrentBoss()
    {
      // The unique ID of the boss
      Program.Write(ConsoleColor.Blue, "Enter Lookup: ");
      string lookup = Console.ReadLine().ToLower();

      // Manager handles the actual data
      Program.StatsManager.SetCurrentBoss(lookup);
    }
    private void UnsetCurrentBoss()
    {
      // Manager handles the actual data
      Program.StatsManager.SetCurrentBoss(String.Empty);
    }
    private void DefeatBoss()
    {
      // Manager handles the actual data
      Program.StatsManager.DefeatCurrentBoss();
    }
    private void DeleteBoss()
    {
      // Lookup is used as the unique identifier for each boss
      Program.Write(ConsoleColor.Blue, "Enter Lookup: ");
      string lookup = Console.ReadLine().ToLower();

      // Manager handles the actual data
      Program.StatsManager.DeleteBoss(lookup);
    }
    private void RenameBoss()
    {
      // Lookup is used as the unique identifier for each boss
      Program.Write(ConsoleColor.Blue, "Enter Lookup: ");
      string lookup = Console.ReadLine().ToLower();

      if (lookup == "esc")
      {
        return;
      }

      Boss boss = Program.StatsManager.GetBoss(lookup);
      if (boss == null)
      {
        Program.WriteLine(ConsoleColor.Red, "{0} lookup doesn't exist", lookup);
        RenameBoss();
        return;
      }

      Program.Write(ConsoleColor.Blue, "Enter New Boss Name: ");
      string bossName = Console.ReadLine();
      Program.StatsManager.RenameBoss(lookup, bossName);

    }
    private void NextBoss()
    {
      Program.StatsManager.NextBoss();
    }
    private void PreviousBoss()
    {
      Program.StatsManager.PreviousBoss();
    }
    private void AddDeath()
    {
      // Manager handles the actual data
      Program.StatsManager.AddDeath(true);
    }
    private void SubtractDeath()
    {
      // Manager handles the actual data
      Program.StatsManager.SubtractDeath(true);
    }
    private void AddBossRunDeath()
    {
      // Manager handles the actual data
      Program.StatsManager.AddDeath(false);
    }
    private void SubtractBossRunDeath()
    {
      // Manager handles the actual data
      Program.StatsManager.SubtractDeath(false);
    }
    private void EditSetting()
    {
      Program.Write(ConsoleColor.Blue, "Setting to Modify: ");
      string setting = Console.ReadLine();

      // Look for the property
      Object property = Program.Settings.GetPropertyValue(setting);

      // Bail if it's null
      if (property == null)
      {
        Program.WriteLine(ConsoleColor.Red, "Property {0} doesn't exist", setting);
        return;
      }

      // Get the property type
      Type propType = property.GetType();

      // Get the new value
      Program.Write(ConsoleColor.Blue, "New Value: ");
      string value = Console.ReadLine();

      // Convert that string value to an actual value
      Object newValue = null;
      try
      {
        newValue = Convert.ChangeType(value, propType);
      }
      catch (Exception ex)
      {
        Program.WriteLine(ConsoleColor.Red, "Failed to convert {0} for reason: {1}", value, ex.Message);
        return;
      }

      // On the off chance that the complete invalid value is entered but is somehow converted (unlikely)
      if (newValue == null)
      {
        Program.WriteLine(ConsoleColor.Red, "Conversion of value {0} failed for unknown reason", value);
        return;
      }

      // Otherwise set the new value and save
      Program.Settings.SetPropertyValue(setting, newValue);
      Program.SaveSettings();

      Program.WriteLine(ConsoleColor.Green, "Set {0} to {1}", setting, value);
    }
    private void ListSettings()
    {
      // Print out all the properties and their values
      foreach (var prop in Program.Settings.GetType().GetProperties())
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("{0}={1}", prop.Name, prop.GetValue(Program.Settings, null));
      }
    }
    private void TwitchConnect()
    {
      String ChannelName = Program.Settings.ChannelName;
      String BotName = Program.Settings.BotName;
      String OAuth = Environment.GetEnvironmentVariable("TWITCH_BOT_OAUTH", EnvironmentVariableTarget.User);

      if (String.IsNullOrEmpty(OAuth))
      {
        Program.WriteLine(ConsoleColor.Red, "OATH TOKEN NOT SET. Please add a User Environment Variable called \"TWITCH_BOT_OAUTH\" ");
        return;
      }

      if (String.IsNullOrEmpty(ChannelName))
      {
        Program.Write(ConsoleColor.Blue, "Enter your Channel Name: ");
        ChannelName = Console.ReadLine();
        Program.Settings.ChannelName = ChannelName;
        Program.WriteLine(ConsoleColor.Green, "Channel Name {0} written to Settings.", ChannelName);
        Program.SaveSettings();
      }

      if (String.IsNullOrEmpty(BotName))
      {
        Program.Write(ConsoleColor.Blue, "Enter your Bot's Name: ");
        BotName = Console.ReadLine();
        Program.Settings.BotName = BotName;
        Program.WriteLine(ConsoleColor.Green, "Bot Name {0} written to Settings.", BotName);
        Program.SaveSettings();
      }

      TwitchBot.InitTwitchListener();
      Thread.Sleep(1000);
    }
    private void TwitchBan()
    {
      Program.Write(ConsoleColor.Blue, "Account to Ban: ");
      string account = Console.ReadLine().ToLower();

      if (Program.Settings.BannedPlayers.Contains(account))
      {
        Program.WriteLine(ConsoleColor.Red, "{0} is already banned from using the bot commands", account);
      }
      else
      {
        Program.Settings.BannedPlayers.Add(account);
        Program.SaveSettings();
        Program.WriteLine(ConsoleColor.Green, "Banned {0} from using the bot commands", account);
      }
    }
    private void TwitchUnban()
    {
      Program.Write(ConsoleColor.Blue, "Account to Unban: ");
      string account = Console.ReadLine().ToLower();

      if (Program.Settings.BannedPlayers.Contains(account))
      {
        Program.Settings.BannedPlayers.Remove(account);
        Program.SaveSettings();
        Program.WriteLine(ConsoleColor.Green, "Unbanned {0} from using the bot commands", account);
      }
      else
      {
        Program.WriteLine(ConsoleColor.Red, "{0} is not in ban list", account);
      }
    }
    private void TwitchUnbanAll()
    {
      Program.Settings.BannedPlayers.Clear();
      Program.SaveSettings();
      Program.WriteLine(ConsoleColor.Green, "Unbanned all accounts from using the bot commands");
    }
    private void TwitchBanList()
    {
      foreach (string account in Program.Settings.BannedPlayers)
      {
        Program.WriteLine(ConsoleColor.Green, account);
      }
    }
    private void Return()
    {
      // Just so I can list the esc option in the help list
    }
  }
}
