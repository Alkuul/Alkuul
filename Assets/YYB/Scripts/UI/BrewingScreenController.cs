using Alkuul.Domain;
using Alkuul.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Alkuul.UI
{
    /// <summary>
    /// 조주 씬 UI를 한 곳에서 바인딩:
    /// - Page1(재료/기법/믹싱/지거/얼음/리셋)
    /// - Page2(잔/가니쉬)
    /// - 페이지 전환(앞/뒤)
    /// - Serve는 FlowController가 있으면 Flow로, 없으면 Bridge로 직접 호출
    /// </summary>
    public class BrewingScreenController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private BrewingPanelBridge bridge;
        [SerializeField] private InGameFlowController flow; // 있으면 OnClickServe() 호출

        [Header("Pages")]
        [SerializeField] private GameObject pageMix;   // 재료+기법+믹싱글라스 화면
        [SerializeField] private GameObject pageFinish;// 잔+가니쉬 화면
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;

        [Header("Common UI")]
        [SerializeField] private Toggle iceToggle;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button serveButton;

        [Header("Bindings")]
        [SerializeField] private List<IngredientBind> ingredients = new();
        [SerializeField] private List<TechniqueBind> techniques = new();
        [SerializeField] private List<GlassBind> glasses = new();
        [SerializeField] private List<GarnishBind> garnishes = new();

        [Header("Debug")]
        [SerializeField] private bool verboseLog = true;

        [Serializable]
        public struct IngredientBind
        {
            public Button button;
            public IngredientSO ingredient;
            public float ml;
        }

        [Serializable]
        public struct TechniqueBind
        {
            public Button button;
            public TechniqueSO technique;
        }

        [Serializable]
        public struct GlassBind
        {
            public Button button;
            public GlassSO glass;
        }

        [Serializable]
        public struct GarnishBind
        {
            public Toggle toggle;
            public GarnishSO garnish;
        }

        private void Awake()
        {
            if (bridge == null) bridge = FindObjectOfType<BrewingPanelBridge>();
            if (flow == null) flow = FindObjectOfType<InGameFlowController>();

            if (bridge == null)
                Debug.LogWarning("[BrewingScreenController] bridge is NULL (Inspector 연결 필요).");

            BindNavigation();
            BindCommon();
            BindIngredients();
            BindTechniques();
            BindGlasses();
            BindGarnishes();

            ShowPageMix();
        }

        private void BindNavigation()
        {
            if (nextPageButton != null)
                nextPageButton.onClick.AddListener(ShowPageFinish);

            if (prevPageButton != null)
                prevPageButton.onClick.AddListener(ShowPageMix);
        }

        private void BindCommon()
        {
            if (iceToggle != null)
                iceToggle.onValueChanged.AddListener(on =>
                {
                    bridge?.SetIce(on);
                    if (verboseLog) Debug.Log($"[UI] Ice={on}");
                });

            if (resetButton != null)
                resetButton.onClick.AddListener(() =>
                {
                    bridge?.ResetMix();
                    if (iceToggle != null) iceToggle.SetIsOnWithoutNotify(false);
                    ClearGarnishToggles();
                    if (verboseLog) Debug.Log("[UI] ResetMix");
                });

            if (serveButton != null)
                serveButton.onClick.AddListener(() =>
                {
                    if (verboseLog) Debug.Log("[UI] Serve Click");
                    if (flow != null) flow.OnClickServe();
                    else bridge?.ServeOnce(); // Flow가 없을 때 단독 테스트용
                });
        }

        private void BindIngredients()
        {
            foreach (var b in ingredients)
            {
                if (b.button == null || b.ingredient == null) continue;
                float ml = b.ml <= 0f ? 30f : b.ml;
                var ing = b.ingredient;

                b.button.onClick.AddListener(() =>
                {
                    bridge?.OnPortionAdded(ing, ml);
                    if (verboseLog) Debug.Log($"[UI] AddPortion {ing.name} {ml}ml");
                });
            }
        }

        private void BindTechniques()
        {
            foreach (var b in techniques)
            {
                if (b.button == null || b.technique == null) continue;
                var t = b.technique;

                b.button.onClick.AddListener(() =>
                {
                    bridge?.SetTechnique(t);
                    if (verboseLog) Debug.Log($"[UI] Technique={t.name}");
                });
            }
        }

        private void BindGlasses()
        {
            foreach (var b in glasses)
            {
                if (b.button == null || b.glass == null) continue;
                var g = b.glass;

                b.button.onClick.AddListener(() =>
                {
                    bridge?.SetGlass(g);
                    if (verboseLog) Debug.Log($"[UI] Glass={g.name}");
                });
            }
        }

        private void ClearGarnishToggles()
        {
            foreach (var b in garnishes)
            {
                if (b.toggle == null) continue;
                b.toggle.SetIsOnWithoutNotify(false);
            }
        }

        private void BindGarnishes()
        {
            foreach (var b in garnishes)
            {
                if (b.toggle == null || b.garnish == null) continue;
                var toggle = b.toggle;
                var garnish = b.garnish;

                toggle.onValueChanged.AddListener(on =>
                {
                    if (bridge == null) return;

                    bool ok = bridge.SetGarnishes(garnish, on);
                    if (!ok)
                        toggle.SetIsOnWithoutNotify(false);

                    if (verboseLog) Debug.Log($"[UI] Garnish {garnish.name} => {on} (ok={ok})");
                });
            }
        }

        public void ShowPageMix()
        {
            if (pageMix != null) pageMix.SetActive(true);
            if (pageFinish != null) pageFinish.SetActive(false);
            if (verboseLog) Debug.Log("[UI] Page=Mix");
        }

        public void ShowPageFinish()
        {
            if (pageMix != null) pageMix.SetActive(false);
            if (pageFinish != null) pageFinish.SetActive(true);
            if (verboseLog) Debug.Log("[UI] Page=Finish");
        }
    }
}
