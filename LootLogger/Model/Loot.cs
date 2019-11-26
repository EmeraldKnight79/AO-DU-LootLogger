using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootLogger.Model
{
    public class Loot
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public DateTime PickupTime { get; set; }

        [JsonIgnore]
        public string BodyName { get; set; }

        [JsonProperty]
        public string ItemName { get; set; }
        [JsonProperty]
        public int Quantity { get; set; }

        [JsonIgnore]
        public string LooterName { get; set; }

        [JsonIgnore]
        public bool IsTrash => Id > 1184 && Id < 1193;

        public string GetLine()
        {
            return String.Join(", ", PickupTime.ToString("HH:mm:ss"), LooterName, Quantity, ItemName, BodyName)+"\\n";
        }
    }
}
