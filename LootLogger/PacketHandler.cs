using LootLogger.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LootLogger
{
    public class PacketHandler
    {
        private ILootService lootService;
        private HttpClient client;
        private const string itemsMappingUrl = "http://albion-items.s3.eu-central-1.amazonaws.com/items.txt";
        private bool isInitialized = false;
        private Dictionary<int, string> itemDictionary = new Dictionary<int, string>();
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public PacketHandler(ILootService lootService)
        {
            this.lootService = lootService;
            this.client = new HttpClient();
        }

        public async void OnEvent(byte code, Dictionary<byte, object> parameters)
        {
            if (!this.isInitialized)
            {
                await this.InitializeAsync();
            }
            if (code == 2)
            {
                return;
            }
            object val;
            parameters.TryGetValue(252, out val);
            if (val == null)
            {
                return;
            }
            int iCode = 0;
            if (!int.TryParse(val.ToString(), out iCode))
            {
                return;
            }
            EventCodes eventCode = (EventCodes)iCode;
            if (eventCode == EventCodes.evOtherGrabbedLoot)
            {
                this.OnLootPicked(parameters);
            }

        }

        public void OnResponse(byte operationCode, short returnCode, Dictionary<byte, object> parameters)
        {
        }

        public void OnRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
            int iCode = 0;
            if (!int.TryParse(parameters[253].ToString(), out iCode))
            {
                return;
            }
        }

        private void OnLootPicked(Dictionary<byte, object> parameters)
        {
            try
            {
                string looter = parameters[2].ToString();
                string quantity = parameters[5].ToString();
                int itemId = int.Parse(parameters[4].ToString());
                string itemName = itemDictionary[itemId];
                string deadPlayer = parameters[1].ToString();

                Loot loot = new Loot
                {
                    Id = itemId,
                    ItemName = itemName,
                    Quantity = int.Parse(quantity.ToString()),
                    PickupTime = DateTime.UtcNow,
                    BodyName = deadPlayer,
                    LooterName = looter
                };

                if (!loot.IsTrash)
                {
                    lootService.AddLootForPlayer(loot, looter);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "logs.txt");
                    string line = $"{looter} has looted {quantity}x {itemName} on {deadPlayer}";
                    Console.WriteLine(line);
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private async Task InitializeAsync()
        {
            semaphore.Wait();
            try
            {
                var reponse = await this.client.GetAsync(new Uri(itemsMappingUrl));
                var content = await reponse.Content.ReadAsStringAsync();
                var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines ?? new string[] { })
                {
                    string[] split = line.Split(new[] { ":" }, StringSplitOptions.None);
                    itemDictionary.Add(int.Parse(split[0]), split[1]);
                }
                this.isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
