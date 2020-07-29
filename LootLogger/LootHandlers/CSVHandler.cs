using CsvHelper;
using LootLogger.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootLogger.LootHandlers
{
    class CSVHandler : ILootHandler
    {
        private List<Loot> AllLoot = new List<Loot>();
        public void AddLoot(Loot loot)
        {
            AllLoot.Add(loot);
        }

        public void Complete()
        {
            using (var fs = File.Create(Path.Combine(Directory.GetCurrentDirectory(), $"CombatLoots-{DateTime.Now.ToString("dd-MMM-HH-mm-ss")}.csv")))
            {
                using (CsvWriter writer = new CsvWriter(new StreamWriter(fs), CultureInfo.CurrentCulture))
                {
                    // Write header
                    // Timestamp
                    // Player
                    // Looted From
                    // Item
                    // ItemID
                    // Amount
                    writer.WriteField("Timestamp");
                    writer.WriteField("Player");
                    writer.WriteField("Looted From");
                    writer.WriteField("Item");
                    writer.WriteField("ItemId");
                    writer.WriteField("Amount");
                    foreach(var loot in AllLoot.Where(i => !i.IsTrash))
                    {
                        writer.WriteField(loot.PickupTime);
                        writer.WriteField(loot.LooterName);
                        writer.WriteField(loot.BodyName);
                        writer.WriteField(loot.ItemName);
                        writer.WriteField(loot.Id);
                        writer.WriteField(loot.Quantity);
                        writer.NextRecord();
                    }
                }
            }
        }
    }
}
