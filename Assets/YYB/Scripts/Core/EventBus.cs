using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Alkuul.Core
{
    /// <summary>간단 이벤트 브로커</summary>
    public static class EventBus
    {
        public static event Action OnDayStarted;
        public static event Action OnDayEnded;

        public static void RaiseDayStarted() => OnDayStarted?.Invoke();
        public static void RaiseDayEnded() => OnDayEnded?.Invoke();
    }
}
