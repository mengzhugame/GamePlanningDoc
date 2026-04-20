# UI 动画工具 — UIAnimationHelper

> **模板编号**：07  
> **来源工程**：LightVSDecay（光与朽）  
> **提取日期**：2026-04-14  
> **适用场景**：任何需要 UI 属性数值变化反馈的项目  
> **复用价值**：⭐⭐⭐⭐⭐

---

## 概述

`UIAnimationHelper` 是一个静态工具类，提供两种通用 UI 动画：

| 效果 | 方法 | 常见叫法 |
|------|------|---------|
| **Q弹缩放（Scale Punch）** | `PlayScalePunch` | DOPunchScale / Bounce Scale |
| **数字滚动（Rolling Number）** | `RollInt` / `RollFloat` | Odometer / Count Up Effect |
| **组合（先Q弹再滚动）** | `PunchThenRollInt` / `PunchThenRollFloat` | 装备属性更新标准效果 |

所有方法均返回 `IEnumerator`，由调用方通过 `StartCoroutine(...)` 驱动，无框架依赖。

---

## 代码模板

```csharp
// ============================================================
// UIAnimationHelper.cs
// 文件位置: Assets/Scripts/UI/UIAnimationHelper.cs
//
// 用途：UI 通用动画工具（Scale Punch Q弹缩放 + Rolling Number 数字滚动）
//
// 使用方式（均为协程，需要 MonoBehaviour 调用 StartCoroutine）：
//
//   StartCoroutine(UIAnimationHelper.PlayScalePunch(rectTransform));
//   StartCoroutine(UIAnimationHelper.RollInt(text, from: 100, to: 150, prefix: "攻击力："));
//   StartCoroutine(UIAnimationHelper.PunchThenRollInt(text, from, to, prefix: "攻击力："));
// ============================================================

using System.Collections;
using TMPro;
using UnityEngine;

namespace YourProject.UI   // ← 修改为项目命名空间
{
    public static class UIAnimationHelper
    {
        // ─── Scale Punch（Q弹缩放） ───────────────────────────────────────

        /// <summary>
        /// Q弹缩放效果：Scale 1.0 → punchScale → 1.0
        /// </summary>
        /// <param name="rect">目标 RectTransform</param>
        /// <param name="punchScale">放大倍率（1.2 = 放大20%后弹回）</param>
        /// <param name="duration">总时长（秒），两段各占一半</param>
        /// <param name="useUnscaledTime">true = 不受 TimeScale 影响（推荐用于 UI）</param>
        public static IEnumerator PlayScalePunch(
            RectTransform rect,
            float punchScale = 1.2f,
            float duration = 0.2f,
            bool useUnscaledTime = true)
        {
            if (rect == null) yield break;

            Vector3 baseScale = rect.localScale;
            float half = Mathf.Max(0.01f, duration * 0.5f);
            float elapsed = 0f;

            // Phase 1: 放大
            while (elapsed < half)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / half);
                rect.localScale = Vector3.Lerp(baseScale, baseScale * punchScale, t);
                yield return null;
            }

            // Phase 2: 弹回
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / half);
                rect.localScale = Vector3.Lerp(baseScale * punchScale, baseScale, t);
                yield return null;
            }

            rect.localScale = baseScale;
        }

        // ─── Rolling Number（数字滚动） ───────────────────────────────────

        /// <summary>整数滚动：文字在 duration 秒内从 from 滚动到 to</summary>
        public static IEnumerator RollInt(
            TextMeshProUGUI text,
            int from,
            int to,
            float duration = 0.5f,
            string prefix = "",
            string suffix = "")
        {
            if (text == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                int value = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
                text.text = $"{prefix}{value}{suffix}";
                yield return null;
            }

            text.text = $"{prefix}{to}{suffix}";
        }

        /// <summary>浮点数滚动：文字在 duration 秒内从 from 滚动到 to</summary>
        public static IEnumerator RollFloat(
            TextMeshProUGUI text,
            float from,
            float to,
            float duration = 0.5f,
            string format = "F1",
            string prefix = "",
            string suffix = "")
        {
            if (text == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float value = Mathf.Lerp(from, to, t);
                text.text = $"{prefix}{value.ToString(format)}{suffix}";
                yield return null;
            }

            text.text = $"{prefix}{to.ToString(format)}{suffix}";
        }

        // ─── 组合效果 ─────────────────────────────────────────────────────

        /// <summary>先 Q弹缩放 再 整数滚动（装备属性更新标准用法）</summary>
        public static IEnumerator PunchThenRollInt(
            TextMeshProUGUI text,
            int from,
            int to,
            float punchScale = 1.2f,
            float punchDuration = 0.2f,
            float rollDuration = 0.5f,
            string prefix = "",
            string suffix = "")
        {
            if (text == null) yield break;
            yield return PlayScalePunch(text.transform as RectTransform, punchScale, punchDuration);
            yield return RollInt(text, from, to, rollDuration, prefix, suffix);
        }

        /// <summary>先 Q弹缩放 再 浮点数滚动（装备属性更新标准用法）</summary>
        public static IEnumerator PunchThenRollFloat(
            TextMeshProUGUI text,
            float from,
            float to,
            float punchScale = 1.2f,
            float punchDuration = 0.2f,
            float rollDuration = 0.5f,
            string format = "F1",
            string prefix = "",
            string suffix = "")
        {
            if (text == null) yield break;
            yield return PlayScalePunch(text.transform as RectTransform, punchScale, punchDuration);
            yield return RollFloat(text, from, to, rollDuration, format, prefix, suffix);
        }
    }
}
```

---

## 使用示例

### 单独 Scale Punch（如金币栏收到金币时弹一下）

```csharp
// 战斗场景中（跟随 TimeScale）
StartCoroutine(UIAnimationHelper.PlayScalePunch(coinBarRect, punchScale: 1.12f, duration: 0.12f, useUnscaledTime: false));

// UI 弹窗中（不受暂停影响）
StartCoroutine(UIAnimationHelper.PlayScalePunch(titleRect, punchScale: 1.3f, duration: 0.25f));
```

### 打断重启（如连续收到金币，每次都重新弹）

```csharp
private Coroutine _punchCoroutine;
private Vector3 _baseScale;

private void PlayPunch()
{
    if (_punchCoroutine != null)
    {
        StopCoroutine(_punchCoroutine);
        targetRect.localScale = _baseScale;   // 重置到基础 Scale，防止停在放大状态
    }
    _punchCoroutine = StartCoroutine(RunPunch());
}

private IEnumerator RunPunch()
{
    yield return UIAnimationHelper.PlayScalePunch(targetRect, 1.12f, 0.12f, useUnscaledTime: false);
    _punchCoroutine = null;
}
```

### 装备属性变化（Q弹 + 数字滚动）

```csharp
// 整数属性
StartCoroutine(UIAnimationHelper.PunchThenRollInt(attackText, oldAtk, newAtk, prefix: "攻击力："));

// 百分比属性
StartCoroutine(UIAnimationHelper.PunchThenRollFloat(critText, oldCrit * 100f, newCrit * 100f, prefix: "暴击率：", suffix: "%"));

// 多属性同时播放（各自独立协程）
StartCoroutine(UIAnimationHelper.PunchThenRollInt(hpText,     oldHp,     newHp,     prefix: "生命值："));
StartCoroutine(UIAnimationHelper.PunchThenRollInt(shieldText, oldShield, newShield, prefix: "护盾值："));
```

### 打断中途正在滚动的数字（复用 Dictionary 缓存）

```csharp
private readonly Dictionary<TextMeshProUGUI, Coroutine> _statCoroutines = new();

private void AnimateStat(TextMeshProUGUI text, int from, int to, string label)
{
    if (_statCoroutines.TryGetValue(text, out var existing) && existing != null)
        StopCoroutine(existing);

    _statCoroutines[text] = StartCoroutine(RunAnimateStat(text, from, to, label));
}

private IEnumerator RunAnimateStat(TextMeshProUGUI text, int from, int to, string label)
{
    yield return UIAnimationHelper.PunchThenRollInt(text, from, to, prefix: label);
    _statCoroutines[text] = null;
}
```

---

## 注意事项

1. **useUnscaledTime**：UI 弹窗通常在游戏暂停（`Time.timeScale = 0`）时显示，必须用 `unscaledDeltaTime`，否则动画卡住。战斗 HUD 中的金币栏建议用 `useUnscaledTime: false`，跟随游戏节奏。

2. **中断恢复**：打断协程后一定要手动将 `localScale` 重置为基础值，否则下次 Punch 会从放大后的状态开始，视觉错乱。

3. **PunchThenRoll 不可中断分段**：`PunchThenRollInt` 是顺序执行（先 Punch，再 Roll）。如果在 Roll 阶段被打断，只需重新 `StartCoroutine` 即可，`from` 值传入当前已显示的数值。

4. **TextMeshPro 依赖**：需要项目中已导入 TextMeshPro 包（Unity Package Manager）。

5. **命名空间**：复制时将 `namespace YourProject.UI` 改为项目实际命名空间。
