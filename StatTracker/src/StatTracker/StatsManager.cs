using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StatTracker
{
    class StatsManager
    {
        // Data for all playthroughs 
        public List<Playthrough> Playthroughs = new List<Playthrough>();
        // Current info
        public string CurrentPlaythroughLookup = String.Empty;
        public string CurrentBossLookup = String.Empty;
        // Cache death data
        public int CurrentGameDeaths = 0;
        public int CurrentBossDeaths = 0;
        public int TotalDeaths = 0;

        public StatsManager()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            LoadPlaythroughs();

            // If there aren't any playthroughs check if there is legacy data to import
            if (Playthroughs.Count == 0)
            {
                ConvertLegacyData();
            }
        }
        private void LoadPlaythroughs()
        {
            Playthroughs.Clear();
            string fileName = "Stats\\Playthroughs\\_Playthroughs.txt";

            // Create the file if it doesn't exist
            if (!File.Exists(fileName))
            {
                Program.WriteLine(ConsoleColor.Yellow, "Creating _Playthroughs.txt");
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
                    CurrentPlaythroughLookup = playthrough.Lookup;
                    CurrentGameDeaths = playthrough.Deaths;

                    Boss currentBoss = playthrough.Bosses.Find(b => b.Status == "Current");
                    if (currentBoss != null)
                    {
                        CurrentBossLookup = currentBoss.Lookup;
                        CurrentBossDeaths = currentBoss.Deaths;
                    }
                }
            }

            // Update the death files
            SaveDeaths();
        }
        public void ConvertLegacyData()
        {
            Playthroughs.Clear();

            // Look for the old playthroughs file
            string playthroughFile = "Stats\\Playthroughs.json";
            if (File.Exists(playthroughFile))
            {
                Program.WriteLine(ConsoleColor.Yellow, "Converting Playthroughs.JSON");

                // Go through all the legacy playthroughs
                string jsonString = System.IO.File.ReadAllText(playthroughFile);
                LegacyPlaythroughContainer playthroughs = JsonSerializer.Deserialize<LegacyPlaythroughContainer>(jsonString);
                foreach (Playthrough playthrough in playthroughs.Playthroughs)
                {
                    // Add to the playthroughs
                    Program.WriteLine(ConsoleColor.Green, "Converting {0}", playthrough.Lookup);
                    Playthroughs.Add(playthrough);

                    // See if there is boss data associated with the playthrough
                    string bossesFile = String.Format("Stats\\Bosses\\{0}.json", playthrough.Lookup);
                    if (File.Exists(bossesFile))
                    {
                        // Get all the legacy bosses
                        jsonString = System.IO.File.ReadAllText(bossesFile);
                        LegacyBossContainer bosses = JsonSerializer.Deserialize<LegacyBossContainer>(jsonString);
                        if (bosses.Bosses != null)
                        {
                            // Go through all the legacy bosses
                            foreach (Boss boss in bosses.Bosses)
                            {
                                Program.WriteLine(ConsoleColor.Blue, "\tConverting {0}", boss.Lookup);
                                // Add the boss to the playthrough
                                Playthroughs.Find(p => p.Lookup == playthrough.Lookup).Bosses.Add(boss);
                            }
                        }
                    }

                    // Set current playthrough
                    if (playthrough.Status == "Current")
                    {
                        CurrentPlaythroughLookup = playthrough.Lookup;
                        CurrentGameDeaths = playthrough.Deaths;
                    }

                    // Set current boss
                    Boss currentBoss = playthrough.Bosses.Find(b => b.Status == "Current");
                    if (currentBoss != null)
                    {
                        CurrentBossLookup = currentBoss.Lookup;
                        CurrentBossDeaths = currentBoss.Deaths;
                    }
                }

                // Save all the playthrough file
                SavePlaythroughs();
                SaveDeaths();

                // Delete all the old data (if the user wants to)
                Program.WriteLine(ConsoleColor.Yellow, "Data converted. It is now safe to delete Playthroughs.json and the Bosses folder");
                Program.Write(ConsoleColor.White, "Would you like to do this now? (y/n): ");
                string answer = Console.ReadLine().ToLower();
                if (answer == "y")
                {
                    File.Delete(playthroughFile);
                    Program.WriteLine(ConsoleColor.Yellow, "Deleted Playthroughs.json");
                    Directory.Delete("Stats\\Bosses", true);
                    Program.WriteLine(ConsoleColor.Yellow, "Deleted Bosses folder");
                }
            }
        }
        private void SavePlaythroughs()
        {
            // Save all playthroughs
            foreach (Playthrough playthrough in Playthroughs)
            {
                string playthroughFile = String.Format("Stats\\Playthroughs\\{0}.json", playthrough.Lookup);
                string json = JsonSerializer.Serialize<Playthrough>(playthrough, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                File.WriteAllText(playthroughFile, json);
            }

            // Save out the list data
            SaveList();
        }
        private void SaveCurrentPlaythrough()
        {
            // Find and save the current playthrough
            Playthrough current = Playthroughs.Find(p => p.Lookup == CurrentPlaythroughLookup);
            if (current != null)
            {
                string playthroughFile = String.Format("Stats\\Playthroughs\\{0}.json", current.Lookup);
                string json = JsonSerializer.Serialize<Playthrough>(current, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                File.WriteAllText(playthroughFile, json);
            }
        }
        private void SavePlaythrough(String Lookup)
        {
            // Save a specific playthrough
            Playthrough playthrough = Playthroughs.Find(p => p.Lookup == Lookup);
            if (playthrough != null)
            {
                string playthroughFile = String.Format("Stats\\Playthroughs\\{0}.json", playthrough.Lookup);
                string json = JsonSerializer.Serialize<Playthrough>(playthrough, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                File.WriteAllText(playthroughFile, json);
            }
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
        private void SaveList()
        {
            string fileName = "Stats\\Playthroughs\\_Playthroughs.txt";

            // Create the file if it doesn't exist
            if (!File.Exists(fileName))
            {
                Program.WriteLine(ConsoleColor.Yellow, "Creating _Playthroughs.txt");
                File.Create(fileName).Dispose();
            }

            string list = String.Empty;
            foreach (Playthrough playthrough in Playthroughs)
            {
                list += String.Format("{0}\n", playthrough.Lookup);
            }
            File.WriteAllText(fileName, list);
        }
        public void AddNewPlaythrough(string Lookup, string Name)
        {
            // Create new playthrough entry
            Playthrough newPlaythrough = new Playthrough();
            newPlaythrough.Lookup = Lookup;
            newPlaythrough.Game = Name;
            Playthroughs.Add(newPlaythrough);

            // Save out the playthrough file
            SavePlaythrough(Lookup);
            Program.WriteLine(ConsoleColor.Green, "Added New Playthrough \"{0}\" with Lookup \"{1}\"", Name, Lookup);

            // Set as current playthrough
            SetCurrentPlaythrough(newPlaythrough.Lookup);

            // Check if there is a saved list of bosses and import if so
            string bossesFile = String.Format("Stats\\bosses.txt", Lookup);
            if (File.Exists(bossesFile))
            {
                Program.WriteLine(ConsoleColor.Yellow, "Found bosses.txt. Would you like to import these to this playthrough?");
                Program.Write(ConsoleColor.White, "Would you like to do this now? (y/n): ");
                string answer = Console.ReadLine().ToLower();
                if (answer == "y")
                {
                    // Each line is a boss name and an optional lookup
                    var lines = File.ReadLines(bossesFile);
                    foreach (var line in lines)
                    {
                        // Boss data
                        string boss = String.Empty;
                        string lookup = String.Empty;

                        // Lookup separator
                        string[] subs = line.Split("@");

                        // If there is a lookup use it otherwise generate it
                        if (subs.Count() == 1)
                        {
                            boss = subs[0];
                            lookup = GenerateBossLookup(boss);
                        }
                        else if (subs.Count() > 1)
                        {
                            boss = subs[0];
                            lookup = subs[1];
                        }

                        AddNewBoss(lookup, boss, false);
                    }

                    // If a new boss was added then unset it as this was the start of the playthrough
                    SetCurrentBoss(String.Empty);

                    // Delete the text file (if the user wants to)
                    Program.Write(ConsoleColor.White, "Bosses imported. Delete bosses.txt? (y/n): ");
                    answer = Console.ReadLine().ToLower();
                    if (answer == "y")
                    {
                        File.Delete(bossesFile);
                        Program.WriteLine(ConsoleColor.Yellow, "Deleted bosses.txt");
                    }
                }
            }

            // Save out the list data
            SaveList();
        }
        public void SetCurrentPlaythrough(string Lookup)
        {
            // If there already is a current playthrough then mark it as in-progress
            if (CurrentPlaythroughLookup != String.Empty)
            {
                Playthroughs.Find(p => p.Lookup == CurrentPlaythroughLookup).Status = "In_Progress";
                SavePlaythrough(CurrentPlaythroughLookup);
                Program.WriteLine(ConsoleColor.Green, "{0} set to \"In-Progress\"", CurrentPlaythroughLookup);
            }

            // Set current playthrough
            Playthrough newCurrent = Playthroughs.Find(p => p.Lookup == Lookup);
            if (newCurrent != null)
            {
                // Mark the current playthrough
                newCurrent.Status = "Current";
                CurrentPlaythroughLookup = Lookup;
                CurrentGameDeaths = newCurrent.Deaths;
                Program.WriteLine(ConsoleColor.Green, "{0} set to \"Current\"", Lookup);

                // Set the current boss
                Boss currentBoss = newCurrent.Bosses.Find(b => b.Status == "Current");
                if (currentBoss != null)
                {
                    CurrentBossLookup = currentBoss.Lookup;
                    CurrentBossDeaths = currentBoss.Deaths;
                }
                else
                {
                    CurrentBossLookup = String.Empty;
                    CurrentBossDeaths = 0;
                }
            }
            else
            {
                Program.WriteLine(ConsoleColor.Red, "Lookup not recognised");
            }

            // Save out the data
            SaveCurrentPlaythrough();
            SaveDeaths();
            SaveList();
        }
        public Playthrough GetCurrentPlaythrough()
        {
            return Playthroughs.Find(p => p.Lookup == CurrentPlaythroughLookup);
        }
        public Boss GetCurrentBoss()
        {
            Playthrough currentPlaythrough = GetCurrentPlaythrough();
            if (currentPlaythrough != null)
            {
                return currentPlaythrough.Bosses.Find(b => b.Lookup == CurrentBossLookup);
            }
            return null;
        }
        public Boss GetBoss(String Lookup)
        {
            Playthrough currentPlaythrough = GetCurrentPlaythrough();
            if (currentPlaythrough != null)
            {
                return currentPlaythrough.Bosses.Find(b => b.Lookup == Lookup);
            }
            return null;
        }
        public void CompleteCurrentPlaythrough()
        {
            // If there isn't a current playthrough then it can't be updated
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Mark current playthrough as complete
            GetCurrentPlaythrough().Status = "Complete";

            // Save data (doing it here because we then go on to reset)
            SaveCurrentPlaythrough();

            // Reset our current data
            string cachedCurrent = CurrentPlaythroughLookup;
            CurrentPlaythroughLookup = String.Empty;
            CurrentBossLookup = String.Empty;

            Program.WriteLine(ConsoleColor.Green, "{0} set to \"Complete\"", cachedCurrent);
        }
        public void IncrementCurrentPlaythroughSessions()
        {
            // If there isn't a current playthrough then it can't be updated
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Update the session count
            GetCurrentPlaythrough().Sessions++;

            // Save data
            SaveCurrentPlaythrough();

            Program.WriteLine(ConsoleColor.Green, "{0} session count set to {1}", CurrentPlaythroughLookup, GetCurrentPlaythrough().Sessions);
        }
        public void SetCurrentPlaythroughSessions(int Count)
        {
            // If there isn't a current playthrough then it can't be updated
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Update the session count
            GetCurrentPlaythrough().Sessions = Count;

            // Save data
            SaveCurrentPlaythrough();

            Program.WriteLine(ConsoleColor.Green, "{0} session count set to {1}", CurrentPlaythroughLookup, GetCurrentPlaythrough().Sessions);
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
                    CurrentPlaythroughLookup = String.Empty;
                    CurrentBossLookup = String.Empty;
                    CurrentGameDeaths = 0;
                    CurrentBossDeaths = 0;
                }

                // Remove the playthrough
                Playthroughs.Remove(toDelete);

                // Delete the associated playthrough file
                string fileName = String.Format("Stats\\Playthroughs\\{0}.json", Lookup);
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    Program.WriteLine(ConsoleColor.Green, "{0} deleted", fileName);
                }

                SaveDeaths();
            }
            else
            {
                Program.WriteLine(ConsoleColor.Red, "{0} does not exist", Lookup);
            }

            // Update the list
            SaveList();
        }
        public void AddNewBoss(string Lookup, string Name, bool SetCurrent = true)
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
            if (SetCurrent)
            {
                SetCurrentBoss(Lookup);
            }

            // Save the data and update death count
            SaveDeaths();
            SaveCurrentPlaythrough();
        }
        public void SetCurrentBoss(string NewBoss)
        {
            // If there isn't a current playthrough then there won't be boss data
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // If there already is a current boss then set it to undefeated
            if (CurrentBossLookup != String.Empty)
            {
                GetCurrentBoss().Status = "Undefeated";
                Program.WriteLine(ConsoleColor.Green, "{0} set to \"Undefeated\"", CurrentBossLookup);
            }

            if (NewBoss != String.Empty)
            {
                // Set current boss
                Playthrough currentPlaythrough = GetCurrentPlaythrough();
                Boss currentBoss = currentPlaythrough.Bosses.Find(b => b.Lookup == NewBoss);
                if (currentBoss != null)
                {
                    currentBoss.Status = "Current";
                    CurrentBossLookup = NewBoss;
                    CurrentBossDeaths = currentBoss.Deaths;
                    Program.WriteLine(ConsoleColor.Green, "{0} set to \"Current\"", CurrentBossLookup);
                }
                else
                {
                    CurrentBossLookup = String.Empty;
                    CurrentBossDeaths = 0;
                    Program.WriteLine(ConsoleColor.Red, "Lookup not recognised");
                }
            }
            else
            {
                CurrentBossLookup = String.Empty;
                CurrentBossDeaths = 0;
                Program.WriteLine(ConsoleColor.Green, "Unset current boss");
            }

            // Save data and update death count
            SaveCurrentPlaythrough();
            SaveDeaths();
        }
        public void DefeatCurrentBoss()
        {
            // If there isn't a current playthrough then there won't be boss data
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Mark boss as defeated
            Boss currentBoss = GetCurrentPlaythrough().Bosses.Find(b => b.Lookup == CurrentBossLookup);
            if (currentBoss != null)
            {
                currentBoss.Status = "Defeated";

                // Reset current data
                string tempCurrentBoss = CurrentBossLookup;
                CurrentBossLookup = String.Empty;
                CurrentBossDeaths = 0;

                // Save data and update death count
                SaveCurrentPlaythrough();
                SaveDeaths();

                Program.WriteLine(ConsoleColor.Green, "{0} set to \"Defeated\"", tempCurrentBoss);
            }
            else
            {
                Program.WriteLine(ConsoleColor.Red, "No Current Boss");
            }
        }
        public void DeleteBoss(string Lookup)
        {
            // If there isn't a current playthrough then there won't be boss data
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Search for the boss
            Boss toDelete = GetCurrentPlaythrough().Bosses.Find(b => b.Lookup == Lookup);
            if (toDelete != null)
            {
                // If the boss is our current one then we need to reset current data
                if (toDelete.Status == "Current")
                {
                    CurrentBossLookup = String.Empty;
                    CurrentBossDeaths = 0;
                }

                // If a boss is being deleted then we need to remove its deaths from the count
                GetCurrentPlaythrough().Deaths -= toDelete.Deaths;
                CurrentGameDeaths = GetCurrentPlaythrough().Deaths;

                // Remove the boss
                GetCurrentPlaythrough().Bosses.Remove(toDelete);

                // Save data and update death count
                SaveCurrentPlaythrough();
                SaveDeaths();

                Program.WriteLine(ConsoleColor.Green, "{0} deleted", Lookup);
            }
            else
            {
                Program.WriteLine(ConsoleColor.Red, "{0} does not exist", Lookup);
            }
        }
        public void RenameBoss(string Lookup, string bossName)
        {
            // If there isn't a current playthrough then there won't be boss data
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            Playthrough currentPlaythrough = GetCurrentPlaythrough();
            Boss boss = currentPlaythrough.Bosses.Find(b => b.Lookup == Lookup);
            if (boss != null)
            {
                Program.WriteLine(ConsoleColor.Green, "Renamed {0} to {1}", boss.Name, bossName);
                boss.Name = bossName;
            }
            else
            {
                Program.WriteLine(ConsoleColor.Red, "Could not find boss {0}", Lookup);
            }


            // Save data and update death counts
            SaveCurrentPlaythrough();
            SaveDeaths();

        }
        public void NextBoss()
        {
            // If there isn't a current playthrough then it can't be updated
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Get the index of the current boss
            int currentIndex = (CurrentBossLookup != String.Empty) ? GetCurrentPlaythrough().Bosses.FindIndex(0, (b => b.Lookup == CurrentBossLookup)) : 0;
            if (currentIndex == -1)
            {
                currentIndex = 0;
            }

            // Try and find the next boss once we know the current one
            for (int index = (currentIndex + 1); index < GetCurrentPlaythrough().Bosses.Count; ++index)
            {
                Boss boss = GetCurrentPlaythrough().Bosses[index];

                if (boss.Status == "Undefeated")
                {
                    SetCurrentBoss(boss.Lookup);
                    return;
                }
            }

            // Loop back around since we only started checking from the current index
            for (int index = 0; index < currentIndex; ++index)
            {
                Boss boss = GetCurrentPlaythrough().Bosses[index];

                if (boss.Status == "Undefeated")
                {
                    SetCurrentBoss(boss.Lookup);
                    return;
                }
            }

            // Couldn't find an undefeated boss
            Program.WriteLine(ConsoleColor.Red, "Could not find an undefeated boss to go to");

        }
        public void PreviousBoss()
        {
            // If there isn't a current playthrough then it can't be updated
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Get the index of the current boss
            int currentIndex = (CurrentBossLookup != String.Empty) ? GetCurrentPlaythrough().Bosses.FindIndex(0, (b => b.Lookup == CurrentBossLookup)) : 0;
            if (currentIndex == -1)
            {
                currentIndex = 0;
            }

            // Try and find the previous boss once we know the current one
            for (int index = (currentIndex - 1); index >= 0; --index)
            {
                Boss boss = GetCurrentPlaythrough().Bosses[index];

                if (boss.Status == "Undefeated")
                {
                    SetCurrentBoss(boss.Lookup);
                    return;
                }
            }

            // Loop back around since we only started checking from the current index
            for (int index = (GetCurrentPlaythrough().Bosses.Count - 1); index > currentIndex; ++index)
            {
                Boss boss = GetCurrentPlaythrough().Bosses[index];

                if (boss.Status == "Undefeated")
                {
                    SetCurrentBoss(boss.Lookup);
                    return;
                }
            }

            // Couldn't find an undefeated boss
            Program.WriteLine(ConsoleColor.Red, "Could not find an undefeated boss to go to");
        }
        public void AddDeath(bool IncludeBoss)
        {
            // If there isn't a current playthrough then we can't update the deaths for it
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Increment the playthrough deaths and update the current death count
            GetCurrentPlaythrough().Deaths++;
            CurrentGameDeaths = GetCurrentPlaythrough().Deaths;
            Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentPlaythroughLookup, CurrentGameDeaths);

            if (IncludeBoss)
            {
                //If there's a current boss then also update its count
                Boss currentBoss = GetCurrentPlaythrough().Bosses.Find(b => b.Lookup == CurrentBossLookup);
                if (currentBoss != null)
                {
                    currentBoss.Deaths++;
                    CurrentBossDeaths = currentBoss.Deaths;

                    Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentBossLookup, CurrentBossDeaths);
                }
            }

            // Save data and update death counts
            SaveCurrentPlaythrough();
            SaveDeaths();
        }
        public void SubtractDeath(bool IncludeBoss)
        {
            // If there isn't a current playthrough then we can't update the deaths for it
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Decrement the playthrough deaths and update the current death count
            GetCurrentPlaythrough().Deaths--;
            CurrentGameDeaths = GetCurrentPlaythrough().Deaths;
            Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentPlaythroughLookup, CurrentGameDeaths);

            if (IncludeBoss)
            {
                // If there's a current boss then also update its count
                Boss currentBoss = GetCurrentPlaythrough().Bosses.Find(b => b.Lookup == CurrentBossLookup);
                if (currentBoss != null)
                {
                    currentBoss.Deaths--;
                    CurrentBossDeaths = currentBoss.Deaths;

                    Program.WriteLine(ConsoleColor.Green, "Deaths for {0} updated to {1}", CurrentBossLookup, CurrentBossDeaths);
                }
            }

            // Save data and update death counts
            SaveCurrentPlaythrough();
            SaveDeaths();
        }
        public bool CheckCurrentPlaythrough()
        {
            // Checks if there is a current playthrough
            if (CurrentPlaythroughLookup == String.Empty || Playthroughs.Find(p => p.Lookup == CurrentPlaythroughLookup) == null)
            {
                Program.WriteLine(ConsoleColor.Red, "No current game set");

                return false;
            }
            else
            {
                return true;
            }
        }
        public string GeneratePlaythroughLookup(string Name)
        {
            string lookup = String.Empty;

            bool lookupInvalid = true;
            string gameNameShortened = Regex.Replace(Name, "[^0-9a-zA-Z]+", "").ToLower();
            string potentialLookup = gameNameShortened;
            int index = 1;

            do
            {
                // If the lookup doesn't exist then bail out
                if (Playthroughs.Find(p => p.Lookup == potentialLookup) == null)
                {
                    lookupInvalid = false;
                }
                else
                {
                    // Otherwise increase the number on the end and try again
                    index++;
                    potentialLookup = gameNameShortened + index.ToString();
                }

            } while (lookupInvalid);

            lookup = potentialLookup;

            return lookup;
        }
        public string GenerateBossLookup(string Name)
        {
            string lookup = String.Empty;

            bool lookupInvalid = true;
            string bossNameShortened = Regex.Replace(Name, "[^0-9a-zA-Z]+", "").ToLower();
            string potentialLookup = bossNameShortened;
            int index = 1;

            do
            {
                // If the lookup doesn't exist then bail out
                if (GetCurrentPlaythrough().Bosses.Find(b => b.Lookup == potentialLookup) == null)
                {
                    lookupInvalid = false;
                }
                else
                {
                    // Otherwise increase the number on the end and try again
                    index++;
                    potentialLookup = bossNameShortened + index.ToString();
                }

            } while (lookupInvalid);

            lookup = potentialLookup;

            return lookup;
        }

        public void SetPlaytime(int Hours, int Minutes)
        {
            // If there isn't a current playthrough then we can't update the deaths for it
            if (!CheckCurrentPlaythrough())
            {
                return;
            }

            // Just in case
            int hours = Hours + (Minutes / 60);
            int minutes = Minutes %= 60;

            GetCurrentPlaythrough().Playtime = $"{hours}h {minutes}m";

            // Save data
            SaveCurrentPlaythrough();

            Program.WriteLine(ConsoleColor.Green, $"{CurrentPlaythroughLookup} Playtime set to {GetCurrentPlaythrough().Playtime}");
        }
    }
}
