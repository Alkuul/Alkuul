using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain;
using Alkuul.Systems;

namespace Alkuul.UI
{
    [System.Serializable]
    public class ReactionDialogueSet
    {
        [Header("Low Satisfaction (0~39)")]
        public List<string> low = new();

        [Header("Mid Satisfaction (40~74)")]
        public List<string> mid = new();

        [Header("High Satisfaction (75~100+)")]
        public List<string> high = new();
    }

    [System.Serializable]
    public class OrderSlotAuthoring
    {
        [Header("Order Dialogue (legacy single line)")]
        [TextArea] public string dialogueLine;

        [Header("Order Dialogue Pages (recommended)")]
        public List<string> dialogueLines = new();

        [Header("Reaction Dialogue (legacy fallback)")]
        public List<string> postServeLines = new();

        [Header("Reaction Dialogue by Satisfaction")]
        public ReactionDialogueSet reactionLines = new();

        public List<SecondaryEmotionSO> keywords = new();
        public Vector2 abvRange = new Vector2(0, 100);
        public float timeLimit = 60f;
    }

    public struct OrderSlotRuntime
    {
        public Order order;
        public List<SecondaryEmotionSO> keywords;

        public string dialogueLine;                 // legacy
        public List<string> dialogueLines;         // multi-page order dialogue

        public List<string> postServeLines;        // legacy fallback
        public List<string> reactionLinesLow;
        public List<string> reactionLinesMid;
        public List<string> reactionLinesHigh;
    }

    public class CustomerOrdersAuthoring : MonoBehaviour
    {
        [Header("Customer")]
        public CustomerProfile profile;

        [Header("Order Slots (1~3)")]
        public List<OrderSlotAuthoring> slots = new();

        public List<OrderSlotRuntime> BuildRuntime(OrderSystem orderSystem)
        {
            var list = new List<OrderSlotRuntime>();
            if (orderSystem == null) return list;

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