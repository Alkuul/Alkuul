using TMPro;
using UnityEngine;
using Alkuul.Systems;

public class BrewingTutorialController : MonoBehaviour
{
    private enum Step
    {
        None = 0,
        PourToJigger,
        PourToMixingGlass,
        SelectTechnique,
        GoToFinishPage,
        SelectGlass,
        AddGarnish,
        SubmitDrink,
        Completed
    }

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("Rules")]
    [SerializeField] private bool autoBeginOnStart = true;
    [SerializeField] private bool onlyDay1 = true;
    [SerializeField] private string seenKey = "tut.brewing.day1.segmented";
    [SerializeField] private DayCycleController dayCycle;
    [SerializeField] private bool verboseLog = true;

    [Header("Highlights")]
    [SerializeField] private GameObject liquorShelfHighlight;
    [SerializeField] private GameObject jiggerHighlight;
    [SerializeField] private GameObject mixingGlassHighlight;
    [SerializeField] private GameObject techniqueHighlight;
    [SerializeField] private GameObject nextArrowHighlight;
    [SerializeField] private GameObject glassHighlight;
    [SerializeField] private GameObject garnishListHighlight;
    [SerializeField] private GameObject garnishTargetHighlight;
    [SerializeField] private GameObject submitHighlight;

    private Step _step = Step.None;

    public bool IsRunning => _step != Step.None && _step != Step.Completed;
    public bool IsCompleted => _step == Step.Completed;

    private void Awake()
    {
        if (root == null) root = gameObject;

        if (canvasGroup == null)
            canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();

        if (dayCycle == null)
            dayCycle = FindObjectOfType<DayCycleController>(true);

        SetVisible(false);
        SetAllHighlights(false);
    }

    private void Start()
    {
        if (autoBeginOnStart)
            BeginIfNeeded();
    }

    public void BeginIfNeeded()
    {
        if (IsRunning || IsCompleted)
            return;

        if (HasSeen())
        {
            _step = Step.Completed;
            return;
        }

        if (onlyDay1 && dayCycle != null && dayCycle.currentDay != 1)
        {
            Log("Skipped: not day 1.");
            return;
        }

        SetStep(Step.PourToJigger);
    }

    public void NotifyPouredToJigger()
    {
        if (_step != Step.PourToJigger) return;
        SetStep(Step.PourToMixingGlass);
    }

    public void NotifyPouredToMixingGlass()
    {
        if (_step != Step.PourToMixingGlass) return;
        SetStep(Step.SelectTechnique);
    }

    public void NotifyTechniqueSucceeded()
    {
        if (_step != Step.SelectTechnique) return;
        SetStep(Step.GoToFinishPage);
    }

    public void NotifyMovedToFinishPage()
    {
        if (_step != Step.GoToFinishPage) return;
        SetStep(Step.SelectGlass);
    }

    public void NotifyGlassSelected()
    {
        if (_step != Step.SelectGlass) return;
        SetStep(Step.AddGarnish);
    }

    public void NotifyGarnishAdded()
    {
        if (_step != Step.AddGarnish) return;
        SetStep(Step.SubmitDrink);
    }

    public void NotifyDrinkSubmitted()
    {
        if (_step != Step.SubmitDrink) return;
        CompleteTutorial();
    }

    private void SetStep(Step nextStep)
    {
        _step = nextStep;
        ApplyCurrentStepView();
        Log($"Step -> {_step}");
    }

    private void ApplyCurrentStepView()
    {
        SetAllHighlights(false);

        if (_step == Step.None || _step == Step.Completed)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        switch (_step)
        {
            case Step.PourToJigger:
                SetMessage("술 진열장과 지거를 확인하고, 술을 드래그해서 지거에 따라보세요.");
                SetHighlights(liquorShelfHighlight, jiggerHighlight);
                break;

            case Step.PourToMixingGlass:
                SetMessage("이제 지거를 믹싱글라스에 드래그해서 술을 따라주세요.");
                SetHighlights(jiggerHighlight, mixingGlassHighlight);
                break;

            case Step.SelectTechnique:
                SetMessage("조주도구를 선택하고 술을 조주하세요.");
                SetHighlights(techniqueHighlight);
                break;

            case Step.GoToFinishPage:
                SetMessage("오른쪽 화살표를 눌러 잔과 가니쉬를 선택하세요.");
                SetHighlights(nextArrowHighlight);
                break;

            case Step.SelectGlass:
                SetMessage("잔을 선택하세요.");
                SetHighlights(glassHighlight);
                break;

            case Step.AddGarnish:
                SetMessage("가니쉬를 드래그해서 잔 위에 올려보세요.");
                SetHighlights(garnishListHighlight, garnishTargetHighlight);
                break;

            case Step.SubmitDrink:
                SetMessage("완성한 술을 제출하세요.");
                SetHighlights(submitHighlight);
                break;
        }
    }

    private void CompleteTutorial()
    {
        _step = Step.Completed;
        MarkSeen();
        SetAllHighlights(false);
        SetVisible(false);
        Log("Tutorial completed.");
    }

    private void SetMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;
    }

    private void SetHighlights(params GameObject[] targets)
    {
        if (targets == null) return;

        foreach (var go in targets)
        {
            if (go != null) go.SetActive(true);
        }
    }

    private void SetAllHighlights(bool on)
    {
        SetOne(liquorShelfHighlight, on);
        SetOne(jiggerHighlight, on);
        SetOne(mixingGlassHighlight, on);
        SetOne(techniqueHighlight, on);
        SetOne(nextArrowHighlight, on);
        SetOne(glassHighlight, on);
        SetOne(garnishListHighlight, on);
        SetOne(garnishTargetHighlight, on);
        SetOne(submitHighlight, on);
    }

    private void SetOne(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
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

    private void Log(string msg)
    {
        if (verboseLog)
            Debug.Log($"[BrewingTutorial] {msg}");
    }
}