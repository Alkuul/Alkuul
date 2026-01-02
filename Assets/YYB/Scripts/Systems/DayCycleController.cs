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
        [SerializeField] private InnSystem inn;

        public void StartDay()
        {
            Debug.Log($"Day {currentDay} 시작");
            EventBus.RaiseDayStarted();
        }

        public void EndDayPublic()
        {
            Debug.Log($"Day {currentDay} 종료");
            currentDay++;
            EventBus.RaiseDayEnded();
        }

        public void OnCustomerFinished(CustomerResult cr)
        {
            if (rep != null) rep.Apply(cr);
            if (economy != null) economy.Apply(cr);
            if (inn != null) inn.TrySleep(cr);
        }
    }
}
