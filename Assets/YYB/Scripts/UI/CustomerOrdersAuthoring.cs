using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain;
using Alkuul.Systems;

namespace Alkuul.UI
{
    [System.Serializable]
    public class OrderSlotAuthoring
    {
        [TextArea] public string dialogueLine;                 // 비어있으면 키워드 이름으로 자동 생성
        public List<string> postServeLines = new();
        public List<SecondaryEmotionSO> keywords = new();       // 이 슬롯의 2차 감정들
        public Vector2 abvRange = new Vector2(0, 100);          // 옵션(현재 점수엔 미반영)
        public float timeLimit = 60f;                           // 옵션(현재 점수엔 미반영)
    }

    public struct OrderSlotRuntime
    {
        public Order order;                                     // 계산된 목표 벡터
        public List<SecondaryEmotionSO> keywords;               // 대사용(표시용)
        public string dialogueLine;
        public List<string> postServeLines;
    }

    public class CustomerOrdersAuthoring : MonoBehaviour
    {
        [Header("Customer")]
        public CustomerProfile profile;

        [Header("Order Slots (1~3)")]
        public List<OrderSlotAuthoring> slots = new(); // size 1~3 권장

        public List<OrderSlotRuntime> BuildRuntime(OrderSystem orderSystem)
        {
            var list = new List<OrderSlotRuntime>();
            if (orderSystem == null) return list;

            foreach (var s in slots)
            {
                if (s == null) continue;

                var order = orderSystem.CreateOrder(s.keywords, s.abvRange, s.timeLimit);

                list.Add(new OrderSlotRuntime
                {
                    order = order,
                    keywords = new List<SecondaryEmotionSO>(s.keywords),
                    dialogueLine = s.dialogueLine,
                    postServeLines = (s.postServeLines != null) ? new List<string>(s.postServeLines) : new List<string>()
                });
            }

            return list;
        }
    }
}
