// HoleMaskClickBlocker.cs - UGUI 挖孔遮罩点击穿透组件（通用模板）
// 配套：HoleMaskController.cs（必须在同一 GameObject 上）
//
// 功能：
//   - 孔洞内部的点击事件：穿透，传递给下层 UI
//   - 孔洞外部（遮罩区域）的点击：拦截，防止误操作
//
// 命名空间：请根据项目实际修改 namespace

using UnityEngine;
using UnityEngine.EventSystems;

namespace Template.UI
{
    /// <summary>
    /// 挖孔遮罩点击穿透处理器
    /// 需与 HoleMaskController 挂载在同一 GameObject 上
    /// </summary>
    [RequireComponent(typeof(HoleMaskController))]
    public class HoleMaskClickBlocker : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        private HoleMaskController _maskController;

        private void Awake()
        {
            _maskController = GetComponent<HoleMaskController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 点击在孔洞内：穿透，不处理
            if (_maskController != null && _maskController.IsPointInHole(eventData.position))
                return;

            // 点击在遮罩区：拦截
            eventData.Use();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 同上：PointerDown 也需要拦截，防止 Down 事件泄漏到下层
            if (_maskController != null && _maskController.IsPointInHole(eventData.position))
                return;

            eventData.Use();
        }
    }
}
