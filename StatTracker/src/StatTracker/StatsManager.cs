using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatTracker
{
    class StatsManager
    {
        // Data for all playthroughs 
        public List<Playthrough> Playthroughs = new List<Playthrough>();
        // Specific Boss data for the current playthrough
        public List<Boss> Bosses = new List<Boss>();
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
        private PlaythroughContainer BuildPlaythroughData()
        {
            // Build a data container that can be serialised out to JSON
            PlaythroughContainer dataContainer = new PlaythroughContainer();
            dataContainer.Playthroughs = Playthroughs.ToArray();
            return dataContainer;
        }
        private BossContainer BuildBossData()
        {
            // Build a data container that can be serialized out to JSON
            BossContainer dataContainer = new BossContainer();
            dataContainer.Bosses = Bosses.ToArray();
            return dataContainer;
        }
        private void SetFromData(PlaythroughContainer Container)
        {
            // Deserialize all the data from the data container
            int index = 0;
            foreach (Playthrough playthrough in Container.Playthroughs)
            {
                Playthrough newPlaythrough = playthrough;

                // Set the index of the current playthrough
                if (newPlaythrough.Status == "Current")
                {
                    CurrentPlaythrough = newPlaythrough.Lookup;
                    CurrentGameDeaths = newPlaythrough.Deaths;
                }

                Playthroughs.Add(newPlaythrough);
                ++index;
            }
        }
        private void SetFromData(BossContainer Container)
        {
            // Deserialize all the data from the data container
            foreach (Boss boss in Container.Bosses)
            {
                Boss newBoss = boss;
                // Set the index of the current boss
                if (newBoss.Status == "Current")
                {
                    CurrentBoss = newBoss.Lookup;
                    CurrentBossDeaths = newBoss.Deaths;
                }

                Bosses.Add(newBoss);
            }
        }
        private void LoadPlaythroughs()
        {
            Playthroughs.Clear();
            string fileName = "Stats\\Playthroughs.json";

            // Create the file if it doesn't exist
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Creating Playthroughs.json");
                // Create an empty entry so the basic JSON structure is created correctly
                PlaythroughContainer newData = new PlaythroughContainer();
                newData.Playthroughs = new Playthrough[0];
                string json = JsonSerializer.Serialize<PlaythroughContainer>(newData, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                File.WriteAllText(fileName, json);
            }

            // Deserialize the file
            string jsonString = System.IO.File.ReadAllText(fileName);
            PlaythroughContainer loadedPlaythroughs = JsonSerializer.Deserialize<PlaythroughContainer>(jsonString);
            SetFromData(loadedPlaythroughs);
            // Load the boss data for the current playthrough
            LoadBosses();
            // Update the death files
            SaveDeaths();
        }
        private void SavePlaythroughs()
        {
            string fileName = "Stats\\Playthroughs.json";

            // Serialize out the playthrough data
            PlaythroughContainer dataContainer = BuildPlaythroughData();
            string json = JsonSerializer.Serialize<PlaythroughContainer>(dataContainer, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            File.WriteAllText(fileName, json);
        }
        private void LoadBosses()
        {
            Bosses.Clear();
            // Only try and load a boss file if there is a current playthrough
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // The boss file name is based on the lookup name of the current playthrough
            string fileName = String.Format("Stats\\Bosses\\{0}.json", CurrentPlaythrough);
            // If the file doesn't exist then make one
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Creating {0}.json", CurrentPlaythrough);
                // Create an empty entry so the basic JSON structure is created correctly
                BossContainer newData = new BossContainer();
                newData.Bosses = new Boss[0];
                string json = JsonSerializer.Serialize<BossContainer>(newData, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                File.WriteAllText(fileName, json);
            }

            // Deserialize the file
            string jsonString = System.IO.File.ReadAllText(fileName);
            BossContainer loadedBosses = JsonSerializer.Deserialize<BossContainer>(jsonString);
            SetFromData(loadedBosses);
        }
        private void SaveBosses()
        {
            // If there isn't a current playthrough there won't be boss data
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // The boss file name is based on the lookup name of the current playthrough
            string fileName = String.Format("Stats\\Bosses\\{0}.json", CurrentPlaythrough);

            // Serialize out the boss data
            BossContainer dataContainer = BuildBossData();
            string json = JsonSerializer.Serialize<BossContainer>(dataContainer, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Added New Playthrough \"{0}\" with Lookup \"{1}\"", Name, Lookup);

            // If there isn't a current playthrough then mark this one as current
            if (CurrentPlaythrough == String.Empty)
            {
                SetCurrentPlaythrough(newPlaythrough.Lookup);
            }
        }
        public void SetCurrentPlaythrough(string Game)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            // If there already is a current playthrough then mark it as in-progress
            if (CurrentPlaythrough != String.Empty)
            {
                Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Status = "In_Progress";
                Console.WriteLine("{0} set to \"In-Progress\"", CurrentPlaythrough);
            }

            // Set current playthrough
            if (Playthroughs.Find(p => p.Lookup == Game) != null)
            {
                // Mark the current playthrough
                Playthroughs.Find(p => p.Lookup == Game).Status = "Current";
                CurrentPlaythrough = Game;
                CurrentGameDeaths = Playthroughs.Find(p => p.Lookup == Game).Deaths;
                // Load the boss file for this playthrough
                LoadBosses();
                Console.WriteLine("{0} set to \"Current\"", Game);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Lookup not recognised");
            }

            // Save out the data and death info
            SavePlaythroughs();
            SaveDeaths();
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
            Bosses.Clear();

            // Save data
            SavePlaythroughs();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} set to \"Complete\"", tempCurrentPlaythrough);
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} session count set to {1}", CurrentPlaythrough, Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Sessions);
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

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("{0} deleted", Lookup);

                // Delete the associated boss file
                string fileName = String.Format("Stats\\Bosses\\{0}.json", Lookup);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    Console.WriteLine("{0} deleted", fileName);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} does not exist", Lookup);
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
            Bosses.Add(newBoss);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Added New Boss \"{0}\" with Lookup \"{1}\"", Name, Lookup);

            // If a new boss is being added there's an assumption it is the current one being fought
            SetCurrentBoss(Lookup);

            // Save the data and update death count
            SaveBosses();
            SaveDeaths();
        }
        public void SetCurrentBoss(string NewBoss)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            // If there already is a current boss then set it to undefeated
            if (CurrentBoss != String.Empty)
            {
                Bosses.Find(b => b.Lookup == CurrentBoss).Status = "Undefeated";
                Console.WriteLine("{0} set to \"Undefeated\"", CurrentBoss);
            }

            // Set current boss
            if (Bosses.Find(b => b.Lookup == NewBoss) != null)
            {
                Bosses.Find(b => b.Lookup == NewBoss).Status = "Current";
                CurrentBoss = NewBoss;
                CurrentBossDeaths = Bosses.Find(b => b.Lookup == CurrentBoss).Deaths;
                Console.WriteLine("{0} set to \"Current\"", CurrentBoss);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Lookup not recognised");
            }

            // Save data and update death count
            SaveBosses();
            SaveDeaths();
        }
        public void DefeatCurrentBoss()
        {
            // If there isn't a current boss then it can't be updated
            if (!CheckCurrentBoss())
            {
                return;
            }

            // Mark boss as defeated
            Bosses.Find(b => b.Lookup == CurrentBoss).Status = "Defeated";

            // Reset current data
            string tempCurrentBoss = CurrentBoss;
            CurrentBoss = String.Empty;
            CurrentBossDeaths = 0;

            // Save data and update death count
            SaveBosses();
            SaveDeaths();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} set to \"Defeated\"", tempCurrentBoss);
        }
        public void DeleteBoss(string Lookup)
        {
            // Search for the boss
            Boss toDelete = Bosses.Find(b => b.Lookup == Lookup);
            if (toDelete != null)
            {
                // If the boss is our current one then we need to reset current data
                if (toDelete.Status == "Current")
                {
                    CurrentBoss = String.Empty;
                    CurrentBossDeaths = 0;
                }

                // If a boss is being deleted then we need to remove its deaths from the count
                Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths -= toDelete.Deaths;
                CurrentGameDeaths = Playthroughs.Find(p => p.Lookup == CurrentPlaythrough).Deaths;

                // Remove the boss
                Bosses.Remove(toDelete);

                // Save data and update death count
                SaveBosses();
                SavePlaythroughs();
                SaveDeaths();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("{0} deleted", Lookup);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} does not exist", Lookup);
            }
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Deaths for {0} updated to {1}", CurrentPlaythrough, CurrentGameDeaths);

            // If there's a current boss then also update its count
            if (CurrentBoss != String.Empty)
            {
                Bosses.Find(b => b.Lookup == CurrentBoss).Deaths++;
                CurrentBossDeaths = Bosses.Find(b => b.Lookup == CurrentBoss).Deaths;
                SaveBosses();

                Console.WriteLine("Deaths for {0} updated to {1}", CurrentBoss, CurrentBossDeaths);
            }

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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Deaths for {0} updated to {1}", CurrentPlaythrough, CurrentGameDeaths);

            // If there's a current boss then also update its count
            if (CurrentBoss != String.Empty)
            {
                Bosses.Find(b => b.Lookup == CurrentBoss).Deaths--;
                CurrentBossDeaths = Bosses.Find(b => b.Lookup == CurrentBoss).Deaths;
                SaveBosses();

                Console.WriteLine("Deaths for {0} updated to {1}", CurrentBoss, CurrentBossDeaths);
            }

            // Save data and update death counts
            SavePlaythroughs();
            SaveDeaths();
        }
        private bool CheckCurrentPlaythrough()
        {
            // Checks if there is a current playthrough
            if (CurrentPlaythrough == String.Empty || Playthroughs.Find(p => p.Lookup == CurrentPlaythrough) == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No current game set");

                return false;
            }
            else
            {
                return true;
            }
        }
        private bool CheckCurrentBoss()
        {
            // Checks if there is a current playthrough
            if (CurrentBoss == String.Empty || Bosses.Find(b=>b.Lookup == CurrentBoss) == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No current boss set");

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
