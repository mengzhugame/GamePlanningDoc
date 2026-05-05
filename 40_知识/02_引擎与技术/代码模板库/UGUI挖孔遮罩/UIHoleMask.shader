// UIHoleMask.shader - UGUI 单图层 SDF 挖孔遮罩
// 用途：在全屏半透明遮罩上用 SDF 算法挖出圆角矩形孔洞，支持软边抗锯齿
// 使用方式：创建 Material → 赋给 Image 组件 → 通过 C# 设置 _HoleCenter/_HoleSize/_CornerRadius
// 适配：Unity 2021+，UGUI，Screen Space - Overlay / Camera 均可
//
// 参数说明（均为归一化坐标，0-1）：
//   _HoleCenter  : 孔洞中心（屏幕归一化坐标，Screen.width/height 归一化后的值）
//   _HoleSize    : 孔洞宽高（归一化尺寸）
//   _CornerRadius: 圆角半径（归一化，建议 = pixelRadius / min(Screen.width, Screen.height)）
//   _Color       : 遮罩颜色（RGBA，A 控制不透明度）

Shader "UI/HoleMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0, 0, 0, 0.8)
        _HoleCenter ("Hole Center (Normalized)", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize   ("Hole Size (Normalized)",   Vector) = (0.2, 0.1, 0, 0)
        _CornerRadius ("Corner Radius (Normalized)", Float) = 0.02

        // UGUI Stencil 标准属性（必须保留，供 Mask 组件使用）
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex    : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
                float4 color     : COLOR;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4    _Color;
            float4    _HoleCenter;
            float4    _HoleSize;
            float     _CornerRadius;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.texcoord  = v.texcoord;
                o.color     = v.color;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            // 圆角矩形 SDF：返回点到圆角矩形边界的有符号距离
            // centerPos : 点相对于矩形中心的偏移（归一化）
            // size      : 矩形半尺寸（归一化）
            // radius    : 圆角半径（归一化）
            // 返回值 < 0 表示在矩形内部
            float roundedBoxSDF(float2 centerPos, float2 size, float radius)
            {
                float2 d = max(abs(centerPos) - size + radius, 0.0);
                return length(d) - radius;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 计算当前像素的屏幕归一化坐标
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // 计算到孔洞中心的偏移
                float2 diff = screenUV - _HoleCenter.xy;

                // SDF 距离：> 0 在遮罩区，< 0 在孔洞内
                float dist = roundedBoxSDF(diff, _HoleSize.xy * 0.5, _CornerRadius);

                // smoothstep 实现软边抗锯齿（约 ±1px 过渡带）
                float alpha = smoothstep(-0.001, 0.001, dist);

                fixed4 col = _Color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
