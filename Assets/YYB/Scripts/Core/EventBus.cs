using System;
using Alkuul.Domain;

namespace Alkuul.Core
{
    public static class EventBus
    {
        public static event Action OnDayStarted;
        public static event Action OnDayEnded;

        public static event Action<CustomerResult> OnCustomerFinished;

        public static void RaiseDayStarted() => OnDayStarted?.Invoke();
        public static void RaiseDayEnded() => OnDayEnded?.Invoke();

        public static void RaiseCustomerFinished(CustomerResult cr) => OnCustomerFinished?.Invoke(cr);
    }
}

