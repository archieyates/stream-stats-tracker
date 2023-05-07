using System;
using System.IO;

namespace StatTracker
{
    class Reader
    {
        // StatsManager actual manages data including saving and loading
        StatsManager Manager = new StatsManager();

        // Link commands with functions
        Dictionary<string, Tuple<List<string>, string, Action>> TopFunctions;
        Dictionary<string, Tuple<List<string>, string, Action>> GameFunctions;
        Dictionary<string, Tuple<List<string>, string, Action>> BossFunctions;
        Dictionary<string, Tuple<List<string>, string, Action>> DeathFunctions;

        public void Run()
        {
            // Set up the Function links
            InitFunctionData();

            // Program just continues to run parsing inputs
            while (true)
            {
                Console.ResetColor();
                Console.Write("Please enter base command or 'help' for command list: ");
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
        }
        private void InitFunctionData()
        {
            // Functions that can be called at the top level
            TopFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"game", Tuple.Create(new List<string>(){"game", "playthrough"},"Perform actions on the playthrough data (using Game Commands)", Game) },
                {"boss", Tuple.Create(new List<string>(){"boss", "bosses"},"Perform actions on the boss data for the current playthrough (using Boss Commands)", Boss) },
                {"death", Tuple.Create(new List<string>(){"death", "deaths"},"Perform actions on the death counts (using Death Commands)", Death) },
                {"++", Tuple.Create(new List<string>(){ "++"},"Increment the death count shortcut", AddDeath) },
                {"--", Tuple.Create(new List<string>(){ "--"},"Decrement the death count shortcut", SubtractDeath) },
                {"help", Tuple.Create(new List<string>(){ "help", "commands"},"List help", Help) }
            };

            // Functions that can be called when game is input at the top level
            GameFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"newgame", Tuple.Create(new List<string>(){"new"},"Create a new playthrough", NewGame) },
                {"listgame", Tuple.Create(new List<string>(){"list"},"List all the playthroughs", ListGame) },
                {"currentgame", Tuple.Create(new List<string>(){"current"},"Set the current playthrough", SetCurrentGame) },
                {"completegame", Tuple.Create(new List<string>(){"complete"},"Complete the current playthrough", CompleteGame) },
                {"sessions", Tuple.Create(new List<string>(){ "sessions", "session"},"Update the session count for current playthrough", GameSession) },
                {"delete", Tuple.Create(new List<string>(){ "delete"},"Delete a specified playthrough", DeleteGame) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };

            // Functions that can be called when boss is input at the top level
            BossFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"new", Tuple.Create(new List<string>(){"new"},"Create a new boss (sets to current)", NewBoss) },
                {"list", Tuple.Create(new List<string>(){"list"},"List all the bosses for this playthrough", ListBoss) },
                {"current", Tuple.Create(new List<string>(){"current"},"Set the current boss", SetCurrentBoss) },
                {"defeat", Tuple.Create(new List<string>(){"defeat"},"Mark current boss as defeated", DefeatBoss) },
                {"delete", Tuple.Create(new List<string>(){ "delete"},"Delete a specified boss", DeleteBoss) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };

            // Functions that can be called when death is input at the top level
            DeathFunctions = new Dictionary<string, Tuple<List<string>, string, Action>>()
            {
                {"add", Tuple.Create(new List<string>(){"add", "++"},"Increment the death count", AddDeath) },
                {"subtract", Tuple.Create(new List<string>(){ "subtract", "--"},"Decrement the death count", SubtractDeath) },
                {"esc", Tuple.Create(new List<string>(){ "esc"},"Return back to main", Return) }
            };
        }
        private void WriteFunctionData(Dictionary<string, Tuple<List<string>, string, Action>> Functions)
        {
            // Loop through all the dictionaries
            Console.ForegroundColor = ConsoleColor.Green;
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Command '{0}' not found", Command);
            return false;
        }
        private void Game()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Please enter game command: ");
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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Please enter boss command: ");
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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Please enter death command: ");
            // Read the input
            string input = Console.ReadLine().ToLower();
            // Parse and run the command
            if (!ExecuteFunction(input, DeathFunctions))
            {
                Death();
            }
        }
        private void NewGame()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // Lookup is used as the unique identifier for each playthrough
            Console.Write("Enter Lookup: ");
            string lookup = Console.ReadLine().ToLower();

            // Check this playthrough doesn't already exist
            if (Manager.Playthroughs.Find(p => p.Lookup == lookup) != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} already in use", lookup);
                return;
            }

            // Game Name is the actual name of the game and doesn't need to be unique
            Console.Write("Enter Game Name: ");
            string gameName = Console.ReadLine();

            // Manager handles the actual data-side
            Manager.AddNewPlaythrough(lookup, gameName);
        }
        private void ListGame()
        {
            // Prints out some game data
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (Playthrough playthrough in Manager.Playthroughs)
            {
                Console.WriteLine("{0} | {1} | {2} | {3}", playthrough.Lookup, playthrough.Game, playthrough.Status, playthrough.Deaths);
            }
        }
        private void SetCurrentGame()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // The unique ID of the playthrough
            Console.Write("Enter Lookup: ");
            string lookup = Console.ReadLine().ToLower();

            // Manager handles the actual data
            Manager.SetCurrentPlaythrough(lookup);
        }
        private void CompleteGame()
        {
            // Manager handles actual data
            Manager.CompleteCurrentPlaythrough();
        }
        private void GameSession()
        {
            // Manager handles actual data
            Manager.IncrementCurrentPlaythroughSessions();
        }
        private void DeleteGame()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // The unique ID of the playthrough
            Console.Write("Enter Lookup: ");
            string lookup = Console.ReadLine().ToLower();

            // Manager handles actual data
            Manager.DeletePlaythrough(lookup);
        }
        private void NewBoss()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // Lookup is used as the unique identifier for each boss
            Console.Write("Enter Lookup: ");
            string lookup = Console.ReadLine().ToLower();

            // Check this boss doesn't already exist
            if (Manager.Bosses.Find(b => b.Lookup == lookup) != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} already in use", lookup);
                return;
            }

            // Boss Name is the actual name of the boss and doesn't need to be unique
            Console.Write("Enter Boss Name: ");
            string bossName = Console.ReadLine();

            // Manager handles the actual data-side
            Manager.AddNewBoss(lookup, bossName);
        }
        private void ListBoss()
        {
            // List some boss data
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (Boss boss in Manager.Bosses)
            {
                Console.WriteLine("{0} | {1} | {2} | {3}", boss.Lookup, boss.Name, boss.Status, boss.Deaths);
            }
        }
        private void SetCurrentBoss()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // The unique ID of the boss
            Console.Write("Enter Lookup: ");
            string lookup = Console.ReadLine().ToLower();

            // Manager handles the actual data
            Manager.SetCurrentBoss(lookup);
        }
        private void DefeatBoss()
        {
            // Manager handles the actual data
            Manager.DefeatCurrentBoss();
        }
        private void DeleteBoss()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            // Lookup is used as the unique identifier for each boss
            Console.Write("Enter Lookup: ");
            string lookup = Console.ReadLine().ToLower();

            // Manager handles the actual data
            Manager.DeleteBoss(lookup);
        }
        private void AddDeath()
        {
            // Manager handles the actual data
            Manager.AddDeath();
        }
        private void SubtractDeath()
        {
            // Manager handles the actual data
            Manager.SubtractDeath();
        }
        private void Return()
        {
            // Just so I can list the esc option in the help list
        }
    }
}
