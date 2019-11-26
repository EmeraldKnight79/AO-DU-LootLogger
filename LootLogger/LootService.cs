using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LootLogger.Model;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Diagnostics;

namespace LootLogger
{
    public class LootService : ILootService
    {
        private const string url = "https://discordapp.com/api/webhooks/638028770938322944/d7BOBFRCcLVbYFOvSGX_raNDjYQUCQ6Jz2_6wKFSv3ARy4jpFk9AXLELn8J7utGkBA2G";

        private List<Player> players;
        private readonly HttpClient client;

        private DateTime lastUploadDate = DateTime.MinValue;

        public LootService()
        {
            players = new List<Player>();
            client = new HttpClient();
        }

        public void AddLootForPlayer(Loot loot, string playerName)
        {
            var existingPlayer = this.players.FirstOrDefault(p => p.Name == playerName);
            if (existingPlayer != null)
            {
                existingPlayer.Loots.Add(loot);
            }
            else
            {
                players.Add(new Player { Name = playerName, Loots = new List<Loot>() { loot } });
            }
        }

        public void SaveLootsToFile()
        {
            string content = JsonConvert.SerializeObject(this.players, Formatting.Indented);
            using (var fs = File.Create(Path.Combine(Directory.GetCurrentDirectory(), $"CombatLoots-{DateTime.Now.ToString("dd-MMM-HH-mm-ss")}.txt")))
            {
                Byte[] bytes = new UTF8Encoding(true).GetBytes(content);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public async void UploadLoots()
        {
            try
            {
                if (lastUploadDate.AddMinutes(1) > DateTime.UtcNow)
                {
                    Console.WriteLine("No more than 1 upload every 60 seconds");
                    return;
                }

                string content = string.Empty;

                this.players.SelectMany(p => p.Loots).OrderBy(l => l.PickupTime)
                                                     .ToList()
                                                     .ForEach(loot => content += loot.GetLine());

                using (var fs = File.Create(Path.Combine(Directory.GetCurrentDirectory(), $"Upload-{DateTime.UtcNow.ToString("dd-MMM-HH-mm-ss")}.txt")))
                {
                    Byte[] bytes = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(bytes, 0, bytes.Length);
                }

                if (content.Length < 1)
                {
                    Console.WriteLine("Nothing to post");
                    return;
                }

                content = "{\"content\":\"" + content + "\"}";
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Successfully uploaded logs");
                    this.lastUploadDate = DateTime.UtcNow;
                    string url = await response.Content.ReadAsStringAsync();
                    Process.Start(url);
                }
                else
                {
                    Console.WriteLine("Failed to upload logs");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.StackTrace);
            }
        }
    }
}
