using System;
using TMPro;
using UnityEngine;
using Alkuul.Core;
using Alkuul.Systems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialOverlay : MonoBehaviour
{
    [Serializable]
    public struct Line
    {
        public string speaker;
        [TextArea(2, 6)] public string text;
        public Sprite portrait;
    }

    [Header("UI Refs")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Canvas overlayCanvas;

    [Header("Layer")]
    [SerializeField] private int overlaySortingOrder = 10000;

    [Header("Auto Play Condition")]
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private bool onlyDay1 = true;
    [SerializeField] private string seenKey = "tut.order.day1";
    [SerializeField] private bool waitForDayStartEvent = true;
    [SerializeField] private DayCycleController dayCycle;

    [Header("Content")]
    [SerializeField] private Line[] lines;

    [Header("Events")]
    public UnityEvent onCompleted; // 끝났을 때(버튼 활성화 등) 연결 가능
    public event Action<bool> PlayingStateChanged;
    
    private int _index = -1;
    private bool _playing;
    private bool _subscribedDayStart;

    public bool IsPlaying => _playing;

    private void Awake()
    {
        if (root == null) root = gameObject;

        if (canvasGroup == null)
            canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleController>(true);

        EnsureTopmostCanvas();

        // 처음엔 꺼두는 게 일반적
        SetVisible(false);
    }

    private void Start()
    {
        if (!autoPlayOnStart) return;
        TryAutoPlay();
    }

    private void OnEnable()
    {
        TryBindDayStartEvent();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        UnbindDayStartEvent();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        UnbindDayStartEvent();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureTopmostCanvas();
    }

    private void Update()
    {
        if (!_playing) return;

        // 마우스 클릭
        if (Input.GetMouseButtonDown(0))
        {
            Next();
            return;
        }

        // 터치
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Next();
        }
    }

    private void TryAutoPlay()
    {
        if (_playing) return;
        if (HasSeen()) return;

        if (onlyDay1 && !IsDay1())
        {
            TryBindDayStartEvent();
            return;
        }
        UnbindDayStartEvent();
        Play();
    }

    private void TryBindDayStartEvent()
    {
        if (!waitForDayStartEvent || _subscribedDayStart) return;

        EventBus.OnDayStarted += HandleDayStarted;
        _subscribedDayStart = true;
    }

    private void UnbindDayStartEvent()
    {
        if (!_subscribedDayStart) return;

        EventBus.OnDayStarted -= HandleDayStarted;
        _subscribedDayStart = false;
    }

    private void HandleDayStarted()
    {
        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleController>(true);

        TryAutoPlay();
    }

    public void Play()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[TutorialOverlay] lines is empty.");
            return;
        }
        if (root != null && !root.activeSelf)
            root.SetActive(true);

        _playing = true;
        _index = 0;
        PlayingStateChanged?.Invoke(true);

        EnsureTopmostCanvas();
        SetVisible(true);
        RenderCurrent();
    }

    public void Next()
    {
        if (!_playing) return;

        _index++;
        if (_index >= lines.Length)
        {
            Complete();
            return;
        }

        RenderCurrent();
    }

    public void ForcePlay(bool resetSeen = false)
    {
        if (resetSeen) PlayerPrefs.DeleteKey(seenKey);
        Play();
    }

    private void RenderCurrent()
    {
        var line = lines[_index];

        if (speakerNameText != null) speakerNameText.text = line.speaker ?? "";
        if (dialogueText != null) dialogueText.text = line.text ?? "";

        if (portraitImage != null && line.portrait != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = true;
        }
    }

    private void Complete()
    {
        _playing = false;
        _index = -1;
        PlayingStateChanged?.Invoke(false);

        MarkSeen();
        SetVisible(false);

        onCompleted?.Invoke();
    }

    private void SetVisible(bool visible)
    {
        //if (root != null) root.SetActive(visible);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }

    private bool HasSeen()
    {
        return PlayerPrefs.GetInt(seenKey, 0) == 1;
    }

    private void MarkSeen()
    {
        PlayerPrefs.SetInt(seenKey, 1);
        PlayerPrefs.Save();
    }

    private void EnsureTopmostCanvas()
    {
        if (root == null) return;

        if (overlayCanvas == null)
            overlayCanvas = root.GetComponent<Canvas>() ?? root.AddComponent<Canvas>();

        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = overlaySortingOrder;

        if (root.GetComponent<GraphicRaycaster>() == null)
            root.AddComponent<GraphicRaycaster>();

        root.transform.SetAsLastSibling();
    }

    private bool IsDay1()
    {
        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleController>(true);

        if (dayCycle == null)
        {
            Debug.LogWarning("[TutorialOverlay] DayCycleController not found. Fallback to Day1=true.");
            return true;
        }

        return dayCycle.currentDay == 1;
    }
}
