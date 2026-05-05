// ============================================================
// GameSettingsTemplate.cs
// 文件位置: Assets/Scripts/Data/SO/GameSettings.cs
// 用途：游戏全局配置模板（ScriptableObject）
// 使用：复制后根据项目需求修改配置项
// ============================================================

using UnityEngine;

namespace YourGame.Data
{
    /// <summary>
    /// 游戏全局设置 (ScriptableObject)
    /// 统一管理所有游戏配置参数
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "YourGame/Game Settings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 玩家配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 玩家配置 ═══")]
        [Tooltip("玩家初始生命值")]
        public int playerMaxHP = 100;

        [Tooltip("玩家移动速度")]
        public float playerMoveSpeed = 5f;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 经验系统
        // 公式: UpgradeCost = expBase + (CurrentLevel - 1) * expGrowth
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 经验系统 ═══")]
        [Tooltip("升级公式基础值")]
        public int expBase = 100;

        [Tooltip("升级公式增量系数")]
        public int expGrowth = 50;

        [Tooltip("最大等级")]
        public int maxLevel = 20;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 战斗配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 战斗配置 ═══")]
        [Tooltip("基础伤害")]
        public float baseDamage = 10f;

        [Tooltip("基础暴击率（0-1）")]
        [Range(0f, 1f)]
        public float baseCritRate = 0.1f;

        [Tooltip("暴击伤害倍率")]
        public float critDamageMultiplier = 2.0f;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 游戏规则
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 游戏规则 ═══")]
        [Tooltip("单局游戏时长（秒）")]
        public float gameDuration = 180f;

        [Tooltip("复活次数上限")]
        public int maxRevives = 1;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 经济系统
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 经济系统 ═══")]
        [Tooltip("基础金币掉落")]
        public int baseCoinDrop = 10;

        [Tooltip("胜利奖励金币")]
        public int victoryBonus = 100;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 便捷方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 计算指定等级升级所需经验
        /// </summary>
        public int CalculateExpToNextLevel(int level)
        {
            return expBase + (level - 1) * expGrowth;
        }

        /// <summary>
        /// 计算累计经验
        /// </summary>
        public int CalculateTotalExpForLevel(int targetLevel)
        {
            int total = 0;
            for (int lv = 1; lv < targetLevel; lv++)
            {
                total += CalculateExpToNextLevel(lv);
            }
            return total;
        }

        /// <summary>
        /// 判断是否暴击
        /// </summary>
        public bool RollCrit()
        {
            return Random.value < baseCritRate;
        }

        /// <summary>
        /// 计算最终伤害（含暴击）
        /// </summary>
        public float CalculateDamage(float baseDmg, bool isCrit)
        {
            return isCrit ? baseDmg * critDamageMultiplier : baseDmg;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 编辑器工具
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
        [ContextMenu("打印经验表")]
        public void PrintExpTable()
        {
            Debug.Log("=== 经验表 ===");
            int cumulative = 0;
            for (int lv = 1; lv < maxLevel; lv++)
            {
                int need = CalculateExpToNextLevel(lv);
                cumulative += need;
                Debug.Log($"Lv.{lv} → {lv + 1}: {need} XP (累计: {cumulative})");
            }
        }

        [ContextMenu("验证数值")]
        public void ValidateSettings()
        {
            Debug.Log("=== GameSettings 验证 ===");
            Debug.Log($"玩家: HP={playerMaxHP}, Speed={playerMoveSpeed}");
            Debug.Log($"战斗: 伤害={baseDamage}, 暴击率={baseCritRate:P0}, 暴击倍率={critDamageMultiplier}");
            Debug.Log($"游戏: 时长={gameDuration}s, 复活={maxRevives}次");
            Debug.Log($"经验: Lv1→{maxLevel} 累计={CalculateTotalExpForLevel(maxLevel)}");
        }
#endif
    }
}
