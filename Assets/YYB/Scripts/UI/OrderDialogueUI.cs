using TMPro;
using UnityEngine;
using Alkuul.Domain;

    public class OrderDialogueUI : MonoBehaviour
    {
        [Header("TMP Refs")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text orderIndexText;
        [SerializeField] private TMP_Text metaText;

        public void SetSystemLine(string line)
        {
            if (nameText != null) nameText.text = "";
            if (orderIndexText != null) orderIndexText.text = "";
            if (metaText != null) metaText.text = "";
            if (dialogueText != null) dialogueText.text = line ?? "";
        }

        // 기존 호환용(다른 코드에서 호출해도 컴파일 유지)
        public void Set(CustomerProfile customer, int slotIndex1Based, int slotCount, string line, Order order)
        {
            Set(customer, slotIndex1Based, slotCount, line, order, true);
        }

        public void Set(CustomerProfile customer, int slotIndex1Based, int slotCount, string line, Order order, bool showMeta)
        {
            // 슬롯이 없으면 "시스템 문구"처럼 처리
            if (slotCount <= 0)
            {
                SetSystemLine(line);
                return;
            }

            if (nameText != null)
                nameText.text = string.IsNullOrWhiteSpace(customer.displayName) ? customer.id : customer.displayName;

            if (orderIndexText != null)
                orderIndexText.text = $"주문 {slotIndex1Based}/{slotCount}";

            if (dialogueText != null)
                dialogueText.text = line ?? "";

            if (metaText != null)
            {
                metaText.text = showMeta
                    ? $"{IcePrefToKorean(customer.icePreference)} | 제한 {order.timeLimit:0}s"
                    : "";
            }
        }

        private static string IcePrefToKorean(IcePreference pref) => pref switch
        {
            IcePreference.Like => "얼음 좋아함",
            IcePreference.Dislike => "얼음 싫어함",
            _ => "얼음 상관없음"
        };
    }
