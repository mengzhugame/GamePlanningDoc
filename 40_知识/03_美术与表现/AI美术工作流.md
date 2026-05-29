---
type: knowledge
status: review
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

| 阶段      | 工具                                  | 用途                  |
| ------- | ----------------------------------- | ------------------- |
| 前期概念    | MidJourney / ImageFX / Ideogram     | 快速氛围图，确认风格方向        |
| 中期生产    | Stable Diffusion WebUI + ControlNet | 固定姿势/构图，批量生产        |
| 风格统一    | LoRA 微调                             | 训练项目专属风格，解决角色/场景一致性 |
| 大图清晰    | Tiled Diffusion                     | 高分辨率细节强化            |
| 细节修补    | Inpaint                             | 局部重绘，修复瑕疵           |
| 后期修饰    | Photoshop / Clip Studio Paint       | 边框调整、配色统一、手动修补      |
| 2.5D 方案 | Blender 45° 固定角度渲染                  | 把 2D 图片渲染成游戏卡片风格    |
| 动效      | After Effects                       | 动态 Banner / UI 动效   |

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

## 来源: `10_流水/历史聊天/ChatGPT_AI美术资源学习_2025-09-05.md` · 提取日期 2026-05-24

## AI 美术学习路线按生产阶段升级

不要一开始就把目标定成“训练自己的模型”。AI 美术能力应按项目阶段升级：

| 阶段 | 主要目标 | 推荐工具 | 学习重点 |
| --- | --- | --- | --- |
| 前期概念 | 快速探索角色、场景、UI 氛围，帮助立项选方向 | MidJourney、Ideogram、Flux、Krea、在线 SD | Prompt 结构、参考图、Moodboard |
| 中期生产 | 生成能进游戏的角色、怪物、道具和 UI，保证同项目一致性 | Stable Diffusion WebUI、ControlNet、LoRA、Inpaint | 数据集、姿势/构图控制、风格微调 |
| 后期修饰 | 宣传图、商店图、Banner、视频素材商用化 | Photoshop、Clip Studio、Runway、After Effects | 修边、调色、动效、广告规格 |

更适合独立开发者的学习顺序是：先玩现成工具理解生成逻辑，再学 ControlNet 固定构图，最后在项目方向确定后学习 LoRA。LoRA 的价值不是“显得专业”，而是当角色、场景、怪物和 UI 都需要长期批量一致时，降低返工成本。

## 一周入门计划的真正产物是风格方向板

7 天学习计划不要追求学完所有工具，而是产出一张“角色 - 场景 - UI”三栏 Moodboard：

| 天数 | 输出 |
| --- | --- |
| Day 1 | 5 张角色 + 5 张场景氛围图，记录喜欢的风格关键词 |
| Day 2 | 角色 / 场景 / UI 各 1 套 Prompt，保存 3 张可用参考 |
| Day 3 | 用 SD 或在线工具生成同一主题的 5 个版本，比较不同工具特性 |
| Day 4 | 用 Img2Img 锁构图换风格，理解“构图”和“风格”可分离 |
| Day 5 | 收集 10 张图做 Moodboard v1 |
| Day 6 | 生成 5 套 UI / 图标风格，检查是否能和角色场景同屏 |
| Day 7 | 复盘并决定是否进入 LoRA / ControlNet 阶段 |

如果一周后只有“我知道很多工具名”，但没有风格板、提示词版本和筛选理由，就还停留在工具消费，不算进入生产流程。

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

---

## 机制驱动的怪物设计工作流

> 来源：旧美术调研与《光与朽》美术资料。适用于"用 AI 大批量生产功能性游戏怪物"的场景。

**核心原则**：怪物的机制决定视觉——先定机制，再出美术，而不是反过来。

### 四步流程

**Step 1 — 机制转视觉**

| 机制特征 | 视觉转化方式 |
|---------|------------|
| 死后分裂 N 个 | 身体由 N 个隐约粘连的子体组成（剪影暗示机制） |
| 自爆 / 高危 | 膨胀球形 + 龟裂纹 + 中心高亮（"快要炸"的视觉张力） |
| 防护盾 | 外层有单独的护盾挂件（可被打碎的视觉层） |
| 岩浆污染地形 | 死亡特效覆盖范围比怪物体积大一圈，带呼吸闪烁 |

**Step 2 — 定剪影（形状设计）**

剪影优先，提示词之前先定：
- 第一章（同系列）基础剪影 → 第二章差异化（不要换色，要换形状）
- 三角形/倒三角/球形/不规则块状 → 不同剪影传达不同性格

**Step 3 — 三层拆层标准**

所有怪物统一拆成三层输出（配合 Unity Shader）：

| 图层 | 内容 | 制作方式 |
|------|------|---------|
| **Body RT**（底体层） | 纯色轮廓/流体底座，无高光细节 | PS 里用液化工具基于 AI 图手绘纯色剪影 |
| **Eyes**（眼睛层） | 发光核心/眼睛，传达情绪/机制 | PS 从完整图里抠出，透明 PNG |
| **Accessories**（挂件层） | 装甲/配饰/功能性附件 | PS 从完整图里抠出，透明 PNG |

**Step 4 — 2D 提示词模板（适用于 Lovart / SD）**

```
A 2D vector art illustration of [怪物名], top-down view.
[剪影描述]. The main body is [颜色+材质描述].
[眼睛/核心描述]. [挂件/装甲描述].
Cell shading, flat colors with distinct bright anime-style highlights,
clean sharp outlines, pure white background,
cute but dangerous, high quality 2D game asset,
matching the style of casual 2d mobile games.

Negative: 3d, 3d render, realistic, gradient shading,
complex background, blurry, pixel art, lowres, soft edges.
```

**注意**：2D 游戏的提示词里绝对不要出现 "stylized 3D"、"3D render" 等词，否则会破坏风格统一性。

### 实战案例（光与朽第二章）

| 怪物 | 机制 | 剪影思路 | 眼睛层 | 挂件层 |
|------|------|---------|-------|-------|
| 熔岩分裂怪 | 打死分裂3个 | 3圆球粘连体（暗示分裂结构） | 3只大小不一的黄色竖瞳 | 黑曜石碎块装甲 |
| 熔岩自爆怪 | 死后留岩浆斑 | 极度膨胀球体+边缘气泡 | 单只疯狂大眼 | 龟裂的黑岩外壳+裂缝高光 |

## 来源: `10_流水/Openclaw知识库文件/ai_art_workflow_guide.md` · 提取日期 2026-05-26

## ComfyUI 适合作为“概念验证后”的工业化管线

这份旧指南的增量价值是把 AI 美术从“会出图”推进到“可复用工作流”。对个人开发者来说，阶段顺序很关键：概念验证前优先用在线工具和少量母图快速试错；当项目方向已经成立、资产一致性成为瓶颈时，再进入 ComfyUI、LoRA、IPAdapter 和 ControlNet。

推荐升级阈值：

| 阶段 | 工具 | 目标 |
| --- | --- | --- |
| 概念验证 | Midjourney / 即梦 / ChatGPT Image / Krea | 快速验证题材和视觉钩子 |
| 小批量资产 | 在线 SD / 图生图 / 局部重绘 | 产出少量可进 Demo 的图 |
| 工业化生产 | ComfyUI + ControlNet + IPAdapter + LoRA | 批量、可复现、风格一致 |
| 项目专属风格 | 15-30 张高质量图训练 LoRA | 稳定角色、道具、UI 和场景风格 |

## 四类可复用工作流

| 工作流 | 输入 | 关键控制 | 人工修正比例 |
| --- | --- | --- | --- |
| Icon / 道具 | 文本 + 形状参考 | LoRA、IPAdapter、固定负面词 | 低到中 |
| UI 元素 | 草稿/线稿 + 风格图 | Lineart/Canny、IPAdapter | 中到高 |
| 角色立绘 | 草图 + 参考图 | Lineart + Depth + LoRA | 中 |
| 场景背景 | 文本 + 构图/深度 | Depth/Tile、Inpaint、超分 | 中 |

UI 资产不要完全交给 AI。按钮、边框、文字区、交互状态对像素精度要求高，AI 出图后通常仍需要 30%-50% 人工精修。

## LoRA 训练的最低可用标准

- 数据集 15-30 张起步，宁可少而统一，不要多而混乱。
- 统一裁剪和标注，人工修正自动标签里的风格词、颜色词和主体词。
- 每 500 步留 checkpoint，用固定 prompt 对比，不凭单张图判断。
- 过拟合信号是输出越来越像训练集本身，而不是学会风格。

项目专属 LoRA 的意义不是炫技，而是减少后续资产返工。只有当“风格漂移”已经成为持续成本时，训练才值得。

## Unity 集成先做半自动，不要一上来全自动

ComfyUI API 可以从 Unity Editor 发起生成并自动导入 Sprite/Texture，但建议先把它作为半自动工具：生成、下载、命名、人工 QC、再导入。全自动导入如果缺少审图和命名规则，会把大量脏资产直接塞进工程。

更稳的接入顺序：

1. 先固化工作流 JSON 和参数记录。
2. 统一输出目录、命名规则和尺寸。
3. 人工 QC 通过后再导入 Unity。
4. 最后再考虑 Editor Button 或批量导入脚本。

## 来源: `10_流水/Openclaw知识库文件/comfyui_advanced_guide.md` · 提取日期 2026-05-26

## ComfyUI 的节点工作流要按“可控模块”拆

ComfyUI 的优势不是界面更复杂，而是能把资产生产拆成可复用节点模块：加载模型、提示词编码、潜空间生成、采样、解码、预览/保存，每一步都可以被记录、替换和复用。

对游戏美术来说，最小理解模型是：

| 节点/模块 | 作用 | 游戏资产中的用途 |
| --- | --- | --- |
| Checkpoint / CLIP / VAE | 模型、文本理解和像素还原 | 固定项目基础画风 |
| Empty Latent / VAE Encode | 定义生成画布或图生图输入 | 控制尺寸、批次、参考图 |
| KSampler | 核心去噪与随机性控制 | 控制步数、CFG、采样器和 Denoise |
| Save / Preview | 输出和快速审图 | 保留版本和候选样张 |

在项目中不要把一个巨大的工作流当黑盒。更稳的拆法是：

```
底图 / 构图 -> 角色或道具约束 -> 局部修正 -> 高清放大 -> 命名导出 -> 人工 QC
```

## 进阶节点的实际使用边界

| 能力 | 适用 | 注意 |
| --- | --- | --- |
| LoRA | 固定角色、UI 按钮质感、材质风格 | 只有风格漂移成为持续成本时再训练 |
| ControlNet | 姿态、线稿、深度、3D 白模转绘 | 适合把设计约束前置，不适合无脑叠很多控制 |
| Hires / Upscale | 登录图、宣发海报、大图细节 | 先低分辨率定构图，再低 Denoise 放大 |
| Inpainting | 修手、替换图标、局部改装饰 | 只改局部，不破坏已通过的整体构图 |

这套能力适合“项目已经需要稳定产出一批相同风格资产”的阶段；如果只是概念视频或第一轮想法验证，在线工具和少量人工修正通常更快。

## 来源: `10_流水/Openclaw知识库文件/comfyui_expert_roadmap.md` · 提取日期 2026-05-26

## ComfyUI 学习路线要服务游戏生产，不要变成工具炫技

旧路线图把 ComfyUI 学习分成从节点理解到自动化管线的阶段。对当前用户更有用的版本如下：

| 阶段 | 学什么 | 过关标准 |
| --- | --- | --- |
| 初级 | VAE、CLIP、Checkpoint、KSampler、Latent、基础文生图/图生图 | 能稳定复现同一套工作流，不再靠随机抽卡 |
| 中级 | ControlNet、IPAdapter、批处理、透明底和自动命名 | 能批量生成同风格 UI / 道具 / 怪物候选 |
| 高级 | Upscale、Inpainting、Mask、SAM、逻辑节点 | 能局部修正资产，而不是整张重抽 |
| 资深 | AnimateDiff / SVD / Video-to-Video | 能做买量素材和动态展示，但必须先解决帧一致性 |
| 专家 | LoRA 训练、PBR 贴图、2D 转 3D、API 自动化 | 能把管线接入实际项目，而不是只会跑节点 |

当前最推荐的落地路径不是直接追“全自动 AI 工业化美术管线”，而是先做两个能马上改善生产的工作流：

1. `Character_Sprite_Pipeline.json`：草图 / 参考图 -> Lineart / Depth -> 风格图约束 -> 透明底 -> 人工 QC。
2. `UI_Button_Generator.json`：按钮草稿 -> 材质风格约束 -> 多状态输出 -> 手工精修文字区。

只有当这两个工作流在真实项目中反复节省时间，再考虑 Unity Editor 调 ComfyUI API、批量导入、远程生成和自动打包。自动化的顺序永远是：先稳定质量，再提高速度。

## 来源: `10_流水/Openclaw知识库文件/comfyui_knowledge_base.md` · 提取日期 2026-05-26

## ComfyUI 的进阶能力要按生产问题拆，而不是按插件名学

旧学习库里的增量价值，是把 ComfyUI 从“节点会用”推进到“资产生产线怎么稳定”。对独立游戏最有用的拆法如下：

| 生产问题 | 主工具 | 判断规则 |
| --- | --- | --- |
| 轮廓、姿态、空间跑偏 | ControlNet | Canny / Lineart 控轮廓，Depth 控空间，DWPose 控动作 |
| 一批图不像同一个项目 | IP-Adapter Plus | 用章节母图、图标母图、广告母图锁色彩、材质和光影 |
| 透明 PNG 和批量导出混乱 | RMBG / LayerDiffuse / Batch / Save 命名 | 已有图抠底用 RMBG，新图直接透明生成用 LayerDiffuse |
| 大图糊、接缝、局部乱长 | Pixel Upscale / USDU / Tile ControlNet | 先像素放大，再低 denoise 分块重绘，高 denoise 必须加约束 |
| 局部坏掉但整体能用 | Inpainting / Mask / Crop & Stitch | 重做走 inpaint 模型，微调用 Noise Mask，大图只修局部 |
| 手工调参太多 | Primitive / Math / Compare / Lazy Evaluation | 参数集中、自动算尺寸、自动选放大链和导出分支 |
| 静态图需要做投放素材 | SVD / AnimateDiff / VideoHelperSuite | SVD 快速测钩子，AnimateDiff 做可控循环和 vid2vid |

## ControlNet 和 IP-Adapter 的职责边界

ControlNet 是结构约束，不是风格补丁。`Apply ControlNet` 不会自动把普通图变成 Canny、Depth、Pose 或 Lineart，必须先做对应预处理；多路 ControlNet 也应链式叠加，并保持一个主控制器、一个到两个辅助控制器。

IP-Adapter 更像“1-image LoRA”：用一张风格母图给后续生成提供视觉参照。它不适合单独锁姿态或轮廓，但适合解决同章怪物、同套 UI、同批广告图的风格漂移。起手参数可以保守一些：`weight 0.75-0.9`、`steps 28-36`、`cfg 5-7`。

对游戏资产最稳的组合是：

```text
结构草图 -> Lineart / Canny ControlNet
风格母图 -> IP-Adapter Plus
统一 prompt / negative / checkpoint
-> KSampler -> 筛选 -> 局部修补 -> 导出
```

## 透明素材管线要同时保 RGB 和 Mask

RMBG / BiRefNet 适合现成图抠底，LayerDiffuse 适合生成阶段直接产透明前景。两者不要混用成一个万能按钮。

生产级输出不要只留一张 RGBA。更稳的导出是：

| 文件 | 用途 |
| --- | --- |
| RGB foreground | 后续放大、修边、描边、压缩 |
| Alpha / mask | Unity 合成、二次抠边、质量检查 |
| RGBA preview | 快速浏览和临时使用 |

批处理前要统一画布、居中和目标尺寸；批处理后要把日期、章节、分辨率、原文件名和处理类型写进命名。否则素材越多，ComfyUI 省下的时间会在 Unity 导入和找文件时还回去。

## 大图和局部修补要先保形，再补细节

高清放大不要一上来高 denoise。更稳的路线是：

```text
中分辨率定构图 -> Pixel Upscale -> Ultimate SD Upscale 低 denoise -> 必要时 Tile ControlNet
```

推荐起手：

| 资产 | 路线 |
| --- | --- |
| UI 图标 | Pixel Upscale，必要时 USDU `denoise 0.05-0.1` |
| 宣传图 / 商店头图 | Pixel Upscale 后 USDU，`denoise 0.10-0.15` |
| Boss / 怪物大图 | USDU + Tile ControlNet，Tile 强度 `0.35-0.5` |

局部重绘的核心不是 prompt，而是 mask。想重做遮罩区，走 `VAE Encode (for Inpainting)` 和专用 inpaint 模型；想微创精修，走 `Set Latent Noise Mask` 或 `InpaintModelConditioning`，并把 denoise 控在 `0.2-0.4` 起步。2K/4K 图只改一块时，优先 Crop & Stitch，不要整张重新采样。

## 逻辑节点的目标是让工作流自己选路

当工作流开始同时服务 UI 图标、怪物卡面、宣传图、透明底资产时，手工改尺寸和开关会成为新瓶颈。最该先自动化的不是复杂 Loop，而是四件事：

1. 用 Primitive 集中管理目标长边、denoise、CFG、导出开关。
2. 用 Math 计算 `scale = TargetMax / max(width, height)`。
3. 用 Compare 决定直出、单段放大还是二段放大。
4. 用资产类型决定是否走 RMBG / LayerDiffuse / 普通保存。

注意区分数据选路和执行选路。表面 Switch 不一定省算力，真正能跳过未使用分支的是 Lazy Evaluation 或具备执行控制的分支节点。

## 视频管线先做 1-2 秒钩子，不要先做长片

SVD 适合把一张 Boss 立绘、怪物卡面或场景概念图变成 14 / 25 帧轻动效，用于测试“这张图动起来是否更抓眼”。AnimateDiff 适合可控循环、角色待机、实机录屏转绘和 vid2vid。

推荐顺序：

1. 先用静态图或 SDXL 出 3 个关键帧方向。
2. 每个方向用 SVD 做 14 帧测试版。
3. 只把停留感最强的方向送进 AnimateDiff 精修。
4. vid2vid 起手用 `8fps + 16 帧窗口 + overlap 4 + denoise 0.25-0.35`。

视频转绘第一目标是时序一致，不是每帧美图。对《光与朽》这种强玩法项目，优先试激光塔发射、敌人被切穿、Boss 待机或进场这类 1-2 秒核心爽点；长广告必须等短钩子稳定后再做。

## 来源: `10_流水/Openclaw知识库文件/game_art_learning_roadmap.md` · 提取日期 2026-05-26

## 美术能力路线要按生产瓶颈分层

独立游戏的美术学习不应按“软件清单”推进，而应按项目生产瓶颈推进。用户当前最有价值的路线不是成为纯画师，而是成为能判断风格、拆资源、控一致性、理解 Unity 表现和性能边界的 π 型美术/TA。

分层路线：

| 阶段 | 核心目标 | 项目验收 |
| --- | --- | --- |
| 初级 | UI、切图、九宫格、分辨率适配、基础建模 | 能指出现有项目 UI 与资源导入问题 |
| 中级 | PBR、UX 反馈、ZBrush/Substance、动画十二原则 | 能让角色、场景和交互反馈统一 |
| 高级 | 风格控制、URP/HDRP、VFX、Shader Graph | 能把概念图还原成可跑的 Unity 资产 |
| 资深 | 美术管理、资源规范、性能预算 | 能控制 draw call、贴图内存和批量导入 |
| 专家 | TA 工具、自动化、渲染管线判断 | 能把 AI 资产生产接进稳定工程流程 |

对小团队来说，美术能力的第一性目标是“能稳定产出项目可用资产”，不是“每张图都单独好看”。AI、ComfyUI、ControlNet、LoRA、Blender 和 Unity Shader 都要围绕同一件事服务：更低成本地保持风格一致、读图清楚、性能可控。
