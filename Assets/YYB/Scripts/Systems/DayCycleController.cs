using Alkuul.Domain;
using UnityEngine;
using Alkuul.Core;

namespace Alkuul.Systems
{
    public sealed class DayCycleController : MonoBehaviour
    {
        public int currentDay = 1;

        [Header("Systems")]
        [SerializeField] private RepSystem rep;
        [SerializeField] private EconomySystem economy;
        [SerializeField] private DailyLedgerSystem ledger;
        [SerializeField] private PendingInnDecisionSystem innDecision;

        private bool _advanceDayOnNextStart = false;

        private void Awake()
        {
            if (rep == null) rep = FindObjectOfType<RepSystem>(true);
            if (economy == null) economy = FindObjectOfType<EconomySystem>(true);
            if (ledger == null) ledger = FindObjectOfType<DailyLedgerSystem>(true);
            if (innDecision == null) innDecision = FindObjectOfType<PendingInnDecisionSystem>(true);
        }

        public void StartDay()
        {
            // "정산"에서 EndDay가 찍히고, 다음 날은 StartDay 누를 때 day++ 되게
            if (_advanceDayOnNextStart)
            {
                currentDay++;
                _advanceDayOnNextStart = false;
            }

            Debug.Log($"[DayCycle] Day {currentDay} 시작");
            EventBus.RaiseDayStarted();
        }

        public void EndDayPublic()
        {
            Debug.Log($"[DayCycle] Day {currentDay} 종료");
            if (economy != null) economy.ApplyPendingIncome();
            _advanceDayOnNextStart = true;
            EventBus.RaiseDayEnded();
        }

        public void OnCustomerFinished(CustomerResult cr)
        {
            if (rep != null) rep.Apply(cr);
            if (economy != null) economy.Apply(cr);

            // 정산용 기록
            if (ledger != null) ledger.RecordCustomer(cr);

            // 숙박 여부는 "결정"으로 넘김(자동 숙박 X)
            if (innDecision != null)
                innDecision.Enqueue(cr);
        }
    }
}
