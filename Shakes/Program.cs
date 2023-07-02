using MySql.Data.MySqlClient;

namespace Shakes_3
{
    class Program
    {
        private static readonly Database Db = new();
        const int version = 1;

        private static void CheckVersion(int version)
        {
            tryAgain:
            int latestVersion;
            try
            {
                Db.Connect();
                using (var command = new MySqlCommand("SELECT client_version FROM game_data", Db.Cnn))
                {
                    latestVersion = (int)command.ExecuteScalar();
                }
            }
            catch (Exception offline)
            {
                Console.WriteLine("Serwery gry są wyłączone!");
                Thread.Sleep(2000);
                goto tryAgain;
            }

            if (latestVersion == version)
            {
                Data.ClientIsUpToDate = true;
            }
            else
            {
                Data.ClientIsUpToDate = false;
            }
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Ładowanie...");
            CheckVersion(version);
            if (Data.ClientIsUpToDate)
            {
                var game = new Game();
                game.MainMenu();
            }
            else
            {
                Console.WriteLine("Klient nieaktualny!");
                Console.WriteLine("Pobierz najnowszą wersje klienta!");
                Console.ReadLine();
            }
        }
    }
}