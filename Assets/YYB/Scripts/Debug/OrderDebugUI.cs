using UnityEngine;
using System.Collections.Generic;
using Alkuul.Domain;
using Alkuul.Systems;

namespace Alkuul.UI
{
    /// <summary>
    /// 버튼 한 번으로 주문 생성 → BrewingUI.Submit까지 호출하는 임시 드라이버
    /// </summary>
    public class OrderDebugUI : MonoBehaviour
    {
        [SerializeField] private OrderSystem orderSystem;
        [SerializeField] private BrewingUI brewingUI;

        [Header("Order Params")]
        [SerializeField] private List<SecondaryEmotionSO> keywords = new(); // 안도/후회 등
        [SerializeField] private Vector2 abvRange = new Vector2(0, 100);
        [SerializeField] private float timeLimit = 45f;

        [Header("Customer (Debug)")]
        [SerializeField] private string customerId = "debug_customer";
        [SerializeField] private string customerName = "테스트 손님";
        [SerializeField] private Tolerance tolerance = Tolerance.Normal;
        [SerializeField] private IcePreference icePreference = IcePreference.Neutral;

        public void OnMakeAndSubmit()
        {
            if (orderSystem == null || brewingUI == null)
            {
                Debug.LogWarning("OrderDebugUI: refs missing.");
                return;
            }

            var order = orderSystem.CreateOrder(keywords, abvRange, timeLimit);

            var customer = new CustomerProfile
            {
                id = customerId,
                displayName = customerName,
                tolerance = tolerance,
                icePreference = icePreference
            };

            brewingUI.OnSubmit(order, customer);
        }
    }
}
