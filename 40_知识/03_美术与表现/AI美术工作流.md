---
type: knowledge
status: draft
created: 2026-04-24
source_book: ChatGPT AI美术资源学习对话 + Google光与朽美术对话
source_page: ChatGPT_AI美术资源学习_2025-09-05 等
domain: 03_美术与表现
tags: [AI美术, ComfyUI, ControlNet, LoRA, 独立游戏, 2.5D, 工作流]
last_reviewed: 2026-04-24
review_count: 1
---

# AI 美术工作流（独立游戏场景）

> 适用场景：独立开发者用 AI 工具生产游戏美术资产，追求风格统一 + 低成本 + 批量可控。

---

## 工具链全图

| 阶段 | 工具 | 用途 |
|------|------|------|
| 前期概念 | MidJourney / ImageFX / Ideogram | 快速氛围图，确认风格方向 |
| 中期生产 | Stable Diffusion WebUI + ControlNet | 固定姿势/构图，批量生产 |
| 风格统一 | LoRA 微调 | 训练项目专属风格，解决角色/场景一致性 |
| 大图清晰 | Tiled Diffusion | 高分辨率细节强化 |
| 细节修补 | Inpaint | 局部重绘，修复瑕疵 |
| 后期修饰 | Photoshop / Clip Studio Paint | 边框调整、配色统一、手动修补 |
| 2.5D 方案 | Blender 45° 固定角度渲染 | 把 2D 图片渲染成游戏卡片风格 |
| 动效 | After Effects | 动态 Banner / UI 动效 |

---

## 标准生产流程（5 步）

**Step 1 — 确定基础风格**
- 收集 3–5 张风格一致的参考图
- 用 Img2Img 生成初版，调整直到风格对味
- 记录下使用的 Checkpoint + 核心 Prompt 关键词

**Step 2 — LoRA 训练（解决一致性问题）**
- 数据集：30–50 张风格一致的图
- 数据集质量决定输出质量，这是最重要的一步
- 训练后在同一 Checkpoint 环境使用

**Step 3 — ControlNet 叠加**
- 同时控制三个维度：透视 + 姿势 + 光影
- Strength 参数范围：0.7–0.9（过高会失去创意，过低控制力不足）

**Step 4 — 批量生产**
- 先生成 10 个版本，从中筛选最优
- 基于最优版本的参数做微调（改 seed 或 strength）
- 不要每次完全重做，浪费时间

**Step 5 — 后期整合**
- PS 统一配色、调整边框
- AI 生成式填充（Generative Fill）修补接缝
- Blender 可选 2.5D 处理（45° 固定视角渲染游戏卡片）

---

## Prompt 结构模板

```
主体描述 → 风格标签 → 构图控制 → 光影描述 → 细节补充
```

**不同资源类型的 Prompt 关键词**：

| 资源类型 | 推荐关键词 |
|---------|----------|
| 角色 | `character design, full body, clean lineart, game asset` |
| 场景 | `environment, concept art, atmospheric, depth of field` |
| UI 图标 | `icon, minimalistic, flat design, game UI` |
| Boss/怪物 | `creature design, menacing, detailed silhouette` |

---

## 风格控制关键技巧

1. **Checkpoint 是基础**：先选对 Checkpoint（日系二次元/黑暗奇幻等），再叠 LoRA
2. **同一环境生产所有同类资源**：换环境就换风格，一致性会崩
3. **固定 seed + 权重组合**：确定后记录下来，后续同类资源复用
4. **ControlNet strength = 0.7–0.9**：偏低更有创意，偏高更可控，根据任务选择
5. **分层生产**：不同类型资源（角色/场景/UI）分开批次，各自维护 Prompt

---

## 2.5D 低成本方案

适合独立开发者制作带立体感的游戏卡片/道具图：
1. 用 SD 生成 2D 素材
2. 导入 Blender，按 45° 固定摄像机角度
3. 光源固定（保证批量一致性）
4. 渲染输出，作为游戏内资产

成本优势：不需要 3D 建模能力，只需 Blender 基础操作。

---

## 踩过的坑

- 不同批次的 Checkpoint 不一样 → 风格不统一，需要重做
- 数据集质量差（图片清晰度不一、风格混杂）→ LoRA 训练失败，输出效果随机
- 不记录参数（seed、Checkpoint 版本、LoRA 权重）→ 效果好的图无法复现
- 对所有资源用同一个 ControlNet strength → 角色需要强控制（0.8+），场景可以宽松（0.7 左右）

---

## Style Bible（角色风格圣经）

> 来源：Google_光与朽美术_2026-04-03.md。生产任何角色资产前必须建立此文档。

**目的**：把模糊审美变成可量产的风格规范，解决"换了批次就像换了团队"问题。

### Style Bible 必须定义的 6 个维度

| 维度 | 选项 | 示例 |
|------|------|------|
| 游戏类型 | 2D / 俯视 / 横版 / 卡牌 / 塔防 | 竖版塔防 |
| 角色视角 | 正侧 / 45° / 正面 | 正面 |
| 比例风格 | Q版 / 半Q / 正常 / 夸张 | 半Q（头大身小） |
| 线条风格 | 无描边 / 细描边 / 粗描边 | 细描边 |
| 上色方式 | 平涂 / 轻渐变 / 复杂体积 | 轻渐变 |
| 光影要求 | 单主光源 / 无写实阴影 | 单主光源 |

### 风格锁定 Prompt（每次生成都必须附加）

```
same art style as previous characters,
consistent proportions and rendering style,
game asset style, not illustration,
simple shapes, high readability,
cartoon 2D game character sheet style
```

### 反向 Prompt（必须排除的风格污染）

```
realistic, anime style, painterly,
complex background, high detail texture,
photorealistic lighting, 3D render,
overly detailed armor, messy proportions
```

### 风格自审清单（每批资产生成后检查）

- [ ] 线条粗细一致？
- [ ] 形状语言统一（圆润/尖锐/方块，不混用）？
- [ ] 色彩饱和度/明度在同一范围？
- [ ] 材质表现一致（扁平 vs 体积感）？
- [ ] 光影复杂度一致？
- [ ] 所有角色视角一致？
