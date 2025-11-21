using Alkuul.Domain;
using System.Collections;
using UnityEngine;

namespace Alkuul.Systems
{
    /// <summary>하루 시작/영업/종료 루프(MVP)</summary>
    public sealed class DayCycleController : MonoBehaviour
    {
        public int currentDay = 1;

        [Header("Systems")]
        [SerializeField] private RepSystem rep;
        [SerializeField] private EconomySystem economy;
        [SerializeField] private InnSystem inn;

        public void StartDay()
        {
            Debug.Log($"Day {currentDay} 시작");
            StartCoroutine(DayRoutine());
        }

        private IEnumerator DayRoutine()
        {
            // TODO: 여기에서 손님 큐/스폰/주문/평가 루프를 실제로 연결
            // 지금은 단순히 1초 뒤에 종료만 시뮬레이션
            yield return new WaitForSeconds(1f);
            EndDay();
        }

        public void OnCustomerFinished(CustomerResult cr)
        {
            // 한 손님 처리 후 Reputation / Economy / InnSystem 반영
            if (rep != null) rep.Apply(cr);
            if (economy != null) economy.Apply(cr);
            if (inn != null) inn.TrySleep(cr);
        }

        private void EndDay()
        {
            Debug.Log($"Day {currentDay} 종료");
            currentDay++;
        }
    }
}
