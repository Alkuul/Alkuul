using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialOverlay : MonoBehaviour
{
    [Serializable]
    public struct Line
    {
        public string speaker;
        [TextArea(2, 6)] public string text;
        public Sprite portrait; // 선택: 줄마다 다른 이미지 쓰고 싶으면
    }

    [Header("UI Refs")]
    [SerializeField] private GameObject root;          // Tutorial 패널 루트(보통 이 오브젝트)
    [SerializeField] private TMP_Text speakerNameText; // SpeakerNameText
    [SerializeField] private TMP_Text dialogueText;    // DialogueText
    [SerializeField] private Image portraitImage;      // OwnerImage(선택)
    [SerializeField] private CanvasGroup canvasGroup;  // 선택(없으면 자동 추가)

    [Header("Auto Play Condition")]
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private bool onlyDay1 = true;     // Day1에서만
    [SerializeField] private string seenKey = "tut.order.day1"; // 씬마다 다르게!

    [Header("Content")]
    [SerializeField] private Line[] lines;

    [Header("Events")]
    public UnityEvent onCompleted; // 끝났을 때(버튼 활성화 등) 연결 가능

    private int _index = -1;
    private bool _playing;

    private void Awake()
    {
        if (root == null) root = gameObject;

        if (canvasGroup == null)
            canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();

        // 처음엔 꺼두는 게 일반적
        SetVisible(false);
    }

    private void Start()
    {
        if (!autoPlayOnStart) return;

        if (HasSeen()) return;
        if (onlyDay1 && !IsDay1()) return;

        Play();
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
            return;
        }
    }

    public void Play()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[TutorialOverlay] lines is empty.");
            return;
        }

        _playing = true;
        _index = 0;

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

        if (portraitImage != null)
        {
            if (line.portrait != null)
            {
                portraitImage.sprite = line.portrait;
                portraitImage.enabled = true;
            }
            else
            {
                // 줄마다 이미지 바꾸는 게 아니라면, Inspector에서 세팅된 sprite 유지하고 싶을 수도 있음
                // 여기서는 "portrait가 없으면 그대로 유지"로 둠
            }
        }
    }

    private void Complete()
    {
        _playing = false;
        _index = -1;

        MarkSeen();
        SetVisible(false);

        onCompleted?.Invoke();
    }

    private void SetVisible(bool v)
    {
        if (root != null) root.SetActive(v);

        // 아래 UI 입력을 막고 싶으면(튜토리얼 중 버튼 눌림 방지):
        // root 배경 Image(Raycast Target ON)만 있어도 보통 충분하지만,
        // CanvasGroup도 같이 세팅해두면 확실함.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = v ? 1f : 0f;
            canvasGroup.blocksRaycasts = v;
            canvasGroup.interactable = v;
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

    // Day1 판정: DayCycleController가 있으면 그걸 우선, 없으면 "Day1이라고 가정" (초기 개발용)
    private bool IsDay1()
    {
        // 너 프로젝트에 DayCycleController가 있으면 여기를 실제 타입으로 바꿔줘.
        // 예: var day = FindObjectOfType<DayCycleController>(true);
        // return day != null ? day.currentDay == 1 : true;

        // 타입 의존이 싫으면 일단 PlayerPrefs 기반으로만 제어해도 됨.
        // 지금은 DayCycleController 타입명을 내가 100% 확정 못 해서 안전하게 true 처리.
        return true;
    }
}
