using UnityEngine;
using UnityEngine.UI;
using Alkuul.Domain;

public class CustomerPortraitView : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private Animator animator; // 있으면 사용, 없으면 스프라이트만

    private CustomerPortraitSet _set;
    private IntoxStage _stage;
    private RectTransform _portraitRect;
    private Vector3 _baseLocalScale;
    private Vector2 _baseSizeDelta;
    private Vector2 _baseAnchoredPosition;
    private Quaternion _baseLocalRotation;

    // 드래그 상태(내쫓기/재우기 중)일 때 표시를 잠깐 덮어씌움
    private bool _overrideVisual;
    private RuntimeAnimatorController _baseController;

    private void Awake()
    {
        if (portraitImage == null) portraitImage = GetComponent<Image>();
        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null) _baseController = animator.runtimeAnimatorController;
        if (portraitImage != null)
        {
            _portraitRect = portraitImage.rectTransform;
            _baseLocalScale = _portraitRect.localScale;
            _baseSizeDelta = _portraitRect.sizeDelta;
            _baseAnchoredPosition = _portraitRect.anchoredPosition;
            _baseLocalRotation = _portraitRect.localRotation;
        }
        SetVisible(false);
    }

    public void Bind(CustomerPortraitSet set, IntoxStage startStage)
    {
        _set = set;
        ResetPortraitTransform();
        SetVisible(true);
        SetStage(startStage);
    }

    public void SetStage(IntoxStage stage)
    {
        _stage = stage;
        if (_overrideVisual) return; // 드래그 중이면 stage 반영 잠시 보류

        ApplyStageVisual();
    }

    public IntoxStage CurrentStage => _stage;
    public bool HasSet => _set != null;

    public void Clear()
    {
        _set = null;
        _stage = IntoxStage.Sober;
        _overrideVisual = false;

        if (animator != null)
        {
            animator.enabled = false;
            animator.runtimeAnimatorController = _baseController;
        }

        if (portraitImage != null)
            portraitImage.sprite = null;

        ResetPortraitTransform();
        SetVisible(false);
    }

    private void ApplyStageVisual()
    {
        if (_set == null) return;

        var stageSprite = _set.GetStageSprite(_stage);
        if (portraitImage != null)
            portraitImage.sprite = stageSprite;


        // 만취면(선택) 만취 루프 애니를 우선 사용
        if (animator != null && _stage == IntoxStage.Wasted && _set.wastedLoopController != null)
        {
            animator.enabled = true;
            animator.runtimeAnimatorController = _set.wastedLoopController;
            return;
        }

        // 애니를 쓰지 않으면 스프라이트 교체
        if (animator != null)
        {
            animator.runtimeAnimatorController = _baseController;
            animator.enabled = false;
        }

        ResetPortraitTransform();
    }

    private void SetVisible(bool visible)
    {
        if (portraitImage != null) portraitImage.enabled = visible;
        if (!visible && animator != null) animator.enabled = false;
    }

    // 드래그 중 시각 연출 (evict/sleep)
    public void SetDragVisual(string mode) // "evict" | "sleep"
    {
        if (_set == null) return;
        _overrideVisual = true;

        if (animator != null)
        {
            var ctrl = mode == "sleep" ? _set.dragSleepController : _set.dragEvictController;
            if (ctrl != null)
            {
                animator.enabled = true;
                animator.runtimeAnimatorController = ctrl;
                return;
            }
        }

        // animator 없으면 sprite로 대체
        if (portraitImage != null)
        {
            var sp = mode == "sleep" ? _set.dragSleepSprite : _set.dragEvictSprite;
            if (sp != null) portraitImage.sprite = sp;
        }
    }

    public void ClearDragVisual()
    {
        _overrideVisual = false;
        ApplyStageVisual();
    }

    private void ResetPortraitTransform()
    {
        if (_portraitRect == null) return;
        _portraitRect.localScale = _baseLocalScale;
        _portraitRect.sizeDelta = _baseSizeDelta;
        _portraitRect.anchoredPosition = _baseAnchoredPosition;
        _portraitRect.localRotation = _baseLocalRotation;
    }
}
