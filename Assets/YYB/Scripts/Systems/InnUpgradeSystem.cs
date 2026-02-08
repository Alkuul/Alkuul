using UnityEngine;

namespace Alkuul.Systems
{
    /// <summary>
    /// 여관 업그레이드(1~3):
    /// - 레벨에 따라 가니쉬 슬롯(1/2/3) 제공
    /// - TryUpgrade로 골드 소모 후 레벨 증가
    /// </summary>
    public sealed class InnUpgradeSystem : MonoBehaviour
    {
        [SerializeField, Range(1, 3)] private int level = 1;

        [Header("Costs")]
        [SerializeField] private int costLv2 = 200;
        [SerializeField] private int costLv3 = 500;

        public int Level => level;

        // 기획: Lv1=1, Lv2=2, Lv3=3
        public int MaxGarnishSlots => level;

        public bool CanUpgrade => level < 3;

        public int NextCost
        {
            get
            {
                if (level == 1) return costLv2;
                if (level == 2) return costLv3;
                return -1;
            }
        }

        public bool TryUpgrade(EconomySystem economy)
        {
            if (!CanUpgrade)
            {
                Debug.Log("[InnUpgrade] Already max level.");
                return false;
            }

            if (economy == null)
            {
                Debug.LogWarning("[InnUpgrade] EconomySystem is null.");
                return false;
            }

            int cost = NextCost;
            if (economy.money < cost)
            {
                Debug.Log($"[InnUpgrade] Not enough gold. need={cost} have={economy.money}");
                return false;
            }

            economy.money -= cost;
            level = Mathf.Clamp(level + 1, 1, 3);

            Debug.Log($"[InnUpgrade] Upgraded to Lv{level} (MaxGarnishSlots={MaxGarnishSlots})");
            return true;
        }
    }
}

