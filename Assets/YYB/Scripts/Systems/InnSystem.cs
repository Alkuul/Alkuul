using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    public sealed class InnSystem : MonoBehaviour
    {
        [SerializeField] private EconomySystem economy;

        [Tooltip("숙박 1회 기본 수익(오버면 반값)")]
        [SerializeField] private int baseInnReward = 100;

        public int ComputeSleepIncome(CustomerResult cr)
        {
            if (!cr.canSleepAtInn) return 0;

            int amount = baseInnReward;
            if (cr.isOver) amount /= 2;
            return amount;
        }

        public bool Sleep(CustomerResult cr)
        {
            if (!cr.canSleepAtInn) return false;

            int amount = ComputeSleepIncome(cr);
            if (economy != null) economy.AddIncome(amount);

            Debug.Log($"[Inn] Sleep income={amount} stage={cr.intoxStage} over={cr.isOver}");
            return true;
        }
    }
}
