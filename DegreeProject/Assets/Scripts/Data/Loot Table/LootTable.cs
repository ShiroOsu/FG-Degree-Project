using System.Collections.Generic;
using System.Linq;
using System;

public static class LootTable
{
    public static string ItemDrop(bool boss)
    {
        // If enemy that died is not a boss, drop something from normal loot table.
        if (!boss)
        {
            var converted = new List<Items<string>>(LootTableData.dropItemList.Count);
            double sum = 0.0d;

            foreach (var item in LootTableData.dropItemList.Take(LootTableData.dropItemList.Count - 1))
            {
                sum += item.Probabilty;
                converted.Add(new Items<string> { Probabilty = sum, Item = item.Item });
            }
            converted.Add(new Items<string> { Probabilty = 1.0d, Item = LootTableData.dropItemList.Last().Item });

            Random random = new System.Random();

            while (true) // Dangerous while true loop.
            {
                double probability = random.NextDouble();
                var selected = converted.SkipWhile(i => i.Probabilty < probability).First();

                // For printing it will show what "dropped".
                // ($"Selected item = {selected.Item}")

                // When we got an drop, break the loop.
                if (selected.Item != StringData.nullString)
                {
                    return selected.Item; // Return will break the loop.
                }
            }
        }
        else // If enemy that died is a boss, drop something from boss loot table.
        {
            var converted = new List<Items<string>>(LootTableData.BossDrops.Count);
            double sum = 0.0d;

            foreach (var item in LootTableData.BossDrops.Take(LootTableData.BossDrops.Count - 1))
            {
                sum += item.Probabilty;
                converted.Add(new Items<string> { Probabilty = sum, Item = item.Item });
            }
            converted.Add(new Items<string> { Probabilty = 1.0d, Item = LootTableData.BossDrops.Last().Item });

            Random random = new System.Random();

            while (true)
            {
                double probability = random.NextDouble();
                var selected = converted.SkipWhile(i => i.Probabilty < probability).First();

                if (selected.Item != StringData.nullString)
                {
                    return selected.Item;
                }
            }
        }
    }
}