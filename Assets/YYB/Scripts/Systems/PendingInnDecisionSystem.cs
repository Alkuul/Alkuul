using System;
using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain;
using Alkuul.Systems;

public class PendingInnDecisionSystem : MonoBehaviour
{
    [SerializeField] private InnSystem inn;
    [SerializeField] private DailyLedgerSystem ledger;

    private readonly Queue<CustomerResult> _queue = new();
    public event Action QueueChanged;

    private void Awake()
    {
        if (inn == null) inn = FindObjectOfType<InnSystem>(true);
        if (ledger == null) ledger = FindObjectOfType<DailyLedgerSystem>(true);
    }

    public int Count => _queue.Count;
    public bool HasPending => _queue.Count > 0;

    public bool TryPeek(out CustomerResult result)
    {
        if (_queue.Count == 0)
        {
            result = default;
            return false;
        }

        result = _queue.Peek();
        return true;
    }
    public void Enqueue(CustomerResult cr)
    {
        // 숙박 불가능이면 큐에 쌓지 않음
        if (!cr.canSleepAtInn) return;
        _queue.Enqueue(cr);
        QueueChanged?.Invoke();
    }

    public bool SleepOne()
    {
        if (_queue.Count == 0) return false;

        var cr = _queue.Dequeue();
        bool ok = inn != null && inn.Sleep(cr);

        if (ok && ledger != null)
            ledger.RecordSleepSuccess();

        QueueChanged?.Invoke();
        return ok;
    }

    public bool EvictOne()
    {
        if (_queue.Count == 0) return false;
        _queue.Dequeue();
        QueueChanged?.Invoke();
        return true;
    }
}
