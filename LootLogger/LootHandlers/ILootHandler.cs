using LootLogger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootLogger.LootHandlers
{
    public interface ILootHandler
    {
        void AddLoot(Loot loot);
        void Complete();

    }
}
