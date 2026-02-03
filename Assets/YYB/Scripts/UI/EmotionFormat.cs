using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alkuul.Domain;
using UnityEngine;

public static class EmotionFormat
{
    // 필요하면 한글 라벨만 네 취향대로 바꿔
    private static readonly (string label, Func<EmotionVector, float> getter)[] Items =
    {
        ("기쁨",   v => v.joy),
        ("슬픔",   v => v.sadness),
        ("분노",   v => v.anger),
        ("공포",   v => v.fear),
        ("혐오",   v => v.disgust),
        ("놀람",   v => v.surprise),
        ("무감정", v => v.neutral),
    };

    public static string ToPercentLines(EmotionVector v, int topK = 7, float minPct = 1f)
    {
        var list = new List<(string label, float value)>();
        foreach (var it in Items)
        {
            float val = it.getter(v);
            list.Add((it.label, val));
        }

        // v는 보통 0~1 정규화라고 가정
        var ordered = list.OrderByDescending(x => x.value).ToList();

        var sb = new StringBuilder();
        int shown = 0;
        foreach (var (label, value) in ordered)
        {
            float pct = value * 100f;
            if (pct < minPct) continue;

            sb.Append(label).Append(": ").Append(pct.ToString("0.#")).Append("%");
            sb.AppendLine();

            shown++;
            if (shown >= topK) break;
        }

        if (shown == 0) sb.Append("표시할 감정 없음");
        return sb.ToString().TrimEnd();
    }
}
