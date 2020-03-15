using System.Collections.Generic;

public static class LootTableData
{
    // Drop item list
    public static List<Items<string>> dropItemList = new List<Items<string>>
    {
        new Items<string> {Probabilty = PropabilityData.itemDrop, Item = StringData.item},
        new Items<string> {Probabilty = PropabilityData.healthDrop, Item = StringData.health},
        new Items<string> {Probabilty = PropabilityData.goldDrop, Item = StringData.gold},
    };

    // Boss drop item list
    public static List<Items<string>> BossDrops = new List<Items<string>>
    {
        new Items<string> {Probabilty = PropabilityData.statStickDrop, Item = StringData.statStick},
        new Items<string> {Probabilty = PropabilityData.speedItemDrop, Item = StringData.speedItem},
        new Items<string> {Probabilty = PropabilityData.damageItemDrop, Item = StringData.damageItem},
        new Items<string> {Probabilty = PropabilityData.jumpBoostItemDrop, Item = StringData.jumpBoostItem},
    };
}

public class Items<T>
{
    public double Probabilty { get; set; }
    public T Item { get; set; }
}