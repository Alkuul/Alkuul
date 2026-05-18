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

                    var builtDialogueLines = new List<string>();
                    if (s.dialogueLines != null)
                    {
                        foreach (var line in s.dialogueLines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                                builtDialogueLines.Add(line.Trim());
                        }
                    }

                    if (builtDialogueLines.Count == 0 && !string.IsNullOrWhiteSpace(s.dialogueLine))
                        builtDialogueLines.Add(s.dialogueLine.Trim());

                    list.Add(new OrderSlotRuntime
                    {
                        order = order,
                        keywords = s.keywords != null ? new List<SecondaryEmotionSO>(s.keywords) : new List<SecondaryEmotionSO>(),

                        dialogueLine = s.dialogueLine,
                        dialogueLines = builtDialogueLines,

                        postServeLines = s.postServeLines != null ? new List<string>(s.postServeLines) : new List<string>(),
                        reactionLinesLow = s.reactionLines != null && s.reactionLines.low != null ? new List<string>(s.reactionLines.low) : new List<string>(),
                        reactionLinesMid = s.reactionLines != null && s.reactionLines.mid != null ? new List<string>(s.reactionLines.mid) : new List<string>(),
                        reactionLinesHigh = s.reactionLines != null && s.reactionLines.high != null ? new List<string>(s.reactionLines.high) : new List<string>()
                    });
                }

                return list;
            }
        }
    }
}