using UnityEngine;
using Alkuul.Core;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    /// <summary>
    /// 하루 정산 + 프로토타입 전체 누적 기록
    /// - DayStarted 때 하루 카운터만 리셋
    /// - CustomerResult 기록 시 하루 기록 + 전체 기록 둘 다 누적
    /// - 2일차 종료 후 전체 평균 만족도 계산에 사용
    /// </summary>
    public sealed class DailyLedgerSystem : MonoBehaviour
    {
        [SerializeField] private EconomySystem economy;
        [SerializeField] private RepSystem rep;

        private int dayStartMoney;
        private float dayStartRep;

        // day-only
        public int ServedCustomers { get; private set; }
        public int ServedDrinks { get; private set; }
        public int SleptCustomers { get; private set; }

        // prototype-total
        public int TotalCustomersOverall { get; private set; }
        public int TotalDrinksOverall { get; private set; }
        public float TotalAverageSatisfactionSum { get; private set; }

        public int IncomeDelta => (economy != null ? economy.money : 0) - dayStartMoney;
        public float RepDelta => (rep != null ? rep.reputation : 2.5f) - dayStartRep;

        public float PrototypeAverageSatisfaction
            => TotalCustomersOverall > 0 ? TotalAverageSatisfactionSum / TotalCustomersOverall : 0f;

        private void OnEnable()
        {
            if (economy == null) economy = FindObjectOfType<EconomySystem>(true);
            if (rep == null) rep = FindObjectOfType<RepSystem>(true);

            EventBus.OnDayStarted += HandleDayStarted;
            EventBus.OnDayEnded += HandleDayEnded;
        }

        private void OnDisable()
        {
            EventBus.OnDayStarted -= HandleDayStarted;
            EventBus.OnDayEnded -= HandleDayEnded;
        }

        private void HandleDayStarted()
        {
            dayStartMoney = economy != null ? economy.money : 0;
            dayStartRep = rep != null ? rep.reputation : 2.5f;

            // 하루 기록만 리셋
            ServedCustomers = 0;
            ServedDrinks = 0;
            SleptCustomers = 0;

            Debug.Log("[Ledger] DayStarted snapshot + day reset.");
        }

        private void HandleDayEnded()
        {
            Debug.Log(
                $"[Ledger] DayEnded incomeDelta={IncomeDelta} repDelta={RepDelta:+0.00;-0.00;0.00} " +
                $"dayCustomers={ServedCustomers} dayDrinks={ServedDrinks} slept={SleptCustomers} " +
                $"totalCustomers={TotalCustomersOverall} prototypeAvg={PrototypeAverageSatisfaction:F1}"
            );
        }

        public void RecordCustomer(CustomerResult cr)
        {
            // 하루 기록
            ServedCustomers++;
            ServedDrinks += (cr.drinkResults != null ? cr.drinkResults.Count : 0);

            // 전체 기록
            TotalCustomersOverall++;
            TotalDrinksOverall += (cr.drinkResults != null ? cr.drinkResults.Count : 0);
            TotalAverageSatisfactionSum += cr.averageSatisfaction;
        }

        public void RecordSleepSuccess()
        {
            SleptCustomers++;
        }

        public void ResetPrototypeTotals()
        {
            TotalCustomersOverall = 0;
            TotalDrinksOverall = 0;
            TotalAverageSatisfactionSum = 0f;
        }
    }
}