using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    public sealed class EconomySystem : MonoBehaviour
    {
        [SerializeField] private RepSystem rep;
        public int money;
        [SerializeField] private int pendingIncome;

        public int PendingIncome => pendingIncome;

        /// <summary>손님 팁 수익 반영 (평판 보정 포함)</summary>
        public void Apply(CustomerResult cr)
        {
            AddIncome(cr.totalTip);
        }

        /// <summary>임의의 수익(팁, 여관 등)에 평판 계수를 곱해서 반영</summary>
        public void AddIncome(int baseAmount)
        {
            if (baseAmount == 0) return;

            float repScore = rep != null ? rep.reputation : 2.5f;
            float mul = IncomeMultiplierByRep(repScore);
            int amount = Mathf.RoundToInt(baseAmount * mul);

            pendingIncome += amount;

            Debug.Log($"[Economy] +{amount} (base {baseAmount}, rep {repScore:0.00}) pending={pendingIncome} money={money}");
        }

        public int ApplyPendingIncome()
        {
            if (pendingIncome == 0) return 0;

            int applied = pendingIncome;
            money += pendingIncome;
            pendingIncome = 0;

            Debug.Log($"[Economy] Applied income +{applied} => money={money}");
            return applied;
        }

        private float IncomeMultiplierByRep(float repScore) =>
            repScore <= 1.0f ? 0.7f :
            repScore <= 2.0f ? 0.9f :
            repScore <= 3.0f ? 1.0f :
            repScore <= 4.0f ? 1.1f :
            repScore <= 4.5f ? 1.25f : 1.5f;
    }
}
