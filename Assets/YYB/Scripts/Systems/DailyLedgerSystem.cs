using UnityEngine;
using Alkuul.Core;
using Alkuul.Domain;

namespace Alkuul.Systems
{
    /// <summary>
    /// 하루 정산용 누적 기록:
    /// - DayStarted 때 스냅샷(골드/평판) + 카운터 리셋
    /// - CustomerResult 기록으로 제공 잔 수/손님 수 집계
    /// - 숙박 성공 시 SleptCustomers 증가
    /// </summary>
    public sealed class DailyLedgerSystem : MonoBehaviour
    {
        [SerializeField] private EconomySystem economy;
        [SerializeField] private RepSystem rep;

        private int dayStartMoney;
        private float dayStartRep;

        public int ServedCustomers { get; private set; }
        public int ServedDrinks { get; private set; }
        public int SleptCustomers { get; private set; }

        public int IncomeDelta => (economy != null ? economy.money : 0) - dayStartMoney;
        public float RepDelta => (rep != null ? rep.reputation : 2.5f) - dayStartRep;

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

            ServedCustomers = 0;
            ServedDrinks = 0;
            SleptCustomers = 0;

            Debug.Log("[Ledger] DayStarted snapshot + reset.");
        }

        private void HandleDayEnded()
        {
            Debug.Log($"[Ledger] DayEnded incomeDelta={IncomeDelta} repDelta={RepDelta:+0.00;-0.00;0.00} drinks={ServedDrinks} slept={SleptCustomers}");
        }

        public void RecordCustomer(CustomerResult cr)
        {
            ServedCustomers++;
            ServedDrinks += (cr.drinkResults != null ? cr.drinkResults.Count : 0);
        }

        public void RecordSleepSuccess()
        {
            SleptCustomers++;
        }
    }
}
