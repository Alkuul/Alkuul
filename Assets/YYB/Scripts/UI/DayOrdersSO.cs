using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain;
using Alkuul.Systems;

namespace Alkuul.UI
{
    [CreateAssetMenu(menuName = "Alkuul/Orders/Day Orders", fileName = "DayOrders_Day1")]
    public class DayOrdersSO : ScriptableObject
    {
        [Min(1)] public int dayNumber = 1;

        [Header("Day Intro Dialogue (optional)")]
        public List<string> dayIntroLines = new();

        [Header("Customers (권장: 최대 3)")]
        public List<CustomerOrdersDefinition> customers = new();

        [System.Serializable]
        public class CustomerOrdersDefinition
        {
            public CustomerProfile profile;

            [Header("Order Slots (권장: 1~3)")]
            public List<OrderSlotAuthoring> slots = new();

            public List<OrderSlotRuntime> BuildRuntime(OrderSystem orderSystem)
            {
                var list = new List<OrderSlotRuntime>();
                if (orderSystem == null) return list;
                if (slots == null) return list;

                foreach (var s in slots)
                {
                    if (s == null) continue;

                    var order = orderSystem.CreateOrder(s.keywords, s.abvRange, s.timeLimit);

                    list.Add(new OrderSlotRuntime
                    {
                        order = order,
                        keywords = (s.keywords != null) ? new List<SecondaryEmotionSO>(s.keywords) : new List<SecondaryEmotionSO>(),
                        dialogueLine = s.dialogueLine,
                        postServeLines = (s.postServeLines != null) ? new List<string>(s.postServeLines) : new List<string>()
                    });
                }

                return list;
            }
        }
    }
}

