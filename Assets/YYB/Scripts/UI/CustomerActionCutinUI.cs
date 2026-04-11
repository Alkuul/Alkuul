using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerActionCutinUI : MonoBehaviour
{
    [Header("Optional Visual Root")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("UI")]
    [SerializeField] private Image illustrationImage;
    [SerializeField] private TMP_Text actionText;

    [Header("Fallback Sprites")]
    [SerializeField] private Sprite defaultSleepSprite;
    [SerializeField] private Sprite defaultEvictSprite;

    [Header("Timing")]
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float fadeDuration = 0.2f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // visualRoot를 따로 안 넣었으면 자기 자신을 시각 루트처럼 취급하되,
        // 절대 gameObject 자체를 비활성화하지는 않음
        if (visualRoot == null)
            visualRoot = gameObject;

        HideImmediate();
    }

    public void PlaySleep(CustomerPortraitSet portraitSet, Action onFinished = null)
    {
        var sprite = portraitSet != null ? portraitSet.GetSleepCutinSprite() : defaultSleepSprite;
        PlayCustom(sprite, "손님을 재우는 중...", onFinished, showDuration);
    }

    public void PlayEvict(CustomerPortraitSet portraitSet, Action onFinished = null)
    {
        var sprite = portraitSet != null ? portraitSet.GetEvictCutinSprite() : defaultEvictSprite;
        PlayCustom(sprite, "손님을 내보내는 중...", onFinished, showDuration);
    }

    public void PlayCustom(Sprite sprite, string label, Action onFinished = null, float? overrideDuration = null)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[CustomerActionCutinUI] GameObject is inactive. Enable the object in scene.");
            return;
        }

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(CoPlay(sprite, label, onFinished, overrideDuration ?? showDuration));
    }

    private IEnumerator CoPlay(Sprite targetSprite, string label, Action onFinished, float duration)
    {
        if (illustrationImage != null)
        {
            illustrationImage.sprite = targetSprite;
            illustrationImage.enabled = targetSprite != null;
        }

        if (actionText != null)
            actionText.text = label;

        ShowVisual(true);

        yield return Fade(0f, 1f, fadeDuration);
        yield return new WaitForSeconds(duration);
        yield return Fade(1f, 0f, fadeDuration);

        HideImmediate();

        currentRoutine = null;
        onFinished?.Invoke();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float t = 0f;
        canvasGroup.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, p);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void ShowVisual(bool value)
    {
        // visualRoot가 자기 자신이 아닐 때만 SetActive 사용
        if (visualRoot != null && visualRoot != gameObject)
            visualRoot.SetActive(value);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = value ? 1f : 0f;
            canvasGroup.blocksRaycasts = value;
            canvasGroup.interactable = value;
        }
    }

    private void HideImmediate()
    {
        if (visualRoot != null && visualRoot != gameObject)
            visualRoot.SetActive(false);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}