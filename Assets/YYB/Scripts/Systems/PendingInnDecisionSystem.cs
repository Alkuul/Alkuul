using System.Collections.Generic;
using UnityEngine;
using Alkuul.Domain;
using Alkuul.Systems;

public class PendingInnDecisionSystem : MonoBehaviour
{
    [SerializeField] private InnSystem inn;
    [SerializeField] private DailyLedgerSystem ledger;

    private readonly Queue<CustomerResult> _queue = new();

    private void Awake()
    {
        if (inn == null) inn = FindObjectOfType<InnSystem>(true);
        if (ledger == null) ledger = FindObjectOfType<DailyLedgerSystem>(true);
    }

    public int Count => _queue.Count;
    public bool HasPending => _queue.Count > 0;

    public void Enqueue(CustomerResult cr)
    {
        // 숙박 불가능이면 큐에 쌓지 않음
        if (!cr.canSleepAtInn) return;
        _queue.Enqueue(cr);
    }

    public bool SleepOne()
    {
        if (_queue.Count == 0) return false;

        var cr = _queue.Dequeue();
        bool ok = inn != null && inn.Sleep(cr);

        if (ok && ledger != null)
            ledger.RecordSleepSuccess();

        return ok;
    }

    public bool EvictOne()
    {
        if (_queue.Count == 0) return false;
        _queue.Dequeue();
        return true;
    }
}
