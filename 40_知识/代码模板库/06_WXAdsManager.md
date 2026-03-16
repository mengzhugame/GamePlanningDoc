# WXAdsManager 微信激励视频广告管理器

## 适用场景
微信小游戏项目中接入激励视频广告（Rewarded Video Ad）。玩家完整观看广告后触发回调，给予游戏内奖励。

## 核心特性
1. 单例持久化，整个游戏生命周期只创建一次
2. `#if !UNITY_EDITOR && UNITY_WEBGL` 条件编译：编辑器内直接模拟成功回调，不影响开发调试
3. `Action onSuccess` 回调设计，调用方灵活定义奖励逻辑，与广告SDK完全解耦
4. 支持多广告位 ID，通过 `adType` 枚举区分不同用途（解锁关卡、使用道具、复活等）

## 如何扩展更多广告位
在 `adUnitId` 字段区增加新的广告位ID，在 `ShowAd` 的 switch 中加一个 case 即可。

## 使用方法

```csharp
// 关卡失败时，显示复活广告
WXAdsManager.Instance.ShowAd(AdType.Revive, () =>
{
    player.ReviveWithFullHP();
    ResumeGame();
});

// 解锁道具时显示广告
WXAdsManager.Instance.ShowAd(AdType.UnlockTool, () =>
{
    powerUpManager.GrantFreeTool();
});
```

## 代码实现

```csharp
// WXAdsManager.cs
using UnityEngine;
using System;

/// <summary>
/// 广告类型枚举（按业务场景定义，不要用0/1裸数字）
/// </summary>
public enum AdType
{
    UnlockOrder = 0,   // 解锁订单/关卡
    UseTool     = 1,   // 使用道具
    Revive      = 2,   // 复活
    DoubleReward= 3,   // 双倍奖励
}

public class WXAdsManager : MonoBehaviour
{
    public static WXAdsManager Instance;

    // ===== 广告位 ID（在微信小程序后台申请后填入）=====
    private readonly string[] adUnitIds =
    {
        "adunit-xxxxxxxxxxxxxxxx", // UnlockOrder
        "adunit-yyyyyyyyyyyyyyyy", // UseTool
        "adunit-zzzzzzzzzzzzzzzz", // Revive
        "adunit-wwwwwwwwwwwwwwww", // DoubleReward
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 播放激励视频广告
    /// </summary>
    /// <param name="adType">广告类型（决定使用哪个广告位）</param>
    /// <param name="onSuccess">广告完整播放后的成功回调</param>
    public void ShowAd(AdType adType, Action onSuccess)
    {
        int index = (int)adType;
        if (index < 0 || index >= adUnitIds.Length)
        {
            Debug.LogError($"[WXAdsManager] 未知广告类型: {adType}");
            return;
        }

        string unitId = adUnitIds[index];

#if !UNITY_EDITOR && UNITY_WEBGL
        var ad = WeChatWASM.WX.CreateRewardedVideoAd(new WeChatWASM.WXCreateRewardedVideoAdParam()
        {
            adUnitId = unitId
        });

        ad.OnClose((res) =>
        {
            if (res.isEnded)
            {
                Debug.Log($"[WXAdsManager] 广告完整播放，触发奖励: {adType}");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.Log($"[WXAdsManager] 广告提前关闭，不发放奖励: {adType}");
            }
            ad.OffClose(null);
        });

        ad.OnError((res) =>
        {
            Debug.LogError($"[WXAdsManager] 广告加载失败: {res.errMsg}");
        });

        ad.Show();
#else
        // 编辑器内模拟广告成功
        Debug.Log($"[WXAdsManager][Editor] 模拟广告播放成功: {adType} (unitId: {unitId})");
        onSuccess?.Invoke();
#endif
    }
}
```

## 注意事项
- 广告位ID需要在微信小程序后台「流量主」功能申请，审核通过后才能正常展示
- 正式发版前，将所有占位符 `adunit-xxx` 替换为真实申请的广告位ID
- 建议在真机上测试广告展示，模拟器无法加载真实广告
