using System.Reflection;

namespace StatTracker
{
    class Program
    {
        static readonly HttpClient WebClient = new HttpClient();

        static void Main(string[] args)
        {
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
            // Check the repo to see if a new version is available
            var webRequest = new HttpRequestMessage(HttpMethod.Get, "https://raw.githubusercontent.com/archieyates/stream-stats-tracker/main/Releases/StatTracker/version.txt");
            var response = WebClient.Send(webRequest);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            String content = reader.ReadToEnd();

            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var latestVersion = Version.Parse(content);

            if(appVersion < latestVersion)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("There is a newer version of Stream Stat Tracker available at https://raw.githubusercontent.com/archieyates/stream-stats-tracker/Releases/StatTracker");
            }
        }
    }
}
