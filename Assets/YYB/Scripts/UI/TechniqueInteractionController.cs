using Alkuul.Domain;
using Alkuul.Domain.Brewing;
using Alkuul.UI.Brewing;
using UnityEngine;

public class TechniqueInteractionController : MonoBehaviour
{
    [SerializeField] private TechniqueInteractionRegistry registry;
    [SerializeField] private RectTransform interactionRoot;
    [SerializeField] private CanvasGroup blocker;
    [SerializeField] private BrewingPanelBridge bridge;

    private TechniqueInteractionBase currentInteraction;
    private TechniqueSO pendingTechniqueData;

    public bool IsBusy => currentInteraction != null;

    public void StartTechniqueInteraction(TechniqueSO techniqueData)
    {
        if (IsBusy)
        {
            Debug.Log("[TechniqueInteractionController] Already running interaction.");
            return;
        }

        if (techniqueData == null)
        {
            Debug.LogWarning("[TechniqueInteractionController] Technique data is null.");
            return;
        }

        if (registry == null)
        {
            Debug.LogWarning("[TechniqueInteractionController] Registry is null.");
            return;
        }

        if (!registry.TryGetEntry(techniqueData, out var entry))
        {
            Debug.LogWarning($"[TechniqueInteractionController] No entry found for {techniqueData.name}");
            return;
        }

        pendingTechniqueData = techniqueData;

        SetBlocker(true);

        currentInteraction = Instantiate(entry.prefab, interactionRoot);
        currentInteraction.Initialize(entry.spec);
        currentInteraction.Completed += HandleCompleted;
        currentInteraction.Cancelled += HandleCancelled;
        currentInteraction.Begin();
    }

    private void HandleCompleted(TechniqueInteractionResult result)
    {
        if (result.Success)
        {
            if (bridge != null && pendingTechniqueData != null)
            {
                bridge.SetTechnique(pendingTechniqueData);
            }
        }

        if (bridge != null)
        {
            bridge.ApplyTechniqueInteractionResult(result);
        }

        CleanupCurrent();
    }

    private void HandleCancelled()
    {
        CleanupCurrent();
    }

    private void CleanupCurrent()
    {
        if (currentInteraction != null)
        {
            currentInteraction.Completed -= HandleCompleted;
            currentInteraction.Cancelled -= HandleCancelled;
            Destroy(currentInteraction.gameObject);
            currentInteraction = null;
        }

        pendingTechniqueData = null;
        SetBlocker(false);
    }

    private void SetBlocker(bool value)
    {
        if (blocker == null) return;

        blocker.alpha = value ? 1f : 0f;
        blocker.blocksRaycasts = value;
        blocker.interactable = value;
    }
}