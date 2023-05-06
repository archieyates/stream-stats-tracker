using System.Reflection;
using System.Xml;

namespace StatTracker
{
    class Program
    {
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

            Console.ResetColor();
            // The reader is what handles the input
            Reader reader = new Reader();
            reader.Run();
        }

        private static void CheckMissingDirectories()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            // Ensure that the required directories always exist
            List<Tuple<string, string>> dirs = new List<Tuple<string, string>>()
            {
                Tuple.Create("Stats", System.AppDomain.CurrentDomain.BaseDirectory + "Stats"),
                Tuple.Create("Deaths", System.AppDomain.CurrentDomain.BaseDirectory + "Stats\\Deaths"),
                Tuple.Create("Bosses", System.AppDomain.CurrentDomain.BaseDirectory + "Stats\\Bosses")
            };

            foreach(var dir in dirs)
            {
                if (!Directory.Exists(dir.Item2))
                {
                    Directory.CreateDirectory(dir.Item2);
                    Console.WriteLine("Creating Directory {0}", dir.Item1);
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
                Console.WriteLine("There is a newer version of Stream Stat Tracker available at https://raw.githubusercontent.com/archieyates/stream-stats-tracker/Releases/StatTracker");
            }
        }
    }
}
