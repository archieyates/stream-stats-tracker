using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Xml;

namespace StatTracker
{
    class Program
    {
        public static Settings Settings = new Settings();

        static void Main(string[] args)
        {
            Console.Title = "Stream Stat Tracker";

            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0} v{1}", appName, version);

            // Check for a version update
            VersionCheck();

            // Check for any missing data
            CheckMissingDirectories();

            // Load Settings
            LoadSettings();

            Console.ResetColor();
            // The reader is what handles the input
            Reader reader = new Reader();
            reader.Run();
        }
        private static void CheckMissingDirectories()
        {
            // Ensure that the required directories always exist
            List<Tuple<string, string>> dirs = new List<Tuple<string, string>>()
            {
                Tuple.Create("Stats", System.AppDomain.CurrentDomain.BaseDirectory + "Stats"),
                Tuple.Create("Deaths", System.AppDomain.CurrentDomain.BaseDirectory + "Stats\\Deaths"),
                Tuple.Create("Playthroughs", System.AppDomain.CurrentDomain.BaseDirectory + "Stats\\Playthroughs")
            };

            foreach(var dir in dirs)
            {
                if (!Directory.Exists(dir.Item2))
                {
                    Directory.CreateDirectory(dir.Item2);
                    Program.WriteLine(ConsoleColor.Red, "Creating Directory {0}", dir.Item1);
                }
            }
        }
        private static void VersionCheck()
        {
            // Grab the project info from the repo
            String projData = String.Empty;
            using (HttpClient webClient = new HttpClient())
            {
                var webRequest = new HttpRequestMessage(HttpMethod.Get, "https://raw.githubusercontent.com/archieyates/stream-stats-tracker/main/StatTracker/src/StatTracker/StatTracker.csproj");
                var response = webClient.Send(webRequest);
                using var webReader = new StreamReader(response.Content.ReadAsStream());
                projData = webReader.ReadToEnd();
            }

            // Parse this data to find the version
            XmlDocument xmlDoc = new XmlDocument();
            string latestVersionString = String.Empty;
            using (StringReader xmlReader = new StringReader(projData))
            {
                xmlDoc.Load(xmlReader);

                XmlNode node = xmlDoc.DocumentElement.FirstChild;
                latestVersionString = node.SelectSingleNode("AssemblyVersion").InnerText;
            }

            // Compare the app with the latest version
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var latestVersion = Version.Parse(latestVersionString);

            if(appVersion < latestVersion)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("There is a newer version of Stream Stat Tracker available at https://github.com/archieyates/stream-stats-tracker/releases");
            }
        }
        private static void LoadSettings()
        {
            string fileName = "Stats\\Settings.json";

            // Create the file if it doesn't exist
            if (!File.Exists(fileName))
            {
                Program.WriteLine(ConsoleColor.Yellow, "Creating Settings.json");
                // Create an empty entry so the basic JSON structure is created correctly
                Settings newData = new Settings();
                string json = JsonSerializer.Serialize<Settings>(newData, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                File.WriteAllText(fileName, json);
            }

            // Deserialize the file
            string jsonString = System.IO.File.ReadAllText(fileName);
            Settings = JsonSerializer.Deserialize<Settings>(jsonString);
            
        }
        public static void SaveSettings()
        {
            // Save out the data
            string fileName = "Settings.json";
            string json = JsonSerializer.Serialize<Settings>(Settings, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            File.WriteAllText(fileName, json);
        }
        public static void WriteLine(ConsoleColor Colour, string Input, params object[] args)
        {
            Console.ForegroundColor = Colour;
            string output = Input;

            // If we are using timestamps then prefix our output string with them
            if(Settings != null)
            {
                if(Settings.UseTimeStamps)
                {
                    output = "[" + DateTime.Now.ToString() + "] " + output;
                }
            }

            Console.WriteLine(output, args);
        }
        public static void Write(ConsoleColor Colour, string Input, params object[] args)
        {
            Console.ForegroundColor = Colour;
            string output = Input;

            // If we are using timestamps then prefix our output string with them
            if (Settings != null)
            {
                if (Settings.UseTimeStamps)
                {
                    output = "[" + DateTime.Now.ToString() + "] " + output;
                }
            }

            Console.Write(output, args);
        }

    }
}
