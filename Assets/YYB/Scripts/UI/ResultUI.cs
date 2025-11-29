using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.UI
{
    public class ResultUI : MonoBehaviour
    {
        public void ShowDrinkResult(DrinkResult r)
            => Debug.Log($"Drink ▶ 만족도 {r.satisfaction}% / 팁 {r.tip} / 떠남 {r.customerLeft}");

        public void ShowCustomerResult(CustomerResult c)
            => Debug.Log(
                $"Customer ▶ 평균 {c.averageSatisfaction:F1}% / 평판Δ {c.reputationDelta:+0.00;-0.00} " +
                $"/ 취함 {c.intoxStage}단계({c.intoxPoints}pt) / 여관재움:{c.canSleepAtInn} / 오버:{c.isOver}"
            );
    }
}
