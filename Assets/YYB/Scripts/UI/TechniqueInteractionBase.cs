using System;
using UnityEngine;
using Alkuul.Domain.Brewing;

namespace Alkuul.UI.Brewing
{
    public abstract class TechniqueInteractionBase : MonoBehaviour
    {
        public event Action<TechniqueInteractionResult> Completed;
        public event Action Cancelled;

        protected TechniqueInteractionSpec spec;
        protected float elapsed;
        protected bool isRunning;

        public TechniqueType TechniqueType => spec != null ? spec.techniqueType : default;

        public virtual void Initialize(TechniqueInteractionSpec interactionSpec)
        {
            spec = interactionSpec;
            OnInitialized();
        }

        public virtual void Begin()
        {
            elapsed = 0f;
            isRunning = true;
            OnBegin();
        }

        protected virtual void Update()
        {
            if (!isRunning) return;

            elapsed += Time.deltaTime;

            if (spec != null && spec.timeLimit > 0f && elapsed >= spec.timeLimit)
            {
                ForceFinish();
                return;
            }

            TickInteraction(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
            }
        }

        public virtual void Cancel()
        {
            if (!isRunning) return;

            isRunning = false;
            OnCancelled();
            Cancelled?.Invoke();
        }

        protected void Complete(bool success, float quality01, float progress01, string summary = "")
        {
            if (!isRunning) return;

            isRunning = false;

            TechniqueType type = TechniqueType.None;
            if (spec != null)
                type = spec.techniqueType;

            var result = new TechniqueInteractionResult(
                spec.techniqueType,
                success,
                quality01,
                progress01,
                elapsed,
                summary
            );

            OnCompleted(result);
            Completed?.Invoke(result);
        }

        protected virtual void ForceFinish()
        {
            float progress = GetCurrentProgress01();
            bool success = progress >= spec.successThreshold;
            Complete(success, progress, progress, "Time Over");
        }

        protected abstract void TickInteraction(float deltaTime);
        protected abstract float GetCurrentProgress01();

        protected virtual void OnInitialized() { }
        protected virtual void OnBegin() { }
        protected virtual void OnCompleted(TechniqueInteractionResult result) { }
        protected virtual void OnCancelled() { }
    }
}
