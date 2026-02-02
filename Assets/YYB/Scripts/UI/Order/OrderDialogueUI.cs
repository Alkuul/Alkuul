using TMPro;
using UnityEngine;
using Alkuul.Domain;

namespace Alkuul.UI
{
    public class OrderDialogueUI : MonoBehaviour
    {
        [Header("TMP Refs")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text orderIndexText; // 선택(주문 1/3)
        [SerializeField] private TMP_Text metaText;       // 선택(얼음선호/시간 등 디버그용)

        public void Set(CustomerProfile customer, int slotIndex1Based, int slotCount, string line, Order order)
        {
            if (nameText != null)
                nameText.text = string.IsNullOrWhiteSpace(customer.displayName) ? customer.id : customer.displayName;

            if (orderIndexText != null)
                orderIndexText.text = $"주문 {slotIndex1Based}/{slotCount}";

            if (dialogueText != null)
                dialogueText.text = line ?? "";

            if (metaText != null)
                metaText.text = $"{IcePrefToKorean(customer.icePreference)} · 제한 {order.timeLimit:0}s";
        }

        private static string IcePrefToKorean(IcePreference pref) => pref switch
        {
            IcePreference.Like => "얼음 좋아함(+15)",
            IcePreference.Dislike => "얼음 싫어함(-10)",
            _ => "얼음 상관없음"
        };
    }
}
