using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    public sealed class InnSystem : MonoBehaviour
    {
        [SerializeField] private EconomySystem economy;

        [Tooltip("여관 1회 기본 수익(만취 정상 상태 기준)")]
        [SerializeField] private int baseInnReward = 100;

        /// <summary>
        /// 손님을 재우면: 만취(4단계) = 100%, 오버(5단계↑) = 반값 수익.
        /// canSleepAtInn=false거나 일찍 떠난 손님은 재울 수 없음.
        /// </summary>
        public bool TrySleep(CustomerResult cr)
        {
            if (!cr.canSleepAtInn)
                return false;

            int baseAmount = baseInnReward;

            // 오버 상태면 반값
            if (cr.isOver)
                baseAmount /= 2;

            if (economy != null)
                economy.AddIncome(baseAmount);

            Debug.Log($"[Inn] 손님 재움 ▶ base {baseAmount} (stage {cr.intoxStage}, over={cr.isOver})");

            return true;
        }
    }
}
