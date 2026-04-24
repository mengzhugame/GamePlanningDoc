# AI美术工业化工作流指南 (AI Art Pipeline Guide)

> 创建时间: 2026-02-11 | 状态: v1.0 初版
> 目标受众: 独立游戏开发者（尤其小团队/个人开发者）

---

## 一、总览：为什么需要AI美术工作流？

独立开发者最大痛点：**美术产能不足**。AI美术工作流可以：
- 将单张Icon/UI制作时间从 30-60分钟 → 5-10分钟
- 批量生成风格统一的素材
- 快速迭代概念设计，降低试错成本

**核心工具选型：ComfyUI**（非WebUI）
- 节点式工作流，可保存/复用/分享
- 显存占用更小，速度更快
- 支持API调用，可与Unity集成
- 生态成熟（ControlNet、IPAdapter、LoRA全支持）

---

## 二、工具链与环境搭建

### 2.1 基础环境
| 组件 | 推荐版本 | 用途 |
|------|---------|------|
| ComfyUI | ≥ 0.3.30 | 核心工作流引擎 |
| PyTorch | ≥ 2.1 | 推理后端 |
| SDXL模型 | 各种finetune版 | 基础生成模型 |
| Flux.1 | 最新版 | 新一代模型（文本理解更好） |

### 2.2 关键插件
- **ComfyUI_IPAdapter_plus** — 风格迁移（参考图控制风格）
- **comfyui_controlnet_aux** — ControlNet预处理器
- **ComfyUI_ADV_CLIP_emb** — 高级文本编码
- **UltimateSDUpscale** — 分块高清放大

### 2.3 硬件需求
- **最低**: 8GB VRAM (RTX 3060/4060) — 可跑SDXL，开xformers
- **推荐**: 12GB+ VRAM (RTX 4070+) — 流畅跑所有工作流
- **云端方案**: RunPod / 阿里云PAI（按需租GPU）

---

## 三、四大核心工作流

### 3.1 🎮 游戏Icon批量生成工作流

**场景**: 技能图标、道具图标、状态图标等

**流程**:
1. **文本描述** → SDXL/Flux生成基础图
2. **ControlNet (Canny/Depth)** → 控制构图和形状
3. **LoRA** → 统一风格（如像素风、卡通风、写实风）
4. **后处理** → 去背景 + 统一尺寸 + 色彩校正

**关键参数**:
- 分辨率: 512x512 或 1024x1024（后缩小）
- CFG Scale: 5-8（Icon偏低以避免过度渲染）
- 步数: 20-30步
- 采样器: DPM++ 2M / Euler

**风格统一技巧**:
- 使用同一个LoRA模型（权重0.6-0.8）
- 固定负面提示词模板
- IPAdapter参考同一张风格图（权重0.3-0.5）
- 固定seed后微调prompt，保持一致性

**批量生产**: ComfyUI的Batch功能，一次生成多张变体

### 3.2 🎨 UI界面元素生成工作流

**场景**: 按钮、面板、边框、背景等

**流程**:
1. **手绘草稿/线稿** → 输入参考
2. **ControlNet (Lineart/Canny)** → 保持结构
3. **IPAdapter** → 风格参考（已有UI的截图）
4. **SDXL生成** → 出图
5. **Photoshop/GIMP精修** → 切图可用

**注意**: UI元素对精度要求高，AI出图通常需要人工精修30-50%

### 3.3 🧑‍🎨 角色立绘/概念设计工作流

**场景**: 角色概念图、立绘、表情差分

**推荐工作流**（基于ComfyUI.org的Game Design工作流）:
1. **草稿输入** → 手绘/简单线稿
2. **ControlNet双重控制**:
   - Lineart（权重0.25）→ 控制线条
   - Depth（权重0.8）→ 控制空间关系
3. **IPAdapter-MoE** → 多风格融合适配器
4. **KSampler** → DPM++ 2M, 38步生成
5. **UltimateSDUpscale + SwinIR_4x** → 高清放大（分块512x512）

**输入建议**:
- 草稿推荐分辨率: 768x1280（竖版立绘）
- 图生图推荐: 2048x2048
- 参考图: 干净背景，风格明确

### 3.4 🖼️ 场景/背景生成工作流

**场景**: 游戏关卡背景、主界面背景

**流程**:
1. **文本描述 + 构图参考** → 基础生成
2. **ControlNet (Depth/Tile)** → 空间控制
3. **Inpaint局部修复** → 修正不合理区域
4. **超分辨率** → 放大到游戏所需尺寸

---

## 四、LoRA训练 — 统一风格的核心

### 4.1 为什么要训练自己的LoRA？

公开LoRA大多是二次元美少女风格，**游戏项目需要定制化风格**。
训练自己的LoRA = 让AI学会你的美术风格。

### 4.2 训练工具选择

| 工具 | 特点 | 推荐度 |
|------|------|--------|
| **Kohya_ss** | 最主流，功能完整，社区大 | ⭐⭐⭐⭐⭐ |
| **OneTrainer** | 界面友好，2025年新起之秀 | ⭐⭐⭐⭐ |
| **阿里云PAI ArtLab** | 云端训练，零配置 | ⭐⭐⭐（适合无GPU用户） |

### 4.3 LoRA训练SOP

**Step 1: 数据准备**（最关键！）
- 收集15-30张目标风格的图片
- 统一裁剪为512x512或1024x1024
- 用BLIP/WD Tagger自动打标签
- 人工校正标签（重点：风格特征词、颜色词）

**Step 2: 训练参数**
```
模型基底: SDXL 1.0
网络类型: LoRA
网络维度(Rank): 32-64（Icon/UI建议32，角色建议64）
学习率: 1e-4 (unet) / 5e-5 (text encoder)
训练步数: 1500-3000步（15张图约2000步）
批量大小: 1-2（根据VRAM）
优化器: AdamW8bit
```

**Step 3: 验证与调优**
- 每500步保存checkpoint
- 用固定prompt测试各checkpoint
- 选择最佳效果的版本
- 过拟合信号：生成图与训练集过于相似

### 4.4 独立游戏LoRA实战建议

- **先用Midjourney/现有工具确定美术风格** → 产出20张风格参考
- **用这20张训练LoRA** → 获得风格一致的生成能力
- **后续所有素材用此LoRA生成** → 风格自动统一
- LoRA文件很小（通常50-200MB），方便管理多个风格

---

## 五、Midjourney辅助灵感

### 5.1 定位
- **Midjourney**: 灵感探索、概念阶段、快速出效果图
- **ComfyUI/SD**: 生产阶段、精确控制、批量输出

### 5.2 配合流程
1. Midjourney快速探索10-20种风格方向
2. 选定方向后，用MJ出20-30张参考图
3. 参考图用于：训练LoRA + IPAdapter风格参考
4. ComfyUI接管后续所有量产工作

---

## 六、与Unity集成

### 6.1 ComfyUI API方式
ComfyUI支持REST API调用，可以：
- 从Unity Editor发送生成请求
- 自动接收生成图片并导入为Asset
- 实现"描述 → 生成 → 导入"全自动管线

### 6.2 实用脚本思路
```
Unity Editor Button → HTTP POST to ComfyUI API
  → 传入prompt + 参数
  → ComfyUI生成图片
  → 回调下载图片
  → 自动导入为Sprite/Texture
```

### 6.3 注意事项
- ComfyUI需要保持运行（本地或云端）
- 网络延迟考虑：本地部署最佳
- 生成图仍需人工Quality Check

---

## 七、SOP总结：独立游戏AI美术生产流程

```
阶段1: 风格定义（1-2天）
├── Midjourney探索风格方向
├── 确定美术风格板(Mood Board)
└── 产出20-30张风格参考图

阶段2: 工具准备（1天）
├── 搭建ComfyUI环境
├── 下载基础模型(SDXL/Flux)
├── 安装必要插件
└── 训练项目专属LoRA

阶段3: 工作流搭建（1-2天）
├── Icon生成工作流
├── UI元素生成工作流
├── 角色立绘工作流
└── 场景背景工作流

阶段4: 量产（持续）
├── 按需求批量生成素材
├── 人工QC + 精修（预计30-50%需要修改）
├── 导入Unity
└── 迭代优化prompt和参数
```

**预计效率提升**: 
- Icon: 传统30min/个 → AI 5min/个（含精修）= **6x提速**
- UI: 传统2h/套 → AI 30min/套 = **4x提速**
- 立绘: 传统1-2天/张 → AI 2-4h/张 = **4-8x提速**

---

## 八、常见问题与解决方案

| 问题 | 解决方案 |
|------|---------|
| 风格不统一 | 固定LoRA+IPAdapter参考图+统一negative prompt |
| 细节模糊 | UltimateSDUpscale分块放大，或提高生成分辨率 |
| 手/文字变形 | ControlNet约束+后期手动修复（AI通病） |
| 训练LoRA过拟合 | 减少训练步数，增加训练图多样性 |
| 显存不足 | 开启xformers/--lowvram，减小batch/分辨率 |
| 生成速度慢 | 考虑云端GPU（RunPod按小时计费）|

---

## 九、推荐资源

- [ComfyUI官方示例](https://github.com/comfyanonymous/ComfyUI_examples)
- [ComfyUI.org工作流库](https://comfyui.org) — 大量现成游戏美术工作流
- [Kohya_ss LoRA训练](https://github.com/bmaltais/kohya_ss)
- [CivitAI模型库](https://civitai.com) — 公开LoRA/Checkpoint下载
- [scenario.gg](https://scenario.gg) — 专业游戏素材AI生成平台（商业）

---

*本指南为v1.0，后续随实践深入持续更新。*
*下一步: 实际搭建ComfyUI环境，跑通第一个Icon生成工作流。*
