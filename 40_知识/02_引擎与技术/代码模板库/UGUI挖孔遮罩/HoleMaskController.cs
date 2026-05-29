// HoleMaskController.cs - UGUI 挖孔遮罩控制器（通用模板）
// 配套 Shader：UI/HoleMask（UIHoleMask.shader）
//
// 使用方式：
//   1. 在全屏 Image GameObject 上挂载此脚本（[RequireComponent(typeof(Image))]）
//   2. 确保 UIHoleMask.shader 已导入到项目的 Shaders 文件夹
//   3. 调用 SetHoleTarget(rectTransform) 设置挖孔目标
//   4. 配合 HoleMaskClickBlocker(ICanvasRaycastFilter) 实现孔洞区域点击穿透
//
// 命名空间：请根据项目实际修改 namespace

using UnityEngine;
using UnityEngine.UI;

namespace Template.UI
{
    public enum HoleShape
    {
        Rect = 0,
        RoundedRect = 1,
        Circle = 2
    }

    /// <summary>
    /// UGUI 挖孔遮罩控制器
    /// 依赖：UIHoleMask.shader（Shader "UI/HoleMask"）
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class HoleMaskController : MonoBehaviour
    {
        [Header("遮罩配置")]
        [SerializeField] private Color maskColor = new Color(0f, 0f, 0f, 0.8f);
        [SerializeField] private float cornerRadius = 20f;          // 圆角半径（像素）
        [SerializeField] private Vector2 holePadding = new Vector2(20f, 20f); // 挖孔扩展边距（像素）
        [SerializeField] private HoleShape holeShape = HoleShape.RoundedRect;

        [Header("运行时调试")]
        [Tooltip("启用后可在 Play 模式实时调整下方调试参数，不走自动计算")]
        [SerializeField] private bool enableRuntimeDebug = false;
        [Tooltip("可选。拖入目标 RectTransform 后，调试模式会自动对齐目标，再用 Debug Padding / Radius 微调。")]
        [SerializeField] private RectTransform debugTarget;
        [SerializeField] private Vector2 debugPadding = new Vector2(20f, 20f);
        [SerializeField] private float debugCornerRadiusPixels = 20f;
        [SerializeField] private Vector2 debugHoleCenter = new Vector2(0.5f, 0.5f); // 归一化 0-1
        [SerializeField] private Vector2 debugHoleSize   = new Vector2(0.3f, 0.2f); // 归一化 0-1
        [SerializeField] private float   debugCornerRadius = 0.02f;                 // 归一化 0-1
        [SerializeField] private HoleShape debugHoleShape = HoleShape.RoundedRect;
        [SerializeField] private Color   debugMaskColor = new Color(0f, 0f, 0f, 0.8f);

        private Image    _maskImage;
        private Material _maskMaterial;
        private Canvas   _rootCanvas;

        // 当前挖孔参数（用于 C# SDF 点击检测，与 Shader 保持一致）
        private Vector2 _currentHoleCenter;
        private Vector2 _currentHoleSize;
        private float   _currentCornerRadius;

        // 点击穿透使用物理像素坐标，避免 Canvas Scaler / 归一化误差导致视觉孔洞和点击孔洞不一致。
        private Vector2 _holeScreenCenter;
        private Vector2 _holeScreenHalfSize;
        private float   _cornerRadiusPixels;
        private HoleShape _currentHoleShape;

        private RectTransform _currentTarget;

        private void Awake()
        {
            _maskImage   = GetComponent<Image>();
            _rootCanvas  = GetComponentInParent<Canvas>()?.rootCanvas;

            if (_rootCanvas == null)
                Debug.LogError("[HoleMaskController] 找不到根 Canvas，请确认 GameObject 在 Canvas 层级内");

            InitializeMaterial();
        }

        private void InitializeMaterial()
        {
            Shader shader = Shader.Find("UI/HoleMask");
            if (shader == null)
            {
                Debug.LogError("[HoleMaskController] 找不到 Shader \"UI/HoleMask\"，请确认 UIHoleMask.shader 已导入");
                return;
            }

            _maskMaterial = new Material(shader);
            _maskMaterial.SetColor("_Color", maskColor);
            _maskImage.material = _maskMaterial;
        }

        private void Update()
        {
            if (!enableRuntimeDebug)
                return;

            if (debugTarget != null)
            {
                Vector2 oldPadding = holePadding;
                float oldRadius = cornerRadius;
                HoleShape oldShape = holeShape;
                Color oldColor = maskColor;

                holePadding = debugPadding;
                cornerRadius = debugCornerRadiusPixels;
                holeShape = debugHoleShape;
                maskColor = debugMaskColor;
                UpdateHoleFromTarget(debugTarget);

                holePadding = oldPadding;
                cornerRadius = oldRadius;
                holeShape = oldShape;
                maskColor = oldColor;
                return;
            }

            // 调试模式：实时更新 Shader 参数
            if (_maskMaterial != null)
            {
                _maskMaterial.SetVector("_HoleCenter", debugHoleCenter);
                _maskMaterial.SetVector("_HoleSize",   debugHoleSize);
                _maskMaterial.SetFloat ("_CornerRadius", debugCornerRadius);
                _maskMaterial.SetFloat ("_HoleShape", (float)debugHoleShape);
                _maskMaterial.SetColor ("_Color", debugMaskColor);
            }

            _currentHoleCenter    = debugHoleCenter;
            _currentHoleSize      = debugHoleSize;
            _currentCornerRadius  = debugCornerRadius;
            _currentHoleShape     = debugHoleShape;

            _holeScreenCenter = new Vector2(debugHoleCenter.x * Screen.width, debugHoleCenter.y * Screen.height);
            _holeScreenHalfSize = new Vector2(debugHoleSize.x * Screen.width * 0.5f, debugHoleSize.y * Screen.height * 0.5f);
            _cornerRadiusPixels = debugCornerRadius * Mathf.Min(Screen.width, Screen.height);
        }

        /// <summary>
        /// 根据目标 RectTransform 自动计算并设置挖孔位置和大小
        /// </summary>
        public void SetHoleTarget(RectTransform target)
        {
            SetHoleTarget(target, null, null, null);
        }

        /// <summary>
        /// 根据目标 RectTransform 自动计算并设置挖孔位置和大小，可临时覆盖 padding / radius / shape。
        /// </summary>
        public void SetHoleTarget(RectTransform target, Vector2? padding, float? radius, HoleShape? shape)
        {
            if (target == null)
            {
                Debug.LogWarning("[HoleMaskController] SetHoleTarget: 目标为空");
                return;
            }

            _currentTarget = target;
            Vector2 oldPadding = holePadding;
            float oldRadius = cornerRadius;
            HoleShape oldShape = holeShape;

            if (padding.HasValue) holePadding = padding.Value;
            if (radius.HasValue) cornerRadius = radius.Value;
            if (shape.HasValue) holeShape = shape.Value;

            if (enableRuntimeDebug)
            {
                holePadding = oldPadding;
                cornerRadius = oldRadius;
                holeShape = oldShape;
                return;
            }

            UpdateHoleFromTarget(target);

            holePadding = oldPadding;
            cornerRadius = oldRadius;
            holeShape = oldShape;
        }

        private void UpdateHoleFromTarget(RectTransform target)
        {
            Vector3[] worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);

            Camera cam = _rootCanvas != null ? _rootCanvas.worldCamera : null;

            Vector2 screenMin = new Vector2(float.MaxValue,  float.MaxValue);
            Vector2 screenMax = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < worldCorners.Length; i++)
            {
                Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
                screenMin = Vector2.Min(screenMin, sp);
                screenMax = Vector2.Max(screenMax, sp);
            }

            // 添加边距
            screenMin -= holePadding;
            screenMax += holePadding;

            Vector2 screenCenter = (screenMin + screenMax) * 0.5f;
            Vector2 screenSize   = screenMax - screenMin;

            float sw = Screen.width;
            float sh = Screen.height;

            Vector2 normalizedCenter = new Vector2(screenCenter.x / sw, screenCenter.y / sh);
            Vector2 normalizedSize   = new Vector2(screenSize.x   / sw, screenSize.y   / sh);
            float   normalizedRadius = cornerRadius / Mathf.Min(sw, sh);

            _currentHoleCenter   = normalizedCenter;
            _currentHoleSize     = normalizedSize;
            _currentCornerRadius = normalizedRadius;
            _currentHoleShape    = holeShape;

            _holeScreenCenter = screenCenter;
            _holeScreenHalfSize = screenSize * 0.5f;
            _cornerRadiusPixels = cornerRadius;

            // Shader 缺失时仍保留 C# 点击几何，避免视觉降级后交互一起失效。
            if (_maskMaterial != null)
            {
                _maskMaterial.SetVector("_HoleCenter",   new Vector4(normalizedCenter.x, normalizedCenter.y, 0, 0));
                _maskMaterial.SetVector("_HoleSize",     new Vector4(normalizedSize.x,   normalizedSize.y,   0, 0));
                _maskMaterial.SetFloat ("_CornerRadius", normalizedRadius);
                _maskMaterial.SetFloat ("_HoleShape", (float)holeShape);
                _maskMaterial.SetColor ("_Color", maskColor);
            }
        }

        /// <summary>
        /// 判断屏幕坐标点是否在孔洞内（用于点击穿透判断）
        /// </summary>
        public bool IsPointInHole(Vector2 screenPoint)
        {
            if (_holeScreenHalfSize.sqrMagnitude <= 0.001f)
                return false;

            Vector2 diff = screenPoint - _holeScreenCenter;
            float dist;
            switch (_currentHoleShape)
            {
                case HoleShape.Rect:
                    dist = RectSDF(diff, _holeScreenHalfSize);
                    break;
                case HoleShape.Circle:
                    dist = diff.magnitude - Mathf.Min(_holeScreenHalfSize.x, _holeScreenHalfSize.y);
                    break;
                default:
                    dist = RoundedBoxSDF(diff, _holeScreenHalfSize, _cornerRadiusPixels);
                    break;
            }

            return dist < 0f;
        }

        private static float RectSDF(Vector2 centerPos, Vector2 halfSize)
        {
            Vector2 q = new Vector2(Mathf.Abs(centerPos.x), Mathf.Abs(centerPos.y)) - halfSize;
            return Mathf.Max(q.x, q.y);
        }

        /// <summary>
        /// 圆角矩形 SDF（与 Shader 中算法完全一致）
        /// </summary>
        private static float RoundedBoxSDF(Vector2 centerPos, Vector2 halfSize, float radius)
        {
            radius = Mathf.Max(0f, radius);
            Vector2 q = new Vector2(Mathf.Abs(centerPos.x), Mathf.Abs(centerPos.y)) - halfSize + new Vector2(radius, radius);
            float outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude;
            float inside = Mathf.Min(Mathf.Max(q.x, q.y), 0f);
            return outside + inside - radius;
        }

        // ContextMenu 调试工具
        [ContextMenu("刷新挖孔位置")]
        private void RefreshHolePosition()
        {
            if (_currentTarget != null && !enableRuntimeDebug)
                UpdateHoleFromTarget(_currentTarget);
        }

        [ContextMenu("复制当前参数到调试字段")]
        private void CopyCurrentToDebug()
        {
            if (_maskMaterial == null) return;
            debugHoleCenter   = _maskMaterial.GetVector("_HoleCenter");
            debugHoleSize     = _maskMaterial.GetVector("_HoleSize");
            debugCornerRadius = _maskMaterial.GetFloat("_CornerRadius");
            debugMaskColor    = _maskMaterial.GetColor("_Color");
        }

        /// <summary>隐藏孔洞（显示全屏遮罩）</summary>
        public void HideHole()
        {
            _maskMaterial?.SetVector("_HoleSize", Vector4.zero);
            _holeScreenHalfSize = Vector2.zero;
        }

        /// <summary>设置遮罩颜色</summary>
        public void SetMaskColor(Color color)
        {
            maskColor = color;
            if (!enableRuntimeDebug)
                _maskMaterial?.SetColor("_Color", color);
        }

        private void OnDestroy()
        {
            if (_maskMaterial != null)
                Destroy(_maskMaterial);
        }
    }
}
