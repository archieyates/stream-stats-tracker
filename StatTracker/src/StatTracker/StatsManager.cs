using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatTracker
{
    class StatsManager
    {
        // Data for all playthroughs 
        public List<Playthrough> Playthroughs = new List<Playthrough>();
        // Current info
        public string CurrentPlaythrough = String.Empty;
        public string CurrentBoss = String.Empty;
        // Cache death data
        public int CurrentGameDeaths = 0;
        public int CurrentBossDeaths = 0;
        public int TotalDeaths = 0;

        public StatsManager()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            LoadPlaythroughs();
        }
        private void LoadPlaythroughs()
        {
            Playthroughs.Clear();
            string fileName = "Stats\\Playthroughs\\list.txt";

            // Create the file if it doesn't exist
            if (!File.Exists(fileName))
            {
                Program.WriteLine(ConsoleColor.Yellow, "Creating list.txt");
                File.Create(fileName).Dispose();
            }

            var lines = File.ReadLines(fileName);
            foreach (var line in lines)
            {
                // Deserialize the file
                string playthroughFile = String.Format("Stats\\Playthroughs\\{0}.json", line);
                if (!File.Exists(playthroughFile))
                {
                    Program.WriteLine(ConsoleColor.Yellow, "Creating {0}.json", line);
                    // Create an empty entry so the basic JSON structure is created correctly
                    Playthrough newPlaythrough = new Playthrough();
                    string json = JsonSerializer.Serialize<Playthrough>(newPlaythrough, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                    File.WriteAllText(playthroughFile, json);
                }

                string jsonString = System.IO.File.ReadAllText(playthroughFile);
                Playthrough playthrough = JsonSerializer.Deserialize<Playthrough>(jsonString);
                Playthroughs.Add(playthrough);

                if (playthrough.Status == "Current")
                {
                    CurrentPlaythrough = playthrough.Lookup;
                    CurrentGameDeaths = playthrough.Deaths;
                }

                Boss currentBoss = playthrough.Bosses.Find(b => b.Status == "Current");
                if (currentBoss != null)
                {
                    CurrentBoss = currentBoss.Lookup;
                    CurrentBossDeaths = currentBoss.Deaths;
                }
            }

            // Update the death files
            SaveDeaths();
        }
        // TODO: FROM HERE
        private void SavePlaythroughs()
        {
            string fileName = "Stats\\Playthroughs.json";

            // Serialize out the playthrough data
            PlaythroughContainer dataContainer = BuildPlaythroughData();
            string json = JsonSerializer.Serialize<PlaythroughContainer>(dataContainer, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            File.WriteAllText(fileName, json);
        }
        private void SaveDeaths()
        {
            // Recalculate total deaths
            TotalDeaths = 0;
            foreach (Playthrough playthrough in Playthroughs)
            {
                TotalDeaths += playthrough.Deaths;
            }

            // Save out the 3 death files
            File.WriteAllText("Stats\\Deaths\\Game.txt", CurrentGameDeaths.ToString());
            File.WriteAllText("Stats\\Deaths\\Total.txt", TotalDeaths.ToString());
            File.WriteAllText("Stats\\Deaths\\Boss.txt", CurrentBossDeaths.ToString());
        }
        public void AddNewPlaythrough(string Lookup, string Name)
        {
            // Create new playthrough entry
            Playthrough newPlaythrough = new Playthrough();
            newPlaythrough.Lookup = Lookup;
            newPlaythrough.Game = Name;
            Playthroughs.Add(newPlaythrough);

            // Save out the playthrough file
            SavePlaythroughs();
            Program.WriteLine(ConsoleColor.Green, "Added New Playthrough \"{0}\" with Lookup \"{1}\"", Name, Lookup);

            // If there isn't a current playthrough then mark this one as current
            if (CurrentPlaythrough == String.Empty)
            {
                SetCurrentPlaythrough(newPlaythrough.Lookup);
            }
        }
        public void SetCurrentPlaythrough(string Game)
        {
            // If there already is a current playthrough then mark it as in-progress
            if (CurrentPlaythrough != String.Empty)
            {
                Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Status = "In_Progress";
                Program.WriteLine(ConsoleColor.Green, "{0} set to \"In-Progress\"", CurrentPlaythrough);
            }

            // Set current playthrough
            if (Playthroughs.Find(p => p.Lookup == Game) != null)
            {
                // Mark the current playthrough
                Playthroughs.Find(p => p.Lookup == Game).Status = "Current";
                CurrentPlaythrough = Game;
                CurrentGameDeaths = Playthroughs.Find(p => p.Lookup == Game).Deaths;
                Program.WriteLine(ConsoleColor.Green, "{0} set to \"Current\"", Game);
            }
            else
            {
                Program.WriteLine(ConsoleColor.Red, "Lookup not recognised");
            }

            // Save out the data and death info
            SavePlaythroughs();
            SaveDeaths();
        }
        public Playthrough GetCurrentPlaythrough()
        {
            return Playthroughs.Find(p => p.Lookup == CurrentPlaythrough);
        }
        public void CompleteCurrentPlaythrough()
        {
            // If there isn't a current playthrough then it can't be updated
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Mark current playthrough as complete
            Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Status = "Complete";
            // Reset our current data
            string tempCurrentPlaythrough = CurrentPlaythrough;
            CurrentPlaythrough = String.Empty;
            CurrentBoss = String.Empty;

            // Save data
            SavePlaythroughs();

            Program.WriteLine(ConsoleColor.Green, "{0} set to \"Complete\"", tempCurrentPlaythrough);
        }
        public void IncrementCurrentPlaythroughSessions()
        {
            // If there isn't a current playthrough then it can't be updated
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Update the session count
            Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Sessions++;

            // Save data
            SavePlaythroughs();

            Program.WriteLine(ConsoleColor.Green, "{0} session count set to {1}", CurrentPlaythrough, Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Sessions);
        }
        public void DeletePlaythrough(string Lookup)
        {
            // Look for the playthrough
            Playthrough toDelete = Playthroughs.Find(p => p.Lookup == Lookup);
            if (toDelete != null)
            {
                // Check if the playthrough to be deleted is the current one
                if (toDelete.Status == "Current")
                {
                    // If it is then reset the current data and update the death count
                    CurrentPlaythrough = String.Empty;
                    CurrentBoss = String.Empty;
                    CurrentGameDeaths = 0;
                    CurrentBossDeaths = 0;
                    SaveDeaths();
                }

                // Remove the playthrough
                Playthroughs.Remove(toDelete);

                // Save data
                SavePlaythroughs();

                Program.WriteLine(ConsoleColor.Green, "{0} deleted", Lookup);

                // Delete the associated boss file
                string fileName = String.Format("Stats\\Bosses\\{0}.json", Lookup);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    Program.WriteLine(ConsoleColor.Green, "{0} deleted", fileName);
                }
            }
            else
            {
                Program.WriteLine(ConsoleColor.Red, "{0} does not exist", Lookup);
            }
        }
        public void AddNewBoss(string Lookup, string Name)
        {
            // If there isn't a current playthrough then there won't be boss data
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Create new boss data
            Boss newBoss = new Boss();
            newBoss.Lookup = Lookup;
            newBoss.Name = Name;
            GetCurrentPlaythrough().Bosses.Add(newBoss);
            Program.WriteLine(ConsoleColor.Green, "Added New Boss \"{0}\" with Lookup \"{1}\"", Name, Lookup);

            // If a new boss is being added there's an assumption it is the current one being fought
            SetCurrentBoss(Lookup);

            // Save the data and update death count
            SaveDeaths();
            SavePlaythroughs();
        }
        public void SetCurrentBoss(string NewBoss)
        {
            //// If there already is a current boss then set it to undefeated
            //if (CurrentBoss != String.Empty)
            //{
            //    Bosses.Find(b => b.Lookup == CurrentBoss).Status = "Undefeated";
            //    Program.WriteLine(ConsoleColor.Green, "{0} set to \"Undefeated\"", CurrentBoss);
            //}

            //// Set current boss
            //if (Bosses.Find(b => b.Lookup == NewBoss) != null)
            //{
            //    Bosses.Find(b => b.Lookup == NewBoss).Status = "Current";
            //    CurrentBoss = NewBoss;
            //    CurrentBossDeaths = Bosses.Find(b => b.Lookup == CurrentBoss).Deaths;
            //    Program.WriteLine(ConsoleColor.Green, "{0} set to \"Current\"", CurrentBoss);
            //}
            //else
            //{
            //    Program.WriteLine(ConsoleColor.Red, "Lookup not recognised");
            //}

            //// Save data and update death count
            //SaveBosses();
            //SaveDeaths();
        }
        public void DefeatCurrentBoss()
        {
            //// If there isn't a current boss then it can't be updated
            //if (!CheckCurrentBoss())
            //{
            //    return;
            //}

            //// Mark boss as defeated
            //Bosses.Find(b => b.Lookup == CurrentBoss).Status = "Defeated";

            //// Reset current data
            //string tempCurrentBoss = CurrentBoss;
            //CurrentBoss = String.Empty;
            //CurrentBossDeaths = 0;

            //// Save data and update death count
            //SaveBosses();
            //SaveDeaths();

            //Program.WriteLine(ConsoleColor.Green, "{0} set to \"Defeated\"", tempCurrentBoss);
        }
        public void DeleteBoss(string Lookup)
        {
            //// Search for the boss
            //Boss toDelete = Bosses.Find(b => b.Lookup == Lookup);
            //if (toDelete != null)
            //{
            //    // If the boss is our current one then we need to reset current data
            //    if (toDelete.Status == "Current")
            //    {
            //        CurrentBoss = String.Empty;
            //        CurrentBossDeaths = 0;
            //    }

            //    // If a boss is being deleted then we need to remove its deaths from the count
            //    Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths -= toDelete.Deaths;
            //    CurrentGameDeaths = Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths;

            //    // Remove the boss
            //    Bosses.Remove(toDelete);

            //    // Save data and update death count
            //    SaveBosses();
            //    SavePlaythroughs();
            //    SaveDeaths();

            //    Program.WriteLine(ConsoleColor.Green, "{0} deleted", Lookup);
            //}
            //else
            //{
            //    Program.WriteLine(ConsoleColor.Red, "{0} does not exist", Lookup);
            //}
        }
        public void AddDeath()
        {
            // If there isn't a current playthrough then we can't update the deaths for it
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Increment the playthrough deaths and update the current death count
            Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths++;
            CurrentGameDeaths = Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths;
            Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentPlaythrough, CurrentGameDeaths);

            // If there's a current boss then also update its count
            //if (CurrentBoss != String.Empty)
            //{
            //    Bosses.Find(b => b.Lookup == CurrentBoss).Deaths++;
            //    CurrentBossDeaths = Bosses.Find(b => b.Lookup == CurrentBoss).Deaths;
            //    SaveBosses();

            //    Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentBoss, CurrentBossDeaths);
            //}

            // Save data and update death counts
            SavePlaythroughs();
            SaveDeaths();
        }
        public void SubtractDeath()
        {
            // If there isn't a current playthrough then we can't update the deaths for it
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Decrement the playthrough deaths and update the current death count
            Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths--;
            CurrentGameDeaths = Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths;
            Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentPlaythrough, CurrentGameDeaths);

            // If there's a current boss then also update its count
            //if (CurrentBoss != String.Empty)
            //{
            //    Bosses.Find(b => b.Lookup == CurrentBoss).Deaths--;
            //    CurrentBossDeaths = Bosses.Find(b => b.Lookup == CurrentBoss).Deaths;
            //    SaveBosses();

            //    Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentBoss, CurrentBossDeaths);
            //}

            // Save data and update death counts
            SavePlaythroughs();
            SaveDeaths();
        }
        public bool CheckCurrentPlaythrough()
        {
            // Checks if there is a current playthrough
            if (CurrentPlaythrough == String.Empty || Playthroughs.Find(p => p.Lookup == CurrentPlaythrough) == null)
            {
                Program.WriteLine(ConsoleColor.Red, "No current game set");

                return false;
            }
            else
            {
                return true;
            }
        }
        public bool CheckCurrentBoss()
        {
            // Checks if there is a current playthrough
            //if (CurrentBoss == String.Empty || Bosses.Find(b=>b.Lookup == CurrentBoss) == null)
            //{
            //    Program.WriteLine(ConsoleColor.Green, "No current boss set");

            //    return false;
            //}
            //else
            {
                return true;
            }
        }
    }
}
