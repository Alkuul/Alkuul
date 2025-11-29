using UnityEngine;
using System.Collections.Generic;
using Alkuul.Domain;
using Alkuul.Systems;

namespace Alkuul.UI
{
    /// <summary>지거 입력/선택값을 Systems로 전달하는 UI</summary>
    public class BrewingUI : MonoBehaviour
    {
        [SerializeField] private BrewingSystem brewing;
        [SerializeField] private ServeSystem serve;

        [Header("Selections")]
        [SerializeField] private TechniqueSO technique;
        [SerializeField] private GlassSO glass;
        [SerializeField] private List<GarnishSO> garnishes = new();
        [SerializeField] private bool useIce;

        public void OnAdd(IngredientSO ingredient, float ml)
        {
            if (ingredient == null) return;
            brewing.Add(ingredient, ml);
        }

        public void OnToggleIce(bool v) => useIce = v;

        /// <summary>한 잔 완성 → 평가까지</summary>
        public void OnSubmit(Order order, CustomerProfile customer)
        {
            if (brewing == null || serve == null)
            {
                Debug.LogWarning("BrewingUI: 시스템 참조 없음");
                return;
            }

            Drink d = brewing.Compute(useIce);
            var meta = ServeSystem.Meta.From(technique, glass, garnishes, useIce);
            var r = serve.ServeOne(order, d, meta, customer);

            Debug.Log($"만족도: {r.satisfaction:F1}% | 팁: {r.tip} | 떠남: {r.customerLeft}");
        }

        public void ResetUI()
        {
            useIce = false;
            garnishes.Clear();
            brewing.ResetMix();
        }

        public TechniqueSO SelectedTechnique => technique;
        public GlassSO SelectedGlass => glass;
        public IReadOnlyList<GarnishSO> SelectedGarnishes => garnishes;
        public bool UseIce => useIce;
    }
}

