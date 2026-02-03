using UnityEngine;
using Alkuul.Systems;

public class InnUpgradeSystem : MonoBehaviour
{
    [SerializeField, Range(1, 3)] private int level = 1;

    [Header("Costs")]
    [SerializeField] private int costLv2 = 200;
    [SerializeField] private int costLv3 = 500;

    public int Level => level;
    public int MaxGarnishSlots => level; // Lv1=1, Lv2=2, Lv3=3

    public int NextCost =>
        level == 1 ? costLv2 :
        level == 2 ? costLv3 : -1;

    public bool CanUpgrade => level < 3;

    public bool TryUpgrade(EconomySystem economy)
    {
        if (!CanUpgrade) { Debug.Log("[InnUpgrade] max level"); return false; }
        int cost = NextCost;
        if (economy.money < cost)
        {
            Debug.Log($"[InnUpgrade] not enough gold. need={cost} have={economy.money}");
            return false;
        }

        economy.money -= cost;
        level++;
        Debug.Log($"[InnUpgrade] upgraded to Lv{level} (maxGarnish={MaxGarnishSlots})");

        return true;
    }
}
