using System.Net.Sockets;

namespace StatTracker
{
    class TwitchCommand
    {
        // User who sent the command
        public string User;
        // Category of the command
        public string Category;
        // First arg relative to the category
        public string MainArg;
        // Second arg relative to the category
        public string SecondaryArg;
    }

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

        // For parsing commands
        Dictionary<string, Tuple<string, Action<TwitchCommand>>> Functions;

        // Only 1 thread allowed at a time
        private static Thread TwitchThread;

        public static void InitTwitchListener()
        {
            if(TwitchThread!= null && TwitchThread.IsAlive)
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

            // All the different 
            Functions = new Dictionary<string, Tuple<string, Action<TwitchCommand>>>()
            {
                {"total", Tuple.Create("Get the total deaths", OutputTotal) },
                {"game", Tuple.Create("Get the deaths for the current or specified game [arg1].", OutputPlaythrough) },
                {"boss", Tuple.Create("Get the deaths for the current boss, specific boss [arg1] of the current game, or specific boss [arg1] of a specified game [arg2].", OutputBoss) },
                {"list", Tuple.Create("Get a list of all playthroughs, all bosses of the current game [arg1], or all bosses of a specific game [arg2].", OutputList) },
                {"sessions", Tuple.Create("Get how many sessions have happened for a playthrough.", OutputSessions) },
                {"help", Tuple.Create("List all commands or the details of a specific command [arg1].", OutputHelp) },
                {"vod", Tuple.Create("Get the Youtube VOD link for the current or specified game [arg1].", OutputVOD) },
                {"time", Tuple.Create("Get the playtime of the current or specified game [arg1]", OutputTime) }
            };

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
                        ParseDeathsCommand(username, message);
                    }
                }
            }
        }

        private void ParseDeathsCommand(string username, string message)
        {
            string[] args = message.Split(" ");

            TwitchCommand Command = new TwitchCommand();
            Command.User = username;
            Command.Category = args.Length > 1 ? args[1] : String.Empty;
            Command.MainArg = args.Length > 2 ? args[2] : String.Empty;
            Command.SecondaryArg = args.Length > 3 ? args[3] : String.Empty;

            ExecuteFunction(Command);
        }

        private bool ExecuteFunction(TwitchCommand Command)
        {
            // Search the dictionary for a matching command
            foreach (KeyValuePair<string, Tuple<string, Action<TwitchCommand>>> entry in Functions)
            {
                if (Command.Category == entry.Key)
                {
                    // Call the function associated with the command
                    entry.Value.Item2(Command);
                    return true;
                }
            }

            // If no commands args are set at all then just output the total
            if (Command.Category == String.Empty)
            {
                OutputTotal(Command);
            }
            else
            {
                // Invalid command
                SendMessage($"Sorry {Command.User} but I don't understand the command {Command.Category}");
            }
            return false;
        }

        private void SendMessage(string message)
        {
            StreamWriter.WriteLine($"PRIVMSG #{ChannelName} :{message}");
        }
        private Playthrough TryFindPlaythrough(string GameName)
        {
            if (string.IsNullOrEmpty(GameName))
            {
                GameName = Program.StatsManager.GetCurrentPlaythrough() != null ? Program.StatsManager.GetCurrentPlaythrough().Lookup : string.Empty;
            }

            var playthrough = Program.StatsManager.Playthroughs.Find(p => p.Lookup == GameName);
            if (playthrough == null)
            {
                // Last ditch attempt to search the game name
                playthrough = Program.StatsManager.Playthroughs.Find(p => p.Game.Contains(GameName, StringComparison.OrdinalIgnoreCase));
            }

            return playthrough;
        }

        private Boss TryFindBoss(Playthrough Playthrough, String BossName)
        {
            Boss boss = null;

            if (string.IsNullOrEmpty(BossName))
            {
                // If no arguments are specified then the boss is just the current one
                boss = Playthrough.Bosses.Find(b => b.Status == "Current");
            }
            else
            {
                boss = Playthrough.Bosses.Find(b => b.Lookup == BossName);
                if (boss == null)
                {
                    // Last ditch attempt to search the boss name
                    boss = Playthrough.Bosses.Find(b => b.Name.Contains(BossName, StringComparison.OrdinalIgnoreCase));
                }
            }

            return boss;
        }
        private void WIP(TwitchCommand Command)
        {
            int totalDeaths = Program.StatsManager.TotalDeaths;
            SendMessage($"Support Coming Soon");
        }
        private void OutputTotal(TwitchCommand Command)
        {
            int totalDeaths = Program.StatsManager.TotalDeaths;
            SendMessage($"{ChannelName} has died a total of {totalDeaths} times.");
        }
        private void OutputPlaythrough(TwitchCommand Command)
        {
            string gameToLookup = Command.MainArg;
            var playthrough = TryFindPlaythrough(gameToLookup);
            if (playthrough == null)
            {
                SendMessage($"Sorry {Command.User} but I can't find {gameToLookup}. Try the '!deaths list' command");
                return;
            }

            // Output the info
            string timeFormat = (playthrough.Deaths == 1) ? "time" : "times";

            if (playthrough.Status == "Complete")
            {
                SendMessage($"{ChannelName} died a total of {playthrough.Deaths} {timeFormat} in {playthrough.Game}.");
            }
            else
            {
                SendMessage($"{ChannelName} has died a total of {playthrough.Deaths} {timeFormat} so far in {playthrough.Game}.");
            }
        }
        private void OutputBoss(TwitchCommand Command)
        {
            // Need to find the playthrough first
            string gameToLookup = Command.SecondaryArg;
            var playthrough = TryFindPlaythrough(gameToLookup);
            if (playthrough == null)
            {
                SendMessage($"Sorry {Command.User} but I can't find {gameToLookup}. Try the '!deaths list' command");
                return;
            }

            // Find the boss in the playthrough
            string bossToLookup = Command.MainArg;
            Boss boss = TryFindBoss(playthrough, bossToLookup);

            if (boss == null)
            {
                // If an arg was specified then that boss couldn't be found otherwise there is no current boss
                if(Command.MainArg != String.Empty)
                {
                    SendMessage($"Sorry {Command.User} but I can't find {bossToLookup} for {gameToLookup}. Try the '!deaths list' command");
                }
                else
                {
                    SendMessage($"Sorry {Command.User} but {ChannelName} does not appear to currently be on a boss.");
                }
                
                return;
            }

            // Output the info
            string timeFormat = (boss.Deaths == 1) ? "time" : "times";
            if (boss.Status == "Defeated")
            {
                SendMessage($"{ChannelName} died a total of {boss.Deaths} {timeFormat} before killing {boss.Name} in {playthrough.Game}.");
            }
            else
            {
                SendMessage($"{ChannelName} has so far died a total of {boss.Deaths} {timeFormat} trying to kill {boss.Name} in {playthrough.Game}.");
            }
        }
        private void OutputList(TwitchCommand Command)
        {
            // If no args then list all playthroughs, otherwise list bosses for specified one
            if(Command.MainArg == String.Empty)
            {
                // Twitch has a character limit of 500 so we may need to send multiple messages
                List<string> messageList = new List<string>();
                messageList.Add("Playthroughs: ");
                int listIndex = 0;

                foreach (Playthrough playthrough in Program.StatsManager.Playthroughs)
                {
                    // If we are going to overrun then create a new message and add to the list
                    string playthroughLookup = String.Format("{0}, ", playthrough.Lookup);
                    if ((messageList[listIndex] + playthroughLookup).Length >= 500)
                    {
                        // Removes the last ", "
                        messageList[listIndex] = messageList[listIndex].Remove(messageList[listIndex].Count() - 2);
                        messageList.Add("Playthroughs cont: " + playthroughLookup);
                        listIndex++;
                    }
                    else
                    {
                        messageList[listIndex] += playthroughLookup;
                    }
                }
                // Removes the last ", "
                messageList[listIndex] = messageList[listIndex].Remove(messageList[listIndex].Count() - 2);

                // Send all the messages
                foreach (string message in messageList)
                {
                    SendMessage($"{message}");
                }
            }
            else
            {
                var playthrough = TryFindPlaythrough(Command.MainArg);
                if (playthrough == null)
                {
                    SendMessage($"Sorry {Command.User} but I can't find {Command.MainArg}. Try the '!deaths list' command without any args");
                }
                else
                {
                    // Twitch has a character limit of 500 so we may need to send multiple messages
                    List<string> messageList = new List<string>();
                    messageList.Add(String.Format("Bosses in {0}: ", playthrough.Game));
                    int listIndex = 0;

                    foreach (Boss boss in playthrough.Bosses)
                    {
                        // If we are going to overrun then create a new message and add to the list
                        string bossLookup = String.Format("{0}, ", boss.Lookup);
                        if ((messageList[listIndex] + bossLookup).Length >= 500)
                        {
                            // Removes the last ", "
                            messageList[listIndex] = messageList[listIndex].Remove(messageList[listIndex].Count() - 2);
                            messageList.Add("Bosses cont: " + bossLookup);
                            listIndex++;
                        }
                        else
                        {
                            messageList[listIndex] += bossLookup;
                        }
                    }
                    // Removes the last ", "
                    messageList[listIndex] = messageList[listIndex].Remove(messageList[listIndex].Count() - 2);

                    // Send all the messages
                    foreach (string message in messageList)
                    {
                        SendMessage($"{message}");
                    }
                }
            }
            
        }
        private void OutputSessions(TwitchCommand Command)
        {
            // Find the session count for either a specified playthrough or fallback to the current one
            string gameToLookup = Command.MainArg;
            var playthrough = TryFindPlaythrough(gameToLookup);
            if (playthrough == null)
            {
                SendMessage($"Sorry {Command.User} but I can't find {gameToLookup}. Try the '!deaths list' command");
                return;
            }

            string sessionFormat = playthrough.Sessions == 1 ? "session" : "sessions";
            if(playthrough.Sessions == 0)
            {
                SendMessage($"{ChannelName} has not logged any sessions for {playthrough.Game}.");
            }
            else
            {
                SendMessage($"{ChannelName} has logged {playthrough.Sessions} {sessionFormat} for {playthrough.Game}.");
            }
        }
        private void OutputHelp(TwitchCommand Command)
        {
            if(Command.MainArg == String.Empty)
            {
                string message = String.Empty;
                message += "Help Commands: ";

                foreach (KeyValuePair<string, Tuple<string, Action<TwitchCommand>>> entry in Functions)
                {
                    message += $"{entry.Key}, ";
                }

                // Removes the last ", "
                message = message.Remove(message.Count() - 2);
                SendMessage($"{message}");
            }
            else
            {
                if(Functions.ContainsKey(Command.MainArg))
                {
                    SendMessage($"Command '{Command.MainArg}' - {Functions[Command.MainArg].Item1}");
                }
                else
                {
                    SendMessage($"Sorry {Command.User} but I don't recognise that help command. Try'!deaths help' to get the list");
                }
            }
        }
        private void OutputVOD(TwitchCommand Command)
        {
            string gameToLookup = Command.MainArg;
            var playthrough = TryFindPlaythrough(gameToLookup);
            if (playthrough == null)
            {
                SendMessage($"Sorry {Command.User} but I can't find {gameToLookup}. Try the '!deaths list' command");
                return;
            }

            if (String.IsNullOrEmpty(playthrough.VOD))
            {
                SendMessage($"{ChannelName} has not set a VOD link for {playthrough.Game}.");
            }
            else
            {
                SendMessage($"{playthrough.Game} VOD: {playthrough.VOD}");
            }
        }

        private void OutputTime(TwitchCommand Command)
        {
            string gameToLookup = Command.MainArg;
            var playthrough = TryFindPlaythrough(gameToLookup);
            if (playthrough == null)
            {
                SendMessage($"Sorry {Command.User} but I can't find {gameToLookup}. Try the '!deaths list' command");
                return;
            }

            if (String.IsNullOrEmpty(playthrough.Playtime))
            {
                SendMessage($"{ChannelName} has not set a playtime {playthrough.Game}.");
            }
            else
            {
                // Output the info
                string[] split = playthrough.Playtime.Split(" ");
                int hours = Int32.Parse(split[0].Split("h")[0]);
                string hourFormat = (hours == 1) ? "hour" : "hours";
                int minutes = Int32.Parse(split[1].Split("m")[0]);
                string minuteformat = (hours == 1) ? "minute" : "minutes";

                if (playthrough.Status == "Complete")
                {
                    SendMessage($"{ChannelName} took {hours} {hourFormat} and {minutes} {minuteformat} to finish {playthrough.Game}.");
                }
                else
                {
                    SendMessage($"{ChannelName} has spent {hours} {hourFormat} and {minutes} {minuteformat} so far on {playthrough.Game}.");
                }
            }
        }

    }
}
