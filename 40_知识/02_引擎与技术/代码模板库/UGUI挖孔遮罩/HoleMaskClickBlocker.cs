// HoleMaskClickBlocker.cs - UGUI 挖孔遮罩点击穿透组件（通用模板）
// 配套：HoleMaskController.cs（必须在同一 GameObject 上）
//
// 功能：
//   - 孔洞内部的点击事件：在射线检测阶段穿透，传递给下层 UI
//   - 孔洞外部（遮罩区域）的点击：由遮罩 Image 接收，防止误操作
//
// 命名空间：请根据项目实际修改 namespace

using UnityEngine;

namespace Template.UI
{
    /// <summary>
    /// 挖孔遮罩点击穿透处理器
    /// 需与 HoleMaskController 挂载在同一 GameObject 上
    /// </summary>
    [RequireComponent(typeof(HoleMaskController))]
    public class HoleMaskClickBlocker : MonoBehaviour, ICanvasRaycastFilter
    {
        private HoleMaskController _maskController;

        private void Awake()
        {
            _maskController = GetComponent<HoleMaskController>();
        }

        /// <summary>
        /// GraphicRaycaster 调用。返回 false 表示此 Graphic 不接收射线，事件继续穿透到下层按钮。
        /// </summary>
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (_maskController != null && _maskController.IsPointInHole(sp))
                return false;

            return true;
        }
    }
}
