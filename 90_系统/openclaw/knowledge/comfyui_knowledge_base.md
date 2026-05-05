# 📚 ComfyUI 模型生态初探学习笔记 (2026-04-01)

## 🎯 节点/技术：SD 1.5 vs SDXL 架构差异与模型加载策略

### 📖 学习要点：模型架构深度解析

#### 1. SD 1.5 vs SDXL 的本质差异
- **SD 1.5**：基于 512x512 训练，模型体积较小，显存需求较低，生态成熟，LoRA/Embedding 资源海量。
- **SDXL**：原生面向更高分辨率，双文本编码器结构更强，语义理解和细节表现更好，但显存占用更高，推理更慢。
- **实际选择原则**：
  - **快速出草图 / 批量探索** → SD 1.5 更轻更快
  - **商业成品 / 高质感宣传图** → SDXL 更稳更强
  - **低显存机器** → 优先 SD 1.5

#### 2. ComfyUI 中模型加载的底层逻辑
在 ComfyUI 里，Checkpoint 不只是“一个模型包”，它通常同时包含：
- UNet（真正去噪作画）
- CLIP（理解提示词）
- VAE（潜空间与像素空间转换）

标准 `Checkpoint Loader` 节点会直接输出三路：
- `MODEL`
- `CLIP`
- `VAE`

这就是为什么 ComfyUI 的节点流本质比 WebUI 更清晰，你能直接看到模型内部资源如何被拆开使用。

---

### 🎮 应用示例：在《光与朽》项目中的选择策略
- **怪物概念草图 / 技能图标快速迭代**：优先 SD 1.5 + 风格 LoRA，速度快，适合大量试错。
- **商店头图 / 宣传 KV / 高质感角色立绘**：优先 SDXL + 精选 LoRA，细节和构图理解更好。
- **统一 UI 图标风格**：固定同一 Checkpoint + 同一组 LoRA 权重，比频繁换大模型更稳定。

---

### ⚙️ 参数配置：Checkpoint、LoRA、Embedding 节点连线

#### 1. 基础模型加载链
```text
Load Checkpoint
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode (正/负提示词)
 └─ VAE → VAE Decode
```

#### 2. 加入 LoRA 后的标准链路
```text
Load Checkpoint
 ├─ MODEL → Load LoRA → KSampler
 ├─ CLIP  → Load LoRA → CLIP Text Encode
 └─ VAE   → VAE Decode
```

#### 3. Embedding 的使用方式
Embedding 不走独立加载器节点，而是：
- 放进 `models/embeddings`
- 在 `CLIP Text Encode` 的提示词中直接调用
- 典型写法：
```text
embedding:badhandv4
embedding:my_style_token
```

---

### 🔄 流程分析：节点连线逻辑

#### Checkpoint 的职责
- 决定基础审美、基础能力、整体世界观上限。
- 它像“主脑模型”，LoRA 是外挂补丁，Embedding 是词汇捷径/局部概念强化。

#### LoRA 的职责
- 在不替换基础大模型的前提下，给模型追加某种风格、角色特征、材质倾向或动作偏好。
- 本质是对 `MODEL` 和 `CLIP` 做补丁。

#### Embedding 的职责
- 通过一个特殊 token，把训练好的概念直接注入提示词语义空间。
- 常见用途：负向瑕疵修正、某种固定画风/人脸/质量补偿。

---

### 🚀 硬件适配策略
- **8GB 及以下显存**：主打 SD 1.5，分辨率控制在 512~768，优先做概念探索。
- **12GB 左右显存**：可以较稳定使用 SDXL 基础工作流。
- **高显存机器**：适合 SDXL + ControlNet + 放大工作流并行推进。

---

### 📌 今日重点学习知识点
1. Checkpoint 决定基础盘子，LoRA 决定风格补丁，Embedding 决定词汇级概念注入。
2. ComfyUI 最大优势之一，就是把 MODEL / CLIP / VAE 分离出来，让你能精确知道每个模块在干什么。
3. 《光与朽》最优策略不是天天换大模型，而是固定一个主 Checkpoint，再用 LoRA 和 Embedding 做风格锁定与微调。

## 节点知识：ComfyUI 架构与数据流转逻辑 (VAE, CLIP, Checkpoint, KSampler, Latent)

### 🎯 核心概念

#### 1. **Checkpoint（模型权重包）**
- **作用**：整个生成系统的“大脑”或“知识总包”
- **包含**：UNet + CLIP + VAE（有些模型内置VAE，有些需外接）
- **理解**：它决定你生成图像的基础风格、知识边界和能力上限
- **在ComfyUI中的角色**：通过 `Load Checkpoint` 节点拆分出 MODEL / CLIP / VAE 三路输出

#### 2. **CLIP（文本理解器）**
- **作用**：把提示词（Prompt）翻译成模型能理解的条件向量
- **理解**：你说“破败圣光祭坛、暗金纹理、激光核心”，CLIP负责把这些中文/英文概念编码成AI能读懂的语义
- **关键点**：
  - 正向提示词 → 告诉AI“你该画什么”
  - 负向提示词 → 告诉AI“你别画什么”
- **在ComfyUI中的角色**：通过 `CLIP Text Encode` 节点把文本转成 conditioning

#### 3. **Latent Space（潜空间）**
- **作用**：AI真正“作画”的隐空间，不是在RGB像素层直接瞎画
- **理解**：它像一个压缩过的图像草稿宇宙，模型先在这里构图、塑形、定细节，再交给VAE解码成真实图片
- **意义**：
  - 节省计算资源
  - 让扩散模型生成更高效
- **在ComfyUI中的体现**：`Empty Latent Image` 提供初始画布，`KSampler` 在Latent里反复去噪

#### 4. **KSampler（采样器）**
- **作用**：生成流程的“发动机”
- **做的事**：
  1. 从随机噪声或已有 latent 开始
  2. 按照 CLIP 给出的提示条件不断去噪
  3. 最终得到清晰的 latent 图像
- **关键参数**：
  - `seed`：随机种子，决定初始噪声
  - `steps`：采样步数，越多通常越细，但更慢
  - `cfg`：提示词服从强度
  - `sampler_name` / `scheduler`：采样算法与调度方式
  - `denoise`：图生图时控制“保留原图 vs 重绘幅度”

#### 5. **VAE（编码/解码器）**
- **作用**：潜空间与像素空间之间的翻译官
- **两个方向**：
  - `VAE Encode`：把真实图像压进 latent space
  - `VAE Decode`：把 latent 还原成最终图片
- **理解**：如果没有VAE，你的 latent 只是AI内部草稿，人眼根本看不了

---

### 🔄 标准数据流逻辑

#### TXT2IMG（文生图）工作流
```text
Checkpoint Loader
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode（正向/负向）→ KSampler
 └─ VAE → VAE Decode

Empty Latent Image → KSampler → VAE Decode → Save Image
```

#### IMG2IMG（图生图）工作流
```text
Load Image → VAE Encode → KSampler → VAE Decode → Save Image
Checkpoint Loader
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode（正向/负向）→ KSampler
 └─ VAE → VAE Encode / VAE Decode
```

**核心区别：**
- TXT2IMG：从空 latent 开始
- IMG2IMG：从已有图像编码成 latent 后再重绘

---

### 🎮 对《光与朽》的直接应用理解

#### 1. 怪物/炮塔概念图
- 用 TXT2IMG 快速探索方向
- 通过 seed 固定画面结构，再调 prompt 微调风格

#### 2. UI 图标/技能图标
- 用同一 checkpoint + prompt 模板保持统一风格
- 可后续接 LoRA 做风格锁死

#### 3. 草图精修
- 手绘草图导入后走 IMG2IMG
- 用较低 denoise 保留结构，只增强材质和完成度

---

### ⚠️ 最容易搞混的几个点
- **Checkpoint ≠ 单纯“大模型文件”**，它在 ComfyUI 里会拆出多种能力模块
- **CLIP 不负责画图**，它只负责“理解你说了什么”
- **KSampler 不会凭空理解语义**，它必须依赖 MODEL + conditioning
- **VAE 不是可有可无**，它决定图像还原质量和色彩表现
- **Latent 不是低清图**，它是模型内部语义画布

---

### ✅ 一句话总结
ComfyUI 的本质不是“点按钮生图”，而是：
**Checkpoint 提供世界知识，CLIP 理解指令，KSampler 在 Latent Space 里执行创作，VAE 把结果翻译成人类能看的图。**

这条逻辑打通了，后面学 ControlNet、IP-Adapter、局部重绘、视频生成才不会一团浆糊。

# 📚 ComfyUI 初级节点精讲学习笔记 (2026-04-09)

## 🎯 今日攻克节点：ComfyUI 架构思维解析 —— VAE / CLIP / Checkpoint / KSampler / Latent Space

### 一、为什么这才是最该先啃下的节点
主人，ComfyUI 真正牛逼的地方，不是“节点很多”，而是它把 Stable Diffusion 这套黑盒拆成了可观察、可替换、可组合的数据流。

你如果不先搞懂 `Checkpoint → CLIP → Latent → KSampler → VAE` 这条主干链，后面学 ControlNet、IP-Adapter、局部重绘、放大、视频，全都会变成死记硬背。那种学法，纯属浪费时间。

所以今天这一步不是入门小菜，而是**整个 AI 美术管线的总开关**。

---

### 二、核心数据流：主人必须背下来的主干工作流

#### 1）标准 TXT2IMG 文生图主链路

```text
Load Checkpoint
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode（正向）→ KSampler
 │        └→ CLIP Text Encode（负向）→ KSampler
 └─ VAE → VAE Decode

Empty Latent Image → KSampler → VAE Decode → Save Image
```

#### 2）这条链路本质在干什么
- `Load Checkpoint`：加载大模型，把内部能力拆成 MODEL / CLIP / VAE
- `CLIP Text Encode`：把你写的 prompt 变成模型能理解的条件信号
- `Empty Latent Image`：给 AI 一张还没画的潜空间画布
- `KSampler`：按照提示词，在 latent space 里从噪声一步步去噪成图
- `VAE Decode`：把 latent 草稿翻译成真实像素图
- `Save Image`：保存结果

一句话，**不是 AI 直接在 JPG 上画图，而是在 latent 里“思考”，最后再翻译回图片。**

---

### 三、五大核心概念拆解

#### A. Checkpoint：大脑总包
Checkpoint 不只是一个“模型文件”，它本质上是一整套训练好的视觉知识体系。

它至少影响三件事：
1. 会画什么
2. 擅长什么风格
3. 对提示词理解到什么程度

在 ComfyUI 里，`Load Checkpoint` 往往输出：
- `MODEL`
- `CLIP`
- `VAE`

这就是 ComfyUI 比 WebUI 更强的一点，它把大脑拆开给你看。

**项目理解：**
- 想做《光与朽》的暗金、衰败、圣光腐蚀感，首先要选对 checkpoint
- checkpoint 选错，后面 prompt 再努力都像在给垃圾底盘贴金箔

#### B. CLIP：把人话翻译成条件向量
CLIP 本身不画图，它负责把你写的文字翻译成 AI 能执行的“语义条件”。

比如你输入：
```text
holy decayed laser tower, dark gold metal, glowing core, ruined cathedral style
```
CLIP 会把这些概念编码成 conditioning，送给 KSampler。

所以：
- 正向提示词 = 让它画什么
- 负向提示词 = 让它别画什么

**核心认知：**
提示词不是魔法咒语，本质是喂给 CLIP 的语义指令。

#### C. Latent Space：AI 真正作画的地方
Latent Space 不是缩略图，也不是低清 JPG。
它是一个高压缩、高语义密度的内部空间。

模型不会直接在像素图层上画，而是先在 latent 里决定：
- 构图
- 轮廓
- 光影关系
- 材质倾向

然后 VAE 再把 latent 解码成人类能看的图像。

**为什么这重要？**
因为你后面会学：
- latent 放大
- latent 混图
- latent 局部重绘
- latent 噪声控制

这些高级操作全都是在这个空间里搞事情。

#### D. KSampler：从噪声到图像的核心引擎
KSampler 是整套工作流最像“发动机”的节点。

它做的事是：
1. 拿到初始 latent（空白噪声或已有图像编码结果）
2. 根据 MODEL 和 conditioning 一步步去噪
3. 输出最终 latent 图

关键参数你必须记住：
- `seed`：随机种子，决定初始噪声，想复现就固定它
- `steps`：采样步数，越高通常越细，但收益递减
- `cfg`：提示词服从度，越高越听话，但容易僵硬
- `sampler_name`：采样算法，不同算法风格手感不同
- `scheduler`：噪声调度策略
- `denoise`：图生图时控制“保留原图多少”

**最实战的一句：**
KSampler 决定你这图是“稳稳地收敛”，还是“胡几把乱长”。

#### E. VAE：潜空间与像素空间的翻译官
VAE 是编码器 + 解码器系统。

- `VAE Encode`：把图像压进 latent
- `VAE Decode`：把 latent 还原成图像

没有它，你根本看不到 latent 的结果。

它还会影响：
- 色彩
- 对比度
- 细节观感
- 某些模型的还原稳定性

**项目上的意义：**
如果你做 UI 图标、怪物卡牌、宣传图，VAE 不稳会直接导致颜色脏、发灰、材质糊。

---

### 四、文生图 vs 图生图：数据流差异

#### TXT2IMG 文生图
从空 latent 开始：
```text
Empty Latent Image → KSampler → VAE Decode
```
用途：
- 新概念探索
- 怪物方向草案
- 技能图标初版

#### IMG2IMG 图生图
从已有图片开始：
```text
Load Image → VAE Encode → KSampler → VAE Decode
```
用途：
- 用草图控制结构
- 精修已有图
- 保留构图只改风格

**本质区别：**
- TXT2IMG = 完全从噪声创作
- IMG2IMG = 带着原图记忆去重绘

主人后面做《光与朽》美术时，这个差异非常关键。你有草图就别傻乎乎纯文生图赌结构。

---

### 五、给主人的一套最稳入门参数

#### 文生图通用模板（适合风格探索）
- 分辨率：`768 x 768`
- steps：`24~30`
- CFG：`6.5~8`
- sampler：`dpmpp_2m_sde` 或 `euler`
- scheduler：`karras`
- seed：先随机，出方向后固定

#### 图生图通用模板（适合修草图/统一风格）
- denoise：`0.35 ~ 0.65`
  - `0.2~0.35`：轻修
  - `0.4~0.6`：中度重绘
  - `0.7+`：基本重做
- steps：`20~28`
- CFG：`5.5~7.5`

**原则：**
先稳，再猛。别一上来就 50 steps + 高 CFG，把图抽风搞炸。

---

### 六、《光与朽》落地应用：别学了只会看，不会用

#### 应用 1：怪物概念图探索
目标：快速找方向

流程：
- 选定统一 checkpoint
- 用 TXT2IMG 批量生成
- 固定 seed 微调关键词
- 观察哪些关键词能稳定出“腐朽圣光机械体”的感觉

#### 应用 2：UI 图标批量风格统一
目标：所有技能/炮塔图标看起来像同一游戏出的

流程：
- 固定 checkpoint + 分辨率 + prompt 模板
- 每次只替换核心物件词
- 控制统一材质词、光效词、背景简化词

#### 应用 3：旧草图重绘成商用图
目标：把你的手绘草图或低保真概念稿快速提成高完成度图

流程：
- Load Image → VAE Encode → KSampler
- denoise 控制在 0.35~0.55
- 保留结构，只提升材质、光影、完成度

这才是 AI 真正该干的活，不是拿它抽卡。

---

### 七、常见报错与排查思路

#### 1. 图出不来
先查：
- MODEL 有没有接到 KSampler
- 正负 prompt conditioning 有没有接进去
- latent 有没有输入
- VAE Decode 有没有接对

#### 2. 图像尺寸不对
查 `Empty Latent Image` 或上游 latent 尺寸设定。

#### 3. 画面完全不听提示词
先怀疑：
- checkpoint 不擅长该风格
- CFG 太低
- prompt 写得太散
- 负向词和正向词冲突

#### 4. 图生图改得太狠或几乎没改
直接看 `denoise`：
- 太高 = 原图保不住
- 太低 = 改不动

---

### 八、今天的关键结论
1. ComfyUI 的核心不是节点多，而是数据流透明。
2. Checkpoint 决定世界知识，CLIP 决定语义理解，KSampler 决定生成过程，VAE 决定可视化输出。
3. Latent Space 是后续所有高级玩法的战场。
4. 《光与朽》项目最适合先用这条主干流搭出概念图、图标和草图精修三套模板。
5. 后面学 LoRA、ControlNet、局部重绘时，都只是往这条主干上外挂控制器，不是另起炉灶。

---

### 九、参考来源
- ComfyUI 官方仓库 README
- ComfyUI Community Manual
- ComfyUI examples 官方示例
- 稳定扩散工作流基础原理文档

# 📚 ComfyUI 架构思维解析学习笔记 (2026-04-05)

## 🎯 今日推进节点
**主题：彻底搞懂 ComfyUI 的工作流本质，以及 TXT2IMG / IMG2IMG 的核心节点链路。**

这一步如果没吃透，后面学 ControlNet、IP-Adapter、局部重绘、视频生成，全都会学成一堆散装插件。那种学习方式，效率低得离谱。

---

## 一、为什么 ComfyUI 比 WebUI 更适合做工业化美术管线

### 1. WebUI 思维：面板参数堆叠
WebUI 的问题不是不能出图，而是它把整条生成链藏在按钮后面。
你只能“调结果”，但看不见内部数据怎么流。

坏处有三个：
- 出错时你不知道哪一环炸了
- 想复用工作流时难拆分
- 想加入复杂控制（多 ControlNet / 自动批量 / 逻辑判断）时容易一坨屎

### 2. ComfyUI 思维：数据流 + 节点职责
ComfyUI 的强，是把整个流程拆开：
- 谁负责理解文字
- 谁负责生成 latent
- 谁负责去噪
- 谁负责解码成图

这就意味着：
- 你可以精准替换某一段能力
- 可以做批处理和自动化
- 可以为《光与朽》做成真正能复用的生产流水线

一句话：
**WebUI 更像操作台，ComfyUI 更像工厂流水线控制面板。**

---

## 二、核心节点到底各管什么

### 1. Checkpoint Loader / Load Checkpoint
这是整个工作流的总入口。

它加载的不是“一个图像模型”这么简单，而是一整个生成体系。通常会输出：
- `MODEL`
- `CLIP`
- `VAE`

可理解为：
- `MODEL`：负责真正的图像去噪与生成
- `CLIP`：负责理解你输入的提示词
- `VAE`：负责 latent 和像素图之间的转换

你可以把 Checkpoint 理解成：
**整套画风与知识库的总包。**

---

### 2. CLIP Text Encode
这个节点负责把提示词转换成模型能理解的条件向量。

它本身不画图。
它只是把：
```text
holy laser core, dark fantasy, ruined cathedral, gold metal, decayed sacred machinery
```
翻译成 AI 可执行的语义条件。

重点：
- 正向提示词 = 要什么
- 负向提示词 = 不要什么

如果 CLIP 编得不好，KSampler 后面再努力也是白搭。

---

### 3. Empty Latent Image
这个节点提供一个“空白 latent 画布”。

不是 JPG。
不是 PNG。
而是模型内部真正工作的潜空间初始容器。

你可以理解成：
**画纸不是像素纸，而是潜空间纸。**

设定的宽高，本质是 latent 对应的目标图尺寸。

---

### 4. KSampler
这是发动机。

作用：
- 接收 MODEL
- 接收 prompt conditioning
- 接收初始 latent
- 按采样算法一步步从噪声逼近目标图像

关键参数：
- `seed`：随机起点
- `steps`：迭代次数
- `cfg`：提示词服从强度
- `sampler_name`：采样方法
- `scheduler`：去噪调度策略
- `denoise`：图生图时决定改动幅度

如果说 Checkpoint 是大脑，KSampler 就是执行大脑命令的手术机器人。

---

### 5. VAE Decode
KSampler 输出的仍然是 latent，不是最终图片。

`VAE Decode` 负责把 latent 解码成人眼能看的图。

没有这个节点，你拿到的只是 AI 内部草稿。

反过来，IMG2IMG 时常会多一个 `VAE Encode`：
- 把现有图像编码进 latent
- 再让 KSampler 基于它继续重绘

---

### 6. Save Image
这个就不装深沉了，就是保存图像。
但它在工作流里非常重要，因为：
- 你可以接分支保存不同阶段结果
- 后面批处理时会影响命名与导出管理

---

## 三、标准 TXT2IMG 文生图数据流

### 标准最小闭环
```text
Load Checkpoint
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode（正向）→ KSampler
 │        └→ CLIP Text Encode（负向）→ KSampler
 └─ VAE → VAE Decode

Empty Latent Image → KSampler → VAE Decode → Save Image
```

### 数据类型视角再看一遍
- `Load Checkpoint` 输出的是模型资源
- `CLIP Text Encode` 输出的是 conditioning
- `Empty Latent Image` 输出的是 latent
- `KSampler` 输出的是处理后的 latent
- `VAE Decode` 输出的是 image

ComfyUI 真正专业的点就在这儿：
**每条线不是“连着玩”，而是明确的数据类型传递。**

---

## 四、IMG2IMG 图生图为什么会多一个 VAE Encode

### 标准 IMG2IMG 流程
```text
Load Image → VAE Encode → KSampler → VAE Decode → Save Image

Load Checkpoint
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode（正向/负向）→ KSampler
 └─ VAE → VAE Encode / VAE Decode
```

### 关键参数：denoise
这个参数极其关键。

- `0.1 ~ 0.3`：轻修，基本保留原图
- `0.35 ~ 0.6`：中度改造，适合风格统一
- `0.7 ~ 1.0`：大改，接近重新生成

给主人的一句人话：
**denoise 越高，AI 越不把你的原图当回事。**

---

## 五、KSampler 关键参数怎么理解

### 1. seed
固定随机起点。

用途：
- 复现同一张图
- AB 测试 prompt
- 稳定量产同风格变体

### 2. steps
迭代次数。

不是越高越牛。
通常：
- 20~30 足够大多数基础工作流
- 40 往上收益变小，速度还更慢

### 3. CFG
提示词服从度。

- 太低：不听话
- 太高：僵硬、容易脏、容易炸构图

大多数情况下：
- `6~8` 比较稳

### 4. sampler_name
影响细节表现和收敛手感。
常见如：
- `euler`
- `dpmpp_2m`
- `dpmpp_2m_sde`

### 5. scheduler
常见如 `normal`、`karras`。
一般初期可优先用 `karras`，整体比较稳。

### 6. denoise
图生图灵魂参数，上面已经说了，不再重复。

---

## 六、Latent Space 到底是什么

### 1. 潜空间不是“缩略图”
很多新手会误以为 latent 就是低清图。
这理解不对。

latent 是一种高压缩的语义表达空间，里面存的不是直接像素，而是图像结构与语义特征。

### 2. 为什么 AI 要在潜空间作画
因为直接在像素空间上做扩散，成本太高。

在 latent 里工作：
- 更快
- 更省显存
- 更适合多种图像操作

### 3. 实际意义
后面你会学到：
- latent upscale
- latent composite
- latent noise injection
- latent inpainting

这些全都建立在你理解 latent 是“AI 画布”这件事上。

---

## 七、正向提示词与负向提示词在流里怎么工作

### 正向提示词
告诉模型：
- 主体是什么
- 风格是什么
- 材质是什么
- 光影与镜头感是什么

### 负向提示词
告诉模型：
- 不要低质量
- 不要多肢体
- 不要文字水印
- 不要多余噪点和脏细节

### 在 ComfyUI 里
正负向一般各接一个 `CLIP Text Encode`，再一起送入 KSampler。

所以：
**提示词不是直接进模型，而是先被 CLIP 编码成条件信号。**

---

## 八、常见连线错误与故障判断

### 1. 图出不来
优先检查：
- KSampler 有没有同时接到 MODEL 和 conditioning
- 有没有 latent 输入
- VAE Decode 有没有接到 KSampler 输出

### 2. 图像尺寸不对
看 `Empty Latent Image` 的宽高。

### 3. 画面完全不听提示词
看三件事：
- checkpoint 是否匹配风格
- CFG 是否过低
- prompt 是否太散、太贪

### 4. 图生图改得太狠或几乎没改
直接看 `denoise`。

---

## 九、面向《光与朽》的落地应用

### 1. 怪物概念图
用 TXT2IMG 批量探索“圣光腐坏机械体”方向。
- 固定 checkpoint
- 控制关键词结构一致
- 用不同 seed 扩大量级探索

### 2. UI 图标与技能按钮
建立统一模板：
- 相同 checkpoint
- 相同尺寸
- 相同材质词和光影词
- 仅替换核心物件词

这才有“同一游戏宇宙”感。

### 3. 场景宣传图
先用 TXT2IMG 出氛围，再视情况接后续放大与局部修补。

---

## 十、今天的核心结论
1. ComfyUI 的核心不是节点多，而是数据流透明。
2. `Checkpoint → CLIP → KSampler → VAE` 是必须背熟的主干链。
3. TXT2IMG 与 IMG2IMG 只是初始 latent 来源不同。
4. 理解 latent space，后面所有高级玩法才有根。
5. 对《光与朽》而言，ComfyUI 最先落地的价值就是概念图探索、图标统一和草图精修。

---

## 十一、给后续学习的承接点
下一步最该学的，不是乱装插件，而是：
1. `TXT2IMG / IMG2IMG` 标准工作流熟练搭建
2. Prompt 权重控制
3. Checkpoint / LoRA / Embedding 的组合逻辑

这三步打牢了，后面才配学风格锁定和工业化量产。

---

## 参考来源
- ComfyUI 官方仓库 README
- ComfyUI Examples 官方示例
- 社区节点文档与工作流资料

# 📚 ComfyUI 基础工作流搭建实战学习笔记 (2026-04-09)

## 🎯 今日推进节点
**从零搭建标准 TXT2IMG 与 IMG2IMG 工作流，并搞懂两者到底差在哪。**

这一步就是从“看懂架构”进入“能自己搭线干活”。
如果今天这块没吃透，后面装再多节点都是摆设。

---

## 一、核心结论：TXT2IMG 和 IMG2IMG 在 ComfyUI 里本质是同一套采样逻辑
它们不是两门技术。
它们只是**KSampler 吃进去的 latent 来源不同**：
- TXT2IMG：吃空白 latent + 随机噪声
- IMG2IMG：吃由原图编码来的 latent

也就是说，主干不变：
```text
MODEL + CONDITIONING + LATENT → KSampler → 新 LATENT → VAE Decode → Image
```

差别只在最前面那一段。

---

## 二、标准 TXT2IMG 工作流

### 1. 节点清单
最小可运行版本：
- `Load Checkpoint`
- `CLIP Text Encode`（正向）
- `CLIP Text Encode`（负向）
- `Empty Latent Image`
- `KSampler`
- `VAE Decode`
- `Save Image`

### 2. 连线逻辑
```text
Load Checkpoint
 ├─ MODEL → KSampler.model
 ├─ CLIP → CLIP Text Encode（正向）→ KSampler.positive
 │        └→ CLIP Text Encode（负向）→ KSampler.negative
 └─ VAE → VAE Decode.vae

Empty Latent Image → KSampler.latent_image
KSampler → VAE Decode.samples
VAE Decode → Save Image.images
```

### 3. 每个节点到底在干嘛
- `Load Checkpoint`：加载大模型，把 MODEL / CLIP / VAE 拆出来
- `CLIP Text Encode`：把提示词编码成条件
- `Empty Latent Image`：创建空 latent 画布，顺便定义宽高
- `KSampler`：从噪声中一步步生成 latent 图
- `VAE Decode`：把 latent 解码成图片
- `Save Image`：落盘

### 4. TXT2IMG 关键参数建议

#### `Empty Latent Image`
- 宽高建议先用：
  - 768×768
  - 768×1024
  - 1024×1024（看显存）

#### `KSampler`
- `steps`：24~30
- `cfg`：6~8
- `sampler_name`：`dpmpp_2m_sde` / `euler`
- `scheduler`：`karras`
- `seed`：先随机，出方向后固定

### 5. TXT2IMG 常见错误
- 没接负向 conditioning 不是不能跑，但图更容易脏
- 忘记接 VAE Decode，结果看不到图
- 宽高开太猛直接爆显存
- CFG 拉太高，结果图像僵硬、过饱和、细节乱飞

---

## 三、标准 IMG2IMG 工作流

### 1. 节点清单
比 TXT2IMG 多两样：
- `Load Image`
- `VAE Encode`

完整链：
- `Load Checkpoint`
- `Load Image`
- `VAE Encode`
- `CLIP Text Encode`（正向）
- `CLIP Text Encode`（负向）
- `KSampler`
- `VAE Decode`
- `Save Image`

### 2. 连线逻辑
```text
Load Checkpoint
 ├─ MODEL → KSampler.model
 ├─ CLIP → CLIP Text Encode（正向）→ KSampler.positive
 │        └→ CLIP Text Encode（负向）→ KSampler.negative
 └─ VAE → VAE Encode.vae
         └→ VAE Decode.vae

Load Image → VAE Encode.pixels
VAE Encode → KSampler.latent_image
KSampler → VAE Decode.samples
VAE Decode → Save Image.images
```

### 3. 与 TXT2IMG 的关键区别
TXT2IMG 用的是：
```text
Empty Latent Image
```
IMG2IMG 用的是：
```text
Load Image → VAE Encode
```

也就是：
- TXT2IMG 没有参考结构
- IMG2IMG 先把原图压进 latent，再在这个基础上重绘

### 4. IMG2IMG 的灵魂参数：`denoise`
这个参数是生死线。

推荐理解：
- `0.15~0.3`：轻修，保留结构和大部分细节
- `0.35~0.55`：中度风格统一，很实用
- `0.6~0.8`：大改，适合重设计
- `0.85+`：几乎接近重新抽卡

### 5. IMG2IMG 典型用途
- 把主人手绘草图提成 AI 成品
- 固定构图后换风格
- 把已有图统一成《光与朽》的美术语言
- 修一张已经不错但材质不够、光影不够、细节不够的图

---

## 四、提示词实战原则
主人，很多人搞错了一点：
**TXT2IMG 和 IMG2IMG 不是一个靠 prompt，一个靠原图。它们两个都吃 prompt。**

差别只是 IMG2IMG 多了原图约束。

所以实际写法建议：
- 正向 prompt：主体 + 材质 + 风格 + 光效 + 构图倾向
- 负向 prompt：low quality, blurry, bad anatomy, extra limbs, text, watermark, noisy details

IMG2IMG 时，prompt 反而更该简洁一点。
因为结构已经有了，别再写一大堆把模型拉偏。

---

## 五、最适合主人当前阶段的起手模板

### 1. 《光与朽》怪物/UI 概念草图：TXT2IMG
适合：
- 完全没图，先找方向
- 想快速爆 20 张看看谁最像样

建议：
- checkpoint 固定
- prompt 模板固定
- 用 seed 和核心关键词做 AB 测试

### 2. 已有草图精修成成品：IMG2IMG
适合：
- 你已经手绘了轮廓
- UI/图标结构已有
- 想保留设计但提材质和完成度

建议：
- denoise 从 `0.35~0.5` 起步
- prompt 别写太满
- 重点加材质词、光效词、风格词

---

## 六、今天最重要的操作认知
1. **ComfyUI 工作流不是背图，而是理解数据流。**
2. **TXT2IMG 和 IMG2IMG 的核心差异只在 latent 来源。**
3. **IMG2IMG 成败关键不在“多高级”，而在 denoise 控制。**
4. **对《光与朽》来说，TXT2IMG 适合探索，IMG2IMG 适合精修和统一风格。**
5. **先把这两条基础链搭熟，后面接 LoRA、ControlNet 才有意义。**

---

## 七、后续衔接
下一步最该学两件事：
1. 提示词权重控制
2. Checkpoint / LoRA / Embedding 的组合逻辑

这两块一打通，你就开始真正具备“稳定出货”能力，而不是碰运气生图。

---

## 参考来源
- ComfyUI 官方仓库 README
- ComfyUI Examples 官方工作流
- 社区关于 TXT2IMG / IMG2IMG 的节点说明与经验总结

# 📚 ComfyUI 提示词语法与权重控制学习笔记 (2026-04-09)

## 🎯 今日推进节点
**主题：ComfyUI 原生提示词语法、权重控制、Embedding 调用方式。**

主人，今天这块非常关键。因为你后面想做《光与朽》的统一美术风格，光会“写提示词”没用，必须学会**精准加权**，让模型知道什么是主角，什么只是背景陪衬。

这就是从“瞎试”进入“可控”的分水岭。

---

## 一、先打掉一个常见误区：ComfyUI 到底支不支持提示词权重？
支持，而且原生就支持。

虽然早期有人误以为 ComfyUI 不像 A1111 那样直观，但实际上：
- 普通括号加权可以用
- 显式权重写法也可以用
- Embedding 也能直接在 prompt 里调用

所以别再信什么“ComfyUI 只能接高级节点才支持 prompt 权重”。那是错的。

---

## 二、CLIP Text Encode 节点在这里到底扮演什么角色

### 1）输入输出关系
`CLIP Text Encode` 吃进去的是：
- 一路 `CLIP`
- 一段文本 prompt

吐出来的是：
- `conditioning`

### 2）本质逻辑
这个节点不是“写 prompt 的地方”这么简单，它本质是在做：

```text
自然语言 → CLIP 语义向量 → 供 KSampler 参考的条件信号
```

所以所有提示词权重语法，本质上都是在影响 **CLIP 如何理解词语的重要性**。

---

## 三、最核心的权重语法：括号控制

### 1）提升权重
```text
crystal core
(crystal core)
(crystal core:1.2)
```

理解：
- 不加括号 = 普通权重
- `(词)` = 稍微更重（通常约 1.1 左右）
- `(词:1.2)` = 明确设定更高权重

### 2）降低权重
```text
[background]
(background:0.8)
```

### 3）默认括号
在很多 Stable Diffusion prompt 解析规则里：
- `(word)` ≈ `1.1`
- `((word))` ≈ `1.21`

但工业化生产里，我更推荐你用显式写法，别靠脑补。

### 4）嵌套括号会乘法叠加
```text
(((holy core)))
```
这种写法能进一步拉高，但很容易过头。

### 5）如何输入字面括号
如果你真要输出括号本身而不是权重语法，通常要转义，比如 `\(` `\)`。

---

## 四、权重别乱拉：最稳的实战范围

### 实战建议
- 核心主体词：`1.1 ~ 1.3`
- 重要材质词：`1.05 ~ 1.2`
- 风格词：`1.0 ~ 1.15`
- 构图/镜头词：轻一点，避免过强干扰主体

### 一个非常实用的原则
**权重是微调，不是暴力纠偏。**

如果一个词要拉到 `1.5+` 才勉强生效，通常说明：
- checkpoint 不合适
- prompt 结构有问题
- 你在让模型做它根本不擅长的事

这时候别硬拽，先换底盘。

---

## 五、正向提示词与负向提示词怎么分工

### 正向提示词（Positive）
负责定义：
- 主体是谁
- 材质是什么
- 风格是什么
- 光影/镜头感是什么

示例：
```text
(holy laser tower:1.2), dark fantasy, decayed gold metal, glowing crystal core, cathedral ruin atmosphere
```

### 负向提示词（Negative）
负责压制：
- 模糊
- 畸形
- 多余结构
- 脏噪点
- 低质量质感

示例：
```text
low quality, blurry, bad anatomy, extra limbs, text, watermark, noisy details
```

### 负向词的核心规则
别把负向词当垃圾桶。

你如果往里塞一长串互相冲突的东西，模型会更懵。

### 最落地的理解
- 正向 = 你想强化什么
- 负向 = 你想压掉什么

---

## 六、关键词顺序真的有影响
是的，顺序通常有影响。

尤其在 CLIP 编码里，前面的关键词更容易被优先关注。

### 实战建议
提示词按这个顺序写更稳：
1. 主体
2. 核心风格
3. 关键材质
4. 光影
5. 构图 / 镜头
6. 细节补充

比如《光与朽》某炮塔概念：
```text
(holy decayed laser turret:1.25), dark fantasy, black gold metal, glowing crystal energy, intricate mechanical structure, dramatic lighting, centered composition
```

比你把 30 个词乱堆在一起强得多。

---

## 七、Embedding / Textual Inversion 的调用方式

### 使用规则
把 embedding 文件放到：
```text
models/embeddings
```
然后在 prompt 里直接写：
```text
embedding:embedding_name
```

### 示例
```text
(holy laser altar:1.2), dark gothic fantasy, embedding:my_style_token
```

### 重点警告
Embedding 更像一个“训练好的概念快捷词”。
它不是万能风格修正器。

如果 embedding 本身质量烂，你只是在把烂东西更精准地塞进生成链。

### 它适合干嘛
- 某种固定画风触发
- 某种质量修正
- 特定角色 / 质感概念注入

---

## 八、动态 Prompt：量产时很香
ComfyUI 前端支持类似：
```text
{gold|silver|obsidian} holy tower
```

队列生成时会自动替换成不同词。

### 适合场景
- 批量探索材质方案
- 一次出多个图标方向
- 快速扩玩法视觉变体

### 不适合场景
- 你还没固定主风格
- 你想做严格可复现对照测试

这时候还是固定 prompt + 固定 seed 更靠谱。

---

## 九、ComfyUI 自带的小技巧：快捷键调权重
在部分 ComfyUI / 前端工作流里，选中文本后可以用：
- `Ctrl + ↑` 增加权重
- `Ctrl + ↓` 降低权重

这个很适合快速试错，但最终还是建议你把稳定版本写成显式数值。

---

## 十、给主人的一套最实战 Prompt 结构模板

### 模板 1：游戏资产概念图
```text
(主体:1.2), 核心风格, 材质词, 光影词, 构图词, 细节补充
```

例子：
```text
(holy crystal cannon:1.2), dark fantasy, decayed gold metal, glowing laser core, cinematic lighting, centered composition, intricate mechanical detail
```

### 模板 2：已有草图精修
```text
(主体:1.1), style match, refined materials, detailed lighting, clean silhouette
```

### 模板 3：批量变体探索
```text
({holy|corrupted|ancient} crystal tower:1.15), dark fantasy UI icon, glowing core, black gold material, clean background
```

---

## 十一、《光与朽》项目里的直接落地方法

### 场景 1：炮塔 / 圣物 / 核心装置的方向探索
做法：
- 固定 checkpoint
- 固定 prompt 主结构
- 每次只换一个核心物件词或材质词
- 用权重拉高你最在乎的视觉识别点

这样你才看得出，真正影响风格的到底是什么。

### 场景 2：UI 图标成套统一
做法：
- 每张图共用同一套风格词
- 只替换技能关键词
- 核心材质与光效词权重保持一致

这能大幅提升“同一游戏宇宙”感。

### 场景 3：买量素材快速做 8~12 个变体
做法：
- 用动态 prompt 自动替换主体词或材质词
- 保持构图词和风格词稳定
- 适合快速筛选哪个视觉方向最炸眼

---

## 十二、今天的核心结论
1. ComfyUI 原生支持 prompt 加权，不需要神神叨叨地绕路。
2. 权重控制的本质是影响 CLIP 对词语重要性的理解。
3. `(keyword:1.1~1.3)` 是最常用、最稳的实战区间。
4. Embedding 直接在 prompt 中调用，适合做概念注入。
5. 对《光与朽》来说，先把 prompt 结构模板和统一材质词库建立起来，比盲目追求更多插件重要得多。

---

## 参考来源
- ComfyUI 官方仓库 README
- ComfyUI 社区手册关于 prompt 与文本编码的说明
- Stable Diffusion Prompt 权重通用规则整理

## 📚 ComfyUI 初级进阶学习笔记 (2026-04-10)

### 🎯 今日推进节点
**ComfyUI：CLIP Text Encode 与提示词权重控制**

---

### 一、为什么今天该学这个，而不是乱跳中级节点
主人，ComfyUI 初级阶段最容易犯的蠢错误，就是没把基础的 prompt 编码和权重逻辑学扎实，就急着上 ControlNet、IP-Adapter、各种花里胡哨的插件。

这会导致一个结果：
你表面上会搭更多工作流，实际上每次出图还是在赌。

而 `CLIP Text Encode + Prompt Weight` 这套东西，恰恰是把“随机抽卡”变成“半可控生成”的第一块地基。

如果这块没吃透，后面你给《光与朽》做怪物、炮塔、圣物、技能图标、买量素材，都会经常出现：
- 主体不突出
- 风格飘
- 关键材质丢失
- 同一套词今天灵明天不灵

这不是模型的问题，是你没学会怎么给 CLIP 下命令。

---

### 二、CLIP Text Encode 到底在干什么
`CLIP Text Encode` 本质上做的是：

```text
自然语言 prompt → 语义向量 conditioning
```

它不是“写提示词的输入框”这么简单。
它是整个文生图工作流里，把你的人话翻译成模型条件信号的地方。

标准链路里它的位置是：

```text
Load Checkpoint.CLIP → CLIP Text Encode → KSampler.positive / negative
```

也就是说：
- `Checkpoint` 提供 CLIP 模块
- `CLIP Text Encode` 负责编码提示词
- `KSampler` 根据这些编码结果去引导去噪

所以提示词权重，实际上不是“给词加粗”，而是在影响 **CLIP 对不同语义的关注度**。

---

### 三、ComfyUI 原生提示词权重语法
根据 ComfyUI 官方/社区文档和实际使用习惯，常用写法有这些：

#### 1）普通文本
```text
holy crystal core
```
普通权重。

#### 2）圆括号加权
```text
(holy crystal core)
```
通常表示轻度增强，常见经验值接近 `1.1`。

#### 3）显式权重写法
```text
(holy crystal core:1.2)
```
这是最推荐的工业化写法，因为清晰、可复现。

#### 4）方括号降权
```text
[background clutter]
```
通常表示降低关注。

#### 5）嵌套括号
```text
((holy crystal core))
```
是继续叠加增强，但不如显式写数值稳。

---

### 四、最重要的实战判断：权重不是“越高越听话”
很多新手一看图不对，就把关键词权重干到 `1.5`、`1.8`，这通常会把图搞得更僵、更脏、更诡异。

最实战的经验：
- 主体词：`1.1 ~ 1.3`
- 材质词：`1.05 ~ 1.2`
- 风格词：`1.0 ~ 1.15`
- 超过 `1.35` 就要开始警惕副作用

如果你非得把词拉到很高才出效果，通常说明：
1. 你的 checkpoint 不合适
2. prompt 结构写烂了
3. 你在强迫模型做它不擅长的东西

这时候正确做法不是继续加权，而是回头检查模型底盘。

---

### 五、CLIP Text Encode 在节点流里的位置

#### 标准链路
```text
Load Checkpoint
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode（正向）→ KSampler.positive
 │        └→ CLIP Text Encode（负向）→ KSampler.negative
 └─ VAE → VAE Decode
```

#### 数据流解释
- `CLIP`：文本理解器本体
- `CLIP Text Encode`：把文本转成 conditioning
- `positive conditioning`：引导想要的元素
- `negative conditioning`：压制不想要的元素

#### 本质差别
`CLIP` 本身不决定图片长啥样，
它只负责把“你说的话”转成“模型听得懂的话”。

真正画图的是 `MODEL + KSampler`。

---

### 六、提示词权重的落地打法，不要再瞎写一长串

#### 推荐模板：主体 / 风格 / 材质 / 光影 / 构图 分层
比如：

```text
(holy laser turret:1.2), dark fantasy, decayed gold metal, glowing crystal core, dramatic lighting, centered composition
```

这就比你乱写 30 个词强得多。

#### 不推荐写法
```text
holy, amazing, best quality, masterpiece, perfect, cool, awesome, insane details, super realistic, fantasy, gold, laser, tower, detailed, sharp, beautiful...
```

这玩意就是垃圾堆词，不是提示词工程。

---

### 七、权重、CFG、Seed 三者怎么联动
这块非常关键。

#### 1）先固定 Seed
你不固定 seed，就没法判断到底是权重生效了，还是随机噪声瞎碰上了。

#### 2）权重变大时，CFG 往往要保守
如果你：
- 关键词权重大
- CFG 又高

模型就容易出现：
- 构图僵死
- 局部细节炸裂
- 材质过度强调

实战建议：
- 权重强化时，CFG 尽量控制在 `6 ~ 7.5`
- 不要又强权重又高 CFG，双重施压很容易翻车

#### 3）权重控制的是“语义关注”，不是“必定生成”
它不是开关。
它只是提高这个概念被模型优先考虑的概率和强度。

---

### 八、负向提示词该怎么用才不蠢

#### 常用负向词范围
```text
low quality, blurry, bad anatomy, extra limbs, text, watermark, noisy details
```

#### 对游戏资产的建议
《光与朽》这类游戏概念图、UI 图标，更该关注：
- 模糊
- 脏背景
- 多余结构
- 奇怪噪点
- 文本/水印

#### 不要这样干
别把网上 80 个负向词整段复制粘贴。
那不是专业，是懒。

---

### 九、进阶补充：Advanced CLIP Text Encode 节点为什么存在
有一些高级节点或插件，会扩展更复杂的 prompt 控制，比如：
- 更细的 token 权重
- 时间调度
- 区域 prompt
- 多条件混合

#### 什么时候考虑用 Advanced 节点
- 你已经把基础 prompt 权重吃透了
- 你真的遇到原生 `CLIP Text Encode` 不够用的场景

#### 初学阶段建议
先别碰太多高级变体。
基础的 `CLIP Text Encode` 已经够你把《光与朽》的大部分概念图、图标、宣传图做稳。

---

### 十、给《光与朽》的直接落地方案

#### 场景 1：炮塔/圣物/技能图标量产
做法：
- 固定一个主 checkpoint
- 固定一套风格 prompt
- 每张图只替换主体词
- 用权重固定“holy / laser / crystal / decayed gold”这几个核心视觉识别词

#### 场景 2：怪物概念图风格统一
做法：
- 不同怪物的结构词不同
- 但材质词、光效词、世界观词保持稳定
- 用权重锁住《光与朽》的世界观关键词

#### 场景 3：宣传图 / 商店图
做法：
- 先用主体词 + 光效词加权
- 保持构图词轻权重，别抢主体
- 用固定 seed 做多版本微调

---

### 十一、我给主人的推荐实验法，最稳

#### AB 测试法
固定这些不变：
- checkpoint
- seed
- steps
- CFG
- 分辨率

每次只改一个点：
- 某个关键词从 `1.0` → `1.15`
- 或某个词从普通写法 → 显式加权写法

这样你才能知道到底什么词在起作用。

#### 示例
A：
```text
holy crystal core
```
B：
```text
(holy crystal core:1.15)
```
C：
```text
(holy crystal core:1.25)
```

你一眼就能看出：
- 1.15 可能刚好
- 1.25 可能过头

这样积累一周，你就会拥有自己的**项目专属 prompt 权重表**。
这玩意比网上抄一万条 prompt 模板都值钱。

---

### 十二、今日核心结论
1. `CLIP Text Encode` 的本质是把文本与权重语法编码成 conditioning
2. ComfyUI 原生支持 prompt 加权，普通圆括号默认约 `1.1`
3. `(keyword:1.2)` 比纯嵌套括号更适合工业化复现
4. 权重不是越高越好，复杂加权常常需要下调 CFG
5. 固定 seed 做单变量测试，才是真正能沉淀项目模板的方法
6. 《光与朽》最适合把这个技术用于炮塔/圣物/技能图标和怪物概念图的风格统一

---

### 十三、参考来源
- ComfyUI 官方内置节点文档：`docs.comfy.org/built-in-nodes/ClipTextEncode`
- ComfyUI Community Manual / BlenderNeko 文档：Text Prompts / CLIP Text Encode
- GitHub 自定义节点仓库：`BlenderNeko/ComfyUI_ADV_CLIP_emb`
- Reddit 社区讨论：ComfyUI 与 A1111 在 prompt 权重解释差异、CFG 联动经验
- YouTube / 中文社区教程：ComfyUI 提示词权重与 CLIP 编码实战讲解

## 📚 ComfyUI 初级进阶学习笔记 (2026-04-10)

### 🎯 今日推进节点
**Checkpoint、LoRA、Embedding 在 ComfyUI 中的加载与组合工作流**

---

### 一、为什么今天轮到它
路线图前两块已经打完了，下一步就该啃“模型生态初探”里最实战的一刀：
**Checkpoint 决定底盘，LoRA 决定补丁，Embedding 决定提示词概念注入。**

这三者不会组合，你做《光与朽》时就会反复犯两个蠢错：
1. 一看风格不对就乱换大模型
2. 一装 LoRA 就把整条工作流搞成一坨面条

今天要把这件事彻底讲清。

---

### 二、三种模型资产到底各干什么

#### 1）Checkpoint：基础世界观与画风底盘
它是主模型，通常放在：
```text
models/checkpoints
```

`Load Checkpoint` 会拆出三路：
- `MODEL`
- `CLIP`
- `VAE`

这意味着它不只是“画风包”，而是整个生成系统的底层能力来源。

**主人要记死一句话：**
Checkpoint 决定你这台机器本来会画什么、擅长什么、上限在哪。

#### 2）LoRA：叠加在底盘上的轻量补丁
LoRA 文件通常放在：
```text
models/loras
```

根据 ComfyUI 官方教程和官方 examples，LoRA 是 patch，叠加在主 `MODEL` 和 `CLIP` 上，不是替换 Checkpoint。

它更适合做：
- 特定角色/主体强化
- 特定画风偏移
- 某种材质与笔触锁定
- 某类构图/姿态倾向补充

#### 3）Embedding / Textual Inversion：提示词里的概念 token
Embedding 通常放在：
```text
models/embeddings
```

它不是走独立加载节点，而是直接在 `CLIP Text Encode` 中调用：
```text
embedding:embedding_filename
```
官方 README 明确写了这一点，而且可以省略 `.pt` 后缀。

它最常见的用途是：
- 负向修手、修脸、修脏图
- 某个固定视觉概念触发
- 某种训练出来的特殊风格 token

---

### 三、标准连线逻辑，别再接错

#### 最稳基础链路
```text
Load Checkpoint
 ├─ MODEL → Load LoRA → KSampler.model
 ├─ CLIP  → Load LoRA → CLIP Text Encode（正/负）→ KSampler
 └─ VAE   → VAE Decode

Empty Latent Image / VAE Encode → KSampler.latent_image
KSampler → VAE Decode → Save Image
```

关键点：
- `Load LoRA` 同时吃 `model` 和 `clip`
- 它同时输出加过补丁的 `model` 与 `clip`
- Embedding 不经过 `Load LoRA`，它直接写在 prompt 里给 `CLIP Text Encode` 读

这三个别混。混了就开始瞎。

---

### 四、Load LoRA 两个强度参数到底怎么用
ComfyUI 官方 LoRA 教程把这个讲得很明白：
- `strength_model`：LoRA 对模型权重的影响强度
- `strength_clip`：LoRA 对 CLIP 文本嵌入的影响强度

社区手册与 Reddit 讨论也一致强调：
- 大多数时候，两者可以先设成一样
- 如果画面风格够了，但 prompt 语义开始跑偏，可以下调 `strength_clip`
- 如果风格不够狠，但提示词已经很听话，可以优先抬 `strength_model`

#### 我给主人的实战起手值
- 单个 LoRA 起步：`strength_model = 0.6 ~ 0.9`
- `strength_clip = 0.6 ~ 0.9`
- 新手先同步设置
- 遇到“LoRA 太抢词义”再拆开调

#### 一个很关键的判断
**角色/风格 LoRA 不是越高越好。**
超过 1.0 以后，很多 LoRA 会开始：
- 过拟合
- 吃掉构图
- 让脸和材质变脏
- 把你原 prompt 的语义压死

---

### 五、多个 LoRA 怎么叠，顺序怎么想
官方 `ComfyUI_examples` 直接给了多 LoRA 链式连接示例：
**多个 LoRA 就串联多个 `Load LoRA` 节点。**

```text
Load Checkpoint
 → Load LoRA（风格）
 → Load LoRA（角色）
 → Load LoRA（细节/材质）
 → KSampler
```

对应的 `CLIP` 也一样串过去。

#### 最稳的叠加原则
1. **不超过 2~3 个 LoRA**，再多就容易打架
2. **先大风格，后小特征**
3. 总强度别全拉高，不然会互相污染
4. 每多加一个 LoRA，都固定 seed 做对照测试

#### 推荐叠法
- 第一个 LoRA：世界观/大风格
- 第二个 LoRA：角色或物件特征
- 第三个 LoRA：材质或笔触微调

这才像工业化配方，不是玄学炼丹。

---

### 六、Embedding 怎么和 LoRA 配合才不蠢
Embedding 是 prompt token，LoRA 是模型补丁，它们不是替代关系。

#### 推荐用法
```text
(holy laser turret:1.2), dark fantasy, decayed black gold metal, embedding:badhandv4
```

或者：
```text
(corrupted crystal core:1.15), gothic UI icon, embedding:your_style_token
```

#### 实战原则
- 负向修正类 Embedding，优先放负向 prompt
- 风格类 Embedding，谨慎用，避免与 LoRA 重复施压
- 如果已经有强风格 LoRA，就别再塞一堆风格 Embedding，把系统搞乱

一句人话：
**Embedding 负责补词义，LoRA 负责改模型习惯，Checkpoint 负责定底色。**

---

### 七、给《光与朽》的最佳组合策略

#### 场景 1：炮塔 / 圣物 / 技能图标量产
推荐：
- 固定 1 个主 Checkpoint
- 叠 1 个世界观材质 LoRA
- 可选 1 个 UI/插画风格 LoRA
- 在 prompt 里用 Embedding 做负向清理或细节修正

目标：统一黑金、圣光、腐朽晶核这套视觉语言。

#### 场景 2：怪物概念图
推荐：
- 固定主 Checkpoint
- 叠 1 个怪物风格 LoRA 或材质 LoRA
- 保持 `strength_model` 中等，不要让每个怪都长一张妈生脸
- 用固定 seed 做同一怪物多版本比较

#### 场景 3：宣传图 / 商店图
推荐：
- 主 Checkpoint 偏高质量插画底盘
- LoRA 只留最关键的 1~2 个
- Embedding 只做修图，不做重风格堆叠

原因很简单：宣传图最怕画面脏和风格互相打架。

---

### 八、今天学到的最实战工作流模板

#### 模板 A：单 LoRA 稳定起手
```text
Load Checkpoint
 ├─ MODEL → Load LoRA.model
 ├─ CLIP  → Load LoRA.clip
 └─ VAE   → VAE Decode

Load LoRA
 ├─ model → KSampler.model
 └─ clip  → CLIP Text Encode（正/负）→ KSampler
```

参数建议：
- `strength_model = 0.75`
- `strength_clip = 0.75`
- steps：`24~30`
- CFG：`6~7.5`

#### 模板 B：双 LoRA 工业化量产
```text
Load Checkpoint
 → Load LoRA（世界观风格）
 → Load LoRA（主体特征）
 → KSampler
```

参数建议：
- LoRA1：`0.65`
- LoRA2：`0.55`
- 不要两个都 `1.0`

#### 模板 C：Embedding 辅助修正
正向 prompt：
```text
(holy crystal cannon:1.2), dark fantasy, black gold metal, cinematic lighting
```
负向 prompt：
```text
embedding:badhandv4, low quality, blurry, text, watermark
```

---

### 九、常见翻车点
1. **LoRA 路径放错目录**，ComfyUI 根本识别不到
2. **只给 MODEL 上 LoRA，不给 CLIP**，结果提示词表现不完整
3. **一个工作流塞 4~5 个 LoRA**，最后全在打架
4. **风格 LoRA + 风格 Embedding 一起猛堆**，图像直接发脏
5. **LoRA 强度太高**，生成结果像被一个模板强奸，完全失去主体差异

---

### 十、今天的核心结论
1. Checkpoint 是底盘，LoRA 是补丁，Embedding 是 prompt token，这三者职责完全不同。
2. `Load LoRA` 同时作用于 `MODEL` 和 `CLIP`，两个强度参数应该理解清楚，不然纯靠蒙。
3. 多个 LoRA 的正确姿势是链式串联，但数量最好控制在 2~3 个以内。
4. Embedding 直接在 `CLIP Text Encode` 中调用，最适合做负向修正和概念补充。
5. 《光与朽》最适合建立“固定主 Checkpoint + 少量风格 LoRA + 修正型 Embedding”的稳定量产模板。

---

### 十一、参考来源
- ComfyUI 官方教程：`https://docs.comfy.org/tutorials/basic/lora`
- ComfyUI 官方仓库 README：Embedding 调用方式与模型目录说明
- ComfyUI 官方 Examples：LoRA / Multiple LoRAs 工作流示例
- ComfyUI Community Manual：`Load LoRA` 节点参数说明
- Reddit：`r/comfyui` 关于 `strength_model` 与 `strength_clip` 的经验讨论
- YouTube：`ComfyUI - How to use LoRAs, Embeddings, and VAEs`
- Bilibili：ComfyUI LoRA 加载器与模型管理相关教程

# 📚 ComfyUI 中级进阶学习笔记 (2026-04-11)

### 🎯 今日推进节点
**ControlNet 深度应用：Canny / Depth / OpenPose / Lineart，以及多路 ControlNet 叠加工作流**

---

### 一、为什么今天必须学这个
主人，ControlNet 才是 ComfyUI 从“抽卡”升级成“工业化生产线”的分水岭。

不会 ControlNet，你只能靠 prompt 碰结构。
会 ControlNet，你才能精确控制：
- 轮廓
- 透视
- 姿态
- 景深层次
- 线稿一致性

这对《光与朽》太关键了。你后面做怪物图、UI 图标、宣传插画、买量素材，最怕的不是不够华丽，而是**结构不稳、风格飘、改一张崩一套**。

今天这一步，就是把“可控生成”的骨架搭出来。

---

### 二、官方结论先记死
根据 **ComfyUI 官方 examples** 和 **ComfyUI 官方 docs**：

1. `Apply ControlNet` **不会自动把普通图变成 canny/depth/pose/lineart**，预处理必须你自己先做。
2. 不同 ControlNet 模型，吃进去的 hint image 必须是对应格式，不然效果会很烂。
3. 多个 ControlNet 在 ComfyUI 里是**链式顺序叠加**，不是平铺乱接。
4. 官方明确指出，**T2I-Adapter 比传统 ControlNet 更轻更快**，但 ControlNet 约束更强。

一句话：
**ControlNet 不是一个万能按钮，而是一套“预处理图 + 控制模型 + 条件强度”的严谨管线。**

---

### 三、ComfyUI 里 ControlNet 的标准主链路

#### 最小工作流骨架
```text
Load Image
 └─ Preprocessor（Canny / Depth / OpenPose / Lineart）
      └─ Apply ControlNet

Load Checkpoint
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode（正/负）→ Apply ControlNet → KSampler
 └─ VAE → VAE Decode

Empty Latent Image / VAE Encode → KSampler.latent_image
KSampler → VAE Decode → Save Image
```

#### 核心逻辑
- 原图先走预处理，变成控制图
- `Load ControlNet Model` 加载对应控制模型
- `Apply ControlNet` 把控制条件注入 conditioning
- `KSampler` 在 prompt + ControlNet 双约束下生成

#### 最容易犯的错
很多新手直接把普通 JPG 丢给 `Apply ControlNet`，指望它自动识别成 depth / pose / canny。

**错得离谱。**
官方已经写得很清楚，不会自动转。

---

### 四、四种最关键 ControlNet 的落地职责

## 1）Canny：锁轮廓，适合图标、硬表面、UI 资产

### 作用
Canny 本质是边缘检测。
它保留大轮廓、主要边界、结构分区，非常适合：
- UI 图标
- 炮塔 / 武器 / 机械体
- 硬边轮廓清晰的怪物
- 买量图里需要稳定识别的主体外形

### 适合《光与朽》的场景
- 技能图标统一造型
- 激光炮塔、圣物核心、机械组件
- 先手绘剪影，再批量变体

### 参数原则
在 `comfyui_controlnet_aux` 里，Canny 节点有阈值控制。
- `low threshold`：边缘敏感度下限
- `high threshold`：强边缘保留阈值

#### 推荐起手值
- 简单图标 / 干净线稿：`100 / 200`
- 细节较多的概念图：`80 / 180`
- 如果边太碎，升高阈值
- 如果轮廓抓不住，降低阈值

### 实战判断
- **边线太碎** → 模型会过度追边，画面发脏
- **边线太少** → 约束不够，结构跑掉

所以 Canny 的目标不是“边越多越好”，而是**只保留关键结构边界**。

---

## 2）Depth：锁空间层次，适合场景和宣传图

### 作用
Depth 控制的是前后空间关系，不是具体线条。
它最适合：
- 场景图
- 宣传 KV
- 有明显近景 / 中景 / 远景的构图
- 角色和背景要稳定透视关系的图

### 常见预处理器
根据 `comfyui_controlnet_aux` 仓库：
- MiDaS Depth
- LeReS Depth
- Zoe Depth
- Depth Anything / Depth Anything V2

### 推荐理解
- **MiDaS**：老牌稳定，够用
- **Zoe / Depth Anything**：新一点，深度层次更自然，复杂图更强

### 适合《光与朽》的场景
- 教堂遗迹、腐朽圣坛、激光迷宫式场景海报
- 商店头图
- 有空间纵深的买量素材底图

### 参数与强度建议
Depth 本身预处理参数比 Canny 少，关键更多在 `Apply ControlNet` 的 strength。

#### 推荐起手值
- 场景保构图：`0.7 ~ 1.0`
- 只想轻锁空间：`0.4 ~ 0.7`

### 实战判断
- strength 太低，空间层次会飘
- strength 太高，构图会很死，prompt 发挥空间变小

---

## 3）OpenPose：锁姿态，适合角色、手势、广告动作图

### 作用
OpenPose 控制的是骨架姿态。
对人体、手势、动态动作用处极大。

根据 `comfyui_controlnet_aux`：
- OpenPose Estimator
- DWPose Estimator
都支持输出 OpenPose 格式控制图

### 推荐理解
- **OpenPose**：经典骨架
- **DWPose**：更稳、更细，对手和脸支持更完整，现代工作流更常用

### 适合《光与朽》的场景
虽然《光与朽》主体不是角色驱动，但这玩意依然有用：
- 买量素材里的手部操作演示
- 人物持武器、释放技能的宣传插画
- UI 立绘、NPC 概念图
- 短视频里角色待机 / 指向 / 施法动作

### 性能注意
仓库说明里明确提到：
- DWPose 用 CPU 会很慢
- 可用 TorchScript 或 ONNX 加速
- ONNXRuntime GPU 速度会明显更好

### 强度建议
- 姿势必须稳定：`0.9 ~ 1.2`
- 只保大动作趋势：`0.6 ~ 0.9`

### 实战判断
OpenPose 不是拿来控细节材质的，它只负责骨架。
所以它常常需要配合：
- prompt 控风格
- lineart / canny 控轮廓
- depth 控空间

---

## 4）Lineart：锁线稿，适合二次元、插画、干净轮廓资产

### 作用
Lineart 比 Canny 更偏“绘画线稿逻辑”，而不是纯数学边缘。
适合：
- 插画
- 二次元
- 概念草图精修
- 风格统一的怪物、道具、UI 原稿

### `comfyui_controlnet_aux` 中常见线稿预处理器
- Standard Lineart
- Realistic Lineart
- Anime Lineart
- Manga Lineart
- AnyLine

### 推荐理解
- **Standard / Realistic Lineart**：适合一般概念图
- **Anime Lineart / Manga**：适合更平涂、赛璐璐风
- **AnyLine**：兼容性强，常被拿来做泛用线稿提取

### 适合《光与朽》的场景
- 主人自己画的怪物草图
- 2D 图标与道具草稿
- 想保留形体语言但重做材质
- 下一项目如果走更卡通/软糖风，也很有用

### 强度建议
- 保留草图主线：`0.8 ~ 1.1`
- 只参考线条节奏：`0.5 ~ 0.8`

### 实战判断
Lineart 比 Canny 更“懂画”，更适合插画类资产。
如果你想保的是“绘画感轮廓”，先试 Lineart；
如果你要的是“硬边界结构”，先试 Canny。

---

### 五、多路 ControlNet 叠加怎么接
根据 **ComfyUI 官方 docs mixing-controlnets**：

> 多个 ControlNet 通过 `Apply ControlNet` **顺序链式连接** 来分层叠加。

#### 标准叠加写法
```text
CLIP Text Encode（正向）
 → Apply ControlNet（Pose）
 → Apply ControlNet（Scribble / Canny / Lineart）
 → Apply ControlNet（Depth，可选）
 → KSampler.positive
```

负向 conditioning 通常不走 ControlNet，保持正常接 KSampler。

### 官方强调的重点
- 多个 ControlNet 不是抢同一个插口乱塞
- 是一层层往 conditioning 上叠
- 如果控制不同区域，强度平衡很重要

### 两类常见叠加思路

#### 1）同一主体多重约束
例如：
- `OpenPose + Depth`
- `Pose + Canny`
- `Pose + Reference`

用途：
- 既锁动作，又锁空间或轮廓

#### 2）不同区域分工控制
官方案例里有：
- 左边人物用 Pose
- 右边猫和摩托用 Scribble

用途：
- 复杂场景分区域控制

---

### 六、多 ControlNet 的强度怎么配，别乱来
官方文档明确提到：
如果某一路 strength 明显高于另一条，就会压制其他控制条件。

#### 我的推荐起手模板

### 模板 A：角色动作图
- Pose：`1.0`
- Canny / Lineart：`0.6 ~ 0.8`
- Depth：`0.4 ~ 0.7`

### 模板 B：场景宣传图
- Depth：`0.8 ~ 1.0`
- Canny / Lineart：`0.5 ~ 0.7`
- Pose：没有角色时不用

### 模板 C：UI / 图标 / 道具
- Canny：`0.9 ~ 1.1`
- Lineart：`0.7 ~ 1.0`
- Depth：一般不用

### 核心原则
1. **主控制器 1 个，辅助控制器 1~2 个**
2. **别三路都拉满**
3. **固定 seed 做单变量测试**
4. **先让主结构稳定，再补细节**

---

### 七、T2I-Adapter 和 ControlNet 怎么选
根据官方 examples：
- T2I-Adapter 比 ControlNet 更高效
- ControlNet 每一步采样都参与
- T2I-Adapter 只运行一次，速度负担小得多

### 选择建议
- **要最强结构约束** → ControlNet
- **显存紧张、需要快** → T2I-Adapter
- **批量探索草图** → T2I-Adapter 可以先上
- **正式出货、要稳定复现** → ControlNet 更稳

对主人当前阶段，我的判断很直接：
**先把标准 ControlNet 学透，再考虑 T2I-Adapter 当轻量版工具。**

---

### 八、《光与朽》项目里的直接应用模板

## 模板 1：技能图标 / 圣物 / 炮塔图标量产
```text
输入：手绘草图 / 参考图
Preprocessor：Canny 或 Lineart
ControlNet：对应 canny / lineart 模型
Strength：0.8 ~ 1.0
Prompt：统一材质词 + 圣光腐朽世界观词
```

**目标：** 图标造型稳、轮廓统一、风格不飘。

## 模板 2：怪物概念图精修
```text
输入：主人草图
Preprocessor：Lineart
可选第二路：Depth
Strength：Lineart 0.9，Depth 0.5
```

**目标：** 保怪物外形语言，同时提升材质和完成度。

## 模板 3：宣传图 / 商店头图
```text
输入：场景草稿或拼贴参考
Preprocessor：Depth
可选第二路：Canny
Depth 0.9，Canny 0.5
```

**目标：** 先锁空间层次，再让模型发挥光效和质感。

## 模板 4：买量视频首帧 / 广告角色动作图
```text
输入：人物动作参考
Preprocessor：DWPose
可选第二路：Canny / Lineart
Pose 1.0，辅助路 0.6
```

**目标：** 保住动作冲击力，不让四肢乱飞。

---

### 九、常见翻车点
1. **预处理图和模型类型不匹配**
   - canny 图配 depth 模型，纯属作死

2. **直接把原图喂给 Apply ControlNet**
   - 官方都说了，不会自动转 hint 图

3. **多个 ControlNet 全拉满**
   - 最后不是更稳，是互相打架

4. **把 Pose 当万能结构控制器**
   - 它只管骨架，不管材质、光影、细部轮廓

5. **Canny 线太碎**
   - 画面脏成一锅粥

6. **Depth 太强**
   - 图像构图死板，模型没发挥空间

---

### 十、今天的核心结论
1. ControlNet 的本质是 **预处理图 + 对应控制模型 + 条件强度**。
2. Canny 控边界，Depth 控空间，OpenPose 控姿态，Lineart 控绘画线稿。
3. 多 ControlNet 在 ComfyUI 里要用 `Apply ControlNet` **链式叠加**。
4. 主控制器只留一个，辅助控制器 1~2 个就够，强度必须平衡。
5. 对《光与朽》最实用的起手组合是：
   - 图标 / 道具：Canny 或 Lineart
   - 怪物精修：Lineart + 可选 Depth
   - 宣传图：Depth + 轻度 Canny
   - 动作广告图：DWPose + Canny

---

### 十一、参考来源
- ComfyUI 官方文档：`docs.comfy.org/tutorials/controlnet/mixing-controlnets`
- ComfyUI 官方示例：`comfyanonymous.github.io/ComfyUI_examples/controlnet/`
- `Fannovel16/comfyui_controlnet_aux` GitHub README（预处理器类型、模型对应关系、DWPose/ONNX 加速说明）
- ComfyUI 官方仓库 README
- Reddit 社区关于多 ControlNet 链式连接和强度平衡讨论
- Bilibili / YouTube 社区关于 Canny、Depth、Pose、Lineart 的实战教程

# 📚 ComfyUI 初级工作流实战学习笔记 (2026-04-18)

## 🎯 今日攻克节点：从零搭建标准 TXT2IMG / IMG2IMG 工作流

这一步看着基础，实际上是整个 ComfyUI 工业化管线的地基。
如果主人连 `TXT2IMG` 和 `IMG2IMG` 的节点职责、连线逻辑、参数边界都没打透，后面再学 ControlNet、IP-Adapter、局部重绘、放大，都会越学越乱。

---

## 一、为什么这两个工作流必须先彻底打通

### 1. TXT2IMG 是“从无到有”
本质是：
- 先定义一块空的 latent 画布
- 再把提示词编码成条件
- 然后让 KSampler 从随机噪声里迭代去噪
- 最后通过 VAE 解码成可见图像

### 2. IMG2IMG 是“拿已有图当骨架重做”
本质是：
- 先把输入图片编码进 latent 空间
- 再按 denoise 强度加噪
- 再根据提示词重新去噪生成
- 最后输出新图

最关键的区别只有一句：
**TXT2IMG 的 latent 来自 Empty Latent Image，IMG2IMG 的 latent 来自 VAE Encode。**

---

## 二、标准 TXT2IMG 连线逻辑

### 1. 最小可运行链路
```text
Load Checkpoint
 ├─ MODEL → KSampler.model
 ├─ CLIP → CLIP Text Encode(positive)
 ├─ CLIP → CLIP Text Encode(negative)
 └─ VAE  → VAE Decode.vae

CLIP Text Encode(positive) → KSampler.positive
CLIP Text Encode(negative) → KSampler.negative

Empty Latent Image → KSampler.latent_image
KSampler → VAE Decode.samples
VAE Decode → Save Image
```

### 2. 每个节点到底干什么
- **Load Checkpoint**：拆出 `MODEL / CLIP / VAE` 三路能力。
- **CLIP Text Encode**：把正向、负向提示词编码成条件向量。
- **Empty Latent Image**：创建一张“空白潜空间画布”，本质决定出图尺寸。
- **KSampler**：核心采样器，从噪声开始按提示词迭代去噪。
- **VAE Decode**：把 latent 结果翻译成像素图像。
- **Save Image**：预览并落盘。

### 3. TXT2IMG 的工作直觉
它不是“AI凭空画图”，而是：
**模型在空 latent 里，从随机噪声往目标图像方向反推。**
所以 seed 一变，初始噪声一变，构图和细节就会跟着漂。

---

## 三、标准 IMG2IMG 连线逻辑

### 1. 最小可运行链路
```text
Load Checkpoint
 ├─ MODEL → KSampler.model
 ├─ CLIP → CLIP Text Encode(positive)
 ├─ CLIP → CLIP Text Encode(negative)
 └─ VAE  → VAE Encode.vae / VAE Decode.vae

Load Image → VAE Encode.pixels
VAE Encode → KSampler.latent_image

CLIP Text Encode(positive) → KSampler.positive
CLIP Text Encode(negative) → KSampler.negative

KSampler → VAE Decode.samples
VAE Decode → Save Image
```

### 2. 比 TXT2IMG 多出来的关键节点
- **Load Image**：加载参考图、草图、旧图。
- **VAE Encode**：把像素图压缩进 latent 空间。

这就是图生图的本质差异。
**图片不能直接喂给 KSampler，必须先 VAE Encode。**

### 3. IMG2IMG 的工作直觉
它不是“修图滤镜”，而是：
**先把原图转成 latent，再按 denoise 决定打碎多少，再重新生成。**

所以图生图是否“像原图”，核心不在 prompt 写得多花，而在 `denoise`。

---

## 四、KSampler 关键参数，主人真正要盯的只有这几个

### 1. `seed`
- 决定初始噪声。
- **固定 seed**：便于做 AB 测试和复现。
- **随机 seed**：便于批量探索。

**实战建议：**
- 探索风格时随机。
- 要统一资产系列时固定。

### 2. `steps`
- 去噪步数。
- 越高通常越细，但更慢，不是无限上涨就无限变强。

**实战建议：**
- 初级探索：20~30
- 稳定成图：28~40
- 低端机器别硬堆 50+

### 3. `cfg`
- 提示词服从强度。
- 太低，模型放飞自我。
- 太高，容易僵、脏、过拟合提示词。

**实战建议：**
- SD1.5 常见起手：`6.5 ~ 8`
- SDXL 通常更低一点更稳

### 4. `denoise`
这是图生图最值钱的参数。

官方文档和社区手册都明确强调：
- **IMG2IMG 时 denoise 必须 < 1**
- `denoise = 1` 时，原图特征会被彻底打碎，效果就等同于 TXT2IMG

#### 实战区间理解
- `0.15 ~ 0.35`：轻修图，只改质感和完成度
- `0.35 ~ 0.6`：中度重绘，保结构但换材质/风格
- `0.6 ~ 0.85`：大幅重做，保留大构图，细节会明显重生
- `1.0`：几乎放弃原图，退化成文生图逻辑

### 5. `sampler_name` / `scheduler`
这是采样算法组合。
初学阶段别陷进去瞎折腾，先固定一套稳定方案。

**建议起手：**
- `dpmpp_2m_sde` 或 `euler`
- scheduler 用常见默认配置先跑通

先学会控制结构，再谈风格玄学，不然纯浪费命。

---

## 五、提示词写法的最低可用规范

根据官方基础教程：
- 尽量用英文
- 用英文逗号分隔短语
- 少写长句，多写明确关键词
- 可用 `(keyword:1.2)` 提升权重

### 示例
```text
Positive:
ancient holy relic, dark gold metal, glowing core, fantasy game icon, centered composition, masterpiece, best quality

Negative:
blurry, low quality, extra fingers, text, watermark, messy background
```

### 对游戏项目的意义
主人需要的不是“文案味 prompt”，而是**资产模板 prompt**：
- 世界观词
- 材质词
- 构图词
- 质量词
- 排除词

这样后面量产图标、怪物、宣传图才不会每次从零写。

---

## 六、TXT2IMG 与 IMG2IMG 的最佳使用场景

### TXT2IMG 适合
1. 怪物概念探索
2. 新炮塔外观方向发散
3. 技能图标灵感草案
4. 宣传图构图 brainstorming

### IMG2IMG 适合
1. 主人手绘草图精修
2. 已有图标统一风格
3. 怪物旧稿重做
4. 把线稿、草模截图、拼贴参考转成高完成度图

一句话：
- **没图，用 TXT2IMG 开荒**
- **有结构，用 IMG2IMG 稳定出货**

---

## 七、《光与朽》里的直接落地模板

### 模板 1：技能图标 / 圣物图标快速探索
```text
工作流：TXT2IMG
尺寸：512×512 或 768×768
Seed：随机探索，定稿后固定
CFG：7 左右
Steps：28 左右
```

**打法：**
先用统一 prompt 模板批量出 20 张方向图，再挑一张最对味的进入后续精修。

### 模板 2：主人手绘炮塔草图精修
```text
工作流：IMG2IMG
输入：主人草图
Denoise：0.3 ~ 0.5
目的：保结构，补材质与完成度
```

**打法：**
这最适合《光与朽》当前生产方式，因为主人自己会搭 UI 和美术结构，ComfyUI 负责把“粗稿”打磨成“能上线的资产”。

### 模板 3：旧怪物图重做统一风格
```text
工作流：IMG2IMG
输入：旧怪物立绘
Denoise：0.45 ~ 0.65
固定同一 checkpoint + 同一 prompt 模板
```

**打法：**
把老图一批批重刷，统一圣光腐朽的材质、边缘光、暗金元素。

### 模板 4：宣传图首图生成
```text
第一步：TXT2IMG 出构图
第二步：IMG2IMG 低 denoise 精修
```

**打法：**
先让模型大胆发散，再拿最好的那张做受控重绘，这比一次性赌神图稳得多。

---

## 八、最常见的翻车点

### 1. 把 TXT2IMG 和 IMG2IMG 只理解成“有没有参考图”
太浅了。
真正差别是 **latent 的来源** 不同。

### 2. 图生图 denoise 拉到 1 还以为自己在“参考原图”
错，已经基本把原图炸没了。

### 3. 不固定 seed 就讨论 prompt 是否有效
纯属自欺欺人，变量没控住。

### 4. 一开始就疯狂调 sampler
初学阶段意义不大，先把工作流主链路跑顺。

### 5. 直接拿高分辨率大图做图生图
显存炸、速度慢、定位问题也麻烦。
先从标准尺寸把逻辑跑通，再上放大链。

---

## 九、今天的核心结论
1. **TXT2IMG = Empty Latent Image 作为 latent 来源。**
2. **IMG2IMG = Load Image → VAE Encode 作为 latent 来源。**
3. **KSampler 是主发动机，seed 决定噪声起点，steps 决定去噪轮数，cfg 决定服从提示词强度。**
4. **IMG2IMG 成败看 denoise，denoise=1 基本等于放弃参考图。**
5. 对主人最实用的生产策略是：
   - 前期探索用 TXT2IMG
   - 有草图/旧图后立即切 IMG2IMG 做稳定精修

---

## 十、参考来源
- ComfyUI 官方文档：`https://docs.comfy.org/tutorials/basic/text-to-image`
- ComfyUI 官方文档：`https://docs.comfy.org/tutorials/basic/image-to-image`
- ComfyUI Community Manual：`https://blenderneko.github.io/ComfyUI-docs/Core%20Nodes/Sampling/KSampler/`
- ComfyUI Community Manual：`https://blenderneko.github.io/ComfyUI-docs/Core%20Nodes/Latent/VAEEncode/`
- YouTube：`ComfyUI Tutorial Series: Ep03 - TXT2IMG Basics`
- Bilibili：`comfyui工作流搭建：文生图、图生图、反推、换脸、放大、裁剪等多节点融合一个工作流教程`

# 📚 ComfyUI 中级节点精讲学习笔记 (2026-04-18)

## 🎯 今日攻克节点：IP-Adapter 风格锁死工作流

### 一、为什么今天必须学 IP-Adapter
主人，ControlNet 解决的是“结构别跑偏”，IP-Adapter 解决的是“味道别跑偏”。

对游戏项目来说，结构统一不够，**色彩、材质、光影、笔触语言**不统一，整套怪物、UI、宣传图还是会像东拼西凑的垃圾堆。IP-Adapter 的价值就在这里，它不是让 AI 照抄参考图，而是把参考图的视觉特征编码成额外条件，注入采样过程里，去锁定整体风格倾向。

GitHub 官方/社区说明里对它的定义很直接：**Think of it as a 1-image LoRA**。这句话很准，你可以把它理解成“单张参考图驱动的临时风格补丁”。

---

### 二、IP-Adapter 的本质
#### 1. 它控制什么
- 风格（style）
- 角色/主体特征（subject）
- 构图倾向（composition）
- 人脸一致性（FaceID 分支）

#### 2. 它不是什么
- 不是 ControlNet，不能像 Canny / Pose 那样做硬结构约束
- 不是 LoRA，不会把风格永久烤进模型
- 不是单纯 img2img，不是把原图直接重绘一遍

#### 3. 最该记住的一句话
**ControlNet 管骨架，IP-Adapter 管气质。**

你真要做工业化量产，这俩通常是搭配使用，而不是二选一。

---

### 三、常见模型类型与职责
根据 `comfyorg/comfyui-ipadapter` 与 `cubiq/ComfyUI_IPAdapter_plus` README，可把常见分支理解成下面这张表：

#### 1. Base / Plus
- **Base**：影响较轻，更适合一般参考驱动
- **Plus**：特征提取更强，更适合风格锁定和主体保持
- **经验结论**：真要拿来做游戏资产批量统一，优先从 **Plus** 开始试

#### 2. Face / FaceID
- **Face**：更偏人脸视觉特征参考
- **FaceID**：更强调身份一致性
- **注意**：README 明确指出，大多数 FaceID 模型需要配套 **LoRA**，并且依赖 `insightface`
- **对《光与朽》价值**：当前不高，因为我们重点不是写实人脸，而是怪物、UI、场景风格统一

#### 3. Composition
- 专门拿来迁移画面的布局/构图关系
- GitHub 列出的 `ip_plus_composition_sd15.safetensors` / `ip_plus_composition_sdxl.safetensors` 就是干这个的
- **适用场景**：你想保留“主体在左、能量源在右、背景有大面积留白”这种画面组织，而不是照搬颜色质感

#### 4. Precise Style Transfer / ClipVision Enhancer
- `cubiq` 后期加了更精确的风格迁移节点，目的是减少 style 和 composition 互相串味
- `ClipVision Enhancer` 用于捕捉更细的小特征，适合精细风格传递
- **结论**：做精修海报或高质感宣传图时价值更大，做普通量产怪物卡面时先别上这么复杂

---

### 四、ComfyUI 里的标准 IP-Adapter 连线逻辑
最实用的理解，不是背节点名，而是看它插在哪一层。

#### 1. 标准风格迁移主链
```text
Load Checkpoint
 ├─ MODEL → Apply IPAdapter → KSampler
 ├─ CLIP → CLIP Text Encode（正/负）→ KSampler
 └─ VAE → VAE Decode

Load Image（参考风格图）
 → CLIP Vision Encode / IPAdapter Image Encode
 → Apply IPAdapter

Empty Latent Image / VAE Encode(latent)
 → KSampler
 → VAE Decode
 → Save Image
```

#### 2. 这条链的底层逻辑
- **Checkpoint** 决定基础审美上限
- **Prompt** 决定你这张图要画什么
- **IP-Adapter** 决定它更像哪种风格/主体气质
- **KSampler** 负责在多重条件下真正出图

所以 IP-Adapter 不是替代 prompt，而是给 prompt 增加“视觉参照系”。

---

### 五、参数怎么设，别乱拧
> 下面分两类，一类是仓库/文档明确提到的，一类是社区和实战常见经验。

#### 1. weight（最关键）
`cubiq` README 的经验建议：
- **通常先把 weight 降到至少 0.8 左右，再增加 steps**
- 如果想提升对文字 prompt 的服从度，可以改 `weight type`

我的落地理解：
- **0.4 ~ 0.6**：轻度借风格，只沾一点味
- **0.7 ~ 0.9**：比较稳的风格统一区间，适合批量游戏资产
- **1.0 以上**：容易参考图味太重，开始压 prompt，自由度下降

#### 2. steps
- 仓库建议是：**weight 降一点，steps 加一点**
- 原因很简单，IP-Adapter 给的视觉条件更复杂，如果步数太低，容易又像参考图又不像目标图，结果一坨浆糊
- **实战建议**：
  - 草图量产：24~30
  - 成品精修：30~40

#### 3. cfg
- FaceID/强参考场景里，Reddit 社区经常提到要适当降低 cfg，不然画面容易僵
- **经验区间**：
  - 风格统一资产：5~7
  - 参考图很强时：4.5~6.5
- 原因：cfg 太高，文字约束和图像约束会互相顶牛

#### 4. style_boost / precise style transfer（进阶）
`cubiq` README 提到：
- **SDXL** 下 `style_boost` 可以从 **2** 起试
- **SD1.5** 下建议更保守，style_boost 在 **-1 到 1** 间起步，先从 0 试

这说明一件事：**SDXL 更适合做精细风格迁移，SD1.5 更适合做轻量参考控制。**

#### 5. embeds_scaling
GitHub 更新说明特别提到：
- `embeds_scaling` 对 precise composition transfer 影响很大
- 这类参数不是新手第一天该死磕的，但你要知道：它决定图像特征嵌入被注入注意力层时的方式，强烈影响“参考图到底是管风格，还是管构图”

---

### 六、最稳的三种实战工作流
#### 工作流 A：单张垫图锁死系列怪物风格
**用途**：第二章熔岩怪、第三章极寒怪，要求同章怪物画风一致

```text
风格参考图（同章节定调图）
 → IP-Adapter Plus

怪物功能 prompt
 + 固定 checkpoint
 + 固定负面词
 + Empty Latent
 → KSampler
```

**推荐参数**：
- IP weight：0.75~0.9
- steps：28~36
- cfg：5.5~7

**效果**：
- 每只怪物轮廓和机制不同
- 但色温、材质、笔触、光影更统一

#### 工作流 B：IP-Adapter + ControlNet 双约束
**用途**：既要姿势/轮廓稳，又要风格稳

```text
参考结构图 → ControlNet（Canny / Lineart / Depth）
风格参考图 → IP-Adapter Plus
Prompt → CLIP Text Encode
全部汇入 KSampler
```

**适合**：
- UI icon 重绘
- 已有草图精修
- 买量海报的构图复用

**核心原则**：
- ControlNet 管形
- IP-Adapter 管味

#### 工作流 C：IP-Adapter 做买量图批量统一
**用途**：一套 5~10 张广告图保持同一视觉语言

做法：
1. 先挑一张“最对味”的主视觉当风格母图
2. 所有后续广告图都接同一 IP-Adapter 参考图
3. 只改 prompt 里的卖点、文案场景、主体行为
4. 固定 checkpoint / sampler / steps / negative

**结果**：
- 素材不再像不同人做的
- 账号投放时视觉识别更强

---

### 七、常见坑，踩了就会出一堆脏图
#### 1. weight 拉太满
后果：
- 画面被参考图绑死
- prompt 失效
- 批量结果缺少变化

#### 2. 参考图质量烂
IP-Adapter 吃的是参考图里的视觉特征。
如果参考图本身脏、糊、构图乱、光影垃圾，你就是把垃圾当圣旨灌进模型。

#### 3. 想让 IP-Adapter 代替 ControlNet
这就是典型蠢操作。
IP-Adapter 不是硬结构控制器，姿势、边缘、精确轮廓别指望它单独兜住。

#### 4. FaceID 模型乱用
GitHub 写得很明白：
- 多数 FaceID 需要 `insightface`
- 很多还要专属 LoRA
- 文件命名不对，Unified Loader 也可能自动加载失败

#### 5. 风格图和目标任务差太远
比如你拿超复杂写实油画去套极简 UI icon，这很容易炸。
参考图最好在“材质语言”和“复杂度等级”上接近目标任务。

---

### 八、对《光与朽》的直接落地方案
#### 方案 1：章节怪物风格母图制度
- 每章先做 1 张定调母图
- 后续同章怪物全部走同一 IP-Adapter 参考
- prompt 只改机制差异、形体关键词、颜色微差

这能避免“第二章 8 个怪像 8 个项目”。

#### 方案 2：UI 图标批量统一
- 先选 1 张最满意图标作为风格母图
- 后续新图标都用它做 IP-Adapter 参考
- 配合统一 prompt 模板与固定 seed 策略

这比纯 prompt 靠玄学统一，稳定得多。

#### 方案 3：买量素材主视觉复用
- 先打出一张点击率最高的主视觉
- 用它做 IP-Adapter 风格源，批量衍生不同文案版本和玩法展示图
- 这样能把“能打的视觉感”复用出来

---

### 九、我给主人的执行建议
如果现在就想落地，不要上来就研究 FaceID、Precise Style、Composition 全家桶。

**最优起手式只有四步：**
1. 固定一个主 checkpoint
2. 选一张真正对味的风格母图
3. 上 `IP-Adapter Plus`
4. 先把 weight 跑在 0.8 左右，steps 提到 30 左右看结果

先把“整套资产统一”这件事做出来，再谈更花的进阶玩法。

---

### 十、今日结论
IP-Adapter 的真正价值，不是“让一张图变好看”，而是让**一批图看起来像同一套产品**。

对独立游戏开发，这玩意儿非常值钱，因为它直接砍掉了最耗命的那部分时间：
**反复抽卡、反复修风格、反复觉得‘这张还行但跟上一张不像一个游戏’。**

一句话收尾：
**LoRA 适合长期固化风格，IP-Adapter 适合低成本快速锁风格。现在我们做《光与朽》或下个项目的资产量产，IP-Adapter 是更务实的刀。**

---

### 参考来源
- ComfyUI IPAdapter 官方仓库：`https://github.com/comfyorg/comfyui-ipadapter`
- cubiq / ComfyUI_IPAdapter_plus README 与 examples：`https://github.com/cubiq/ComfyUI_IPAdapter_plus`
- Reddit 社区关于 IP-Adapter 与风格一致性、FaceID、与 ControlNet 搭配的讨论
- YouTube 社区教程：Style Transfer / IPAdapter Workflows / Style and Composition with IPAdapter

# 📚 ComfyUI 中级节点精讲学习笔记 (2026-04-19)

## 🎯 今日攻克节点：IP-Adapter 风格锁死工作流

### 一、当前进度判断与为什么今天还学它
根据 `knowledge/comfyui_expert_roadmap.md` 的顺序，初级阶段的架构、TXT2IMG/IMG2IMG、Prompt 权重、Checkpoint/LoRA/Embedding 已经基本覆盖；中级里 ControlNet 也已推进。

所以下一个最该继续吃透的节点，不是乱跳到高级放大或视频，而是**IP-Adapter 的“风格锁死与工业化量产”**。这玩意儿对独立游戏尤其值钱，因为它解决的不是“能不能出一张好图”，而是**一批图能不能像同一个项目做出来的**。

---

### 二、IP-Adapter 的本质，先说人话
根据 `cubiq/ComfyUI_IPAdapter_plus` README，IP-Adapter 是一种非常强的图像条件控制模块，仓库原话是：

> **Think of it as a 1-image lora.**

这句话非常准。

#### 1. 它到底控制什么
- 风格（style）
- 主体特征（subject）
- 构图倾向（composition）
- 某些分支下的人脸一致性（FaceID）

#### 2. 它不是什么
- 不是 ControlNet，不能像 Canny / Pose 那样做硬结构约束
- 不是 LoRA，不会把风格永久烤进模型
- 不是 IMG2IMG，不是把原图直接重绘一遍

#### 3. 一句话结论
**ControlNet 管骨架，IP-Adapter 管气质。**

---

### 三、为什么它对游戏项目这么重要
AI 生图最大的问题，不是单张图不够好看，而是：
- 今天这张怪物偏油画
- 明天那张 UI 偏二次元
- 后天海报又像另一个美术外包团队做的

这就会导致整套项目视觉语言散架。

IP-Adapter 的价值就是把一张“母图”里的：
- 色彩关系
- 材质倾向
- 光影节奏
- 笔触气质
- 主体视觉特征

编码成额外条件，注入到采样过程中，让后续资产在保留变化空间的同时，**尽量别跑味儿**。

---

### 四、常见模型类型与职责
结合 `comfyorg/comfyui-ipadapter`、`cubiq/ComfyUI_IPAdapter_plus` README 与社区教程，可把常见分支拆成这几类。

#### 1. Base / Plus
- **Base**：参考能力较基础，影响较轻。
- **Plus**：特征提取更强，更适合风格锁定与主体保持。

**实战判断：**
如果目标是给《光与朽》或下个项目做**同章怪物、同套 UI、同批广告图**的风格统一，优先从 **IP-Adapter Plus** 起手。

#### 2. Composition
仓库列出的社区模型里有：
- `ip_plus_composition_sd15.safetensors`
- `ip_plus_composition_sdxl.safetensors`

README 描述非常明确：
- **general composition ignoring style and content**

这意味着它更偏向迁移画面布局，而不是照搬风格或主体细节。

**适合场景：**
- 想保留“主视觉在左、标题留白在右”的广告构图
- 想保留宣传图的大体版式
- 只要构图，不要照搬颜色和材质

#### 3. Face / FaceID
README 提到大量 FaceID 模型，并明确警告：
- 很多 FaceID 模型要配套 **专属 LoRA**
- 依赖 `insightface`
- 统一加载器还涉及命名规范

**结论：**
这条线更适合真人/角色脸一致性，不是当前《光与朽》最优先的方向。当前项目重点还是怪物、UI、场景、宣传图风格统一。

#### 4. Kolors / 社区扩展模型
仓库也列了 Kolors 版 IP-Adapter Plus 与 FaceID Plus，说明这套思路已经被扩展到不同底模体系。

**对主人真正有用的判断：**
先把 SD1.5 / SDXL 上的标准 IP-Adapter Plus 吃透，比研究一堆衍生分支更值钱。

---

### 五、ComfyUI 里的标准 IP-Adapter 工作流

#### 1. 标准主链
```text
Load Checkpoint
 ├─ MODEL → Apply IPAdapter → KSampler.model
 ├─ CLIP  → CLIP Text Encode（正/负）→ KSampler
 └─ VAE   → VAE Decode

Load Image（风格参考图）
 └─ IPAdapter / CLIP Vision 编码链
     └─ Apply IPAdapter

Empty Latent Image / VAE Encode(latent)
 → KSampler.latent_image
 → KSampler
 → VAE Decode
 → Save Image
```

#### 2. 数据流逻辑
- **Checkpoint** 决定底盘能力与基础审美
- **Prompt** 决定你这张图“画什么”
- **IP-Adapter** 决定它“更像哪种视觉气质”
- **KSampler** 在这些条件共同作用下出图

所以 IP-Adapter 不是替代 Prompt，而是给 Prompt 加一个**视觉参照系**。

---

### 六、最关键参数怎么调
结合 `cubiq` README、社区实战经验和 Reddit 讨论，真正该盯的就是这几个。

#### 1. weight
README 的通用建议非常明确：
- **Usually it's a good idea to lower the `weight` to at least `0.8` and increase the number steps.**

这条建议很重要，说明很多人一上来把权重拉太高，结果参考图把 prompt 压死了。

**实战区间：**
- `0.4 ~ 0.6`：轻参考，只借一点味道
- `0.7 ~ 0.9`：最稳的风格统一区间，适合游戏资产量产
- `1.0+`：参考图干预过强，容易压 prompt，自由度下降

#### 2. steps
README 明示：
- 权重降低一点，步数增加一点，通常更稳

**建议：**
- 草稿量产：`24 ~ 30`
- 成品精修：`30 ~ 40`

#### 3. cfg
社区经验里很常见的一点是：
- IP-Adapter 参考很强时，CFG 不宜太高
- 否则文字条件和图像条件会顶牛，画面容易僵、脏、失衡

**建议区间：**
- 一般风格统一：`5 ~ 7`
- 参考图很强：`4.5 ~ 6.5`

#### 4. weight type
README 提到：
- 如果想增加对 prompt 的服从度，可以在 `IPAdapter Advanced` 节点里调整 **weight type**。

这说明什么？
**IP-Adapter 不是只有“强或弱”一个旋钮，不同 weight type 会改变参考图特征注入的方式。**

初学阶段先别研究太花，先记住：
- 普通模式先跑通
- 如果出现“风格够了，但 prompt 不听话”，再尝试切换 weight type

---

### 七、三种最值钱的实战工作流

#### 工作流 A：单张母图锁死同章节风格
**用途：** 同一章怪物、圣物、UI 图标保持统一视觉语言

```text
风格母图 → IP-Adapter Plus
功能/造型 prompt + 固定 checkpoint + 固定负向词
→ KSampler → 输出一组同风格资产
```

**推荐参数：**
- weight：`0.75 ~ 0.9`
- steps：`28 ~ 36`
- cfg：`5 ~ 7`

**适合《光与朽》：**
- 熔岩章一套怪物
- 极寒章一套图标
- 某章节商店图与升级图标成套统一

#### 工作流 B：IP-Adapter + ControlNet 双约束
**用途：** 既要结构稳，又要风格稳

```text
草图/结构参考 → Canny / Lineart / Depth → ControlNet
风格母图 → IP-Adapter Plus
Prompt → CLIP Text Encode
全部条件进入 KSampler
```

**适合：**
- 主人手绘草图精修
- UI 图标重绘
- 怪物草图定型后做批量衍生

**核心原则：**
- ControlNet 管形
- IP-Adapter 管味

#### 工作流 C：买量素材主视觉批量衍生
**用途：** 一张投放效果好的主视觉，衍生出多张同气质广告图

做法：
1. 先挑一张最对味的广告主图作为风格母图
2. 后续广告图都接这张图进 IP-Adapter
3. 只改 prompt 中的卖点、主体、文案场景
4. 固定 checkpoint / sampler / steps / negative

**价值：**
- 视觉识别更统一
- 多图不会像不同设计师东拼西凑
- 更适合做 A/B 测试与素材迭代

---

### 八、和 ControlNet 的关系，必须讲透
根据 ComfyUI 官方 ControlNet examples：
- `Apply ControlNet` 不会自动预处理图片
- 多个 ControlNet / T2I-Adapter 采用链式叠加
- 每个控制器都要吃对应格式的 hint 图

而 Reddit 与社区讨论里常见的问题是：
- ControlNet 太强，会压住 IP-Adapter
- IP-Adapter 太强，又会把 prompt 和结构控制顶歪

#### 最稳组合思路
- 如果你最在乎**轮廓/姿态/草图结构**，让 ControlNet 当主控制器
- 如果你最在乎**风格统一、材质统一、色调统一**，让 IP-Adapter 当主控制器
- 另一个当辅助，不要两个都拉满

#### 推荐起手值
- Lineart / Canny：`0.7 ~ 0.9`
- IP-Adapter：`0.75 ~ 0.85`
- CFG：`5.5 ~ 6.5`
- steps：`30` 左右

---

### 九、最常见的翻车点

#### 1. weight 拉太高
结果：
- 参考图味太重
- prompt 不听话
- 所有图开始长得像同一张图的近亲复制品

#### 2. 参考图本身质量烂
IP-Adapter 吃的是参考图的视觉特征。
如果母图本身：
- 光影烂
- 材质脏
- 构图乱
- 细节糊

那你就是把垃圾稳定复制到整条流水线里。

#### 3. 误把 IP-Adapter 当结构控制器
它不是 Canny，不是 Pose，不是 Depth。
它不能精确锁姿态，也不能保线稿结构。

#### 4. FaceID 乱用
README 里已经提醒了：
- 配套 LoRA
- `insightface`
- 命名规范
- 模型版本兼容

不是当前任务重点，别上来就把自己绕死。

#### 5. 风格母图和目标资产复杂度差太远
比如你拿一张超复杂厚涂概念图，去驱动极简 UI 图标。
这很容易炸。

**原则：母图的复杂度、材质语言、目标场景，尽量和目标资产接近。**

---

### 十、《光与朽》里的最落地打法

#### 方案 1：章节母图制度
每一章先做 1 张“章节母图”，锁定：
- 色温
- 材质
- 光效
- 圣光/腐朽比例
- 氛围关键词

然后同章怪物、圣物、炮塔升级图标都从这张母图衍生。

#### 方案 2：UI 图标母图制度
先从一张最满意的图标定出：
- 黑金金属
- 晶核高光
- 边缘发光
- 背景压暗

后续技能图标都接这张图做 IP-Adapter 参考。

#### 方案 3：买量素材母图制度
从点击率最高或最有冲击力的一张广告图，提炼成母图，后续所有素材延展都沿着这条视觉语言长。

这才是工业化，不然每次都在重新抽卡。

---

### 十一、给主人的可执行模板

#### 模板 1：风格统一怪物
- checkpoint：固定 1 个主模型
- ip-adapter：Plus
- 参考图：本章节母图
- weight：`0.8`
- steps：`30`
- cfg：`6`
- prompt：只改怪物功能、体型、武器词

#### 模板 2：图标量产
- checkpoint：固定 UI 适配底模
- ip-adapter：Plus
- 参考图：最佳图标母图
- weight：`0.75`
- steps：`28`
- cfg：`5.5`
- prompt：统一材质词，替换技能主体词

#### 模板 3：草图精修
- 结构：Lineart ControlNet
- 风格：IP-Adapter Plus
- IP weight：`0.8`
- Lineart strength：`0.85`
- steps：`30 ~ 36`
- cfg：`5.5 ~ 6.5`

---

### 十二、今日核心结论
1. **IP-Adapter 本质是单张图驱动的临时风格补丁，可以理解成 1-image LoRA。**
2. **它最适合解决独立游戏项目“整套资产风格飘”的问题。**
3. **最稳参数起手是 `weight 0.75~0.9 + steps 28~36 + cfg 5~7`。**
4. **ControlNet 管结构，IP-Adapter 管气质，两者搭配比单用更适合工业化。**
5. **《光与朽》最该建立的不是“随机出图习惯”，而是“章节母图 / 图标母图 / 广告母图”制度。**

---

### 十三、参考来源
- ComfyUI 路线图：`knowledge/comfyui_expert_roadmap.md`
- ComfyUI 官方/社区 IP-Adapter 仓库：`https://github.com/comfyorg/comfyui-ipadapter`
- cubiq / ComfyUI_IPAdapter_plus README 与 examples：`https://github.com/cubiq/ComfyUI_IPAdapter_plus`
- ComfyUI 官方 examples（ControlNet / T2I-Adapter）：`https://github.com/comfyanonymous/ComfyUI_examples/tree/master/controlnet`
- Fannovel16 `comfyui_controlnet_aux` README（预处理器与模型对应关系）
- Reddit：`r/comfyui` 关于 IP-Adapter style/composition transfer 与 ControlNet 干扰问题讨论
- Bilibili / YouTube 社区关于 IP-Adapter Plus 风格统一、Style Transfer、Composition Transfer 的实战教程

# 📚 ComfyUI 中级节点精讲学习笔记 (2026-04-19)

## 🎯 今日攻克节点：自动化节点与抠图流水线（BRIA RMBG / LayerDiffuse / Batch / 自动命名导出）

### 一、为什么今天该学这个
前面我们已经解决了“能出图”和“能锁风格”，今天要解决的是更值钱的问题：**怎么把单张漂亮图，变成能批量生产、能直接进 Unity、能给买量和 UI 资产复用的工业管线。**

对《光与朽》这种独立项目来说，抠图流水线不是辅助功能，它直接决定三件事：
1. 怪物、炮塔、图标能不能快速拆成透明素材。
2. 同一批资产能不能一键批处理，而不是手动一张张扣。
3. 输出文件名和目录能不能规范，不然素材一多立刻炸管理。

---

### 二、今天的核心结论先说死
- **BRIA RMBG / BiRefNet 这一类节点**，适合做“现成图像的高精度背景移除”。
- **LayerDiffuse**，适合做“生成时直接产出带透明信息的前景层”。
- **Batch Loader + Save Image 命名规则**，决定你这条流水线是不是工业化，而不是手工艺。
- **一句话分工：RMBG 是后处理抠图刀，LayerDiffuse 是生成阶段透明层刀。**

---

### 三、BRIA RMBG 的本质与适用场景

#### 1. 它到底是什么
根据 BRIA 官方模型页与 ComfyUI-RMBG 仓库，RMBG 系列本质是**专门做前景/背景分离与精细边缘保留的抠图模型**。较新的 ComfyUI-RMBG 不只包含 RMBG-2.0，还整合了 INSPYRENET、BEN、BEN2、BiRefNet、SDMatte、SAM、GroundingDINO 等能力。

这意味着它不是“只能一键抠图”的玩具，而是一套：
- 背景移除
- 目标分割
- 掩码增强
- 文本提示分割
- 多模型切换
的综合分割工具箱。

#### 2. 它最适合什么
- 已经有一张怪物图、角色图、武器图，需要抠成透明 PNG。
- 做 UI 图标、商店素材、广告角色主物时，要把主体和背景分开。
- 先让 SD 产一张完整图，再把前景抽出来做二次合成。
- 做批量抠图，把一组资产先统一变成透明底，再丢进后续风格化或排版节点。

#### 3. 它不适合什么
- 你想“从无到有生成透明前景”，那该上 LayerDiffuse。
- 你要精确改结构或姿态，还是得交给 ControlNet / 重绘。
- 极端透明材质、烟雾、复杂光晕，单纯 RMBG 可能会吞边，需要手工或掩码增强补刀。

---

### 四、RMBG 标准工作流怎么连

#### 方案 A：最基础的透明抠图
```text
Load Image
  → RMBG / BiRefNet Remove Background
    ├─ image_output（前景图）→ Save Image
    └─ mask_output（遮罩）→ Save Image / Mask Preview
```

#### 方案 B：抠图后再换背景
```text
Load Image
  → RMBG
    ├─ foreground image ─┐
    └─ mask ───────────┐ │
                        ↓ ↓
                  Image Composite / Layer Blend
                        ↑
                 New Background Image
                        ↓
                    Save Image
```

#### 方案 C：抠图后进扩散二次精修
```text
Load Image → RMBG → 前景图
                    └→ mask
前景图 + mask → Inpaint / Composite / IP-Adapter / ControlNet
```

---

### 五、RMBG 实战参数与节点选择建议

#### 1. 模型选择思路
结合 ComfyUI-RMBG README：
- **RMBG-2.0**：通用抠图主力，边缘细节和复杂背景表现更稳。
- **BiRefNet / BiRefNet-HR**：高精度边缘更强，适合头发、尖刺、半透明边缘、复杂轮廓。
- **BEN / BEN2**：某些对象分离上速度与质量平衡较好，可做备选。
- **SAM / GroundingDINO / SAM2**：不是纯抠图刀，而是“文本提示选目标 + 分割”路线，适合只抠“武器”“人物”“怪物主体”而非全图前景。

#### 2. 主人最该记的实战判断
- **做图标、角色立绘、怪物卡牌**：先试 RMBG-2.0。
- **做毛发、羽毛、破碎边缘、腐朽丝状结构**：优先 BiRefNet。
- **只想抠出画面里某个指定物体**：用 Segment + 文本提示，不要硬拿普通 RMBG 瞎扣。

#### 3. 常见翻车点
- 背景和主体明度、颜色太接近，边缘容易发灰。
- 发光特效、烟雾、半透明材质容易被当背景吃掉。
- 抠完直接缩放或放大，alpha 会坏，必须**RGB 和 mask 分开处理后再合并**。

Reddit 社区也反复提到：ComfyUI 里透明 PNG 常常被拆成 **RGB + Mask**，后续上采样时要把 alpha 单独保住，不然透明背景会被黑底吞掉。

---

### 六、LayerDiffuse 的本质，跟 RMBG 根本不是一个东西

#### 1. 它到底干什么
根据 LayerDiffuse 官方项目与 ComfyUI-layerdiffuse README，LayerDiffuse 是一种**让扩散模型在生成阶段就携带透明层信息**的方案。它不是先生成一张实底图再扣，而是直接在 latent / decode 流程里把前景层和 alpha 一起推出来。

这东西牛在两点：
1. **前景不是后抠的，是生成时就被当成独立层处理。**
2. 对于 UI 素材、贴图、特效件、悬浮物、道具件，天然适合做“直接可叠加”的 PNG 资产。

#### 2. 它最适合什么
- 直接生成透明底技能图标主体、道具、怪物立绘前景。
- 买量素材里的角色/武器前景层，方便后期自由换底。
- 做多层合成资产，前景单独出，背景单独出。

#### 3. 它的限制
根据插件 README：
- 目前主要支持 **SD15 / SDXL**。
- 生成尺寸要**是 64 的倍数**，否则 RGBA decode 可能报错。
- 有些“FG/BG 抽离”工作流会有颜色偏移，作者自己 README 都提示过要注意。
- 某些 Stop at 参数在 ComfyUI 里不是原生好做，需要额外 img2img pass 模拟。

所以别神化它。LayerDiffuse 很强，但不是所有图都比 RMBG 稳。

---

### 七、LayerDiffuse 标准工作流怎么理解

#### 1. 直接生成透明前景（最实用）
```text
Load Checkpoint
  → CLIP Text Encode
  → Empty Latent Image
  → KSampler
  → LayerDiffuse Decode / RGBA Decode
    ├─ RGBA image → Save Image
    └─ alpha / mask → Save Image（可选）
```

#### 2. 要更稳的生产级做法
别只存一张 RGBA，最好**同时存 RGB 前景图和 alpha mask**：
```text
LayerDiffuse Output
  ├─ RGB foreground → Save Image
  └─ Alpha mask     → Save Image
```

为什么？
因为后续你要：
- 放大
- 描边
- 压缩
- Unity 内再处理

分开存，控制权更大。RGB 坏了修 RGB，Mask 坏了修 Mask，不会一锅端。

#### 3. BG/FG 联合生成适合什么
插件提供了：
- Generate foreground
- Blending FG/BG
- Extract FG from blended + BG
- Generate FG + Blended given BG

这些更像**分层美术工作流**，适合广告合成、角色前景和世界背景分离，不适合一上来就给所有素材都套。

---

### 八、RMBG vs LayerDiffuse，别再混了

| 维度 | RMBG | LayerDiffuse |
|---|---|---|
| 工作阶段 | 图像生成后 | 图像生成时 |
| 输入 | 已有图片 | 扩散生成流程 |
| 核心能力 | 背景移除、分割、mask | 直接生成透明前景层 |
| 最稳用途 | 现有素材抠 PNG | 新资产直接做透明图 |
| 风险点 | 吃边、灰边、半透明误判 | 尺寸限制、兼容性、颜色漂移 |
| 《光与朽》建议 | 旧素材清理、广告拆层、角色抠图 | UI 图标主体、道具件、前景特效件 |

**结论：已有图用 RMBG，新生成资产想直接透明底用 LayerDiffuse。**

---

### 九、批处理才是真正的工业化起点

#### 1. Batch Loader 的价值
社区常用的 `Load Image Batch` / WAS 节点套件，本质是：
- 一次读取整个文件夹
- 支持通配模式
- 让同一条工作流对整批图片循环执行

这一步看起来普通，实际上决定你是不是还在手工时代。

#### 2. 标准批量抠图链路
```text
Load Image Batch
  → RMBG / BiRefNet
    ├─ foreground image → Save Image
    └─ mask → Save Image
```

#### 3. 批量资产精修链路
```text
Load Image Batch
  → RMBG
  → Resize / Pad / Alpha Fix
  → Optional IP-Adapter / Inpaint / Outline
  → Save Image
```

#### 4. 批量管线最重要的不是“能跑”，而是“尺寸统一”
如果一批素材宽高乱七八糟：
- 图标进游戏时锚点会乱
- Unity Sprite 切图规则会炸
- 后续描边、发光、阴影都会歪

所以批处理前最好加：
- Resize to fit
- Pad to canvas
- Center align
- 输出固定尺寸（比如 512x512 / 1024x1024）

---

### 十、自动命名导出，不做这一步就是素材地狱
ComfyUI 社区手册明确写了，`Save Image` 的 `filename_prefix` 支持格式化字符串。

#### 1. 最实用的两种写法
**按分辨率分类：**
```text
%Empty Latent Image.width%x%Empty Latent Image.height%/image
```

**按日期归档：**
```text
%date:yyyy-MM-dd%/asset
```

#### 2. 节点值插入规则
可以用：
```text
%node_name.widget_name%
```
把某节点参数直接塞进文件名。

这对批处理极其重要，因为你可以：
- 按模型版本分类
- 按尺寸分类
- 按章节/批次分类
- 把 prompt 版本、LoRA 权重、场景编号写进路径

#### 3. 给《光与朽》建议的命名规则
```text
lightvsdecay/%date:yyyy-MM-dd%/%Empty Latent Image.width%x%Empty Latent Image.height%/chapter03_monster
```

或者更进一步，如果你用了能输出原始文件名的 loader：
- 原名保留
- 后缀追加 `_rmbg` / `_mask` / `_ldfg`

这样后期导入 Unity 时，一眼就知道：
- 哪个是前景
- 哪个是 mask
- 哪个是 LayerDiffuse 产物

---

### 十一、给主人直接能抄的三套工作流模板

#### 模板 1：旧怪物图快速抠透明底
用途：把现有怪物概念图转成透明 PNG 给立绘展示或买量排版。
```text
Load Image
→ RMBG-2.0
→ Mask Preview
→ Save foreground
→ Save mask
```
建议：先试 RMBG-2.0，不行再切 BiRefNet。

#### 模板 2：UI 技能图标主体直接透明生成
用途：做技能图标、圣物图标、炮塔图标。
```text
Checkpoint + Prompt
→ KSampler
→ LayerDiffuse RGBA Decode
→ Save RGBA
→ Save alpha（可选）
```
建议：画布固定 1024x1024，必须是 64 倍数。

#### 模板 3：整批资产自动抠图并规范输出
用途：一章怪物、一批武器、一套道具集体处理。
```text
Load Image Batch
→ RMBG / BiRefNet
→ Resize / Pad
→ Save foreground（统一命名）
→ Save mask（统一命名）
```
建议：命名里加日期、章节、分辨率，不然素材一周后就找不到爹。

---

### 十二、《光与朽》项目里的落地打法

#### 1. 怪物图鉴资产
- 怪物生成后先走 RMBG
- 输出透明前景 + mask
- 进 Unity 做图鉴卡片、商店展示、升级弹窗立绘

#### 2. UI 图标管线
- 直接用 LayerDiffuse 生成透明主体
- 后续统一叠描边、发光、底板
- 这样图标主体和 UI 底框能完全分层，后面换主题更快

#### 3. 买量广告素材
- 角色主体走 LayerDiffuse 或 RMBG 抽出
- 背景单独生成
- 前景、文案、按钮、光效分层合成
- 一套素材可以快速换 10 个封面和 20 个投放版本

#### 4. 下一项目的工业化价值
以后不只是《光与朽》，任何游戏项目都该建立：
- 章节素材母目录
- 批量抠图工作流
- 标准导出命名
- Unity 导入规范

这玩意一旦搭好，才是真正的美术产能杠杆。

---

### 十三、今天最重要的操作纪律
1. **已有图抠底，用 RMBG；新图直接透明生成，用 LayerDiffuse。**
2. **透明图别只存 RGBA，生产级最好同时存 RGB 和 Mask。**
3. **批处理前统一尺寸，批处理后统一命名。**
4. **别让素材管线停在“会做图”，要推进到“会量产、会归档、会复用”。**

---

### 十四、参考来源
- ComfyUI 路线图：`knowledge/comfyui_expert_roadmap.md`
- ComfyUI-RMBG 官方仓库 README：`https://github.com/1038lab/ComfyUI-RMBG`
- BRIA 官方 Hugging Face 模型页：`https://huggingface.co/briaai/RMBG-2.0`（页面提示受限访问，已确认其为官方模型主页）
- ComfyUI-layerdiffuse 官方仓库 README：`https://github.com/huchenlei/ComfyUI-layerdiffuse`
- LayerDiffuse 官方项目页：`https://github.com/lllyasviel/LayerDiffuse`
- ComfyUI Community Manual - Save File Formatting：`https://blenderneko.github.io/ComfyUI-docs/Interface/SaveFileFormatting/`
- ComfyUI Community Manual - Save Image：`https://blenderneko.github.io/ComfyUI-docs/Core%20Nodes/Image/SaveImage/`
- Reddit `r/comfyui` 关于透明 PNG / alpha 保持与 upscale 的讨论
- YouTube 社区关于 LayerDiffuse 透明图工作流教程（2024 年 3 月多条公开教程）
- 中文社区关于 ComfyUI RMBG 批量抠图工作流的实践整理（知乎 / CSDN / Bilibili）

# 📚 ComfyUI 高级节点精讲学习笔记 (2026-04-24)

## 🎯 今日攻克节点：高清放大矩阵（Latent Upscale vs Pixel Upscale，Ultimate SD Upscale + Tile ControlNet）

### 一、为什么今天该学这个
前面的基础和中级节点，解决的是“能出图、能控图、能统一风格”。
今天这一步解决的是更值钱的问题：**怎么把一张能看的图，放大成商用级宣传图、商店页头图、买量封面，而不是越放越糊、越修越脏。**

对《光与朽》来说，这一步直接影响：
1. Steam/商店头图能不能撑住大图展示。
2. 买量素材能不能在高分辨率下还保住激光、金属、晶核这些关键细节。
3. UI 图标、怪物卡面、章节宣传图能不能从“草稿感”升级成“成品感”。

---

### 二、先把核心结论说死
- **Latent Upscale** 是在潜空间放大，适合做“二段式高分修复”或 Hires Fix，本质是先把 latent 变大，再继续采样。
- **Pixel Upscale** 是在像素空间放大，适合已有图像的清晰化、锐化和传统超分。
- **Ultimate SD Upscale（USDU）** 是“先放大，再按 tile 分块做 img2img 重绘，再做接缝修补”的大图精修工作流。
- **Tile ControlNet** 不是拿来替代 USDU，而是给高 denoise 的大图重绘加约束，防止细节发疯、砖块感和局部幻觉乱长。
- 一句话：**Latent 放大保语义，Pixel 放大保像素，USDU 做商业级细节重绘，Tile ControlNet 给它拴狗链。**

---

### 三、Latent Upscale 到底是什么
根据 `Upscale Latent (by)` 节点说明，它做的是：
**在 latent space 里把编码后的图像张量放大，再交给后续 KSampler / Decode 处理。**

这意味着它不是直接把 JPG 放大，而是在 AI 还没最终解码成人眼图片之前，先扩大“内部草稿画布”。

#### 1. 它的优势
- 保持 prompt 语义连续性更好。
- 更适合做二段式采样（2-pass txt2img / hires fix）。
- 能在较低分辨率先确定构图，再在第二阶段补细节。
- 相比直接大图开采样，更省显存、更稳。

#### 2. 它的局限
- 它不是传统超分，不会凭空保留像素级边缘。
- 如果第二段 denoise 太高，还是会改形。
- 它更适合“重新生成式精修”，不是“无损放大”。

#### 3. 最适合什么场景
- 一开始先用 512 或 768 出构图。
- 再 latent 放大 1.5x~2x。
- 然后做一次低到中等 denoise 的二次采样。

这其实就是 ComfyUI 官方 examples 里经典的 **2-pass / hires fix** 思路。

---

### 四、Pixel Upscale 到底是什么
Pixel Upscale 是在 RGB 图像层放大，常见方式包括：
- `Image Upscale with Model`
- ESRGAN / RealESRGAN / SwinIR / 4x-UltraSharp 这一类超分模型
- 普通的 Lanczos / Bicubic / Nearest 图像缩放

根据 `docs.comfy.org` 的基础 upscaling 教程，像 `4x-ESRGAN` 这类模型本质是**对已有像素做超分增强**，适合：
- 提升清晰度
- 补边缘锐度
- 放大前做一次传统清晰化

#### 它的优势
- 稳，不容易改构图。
- 快，对已有图像友好。
- 适合图标、UI、截图、成品图的第一步清晰化。

#### 它的局限
- 不会像扩散模型那样真正“重绘新细节语义”。
- 放太大时，会出现假细节、边缘发硬、材质塑料感。

#### 最实用的理解
**Pixel Upscale 更像放大镜，Latent Upscale 更像重画大稿。**

---

### 五、Latent vs Pixel，到底怎么选

#### 1. 要保构图、补生成细节
优先：**Latent Upscale + 第二次采样**

适合：
- 怪物立绘
- 宣传图
- 场景图
- 买量封面

#### 2. 已经是一张成品，只想更清晰
优先：**Pixel Upscale**

适合：
- UI 图标
- 截图
- 立绘成图后的轻量放大
- 用于后续进入 USDU 前的预放大

#### 3. 想做真正的大图商用品质
优先：
**Pixel Upscale 预放大 → USDU 分块重绘 → 必要时 Tile ControlNet 约束**

这才是最稳的生产级路线。

---

### 六、Ultimate SD Upscale 的本质
根据 `ssitu/ComfyUI_UltimateSDUpscale` README 和节点文档：
USDU 会先把图像按 `upscale_by` 放大，然后把大图按 tile 切块，对每个 tile 做 img2img redraw，最后再执行 seam fix 去修接缝。

它不是普通放大器，而是：
**大图分块重绘系统。**

#### 标准逻辑
```text
输入图像
 → 先用 upscale model 或 Lanczos 放大
 → 切成多个 tile
 → 每个 tile 走 img2img 采样
 → 拼回整张图
 → seam fix 修补接缝
 → 输出最终大图
```

#### 为什么它值钱
因为扩散模型本来就更擅长 512/768/1024 这类训练尺度。USDU 的思路不是硬让模型一次吞 2K 大图，而是把大图拆回“模型熟悉的小块”去重绘，再拼起来。

---

### 七、USDU 关键参数，真正要盯的只有这些

#### 1. `upscale_by`
- 控制最终放大倍数。
- 官方文档建议常见起手是 `2.0`。
- 如果只是精修已有高分图，用 `No Upscale` 版，把放大步骤前置会更稳。

#### 2. `tile_width / tile_height`
节点文档明确建议：
- **tile 尺寸最好接近模型训练分辨率。**
- SD1.5 常用 `512x512`
- SDXL 常见可用 `1024x1024`，但显存压力更大

#### 3. `denoise`
这是 USDU 的灵魂参数。
官方节点文档给的建议非常明确：
- **0.05 ~ 0.2**：主要用于去糊、轻修细节，最稳
- 更高 denoise 只有配合 Tile ControlNet 一类强约束时才更安全

#### 4. `mode_type`
- `Linear`：按顺序一块块处理
- `Chess`：棋盘式隔块处理，通常更利于减轻接缝
- `None`：跳过 redraw，只保留 seam fix 或单纯放大

我的判断：
**默认优先试 `Chess`，它通常比线性顺刷更不容易留下明显格子感。**

#### 5. `tile_padding`
- 给 tile 周边补上下文
- padding 越合理，边缘融合越稳
- 官方默认示例常见 `32`

#### 6. `mask_blur`
- 用于 tile 回贴时柔化边界
- 默认常见 `8`
- 太低容易出接缝，太高容易让边缘软掉

#### 7. `seam_fix_mode`
文档列出的模式：
- `None`
- `Band Pass`
- `Half Tile`
- `Half Tile + Intersections`

结论：
- 接缝不明显时，先别乱开最重模式，太慢
- 真有砖块感时，用 `Half Tile + Intersections` 最彻底，但也最慢

#### 8. `force_uniform_tiles`
- 开启时，边缘 tile 也尽量保持统一尺寸
- 这更接近 A1111 的处理方式
- 更稳，但稍慢

生产环境建议：**默认开**。

---

### 八、USDU 最稳的三种用法

#### 工作流 A：基础商业精修
```text
Load Image
 → Image Upscale with Model（4x-UltraSharp / ESRGAN 等）
 → 缩放到目标尺寸
 → Ultimate SD Upscale (No Upscale)
 → Save Image
```

**用途：**
已有图先传统超分，再让 USDU 轻度重绘补真实细节。

**推荐参数：**
- denoise：`0.08 ~ 0.18`
- tile：SD1.5 用 `512`
- mode_type：`Chess`
- seam_fix：必要时开 `Half Tile`

#### 工作流 B：2-pass 生成后精修
```text
TXT2IMG 先出 768/1024 基础图
 → Pixel Upscale 或 Lanczos 放大
 → USDU 做 tile redraw
```

**用途：**
宣传图、商店头图、卡面。

#### 工作流 C：高 denoise 创造性重绘
```text
预放大图
 → Tile ControlNet
 → USDU / No Upscale
```

**用途：**
你不仅要变清晰，还想让材质、纹理、微观细节更“重新设计”一轮。

这时候没有 Tile ControlNet 约束，很容易炸。

---

### 九、Tile ControlNet 的价值
Reddit 社区和大量教程里反复提到：
当 USDU 的 denoise 稍高时，最容易翻车的问题就是：
- 局部乱长新东西
- 金属边缘发疯
- 砖块感明显
- 身体结构、线条、纹理不连续

Tile ControlNet 的思路，是把已经放大的图像再作为控制参考，给每个 tile 一个“你别离原图太远”的限制。

#### 它最适合什么
- 高分辨率人物 / 角色立绘
- 机械体、建筑、复杂场景
- 宣传图精修
- 你希望“更清晰，但别改妈”

#### 社区常见经验
根据 Reddit 讨论和实战流程：
- Tile ControlNet 强度常见起手 `0.33 ~ 0.5`
- 保持 tile 尺寸为方形更稳
- residual noise / denoise 要压低，通常 0.1~0.3 更保险

我的判断很直接：
**Tile ControlNet 不是为了让图更花，是为了让 USDU 在高分辨率下别撒野。**

---

### 十、给主人最值钱的参数起手模板

#### 模板 1：宣传图 / 商店头图稳妥放大
- 第一阶段出图：`768~1024` 宽
- Pixel Upscale：放大到目标尺寸的 `1.5x~2x`
- USDU：
  - tile：`512x512`（SD1.5）
  - denoise：`0.10 ~ 0.15`
  - mode：`Chess`
  - padding：`32`
  - mask_blur：`8`
  - seam_fix：`Half Tile`

#### 模板 2：UI 图标 / 技能图标
- 不建议高 denoise USDU 猛修
- 优先：
  - Pixel Upscale（ESRGAN/SwinIR）
  - 需要时 USDU `denoise 0.05~0.1`

原因：UI 图标最怕被扩散模型乱改形。

#### 模板 3：怪物立绘 / Boss 海报
- 先 Pixel Upscale 到大尺寸
- 再 USDU + Tile ControlNet
- 参数建议：
  - Tile CN strength：`0.35 ~ 0.5`
  - USDU denoise：`0.15 ~ 0.25`
  - tile：`512` 或 `768`

#### 模板 4：Latent 二段式 Hires Fix
```text
TXT2IMG → Latent Upscale(by 1.5~2.0) → KSampler(低中 denoise) → VAE Decode
```

**适合：**
从一开始就想让图在语义上更完整，而不是成图后再做像素补救。

---

### 十一、《光与朽》里的直接落地打法

#### 1. Steam 商店头图 / 宣传 KV
最稳路线：
**中分辨率出构图 → Pixel Upscale → USDU 轻度精修**

原因：
宣传图最怕局部乱长和接缝，一味高 denoise 就是在作死。

#### 2. 激光炮塔 / 圣物 / Boss 卡面
如果原图主体已经准：
- 先传统超分
- 再用 USDU 低 denoise 补金属、晶核、边缘光

如果还想强化材质：
- 加 Tile ControlNet 再抬一点 denoise

#### 3. UI 技能图标
别手贱。
图标优先保形，不要为了“更细”把轮廓搞坏。
最推荐：
- Pixel Upscale
- 必要时极低 denoise 的 USDU
- 后期再做统一描边、发光、底板

#### 4. 买量素材封面
这块最适合建立模板：
- 统一尺寸
- 统一 USDU 参数
- 统一 Tile ControlNet 强度

这样一批广告图放大后，视觉质量会稳很多，不会一张锐一张糊、一张像插画一张像塑料。

---

### 十二、最常见的翻车点
1. **把 USDU 当无脑锐化器**
   - 错，它本质是分块 img2img，会改图。

2. **denoise 拉太高却没加控制**
   - 结果就是接缝、幻觉、细节乱长。

3. **tile 尺寸瞎设**
   - 太小，砖块感重；太大，显存炸。
   - 最稳还是靠近模型训练分辨率。

4. **图标也用高 denoise 重绘**
   - 纯属作死，轮廓一变就废。

5. **想靠 Pixel Upscale 解决所有问题**
   - 它只会放大已有像素，不会聪明地补语义细节。

6. **想靠 Latent Upscale 做无损超分**
   - 也不对，它本质还是生成式放大。

---

### 十三、今天的核心结论
1. **Latent Upscale 适合生成阶段的高分修复，Pixel Upscale 适合成图后的稳定清晰化。**
2. **Ultimate SD Upscale 是大图商用品质的关键节点，本质是 tile 分块 img2img + seam fix。**
3. **USDU 最稳起手参数是低 denoise（0.05~0.2）+ 接近模型训练尺寸的 tile。**
4. **想在高分辨率下既补细节又不乱改，Tile ControlNet 是必要的保险丝。**
5. **《光与朽》最该落地的是一条固定的宣传图放大模板，而不是每次临时玄学调参。**

---

### 十四、参考来源
- ComfyUI 路线图：`knowledge/comfyui_expert_roadmap.md`
- ComfyUI Dev 节点说明：`Ultimate SD Upscale`
- ComfyUI Dev 节点说明：`Upscale Latent (by)`
- `ssitu/ComfyUI_UltimateSDUpscale` GitHub README 与 `js/docs/UltimateSDUpscale*.md`
- ComfyUI Examples README（2-pass txt2img / upscale models / controlnet 等示例入口）
- `docs.comfy.org` 基础 upscaling 教程（ESRGAN / Image Upscale with Model）
- Reddit `r/comfyui` 关于 USDU、Tile ControlNet、低 denoise 放大的经验讨论
- Bilibili / YouTube 社区关于 ComfyUI 高清放大、USDU、Tile ControlNet 的实战教程


# 📚 ComfyUI 初级节点精讲学习笔记 (2026-04-25)

## 🎯 今日攻克节点：ComfyUI 架构思维解析 2.0 —— Checkpoint / CLIP / Latent / KSampler / VAE 的真正分工

### 一、为什么这一步必须彻底打通
这不是“基础概念背诵”，这是整个 ComfyUI 的总线图。后面无论你上 LoRA、ControlNet、IP-Adapter、局部重绘、高清放大，全部都是往这条主链上插条件、替换模块、或分阶段采样。主链不懂，后面全是玄学；主链懂了，工作流就只是搭积木。

---

### 二、官方确认的五大核心模块分工

#### 1. Checkpoint = 总模型包 / 主脑
官方 `Load Checkpoint` 文档明确：一个 Checkpoint 会输出三路：
- `MODEL`：真正负责 latent 去噪的扩散模型
- `CLIP`：把提示词编码成条件
- `VAE`：负责像素空间和 latent 空间的双向翻译

**关键理解：**
Checkpoint 不是“一个单文件黑盒”，而是把生成系统的三块关键部件打包交给你。在 ComfyUI 里它被拆开，所以你能替换其中某一部分，比如外挂一个外部 VAE。

#### 2. CLIP = 提示词翻译器，不负责作画
`CLIP Text Encode` 节点做的事情只有一个：
- 把你的提示词转成 `conditioning`
- 再把 conditioning 交给 KSampler 作为“该往哪里收敛”的语义约束

**结论：**
CLIP 决定“AI听懂了什么”，不决定“怎么画出来”。
所以 prompt 改了，通常先影响语义方向；是否能稳定落地，还要看 MODEL 本身是否擅长那种题材。

#### 3. Latent = AI 真正作画的隐空间画布
官方基础工作流说明里写得很直白：图像不是直接在 RGB 像素层计算，而是在 latent space 里完成结构和细节生成，最后才解码回像素。

**实际意义：**
- latent 不是低清图片，而是压缩后的语义画布
- 在 latent 里操作更省算力
- 所以很多高级流程（重绘、放大、分段采样）都尽量先在 latent 层处理，再最后统一解码

#### 4. KSampler = 生成发动机 / 工作流心脏
官方与社区文档都把它视为核心节点。它做两件事：
1. 根据 `seed` 和 `denoise` 往 latent 里加噪或保留部分原信息
2. 根据 `MODEL + positive/negative conditioning` 逐步去噪，生成目标 latent

**关键参数落地理解：**
- `seed`：初始噪声起点；固定后更容易复现构图
- `steps`：去噪轮数；新手先 15~25，别瞎拉太高
- `cfg`：提示词服从强度；太低会飘，太高会糊、脏、对比过爆
- `sampler_name + scheduler`：去噪路径和节奏
- `denoise`：重绘幅度；1.0 接近纯重做，0.2~0.5 更适合精修

#### 5. VAE = latent 与像素世界的翻译官
官方 `VAE Decode` / `Load VAE` 文档说明：
- `VAE Encode`：把图片压进 latent
- `VAE Decode`：把 latent 还原成可见图像
- 外部 VAE 可替换 checkpoint 自带 VAE，以改善颜色、对比、面部边缘和整体还原质量

**狠话总结：**
没有 VAE，你拿到的只是 AI 内部草稿，人根本没法看。

---

### 三、标准工作流连线逻辑

#### 1. TXT2IMG（文生图）
```text
Load Checkpoint
 ├─ MODEL ───────────────┐
 ├─ CLIP → CLIP Text Encode(+) ─┐
 ├─ CLIP → CLIP Text Encode(-) ─┤
 └─ VAE ───────────────────────┐│
                               ↓↓
Empty Latent Image → KSampler → VAE Decode → Save Image
```

**逻辑解释：**
- Empty Latent Image 负责创建初始 latent 画布
- CLIP 把正负提示词编码成条件
- KSampler 在 latent 里迭代生成
- VAE Decode 把 latent 变成最终图像

#### 2. IMG2IMG（图生图）
```text
Load Image → VAE Encode → KSampler → VAE Decode → Save Image
Load Checkpoint
 ├─ MODEL → KSampler
 ├─ CLIP → CLIP Text Encode(正/负) → KSampler
 └─ VAE → VAE Encode / VAE Decode
```

**逻辑解释：**
- 输入图先通过 VAE Encode 进入 latent
- KSampler 按 denoise 强度决定改多少
- 再用 VAE Decode 输出成图

---

### 四、参数怎么设，别再乱试

#### 文生图初学推荐
- `steps`: 20
- `cfg`: 6~8
- `sampler`: `euler_a` 或 `dpmpp_2m`
- `scheduler`: `karras`
- `denoise`: 1.0
- 分辨率：SD1.5 先从 512~768 边长起步

#### 图生图精修推荐
- 构图基本不动，只提质感：`denoise 0.2~0.35`
- 保结构换材质/光影：`denoise 0.35~0.55`
- 大幅改风格：`denoise 0.6~0.8`

#### 外部 VAE 什么时候该上
- 出图颜色发灰、过曝、边缘脏
- 模型自带 VAE 不稳定
- 做统一美术资产时，需要更一致的颜色还原

社区常见稳妥做法：直接准备一个常用外部 VAE 作为默认解码器，减少不同 checkpoint 自带 VAE 的波动。

---

### 五、为什么 ComfyUI 比 WebUI 更适合工业化
官方 README 明确强调它是 graph/nodes/flowchart 工作流，优势不是“更难”，而是：
- 模块能拆开看：知道问题出在 prompt、模型、VAE 还是采样器
- 节点能复用：同一条骨架可复制成 UI 图标、怪物立绘、宣传图三种生产线
- 只重算改动部分：更适合迭代和批量出图
- 更适合接 API 和自动化脚本

**一句话：**WebUI 更像“按钮生图机”，ComfyUI 更像“可编排美术工厂”。

---

### 六、给《光与朽》的直接落地方案

#### 方案1：怪物概念图批量探索
- 固定一个主 checkpoint
- 固定一套负面词和基础风格词
- 只替换怪物关键词、攻击元素和 seed
- 一次出 4 张 batch，挑最能打的再进下一轮

#### 方案2：UI/技能图标统一风格
- 同一 checkpoint + 同一 VAE + 同一 sampler/cfg
- 保持统一尺寸与 prompt 模板
- 把变量只收敛到“图标主体词”

#### 方案3：主人手绘草图精修
- 手绘草图导入 → `VAE Encode`
- `denoise` 先从 0.3 起测
- 目标只做材质、光影、完成度增强，不要一上来重绘到认不出

---

### 七、今天最重要的结论
1. **Checkpoint 提供 MODEL/CLIP/VAE 三件套，是总脑包。**
2. **CLIP 只负责把提示词翻译成条件，不负责画。**
3. **KSampler 才是真正干活的生成心脏。**
4. **Latent 是 AI 的施工现场，VAE 是出入口。**
5. **学 ComfyUI，本质是在学数据流，不是在背按钮。**

---

### 参考来源
- ComfyUI 官方 README（工作流与模块化特性）
- cubiq / ComfyUI_Workflows `basic/README.md`（基础工作流、latent / VAE / KSampler 参数解释）
- BlenderNeko ComfyUI Docs：`LoadCheckpoint.md`、`CLIPTextEncode.md`、`KSampler.md`、`KSampler Advanced.md`、`VAEDecode.md`、`LoadVAE.md`

# 📚 ComfyUI 高级节点精讲学习笔记 (2026-04-27)
## 🎯 今日攻克节点：精准局部重绘（Inpainting）与蒙版（Mask）工作流

### 一、为什么今天该学这个
前面的放大、ControlNet、IP-Adapter 都是在“整张图”层面发力。
真正进入商用美术管线后，最值钱的能力反而是：**不推翻整张图，只改坏掉的 5% 区域**。

对《光与朽》这尤其重要：
- 怪物图已经八成对，只想重做眼睛、武器、核心发光区。
- UI 图标主体已经定了，只想修边缘、符文、金属结构。
- 买量首图已经能用，只想把一处构图瑕疵救回来。

不会局部重绘，就只能整张重抽，效率低得离谱，画风也容易漂。

---

### 二、先把核心结论说死
1. **Inpainting 的本质不是“修图”，而是“限定采样范围”。**
2. **Mask 质量决定上限，Prompt 只是在已有边界里发号施令。**
3. **想 100% 重做遮罩区：走 `VAE Encode (for Inpainting)` + 专用 Inpaint 模型。**
4. **想保留原局部结构、低 denoise 精修：走 `Set Latent Noise Mask` 或 `InpaintModelConditioning` 路线。**
5. **大图局部重绘别傻乎乎整张采样，优先 Crop & Stitch。**

---

### 三、ComfyUI 官方标准内绘链路

#### 1）官方最基础工作流
```text
Load Checkpoint（inpaint checkpoint）
Load Image（带 mask 或从 Mask Editor 生成 mask）
CLIP Text Encode（正/负）
VAE Encode (for Inpainting)
KSampler
VAE Decode
Save Image
```

ComfyUI 官方教程明确强调三件事：
- 局部重绘常见场景是**缺陷修复**与**局部细节优化**。
- `Mask Editor` 是官方推荐的手绘蒙版入口。
- `VAE Encode (for Inpainting)` 的 `mask` 会告诉采样器**哪些区域需要被重绘**。

#### 2）`VAE Encode (for Inpainting)` 四个关键输入
- `pixels`：原始图像
- `vae`：当前模型配套 VAE
- `mask`：需要修改的区域
- `grow_mask_by`：把遮罩向外扩一圈，避免接缝硬切

#### 3）官方推荐模型思路
官方基础教程直接用 `512-inpainting-ema.safetensors`，并指出：
- 专用 inpaint 模型的边缘过渡更自然
- 结果通常比普通模型直接硬改更稳

---

### 四、两条最关键的内绘路线：别再混

#### 路线 A：专用 Inpaint 模型硬替换
适合：
- 去掉错误手、坏掉的眼睛、奇怪物件
- 背景里删东西
- 想让遮罩区几乎重新生成

标准理解：
```text
原图 + mask → VAE Encode (for Inpainting) → KSampler
```

特点：
- 遮罩区改动幅度大
- 通常需要较高 denoise
- 边缘融合效果取决于 mask 扩张与模型质量

关键判断：
- 如果你是“这块我不要了，重做”，走这条
- 如果你是“这块已经差不多，只要微调”，别硬走这条

#### 路线 B：保留原内容的精修路线
适合：
- 改怪物眼神、武器形状、局部材质
- 保留原构图，只修某一块
- 想低 denoise 微创手术

两种常见做法：
1. `VAE Encode` + `Set Latent Noise Mask`
2. `InpaintModelConditioning`（社区大量工作流更爱用）

社区文档和节点说明反复强调：
- `Set Latent Noise Mask` 的本质是：**只给遮罩区加噪声，让 KSampler 只在那块动手**。
- Acly 的 `comfyui-inpaint-nodes` 也明确说：
  - 直接用 `VAE Encode (for Inpainting)` 时，不适合低 denoise 保留既有内容
  - 想做“refine existing content”，要走 `InpaintModelConditioning` 这类路线

一句话判断：
- **重做** → Inpaint 模型
- **精修** → Noise Mask / InpaintModelConditioning

---

### 五、Mask 才是真正的命门

#### 1）手绘 Mask：最稳
官方教程推荐直接右键 `Load Image` → `Open in MaskEditor`。
适合：
- UI 图标
- 小面积武器、眼睛、符文
- 你已经知道到底哪里要改

实战规则：
- 遮罩内部尽量纯白，别灰
- 边缘宁可多包 5~15 像素，也别卡死在轮廓线上
- 错误主要来自“遮罩太抠门”而不是“遮罩太大”

#### 2）SAM 自动分割：提效神器
`storyicon/comfyui_segment_anything` 提供 GroundingDINO + SAM 路线：
- GroundingDINO：靠语义词先找目标
- SAM：做精细分割

模型体积要有概念：
- `mobile_sam`：39MB，轻量快速，适合预览
- `sam_vit_b`：375MB，轻量正式用
- `sam_vit_l`：1.25GB
- `sam_vit_h`：2.56GB，精度高但重
- HQ 版更强调边缘精细度

我的建议：
- 预览/批处理：先 `mobile_sam` 或 `sam_vit_b`
- 角色边缘、武器轮廓、复杂发丝：再上 HQ 或大模型

#### 3）SAM 不是什么神
B 站与社区经验都很一致：
- 小图、主体清晰时，SAM 很香
- 大图、复杂遮挡、边界模糊时，SAM 会慢，也会分歪
- 最终商用品质，常常还是 **SAM 选区 + 手工修遮罩** 最稳

---

### 六、最值钱的参数，不要瞎拧

#### 1）`grow_mask_by`
用途：给边缘留过渡区，避免接缝。

我的起手建议：
- 小修补（眼睛/按钮边角）：`4~8`
- 中修补（武器/头部局部）：`8~16`
- 大修补（整块护甲/大面积背景）：`16~32`

判断逻辑：
- 接缝明显、边缘像贴纸：加大
- 改坏了周围原图：减小

#### 2）`denoise`
- 专用 Inpaint 模型路线：常用高值，接近重做
- Noise Mask / InpaintModelConditioning 路线：可用低值精修

我的实战起手值：
- 轻微精修：`0.2~0.35`
- 中度重绘：`0.4~0.6`
- 近乎重做：`0.75~1.0`

#### 3）遮罩羽化 / blur
社区节点普遍强调：
- 硬边 mask 容易出拼贴感
- 轻微 blur 能让过渡自然

起手值：
- 图标/UI：`2~6`
- 角色/怪物局部：`6~12`
- 大面积背景：`12~24`

#### 4）上下文（context）
局部重绘最常见翻车，不是 prompt 不行，而是**给模型看的上下文太少**。
`Crop & Stitch` 节点里的 `context_from_mask_extend_factor` 非常关键：
- `1.0`：基本不扩
- `1.5`：稳妥起手
- `2.0`：需要更多环境关系时用

如果你要改怪物武器，最好让模型同时看到手臂、肩膀和部分胸口；
只给它看一块刀柄，它很容易长歪。

---

### 七、超大图局部重绘：Crop & Stitch 才是生产级答案

`ComfyUI-Inpaint-CropAndStitch` 这套节点，我直接给高评价：这玩意儿是真生产力，不是玩具。

它的优势：
1. 只对遮罩附近采样，**速度快很多**
2. 能先裁切再放大到模型舒服的分辨率
3. 不会让未遮罩区域重复走 VAE 编解码，保真度更高
4. 自动 blend 回原图，边缘更稳

#### 生产级参数理解
- `output_resize_to_target_size`
  - SD1.5：优先 `512x512`
  - SDXL / Flux：优先 `1024x1024`
- `output_padding`
  - 常用 `32`
- `mask_expand_pixels`
  - 常用 `8~24`
- `mask_blend_pixels`
  - 常用 `16~32`
- `context_from_mask_extend_factor`
  - 起手 `1.5`
- `device_mode`
  - 默认 GPU，超大图/长视频爆显存再切 CPU

#### 什么时候必须用 Crop & Stitch
- 2K/4K 宣传图改局部
- 买量首图换主体局部造型
- Boss 海报改脸、改武器、改发光区
- UI 大图里只修一个角标或按钮，但不能损失整图清晰度

---

### 八、前处理与后处理：把接缝问题狠狠干掉

#### 1）前处理：先填遮罩区再采样
Acly 的 `comfyui-inpaint-nodes` 给了很清晰的思路：

`Fill Masked` 三种模式：
- `neutral`：灰填充，适合要“凭空长新内容”
- `telea`：借周围颜色填，适合物体移除
- `navier-stokes`：同样是边界补全，但更偏连续纹理过渡

我的建议：
- 你想加新器官/新结构/新部件 → `neutral`
- 你想擦掉杂物/去字/去瑕疵 → `telea` 优先

#### 2）后处理：Color Match
如果你发现：
- 遮罩外基本没变
- 但整块输出有统一偏色/偏亮

那就上 `Color Match (Masked)`。
它的价值不是让局部更细，而是把整体色相、亮度拉回原图体系。

---

### 九、三套最值得主人直接抄的工作流

#### 模板 A：怪物“眼睛 / 核心 / 武器”精修
适合《光与朽》当前最常见的需求。

```text
原图
→ 手绘 Mask / SAM 分割
→ VAE Encode
→ Set Latent Noise Mask
→ KSampler（denoise 0.25~0.4）
→ VAE Decode
```

参数纪律：
- mask 稍微包住周围一圈
- 固定 seed 先只测 denoise
- prompt 只描述要改的局部，不要整张图都重新下命令

#### 模板 B：坏手坏脸坏结构，直接重做
```text
原图
→ MaskEditor
→ VAE Encode (for Inpainting)
→ Inpaint Checkpoint / Fooocus Inpaint
→ KSampler（denoise 0.75~1.0）
→ VAE Decode
```

适合：
- 长歪的手
- 不想要的配件
- 背景脏东西

#### 模板 C：4K 宣传图只修一块
```text
原图 + mask
→ Inpaint Crop
→ 标准采样 / InpaintModelConditioning
→（可接放大或 Hires Fix）
→ Inpaint Stitch
```

适合：
- 商店头图
- 海报
- 买量封面
- 封面里只改角色表情或武器

---

### 十、《光与朽》里的直接落地打法

#### 场景 1：怪物图统一后，修每只怪的“发光器官”
不要整张图重做。
先固定同一风格母图，之后只对：
- 眼睛
- 核心能源仓
- 武器末端
- 护甲裂缝发光
做局部重绘。

收益：
- 保住整体造型统一
- 只在“卖点区域”做视觉差异
- 怪物系列图能更快量产

#### 场景 2：UI 技能图标去脏、补符文、改轮廓
UI 图标经常主体对了，但边缘脏、符号太糊。
做法：
- 小遮罩圈住图标中心或边缘
- 低 denoise 精修
- prompt 只写：`sharp rune engraving, clean silhouette, metallic edge, centered icon`

收益：
- 不会毁掉整套图标风格
- 特别适合批量修图标而不是重生图

#### 场景 3：买量素材首图 A/B 测试
同一张封面别重做 10 张。
应该：
- 只换主怪眼神
- 只换激光颜色
- 只换爆点区域亮度与碎片
- 只改角色/炮塔的一个关键动作部位

收益：
- 素材变量更纯
- 更容易知道 CTR 变化到底是哪个视觉点带来的

---

### 十一、最容易翻车的坑
1. **Mask 不够白**：看起来白，实际不是 255，结果原图漏出来。
2. **遮罩扩张不够**：边缘直接拼贴，像补丁。
3. **上下文太少**：模型不知道局部该怎么接身体或环境。
4. **本来该低 denoise 精修，却用专用 inpaint 高强度重做。**
5. **大图直接整张内绘**：又慢又糊，还破坏未修改区域。
6. **Fooocus Inpaint 乱配 Turbo/Lightning/Hyper 这种蒸馏 merge**：仓库明确说不行。
7. **自动分割后不人工复核**：SAM 只负责快，不负责替你背锅。

---

### 十二、今天最重要的结论
1. **局部重绘的核心不是 prompt，而是 mask。**
2. **`VAE Encode (for Inpainting)` 更像“重做遮罩区”，`Set Latent Noise Mask / InpaintModelConditioning` 更像“微创精修”。**
3. **大图局部修图必须养成 Crop & Stitch 习惯。**
4. **SAM 是提效器，不是最终审美判断器。**
5. **对《光与朽》，最值钱的不是重新生一张怪物，而是把现有可用图快速救成商用品质。**

---

### 参考来源
- ComfyUI 官方教程：<https://docs.comfy.org/tutorials/basic/inpaint>
- ComfyUI 官方示例：<https://comfyanonymous.github.io/ComfyUI_examples/inpaint/>
- BlenderNeko 文档：`VAE Encode (for Inpainting)`、`Set Latent Noise Mask`
- GitHub：Acly / `comfyui-inpaint-nodes`
- GitHub：storyicon / `comfyui_segment_anything`
- GitHub：lquesada / `ComfyUI-Inpaint-CropAndStitch`
- Reddit / r/StableDiffusion：`inpainting only on masked area in ComfyUI`
- YouTube：`EASY Inpainting in ComfyUI with SAM`、`Inpainting only on masked area, fast outpainting, and seamless blending`
- Bilibili：`Comfy UI 基础教程(七)——图生图高级蒙版重绘`、`ComfyUI系列教程【4】使用蒙版进行图像修复`

# 📚 ComfyUI 高级节点精讲学习笔记 (2026-04-29)

## 🎯 今日攻克节点：逻辑节点与条件控制（Primitive / Switch / Math Logic / Lazy Evaluation）

### 一、为什么今天必须学这刀
主人，前面的 ControlNet、IP-Adapter、局部重绘、放大矩阵，解决的是“画得准不准”。
但**逻辑节点与条件控制**解决的是另一件更工业化的事：

**同一套工作流，能不能根据输入条件自己选路、自己跳过没必要的分支、自己算参数。**

不会这套，你的 ComfyUI 工作流再强，也只是“手工拼图”。
会了以后，工作流才开始像一条真正的生产线：
- 小图自动走像素放大
- 大图自动跳过放大
- 横图和竖图自动选不同分辨率模板
- 只在需要时才执行高成本分支
- 批处理时根据资产类型走不同导出链

对《光与朽》这种需要同时产出 **UI 图标、怪物半身、宣传图、买量图、透明底资产** 的项目，这一步不是锦上添花，是效率分水岭。

---

### 二、核心认知：ComfyUI 里有两层“控制”

很多人把“节点连线”理解成纯数据流，这是半懂不懂。
真正要分清的是两层：

#### 1）数据控制（Data Routing）
决定**哪个值被传给哪个节点**。
常见工具：
- Primitive
- Reroute
- Switch
- Context/Pipe 类节点
- 任意类型传递节点（Any / Pass Through）

#### 2）执行控制（Execution Control）
决定**某个分支到底要不要真的执行**。
常见工具：
- Lazy Evaluation（官方执行模型）
- 条件分支节点（If / Compare / Boolean Switch）
- Loop / While / For 类节点
- Bypass / Mute / Branch Select

一句话：
**“线连上了”不等于“分支真的会被省掉”。**

这也是很多人工作流看着聪明，实际照样又慢又炸显存的根本原因。

---

### 三、官方层的底层逻辑：Lazy Evaluation 才是条件控制的根

根据 ComfyUI 官方 `Lazy Evaluation` 文档，默认情况下，节点的 `required` 和 `optional` 输入会先被求值，然后节点才执行。

这意味着：
- 你即便最后只用 A 分支
- 只要 B 分支还是普通必算输入
- ComfyUI 依然可能先把 B 分支也算出来

所以官方给出的正解不是“多画几根线”，而是：

#### 让输入变成 lazy input
做法有两步：
1. 在 `INPUT_TYPES` 里把输入标记为 `lazy: True`
2. 写 `check_lazy_status()`，按当前条件返回“到底还需要哪些输入”

官方示例很典型：
- 两张图按 mask 混合
- 如果 mask 全是 0，则根本不需要评估第二张图
- 如果 mask 全是 1，则根本不需要评估第一张图

这个思路非常关键。

#### 结论别记岔了
- **Switch 节点只是表层形式**
- **Lazy Evaluation 才是“没被选中的分支不执行”的底层保障**

所以以后你看任何“条件工作流”，先问自己：
**它只是切了输出，还是连执行都真的省了？**

---

### 四、最实用的四类逻辑节点

## 1）Primitive：统一参数入口，不再到处手改
Primitive 不是花哨节点，它是整个动态工作流的参数总线。

作用：
- 把一个数值、布尔值、文本或类型化输入集中管理
- 一处修改，多处联动
- 给后续 Compare / Math / Switch 提供稳定输入

最值钱的用法不是“少填一次数”，而是：
- 同一个 seed 同时喂两个采样器做 A/B 对照
- 同一组宽高/步数/denoise 控多个分支
- 用一个布尔总开关控制某整段流程

#### 典型连法
```text
Primitive(目标长边) → Math(算倍率) → Resize / Upscale
Primitive(是否导出透明底) → Boolean Switch → 保存链
Primitive(资产类型) → Compare → Branch Select
```

#### 实战建议
- 所有会频繁调的参数，先别直接写死在节点里
- 先抽成 Primitive，再下发
- 尤其是：长边、批量数、denoise、CFG、导出开关、是否抠图

这一步会让工作流从“临时拼装”变成“可维护”。

---

## 2）Compare / Boolean / If-Then-Else：让工作流开始“判断”
这是最基础的条件门。

常见判断：
- 宽 > 高？
- 长边 > 1024？
- 资产类型 == UI_ICON？
- 是否启用透明底？
- 当前倍率 > 2x？

输出通常是：
- True / False
- 0 / 1
- 选择 A / B 路径

#### 典型用途
1. **横竖图分流**
   - 横图走 1536x864
   - 竖图走 864x1536

2. **小图才放大**
   - 小于阈值才进 Ultimate/ESRGAN 分支
   - 大图直接存图，避免无意义耗时

3. **特定资产才抠图**
   - 怪物立绘、道具图标走透明底
   - 宣传图跳过 RMBG

#### 参数原则
- 判断阈值一定要**写死成规则**，别凭感觉临时改
- 最常见阈值：
  - 长边阈值：768 / 1024 / 1536
  - UI 图标目标边：512 / 768
  - 透明底导出：布尔开关

---

## 3）Math / Resolution Logic：自动算尺寸，比手填靠谱
这才是今天最能直接落地的一刀。

路线图里提到的经典例子：
**“根据输入图片尺寸自动计算最佳放大倍率”。**

这件事用纯手工做很蠢，因为：
- 原图尺寸每次不同
- 有些图只该放 1.5x
- 有些图该 2x
- 有些图超过目标后应该直接跳过

#### 最实用的自动尺寸公式
假设你希望输出目标长边为 `T`，原图长边为 `M`：

```text
scale = T / M
```

然后再加三层规则：
1. 若 `scale <= 1.0` → 不放大 / 直接导出
2. 若 `1.0 < scale <= 2.0` → 走单次 upscale
3. 若 `scale > 2.0` → 先像素放大到 2x，再二段细化，或拆成 latent + pixel 两段

#### 生产线里必须再补两条硬规则
- 分辨率尽量对齐模型习惯倍数（常见 8 / 64）
- 放大倍率要设上限，防止一张小破图被放成炸显存巨物

#### 我的建议上限
- UI 图标：目标边 512 或 768
- 宣传图：长边 1536 或 2048
- 小素材自动上限：最多 2x 或 2.5x
- 再大的提升交给二段工作流，不要一把梭

---

## 4）Branch / Loop / Flow Control：复杂工作流的总调度
社区节点包已经把这块卷得很猛。

从 GitHub 与社区资料看，当前常见路线是：
- **ControlFlowUtils**：高级循环、条件分支、逻辑运算、流程控制
- **ComfyUI-Logic**：较早期的条件渲染与比较逻辑扩展
- **rgthree / 分支控制思路**：更偏实际工作流管理、只运行单分支、参数整洁化

#### 这里有个现实判断
官方早期并没有一个“默认就很好用的条件分支节点体系”，社区长期都在补这一刀。
这也是 GitHub 里会有人专门提 “Conditional Branching Nodes” 需求的原因。

#### Loop 的价值
Loop 不只是炫技，它适合：
- 批量尝试多个 prompt 变量
- 多倍率试跑
- 多 seed 扫描
- 逐个素材自动处理

但别一上来就迷恋 Loop。
对你当前阶段，更值钱的是：
**先把判断 + 切支 + 自动算参打通。**
Loop 是第二阶段放大器，不是第一优先级。

---

### 五、真正落地的标准工作流：自动判图尺寸并选最优放大链

下面这套，是最适合你当前项目立刻吸收的动态工作流骨架。

## 工作流目标
输入任意一张图后，自动判断：
- 是否需要放大
- 放多少
- 走 latent 细化还是像素放大
- 最后是否接透明底导出

## 标准思路
```text
Load Image
 → 读取宽高
 → Math：取 max(width, height)
 → Primitive：目标长边 TargetMax
 → Math：scale = TargetMax / maxSide
 → Compare：scale <= 1 ?
    ├─ True  → 直出 / 可选轻修
    └─ False → Compare：scale <= 2 ?
              ├─ True  → Pixel Upscale / Ultimate SD Upscale 轻度细化
              └─ False → 先 2x 像素放大，再二段重绘 / 分步放大
 → Compare：是否透明底资产？
    ├─ True  → RMBG / LayerDiffuse / Alpha 导出
    └─ False → 普通保存
```

## 关键参数建议
### A. 长边目标
- UI / 技能图标：512~768
- 怪物卡面 / 立绘：1024~1536
- 宣传图 / 商店图：1536~2048

### B. 放大倍率阈值
- `<= 1.0`：不放大
- `1.0 ~ 2.0`：单段放大
- `> 2.0`：二段放大，不要一次拉爆

### C. 二段重绘 denoise
- 轻修：`0.15 ~ 0.25`
- 明显补细节：`0.25 ~ 0.35`
- 超过 `0.4` 就开始有明显跑形风险

### D. 分辨率对齐
- 最终宽高尽量对齐 8 或 64 的倍数
- 尤其是接 SDXL / upscale / tile 类链路时更稳

---

### 六、给主人最值钱的应用方式：怎么服务《光与朽》

#### 1）怪物立绘与图鉴头像统一出货
问题：
- 草图来源尺寸不统一
- 有的需要透明底，有的不需要
- 人工每张调尺寸很蠢

解决：
- 自动读尺寸
- 小图走放大，大图跳过
- 立绘走透明底分支
- 宣传图走普通导出

结果：
同一个工作流就能同时处理图鉴头像、怪物卡面、宣传切图。

#### 2）UI 图标生产线
问题：
- 技能图标经常要反复出多版
- 有些图太小直接糊
- 有些已经够大，再放只会烂

解决：
- 目标边固定 512/768
- 自动判断是否放大
- 自动抠透明底
- 同一 Primitive 控 seed / denoise / 导出命名

结果：
以后做《光与朽》的技能图标，不用每版重新搭线。

#### 3）买量素材与商店图
问题：
- 横图、竖图、封面图尺寸需求不同
- 不同平台需要不同长宽比

解决：
- Compare 宽高比
- 横图走一套分辨率模板
- 竖图走另一套模板
- 必要时再接文案区安全边距模板

结果：
你做 Steam、微信、短视频封面时，不会再一张张人工改尺寸。

---

### 七、今天这刀最容易踩的坑

#### 1）以为“接了 Switch 就等于省算力”
错。
如果底层没 lazy / 没真正执行控制，分支可能还是会被提前求值。

#### 2）把逻辑节点当成炫技插件
错。
逻辑节点真正价值不是花里胡哨，而是：
- 节省重复操作
- 减少错参
- 降低炸显存概率
- 提升批量处理稳定性

#### 3）一开始就上超复杂 Loop
错。
先把：
- 参数集中
- 条件判断
- 尺寸自动计算
- 分支导出
这四件事搞稳。

#### 4）忘记倍率上限
这是最容易炸的一刀。
小图自动放大如果不封顶，很快就会把显存和时间一起拖死。

#### 5）规则不写成固定阈值
今天临时改一下，明天临时改一下，最后没人知道工作流到底按什么标准在跑。

---

### 八、我给你的推荐学习顺序

#### 第一阶段（现在就该会）
1. Primitive 统一参数
2. Compare / Boolean 判断
3. Switch / Branch 输出选路
4. Math 自动算倍率和目标宽高

#### 第二阶段（熟了再上）
5. 条件抠图 / 条件导出
6. 横竖图模板自动切换
7. 批处理规则化

#### 第三阶段（再升级）
8. Loop / For / While
9. 多资产类型总控管线
10. 视频帧批处理条件路由

这顺序最稳。别跳级，不然就是拿电锯修指甲。

---

### 九、一句话总结

**逻辑节点与条件控制的本质，不是让 ComfyUI 更复杂，而是让它开始“自己做决定”。**

当你的工作流能根据输入尺寸、资产类型、导出目标自动选路、自动算参、自动跳过无效分支时，它才真正配叫“AI 美术管线”，而不是一坨能跑的节点截图。

---

## 📌 今日关键记忆点
1. ComfyUI 的条件控制要区分 **数据选路** 和 **执行选路**。
2. 真正能省计算的关键不是表面 Switch，而是 **Lazy Evaluation**。
3. Primitive 是参数总线，Math/Compare 是判断大脑，Branch/Switch 是流量调度器。
4. 最值得立刻落地的不是 Loop，而是 **自动算尺寸 + 自动选放大链 + 自动导出分支**。
5. 《光与朽》最该先做的是：
   - 图标自动放大与透明底
   - 怪物卡面尺寸自适配
   - 横竖宣传图分辨率自动切换

## 参考来源
- ComfyUI 官方文档：`Lazy Evaluation - ComfyUI`
- GitHub 议题：`Conditional Branching Nodes`（ComfyUI 社区长期需求）
- GitHub / 社区线索：`VykosX/ControlFlowUtils`、`theUpsider/ComfyUI-Logic`、`rgthree/rgthree-comfy`
- Reddit：关于“未使用分支为何仍会运行”的讨论与执行模型解释
- YouTube：`ComfyUI 003 - Execution Flow Guide: Make a clean workflow`、`Workflow Looping in ComfyUI! All New Automation with For and While Loops!`、`How to run only one branch of a comfyUI workflow (in 60 seconds)`
- Bilibili：`小白也能听懂的ComfyUI工作流搭建教程！节点连线整理技巧+复杂工作流解构`、`Comfyui工作流从零基础到精通（2026新手入门实用版comfyui教程）`

## 2026-04-30｜资深阶段①｜AnimateDiff 与 SVD（Stable Video Diffusion）动效视频管线

### 一、为什么它是当前路线上的下一个关键节点
在完成“逻辑节点与条件控制”后，下一个顺序节点就是把静态资产推进到可投放、可测试、可复用的动态素材层。对《光与朽》与下一阶段 AI 视频反向立项来说，这不是“会做动画”这么简单，而是把单张立绘、怪物设定图、场景概念图，升级成可用于抖音/小红书/B站测试的短动效素材生产线。

### 二、核心定位：AnimateDiff 和 SVD 不是一个东西
1. **AnimateDiff**
   - 本质：在标准 SD/SDXL 采样链上注入 **motion model（运动模块）**，让一批 latent 帧在采样时带上时序运动一致性。
   - 强项：
     - 更适合 **txt2video / img2video / vid2vid 风格化**。
     - 可叠 ControlNet、IPAdapter、Motion LoRA、Prompt Travel。
     - 更像“可编排的视频生成框架”。
   - 弱点：
     - 参数多，容易炸显存。
     - 长序列和高分辨率容易闪烁、漂移。

2. **SVD / SVD-XT（Stable Video Diffusion）**
   - 本质：Stability 官方的 **image-to-video 专用视频模型**。
   - 强项：
     - 上手简单，尤其适合“单张图做 14 帧 / 25 帧短动效”。
     - 非常适合把角色卡面、概念图、封面图变成轻微呼吸/镜头推拉/粒子晃动素材。
   - 弱点：
     - 可控性弱于 AnimateDiff。
     - 本质更偏“让图动起来”，不适合复杂叙事或长镜头调度。

**结论很硬**：
- 想做“静态图轻动效广告素材”——先上 **SVD**。
- 想做“可控循环动画、角色待机、镜头运动、视频重绘”——主力是 **AnimateDiff**。
- 最实战的工业流不是二选一，而是：**先用 SDXL/图生图出关键帧 → SVD 快速试钩子 → 表现好的方向再转 AnimateDiff 精修。**

### 三、官方与仓库确认到的关键事实
#### 1）ComfyUI 官方 Video Examples
官方页面明确给出两套 SVD 检查点：
- `svd.safetensors`：14 帧
- `svd_xt.safetensors`：25 帧
并说明最基础工作流就是 **给一个 init image 直接做 image-to-video**，或者 **先用 SDXL 出图，再送进 SVD 做 txt-to-image-to-video**。

官方解释的关键参数：
- `video_frames`：输出帧数
- `motion_bucket_id`：数值越高，运动幅度越大
- `fps`：越高越不顿，但同样帧数下视频时长更短
- `augmentation_level`：给初始图加多少噪声；越高越不像原图，也通常意味着运动更大
- `VideoLinearCFGGuidance`：让离首帧越远的帧使用更高 CFG，提升后段帧稳定度与服从度

#### 2）AnimateDiff Evolved 仓库确认到的关键结构
`Kosinkadink/ComfyUI-AnimateDiff-Evolved` 是当前 ComfyUI 里最成熟、工程化程度最高的 AnimateDiff 节点体系之一。仓库 README 与文档确认：
- 安装 motion modules 后，可与几乎所有 vanilla/custom KSampler 配合。
- 深度整合：
  - ControlNet
  - SparseCtrl
  - IPAdapter
  - Prompt Scheduling
  - Motion LoRA
  - Context Options / View Options
  - FreeNoise / FreeInit
- 适合从基础 txt2vid 扩展到长序列、循环动画、vid2vid 和条件控制。

#### 3）VideoHelperSuite 在视频管线里的角色
`ComfyUI-VideoHelperSuite` 不是生成模型，但它是视频工作流的运输层：
- `Load Video`：把视频拆成帧，支持改帧率、截取区间、限制批量帧数
- `Video Combine`：把帧重新编码成视频
- `frame_rate`：官方说明 **AnimateDiff 通常建议保持 8fps**，或与输入视频 force_rate 对齐
- `pingpong`：可直接生成首尾往返循环，适合待机、UI 呼吸、循环特效

### 四、最小可用工作流 ①：SVD 图片转视频
适合：
- 角色立绘轻呼吸
- 怪物卡面抖动
- 场景概念图加镜头推进
- 买量首钩子素材快速 A/B

#### 基础连线逻辑
1. `Load Image` / `Load Image Batch`
2. （可选）`Image Resize` / 裁到目标比例
3. `Load Checkpoint`（SVD 或 SVD-XT 模型）
4. `VAE Encode`（如果工作流版本需要）
5. `SVD image-to-video sampler / 官方 video workflow`
6. `VideoLinearCFGGuidance`
7. `VAE Decode`
8. `Video Combine`

#### 推荐起手参数
- **帧数**：
  - 测钩子：14 帧先跑
  - 稍完整镜头：25 帧
- **fps**：
  - 6~8：动势更明显，适合广告感
  - 8~12：更顺，但更像“轻动图”
- **motion_bucket_id**：
  - 低值：轻微呼吸、漂浮、镜头缓推
  - 中值：角色衣摆/粒子/镜头位移
  - 高值：容易形变失控，先别贪
- **augmentation_level**：
  - 低：保形优先
  - 中：需要更明显运动时再加
  - 高：最容易把角色脸和边缘搞坏

#### 实战判断规则
- 如果你要保住角色脸、UI 图标、武器轮廓：**优先低 augmentation + 中低 motion_bucket**。
- 如果你要做“封面图突然活了”的吸睛感：**先拉 motion_bucket，再微调 augmentation**。
- 运动不够，不要第一反应暴力提噪；先换构图、给更有前后景层次的输入图，收益更大。

### 五、最小可用工作流 ②：AnimateDiff 角色待机 / 循环短动画
适合：
- 怪物待机
- Boss 预警动势
- 技能前摇概念动画
- 宣传图局部生命感

#### 基础连线逻辑（Gen2 推荐）
1. `Load Checkpoint`（SD1.5 或 SDXL，对应 motion model 必须匹配）
2. `CLIP Text Encode (Prompt / Negative)`
3. `Empty Latent Image` 或 `Load Image -> VAE Encode`（决定 txt2vid 还是 img2vid）
4. `Load AnimateDiff Model`
5. `Apply AnimateDiff Model (Adv.)`
6. `Context Options`（先从 `Standard Static` 起）
7. `Sample Settings`（需要时开 FreeNoise / FreeInit）
8. `Use Evolved Sampling`
9. `KSampler`
10. `VAE Decode`
11. `Video Combine`

#### 关键节点理解
- **Load AnimateDiff Model**：加载 motion module
- **Apply AnimateDiff Model (Adv.)**：把运动模块注入到采样流程，可控制生效区间
- **Context Options**：长视频最重要的稳定器；按时间窗分段采样，绕过 motion model 的甜点帧数限制
- **View Options**：只限制 motion model 看到的帧窗，速度更快但不减主采样显存
- **Sample Settings**：扩展噪声与迭代策略
- **Video Combine**：真正出 mp4/webm 的收尾节点

### 六、AnimateDiff 的关键参数与判断逻辑
#### 1）motion model 匹配
- SD1.5 motion model 只能配 SD1.5 系底模
- SDXL motion model 只能配 SDXL 系底模
- 错配直接浪费时间，结果会脏、飘或压根跑不通

#### 2）beta_schedule
仓库明确写了：不同运动模型推荐不同 beta_schedule，**优先用 `autoselect`**。
这条很值钱，因为很多炸图不是 prompt 烂，是 schedule 没对上。

#### 3）context_length
- AnimateDiff 常见甜点是 **16 帧上下**；HotshotXL 常见甜点 **8 帧**。
- 超过甜点后，不要硬顶全长一起算，改用 `Context Options`。
- 起手建议：
  - 16 帧短待机：直接 16
  - 32/48 帧：context_length 先 16，overlap 4 左右起步

#### 4）context_overlap
- 作用：相邻时间窗的共用帧数量
- 太低：窗与窗接缝明显
- 太高：更稳，但更慢更吃算力
- 实战起手：**4 是很稳的入门值**

#### 5）scale_multival / effect_multival
- `scale_multival`：控制运动幅度
- `effect_multival`：控制运动模块对采样过程的影响强度
- 真正强的玩法不是写死一个值，而是通过 keyframe / multival：
  - 前半段轻动
  - 中段抬运动
  - 结尾回稳
这对“蓄力—爆发—收尾”的技能演示特别有用。

#### 6）FreeNoise
仓库说明：
- 能让长序列在 context/window 重复时更稳定
- 比 repeated_context 更不容易出现明显重复感
**结论**：做 32 帧以上动画时，优先试 `FreeNoise`，比傻堆步数更聪明。

#### 7）FreeInit
仓库说明：
- 通过多次完整采样，把已有 latent 的低频信息和新噪声高频信息混合
- **2 次 iteration 就是 2 倍时间成本**
**结论**：
- 它是质量救火器，不是默认常开项
- 当角色结构老飘、动作前后不接时再开，不要一上来就把时间炸掉

### 七、SVD vs AnimateDiff：怎么选
#### 选 SVD 的情况
- 你已经有一张不错的立绘/场景图
- 目标是快速做 1~3 秒动态封面
- 想验证“这个画面动起来是否更吸引点击”
- 不需要复杂镜头逻辑

#### 选 AnimateDiff 的情况
- 你要更强控制感
- 你需要循环待机、镜头运动、角色动作节奏
- 你需要接 ControlNet / IPAdapter / Prompt Travel
- 你准备把这个流程沉淀成长期视频素材生产线

#### 组合打法（最实用）
1. 先用静态图出 3 个视觉方向
2. 每个方向用 SVD 各做 14 帧测试版
3. 看哪个最像“会让陌生人停住的广告封面”
4. 只有赢家，才进入 AnimateDiff 精修和批量化

### 八、对《光与朽》最值得先落地的三个小工作流
#### 工作流 A：Boss 立绘呼吸 + 粒子漂浮
- 输入：Boss 概念图 / 宣传图
- 路线：SVD 14 帧
- 参数策略：低 augmentation，中低 motion_bucket
- 作用：做 Steam 图、B站封面、短视频开头第一秒动效

#### 工作流 B：激光塔核心镜头循环
- 输入：塔、激光、敌人三层拆开的合成图
- 路线：AnimateDiff + Looped/或 pingpong 输出
- 作用：做“激光折射翻盘”的核心爽点循环素材
- 重点：别先追求长剧情，先做 1.5~2 秒纯爽点循环

#### 工作流 C：实机录屏转风格化买量素材
- 输入：Unity 实机录屏
- 路线：VideoHelperSuite 读视频 → 抽帧 → AnimateDiff/ControlNet 约束 → Video Combine
- 作用：把真实玩法包装成更抓眼的风格化广告
- 注意：先解决帧一致性，再谈画风；闪烁视频再酷也是垃圾

### 九、今日最该记住的坑
1. **先保形，再加运动。** 一上来追大幅运动，最先坏的是脸、武器边缘和 UI 可读性。
2. **长视频不是多堆帧，是拆 context。** 不懂 context，视频稳定性永远上不去。
3. **FreeInit 很贵。** 它是修问题的，不是默认配置。
4. **SVD 是钩子测试器，不是万金油导演。**
5. **AnimateDiff 真正的强点是可组合。** ControlNet、IPAdapter、Prompt Travel、VideoHelperSuite 一接上，才像生产线。

### 十、明日/下次继续深化的方向
如果沿路线继续推进，下一步应细分为：
1. AnimateDiff 的 **循环动画（loop）与首尾帧闭环技巧**
2. Video-to-Video 的 **时序一致性控制**
3. 用 ControlNet / SparseCtrl 约束实机录屏转绘

### 参考来源
- ComfyUI 官方示例：`https://comfyanonymous.github.io/ComfyUI_examples/video/`
- GitHub：`https://github.com/Kosinkadink/ComfyUI-AnimateDiff-Evolved`
- GitHub 文档：`documentation/nodes/README.md`、`documentation/samples/README.md`
- GitHub：`https://github.com/Kosinkadink/ComfyUI-VideoHelperSuite`
- GitHub：`https://github.com/kijai/ComfyUI-SVD`（仓库 README 已注明官方 Video Examples 已覆盖）
- Reddit：`[GUIDE] ComfyUI AnimateDiff Guide/Workflows Including Prompt Scheduling`
- Reddit：`9 Animatediff Comfy workflows that will steal your weekend...`
- Bilibili：`【保姆级分享2】ComfyUI+SVD图片生成视频工作流快速上手`
- Bilibili：`Comfyui实用工作流教程11期——Animatediff首尾帧丝滑可控动画`

## 2026-05-01｜资深阶段②｜Video-to-Video 时序一致性控制（AnimateDiff + ControlNet / SparseCtrl）

### 一、为什么它是当前路线上的下一个未掌握节点
路线图里资深阶段①已经打通了 **SVD / AnimateDiff 的静态图动效化**，下一步按顺序就该攻克：**Video-to-Video 转绘时，怎么让视频“像同一个镜头”而不是每帧都像不同作品。**

这一步不解决，AI 视频只能当炫技；解决了，才配进《光与朽》买量、B站封面动效、抖音/XHS 测试素材管线。

---

### 二、先把核心结论说死
- **Video-to-Video 的第一目标不是更炫，而是更稳。**
- **时序一致性 = 结构一致 + 运动连续 + 纹理别乱跳。**
- 在 ComfyUI 里，最稳的骨架不是“单靠 AnimateDiff 硬抗”，而是：
  **输入视频帧 + AnimateDiff 运动先验 + ControlNet/SparseCtrl 结构约束 + 低到中等 denoise。**
- 真正能救命的不是多堆节点，而是三条纪律：
  1. **帧率先压低**（先测 8fps）
  2. **上下文分段采样**（16 帧窗口最稳）
  3. **控制强约束数量**（一强一弱通常比三强乱锁更稳）

一句话：**先保形，再保动，最后才补细节。**

---

### 三、官方与仓库里确认到的关键事实

#### 1）VideoHelperSuite 是视频运输层，不是可有可无的小插件
`ComfyUI-VideoHelperSuite` README 明确给出：
- `Load Video`
  - `force_rate`：丢帧或补帧到目标帧率；README 直接点名 **AnimateDiff 通常建议 8fps**。
  - `frame_load_cap`：单次最多读入多少帧，本质就是分段处理上限。
  - `skip_first_frames`：长视频分批处理的关键，用于续跑后续片段。
  - `select_every_nth`：进一步抽帧，降低时序压力。
- `Video Combine`
  - `frame_rate`：通常与 `force_rate` 对齐；AnimateDiff 常规建议也是 **8fps**。
  - 支持可选 `audio` 合成回输出视频。

这说明 ComfyUI 的 vid2vid 不是“一次吞完整视频”最稳，而是：
**先拆帧、限帧、分段，再合成回视频。**

#### 2）AnimateDiff-Evolved 的关键，不只是 motion module
GitHub 搜索结果与仓库说明都强调：
- AnimateDiff 工作流常与 **ComfyUI-Advanced-ControlNet** 搭配。
- 这个组合的价值在于：**让 ControlNet 和 Context Options 协同工作**，并可控制哪些 latent 段受约束。
- 说明里明确提到：**Includes SparseCtrl support**。

这很关键。因为 vid2vid 最容易炸的地方不是“没运动”，而是**长序列每一段各画各的**。能让 ControlNet 在上下文窗口内正确生效，才有资格谈时序一致性。

#### 3）Reddit 社区反复验证的稳定规则
从 `ComfyUI AnimateDiff Guide/Workflows Including Prompt Scheduling` 与相关讨论里，能提炼出几条非常实用的经验：
- **AnimateDiff 的稳定甜点常在 16 帧上下**，偏离太远往往更容易乱。
- **context overlap** 的意义，就是让窗口分段时有重叠，例如“1-16，12-28”，用重叠帧来缓和段与段之间的跳变。
- 单帧 upscale 或 tile 重绘如果不加时序约束，常会出现 **“每帧细节都不一样”** 的问题；Reddit 上有人直接点名 `controlnet tile resample` 会把细节加得很散，结果整体仍不连贯。
- 当结果像“互不相关的一堆图”时，常见原因不是模型坏了，而是：
  1. 没有真正把视频帧当结构锚点
  2. denoise 太高
  3. context / overlap 没配好
  4. 一次强行跑太长序列

---

### 四、Video-to-Video 的真正骨架

#### 最小可用稳态链路
```text
Load Video (force_rate=8, frame_load_cap=N)
  → Video Frames / Image Batch
  → VAE Encode（或直接作为图像参考）
  → ControlNet 预处理（Depth / Canny / Lineart / OpenPose，按题材选）
  → Apply Advanced ControlNet / SparseCtrl
  → Load AnimateDiff Model
  → Apply AnimateDiff Model (Adv.)
  → Context Options
  → KSampler（低~中 denoise）
  → VAE Decode
  → Video Combine (frame_rate=8)
```

#### 这条链每个部分到底在干嘛
- **Load Video**：把原视频变成“时序锚点来源”。
- **ControlNet / SparseCtrl**：告诉模型“每一帧至少长成这个结构”。
- **AnimateDiff**：负责把一批帧之间的运动关系串起来，不让它完全散掉。
- **Context Options**：把长视频切成多个小时间窗去采样，并靠 overlap 粘合。
- **KSampler denoise**：决定你是“保留原视频为主”，还是“强风格化重绘为主”。
- **Video Combine**：只是封装回视频，不负责救一致性。

---

### 五、什么叫“一强一弱”的约束策略
这是今天最值钱的操作认知。

#### 强约束：锁结构 / 动作 / 透视
常见强约束：
- **Depth**：场景空间关系、景深、前后层级
- **Canny / Lineart**：轮廓、硬边、UI、机械结构
- **OpenPose**：人物动作
- **SparseCtrl**：用稀疏关键帧或参考图约束更长段落的结构/运动走向

#### 弱约束：补纹理 / 抑制脏细节飘移
常见弱约束：
- **Tile / Tile Resample**：细节补偿、纹理回填
- 低权重 IP-Adapter：补风格而不是抢结构

#### 为什么是一强一弱，而不是三强叠满
因为 vid2vid 的核心矛盾不是“控制不够”，而是**约束互相打架**：
- 强结构控太多 → 画面僵、边缘脏、动作发抖
- 强风格控太多 → 主体形变、背景呼吸、闪烁加剧

**我的推荐顺序：先选一个主结构锚，再补一个弱纹理锚。**

---

### 六、最稳的参数起手值（先保命，再追风格）

#### 1）帧率与分段
- `force_rate`：**8 fps 起手**
- `frame_load_cap`：**16~24 帧一段**
- `skip_first_frames`：用于长视频分批续跑
- `context_length`：**16** 起手最稳
- `context_overlap`：**4** 是很实用的起手值

#### 2）denoise（vid2vid 的灵魂参数）
- **0.20 ~ 0.30**：保留原视频结构与节奏，适合“实机录屏轻风格化”
- **0.30 ~ 0.45**：风格化更明显，但仍可控，适合买量素材转绘
- **0.50 以上**：除非你就是想重做镜头，否则很容易开始闪、漂、变脸

**主人最该记的实战准则：**
> 想要“这是同一个视频”——先别超过 0.35。

#### 3）ControlNet 权重
- 主结构约束：**0.7 ~ 1.0**
- 弱纹理约束（Tile 等）：**0.2 ~ 0.5**

#### 4）放大策略
如果想输出更高清：
- **先在低分辨率跑稳一致性**
- 再做一轮保守 upscale / vid2vid refine
- 不要一上来 1080p + 高 denoise + Tile 全开，那是作死

---

### 七、SparseCtrl 到底什么时候上
不是所有视频都需要 SparseCtrl，但它在两类情况非常值钱：

#### 1. 镜头跨度更长，普通 AnimateDiff 不够稳
例如：
- 角色从远景走到近景
- 广告镜头做明显推拉
- 多秒钟连续动作，不只是轻微呼吸

#### 2. 你有关键帧想强制它别跑偏
比如：
- 开头必须保住《光与朽》的激光塔 silhouette
- 中段必须保住 Boss 镰刀方向
- 结尾必须保住 Logo / UI 信息

这时 SparseCtrl 的价值不是“提高画质”，而是：
**给长视频一个稀疏但高价值的路标系统。**

---

### 八、三套最实战模板

#### 模板 A：实机录屏 → 赛博风格买量片
用途：把 Unity 录屏快速包装成更抓眼的风格化广告。
```text
Load Video (8fps, 16帧一段)
→ Depth / Canny
→ AnimateDiff
→ 低~中 denoise (0.25~0.35)
→ Video Combine
```
要点：
- Depth 锁场景层次
- Canny 锁高对比轮廓（激光、塔、敌人边界）
- 不要追求每帧大改，只要“像同一支高级广告”就够了

#### 模板 B：角色/怪物宣传视频转绘
用途：让角色立绘演示或 Boss 镜头更统一。
```text
Load Video / 帧序列
→ OpenPose / Lineart（按素材选）
→ AnimateDiff + Context
→ IP-Adapter（低权重，可选）
→ Video Combine
```
要点：
- Pose 锁动作
- 低权重 IP-Adapter 只补风格，不抢动作骨架

#### 模板 C：长镜头 / 多秒镜头保形
用途：做多秒测试素材，避免前后段换脸换世界。
```text
Load Video
→ SparseCtrl / Advanced ControlNet
→ AnimateDiff
→ Context 16 + overlap 4
→ 分段输出后合并
```
要点：
- 重点不是一遍成神，是先把每段稳定，再拼整条

---

### 九、《光与朽》现在最该怎么落地

#### 1. 优先处理什么素材
不是先拿剧情动画练手，先拿：
- 激光塔发射核心镜头
- 敌人被切穿 / 蒸发的 1~2 秒爽点片段
- Boss 待机或进场的短镜头

因为这些素材：
- 结构清楚
- 视觉动词明确
- 更容易验证“风格化之后 CTR 会不会更高”

#### 2. 推荐起手流程
1. Unity 录一段 **2 秒左右** 核心爽点
2. `Load Video` 压到 **8fps**
3. 先只上 **Depth 或 Canny 二选一**
4. AnimateDiff 跑 **16 帧窗口 + overlap 4**
5. `denoise` 先锁 **0.28 左右**
6. 只输出 2~3 个风格版本做陌生人测试

#### 3. 验收标准
- 激光塔、敌人轮廓、UI 读不读得清
- 同一镜头前后帧有没有“换游戏”感
- 首 1 秒会不会比原录屏更抓眼
- 有无闪烁、背景呼吸、角色边缘抖动

如果这四条不过，别急着做长片，先回去减 denoise、减强约束数量、缩短段长。

---

### 十、最常见的翻车点
1. **把 vid2vid 当成逐帧美图器。**
   每帧单独变漂亮，不等于整条视频好看。
2. **一上来就高分辨率。**
   稳定性还没打通前，分辨率越高死得越快。
3. **denoise 开太大。**
   风格是更强了，但角色、敌人、特效已经不是原镜头了。
4. **同时叠太多强 ControlNet。**
   不是更稳，是更冲突。
5. **长视频不分段。**
   不懂 context 和 overlap，就别怪它抽风。
6. **把 Tile 当救命神药。**
   Tile 常常只会给你更多“每帧不同的小聪明细节”。

---

### 十一、今天最重要的操作纪律
1. **vid2vid 先追一致性，再追风格冲击。**
2. **8fps + 16 帧窗口 + overlap 4，是最值得先试的稳态起手式。**
3. **一强一弱两路约束，通常比多路强锁更稳。**
4. **denoise 先保守到 0.25~0.35，把“像同一条视频”放在第一位。**
5. **《光与朽》先从 2 秒核心爽点片段试，不要一上来做 10 秒长广告。**

---

### 十二、下一步衔接
如果沿路线继续推进，下一顺位就是：
1. **音频驱动节点**（让动画跟节奏点动）
2. **首尾帧闭环 / loop 技巧**
3. **把 vid2vid 管线跟批量命名、批量测试素材体系接上**

---

### 参考来源
- ComfyUI 既有学习记录中的官方 Video Examples 摘要（SVD / Video 参数说明）
- GitHub：`Kosinkadink/ComfyUI-VideoHelperSuite` README（`force_rate`、`frame_load_cap`、`skip_first_frames`、`Video Combine frame_rate`）
- GitHub 搜索结果：`Kosinkadink/ComfyUI-AnimateDiff-Evolved`（说明其常与 `ComfyUI-Advanced-ControlNet` 配合，并包含 SparseCtrl 支持）
- Reddit：`[GUIDE] ComfyUI AnimateDiff Guide/Workflows Including Prompt Scheduling`（16 帧与 context overlap 经验）
- Reddit：`Help, I've followed a bunch of animatediff tutorials... no temporal consistency whatsoever`（常见失败症状）
- Reddit：`Best practice for temporally coherent video frames after applying optical flow and upscale?`（逐帧细节增强破坏一致性的案例）
- YouTube：`ANIMATEDIFF COMFYUI TUTORIAL - USING CONTROLNETS AND MORE.`
- YouTube：`AnimateDiff (vid2vid) ComfyUI Workflow Tutorial`
- YouTube：`ComfyUI - Create Consistent & Smooth Animation (Vid2Vid)`
- YouTube：`How To Use AnimateDiff for Video To Video in ComfyUI`
- Bilibili：`细节满满！Comfyui-animatediff-工作流构建 | 从零开始的连连看！`
- Bilibili：`ComfyUI系列14：animatediff视频转绘01，从0开始搭建animatediff视频转绘工作流`
- Bilibili：`AI教程，ComfyUI视频转换成任何风格工作流分享（3D动画、动漫、主机游戏）`

# 📚 ComfyUI 资深阶段学习笔记 (2026-05-02)

## 🎯 今日攻克节点：音频驱动节点（Audio Reactive Nodes）

### 一、为什么这个节点值钱
前一课我们已经解决了视频转视频的时序一致性，下一步不是继续堆风格，而是让“节奏”直接控制画面。音频驱动节点的本质，不是给视频随便加音乐，而是把音频拆成**逐帧可读取的数据流**，再把这些数值喂给 AnimateDiff、IP-Adapter、Prompt Schedule、转场、运动强度、颜色/特效权重。这样视频不是“配乐后贴上去”，而是“被音乐推着长出来”。

对《光与朽》这种激光、能量脉冲、Boss 蓄力、清屏爆发感很强的项目，这玩意儿非常适合做：
- 买量视频里的卡点激光脉冲
- Boss 出场时的节奏驱动镜头/风格切换
- 实机录屏转赛博风宣传片
- 下个项目的情绪化概念片与短视频测试

---

## 二、核心工作流总览

### 方案 A：AudioScheduler——把音频转成数值，再驱动任意参数
这个分支最适合做“通用型音频控制”。

```text
LoadAudio / VHS_LoadAudioUpload
→ AudioToAudioData
→ AudioToFFTs
→ BatchAmplitudeSchedule
→ ClipAmplitude
→ TransientAmplitudeBasic
→ NormalizeAmplitude
→ GateNormalizedAmplitude
→ NormalizedAmplitudeToNumber / NormalizedAmplitudeDrivenString
→ 驱动 KSampler / AnimateDiff / Prompt / 权重 / 转场节点
```

#### 节点职责
1. `AudioToFFTs`
   - 把音频切成逐帧 FFT 频谱数据。
   - 关键是让“每一帧视频”对应到“这一刻音频的频率能量”。
2. `BatchAmplitudeSchedule`
   - 从某个频段提取振幅。
   - `operation` 可选 `avg/max/sum`：
     - `avg`：最稳，适合连续运动强度
     - `max`：峰值更猛，适合爆点触发
     - `sum`：整体更躁，容易过冲
3. `TransientAmplitudeBasic`
   - 给音频包络做 Attack / Hold / Release。
   - 本质是解决“画面抽搐”问题。
4. `NormalizeAmplitude`
   - 把振幅压到 0~1，方便后续统一驱动。
5. `GateNormalizedAmplitude`
   - 设门槛，过滤小噪音。
6. `NormalizedAmplitudeToNumber`
   - 输出 float/int，直接喂给权重、运动强度、放大倍数等数值输入。
7. `NormalizedAmplitudeDrivenString`
   - 把峰值变成 prompt 切换器，比如鼓点来时从“calm energy”切成“laser burst”。

---

### 方案 B：Yvann Nodes——成套音频反应式视频工作流
这个分支更偏“开箱即用”的成品化视频管线，适合快速做买量素材。

```text
Load Audio Separation Model
→ Audio Analysis
→ Audio Peaks Detection
→ Audio IPAdapter Transitions / Audio Prompt Schedule
→ AnimateDiff / IPAdapter / SparseCtrl / ControlNet
→ VHS_VideoCombine
```

#### 关键理解
1. `Load Audio Separation Model`
   - 用 Hybrid Demucs / OpenUnmix 先做音轨分离。
   - 这样可以只盯鼓点、只盯人声、只盯 bass，而不是整首歌混在一起。
2. `Audio Analysis`
   - 输入音频、fps、batch_size。
   - 输出 `audio_weights` 和可视化图。
   - `analysis_mode` 可选 `Drums Only / Vocals / Full Audio`。
3. `Audio Peaks Detection`
   - 从连续权重里抓“峰值帧”。
   - 输出峰值权重、峰值索引、峰值数量。
4. `Audio IPAdapter Transitions`
   - 根据峰值在多张图之间切换/混合权重。
   - 非常适合“鼓点切角色/切场景/切姿态”。
5. `Audio Prompt Schedule`
   - 用峰值索引触发 prompt 段落切换。
   - 适合让音乐副歌时直接进入更高能量的视觉词汇。

---

## 三、节点连线逻辑：主人真正该记住的主干

### 1）音频驱动“运动强度”
```text
音频 → FFT → 振幅 → 归一化 → FLOAT
→ AnimateDiff 的 motion strength / Multival
```

作用：节奏强时动画更活，弱时更稳。

### 2）音频驱动“图像转场”
```text
音频峰值 → Peaks Detection → 权重曲线
→ IP-Adapter Batch / Image Transition 权重
```

作用：鼓点一到，A 图向 B 图过渡；适合概念图、场景图、怪物图轮切。

### 3）音频驱动“提示词变化”
```text
音频振幅 / 峰值 → NormalizedAmplitudeDrivenString
→ Prompt Schedule / CLIP Text Encode
```

作用：安静段是“蓄能”，高潮段切“爆发、粒子、过曝、赛博激光”。

### 4）音频驱动“实机转绘片”
```text
实机录屏 → VideoHelperSuite
音频 → 分析/峰值
→ AnimateDiff + SparseCtrl + Depth/Lineart + IPAdapter
→ 视频输出
```

作用：保留《光与朽》原始可读性，再用音频把风格变化和镜头气氛推起来。

---

## 四、关键参数怎么设，别瞎调

### A. AudioScheduler 参数建议

#### `frames_per_second`
- 必须尽量和最终视频 fps 对齐。
- 首次实验建议：`8` 或 `12`
- 如果最终视频是 8fps 而音频分析按 24fps 跑，节奏会不稳、浪费算力。

#### `lower_band_range / upper_band_range`
- 用来选频段。
- 实战理解：
  - **低频**：kick / bass，适合大幅脉冲、镜头震动、整体能量起伏
  - **中频**：旋律/主体，适合角色细微摆动、材质呼吸
  - **高频**：镲片/尖锐打击，适合闪白、边缘辉光、粒子爆点
- 别一上来全频段。全频段通常最乱。

#### `ClipAmplitude`
- 作用：防止某些爆点把数值拉爆。
- 建议先做裁剪再归一化，不然一个峰值会把整段视频的动态范围毁掉。

#### `TransientAmplitudeBasic`
- 推荐起手：
  - `attack = 0~2`
  - `hold = 2~6`
  - `release = 4~8`
- 规律：
  - attack 小：响应更快
  - hold 大：峰值能撑几帧，不会一闪而过
  - release 大：回落更顺，不抽搐

#### `gate_normalized`
- 建议从 `0.15~0.35` 试。
- 太低：环境噪音都会触发
- 太高：只剩少数大鼓点，画面太死

---

### B. Yvann Nodes 参数建议

#### `Audio Analysis`
- `analysis_mode`
  - `Drums Only`：最适合买量片，节奏抓得最干净
  - `Vocals`：适合人声口播或唱词驱动
  - `Full Audio`：适合氛围片，但更容易乱
- `threshold`
  - 建议从 `0.4~0.6` 起
  - 越高越克制，只保留明显节拍
- `multiply`
  - 建议从 `1.0` 起步
  - 需要更强视觉波动再拉到 `1.2~1.8`

#### `Audio Peaks Detection`
- `peaks_threshold`
  - 建议 `0.35~0.55`
- `min_peaks_distance`
  - 建议 `4~8` 帧
  - 太小会连着触发，像抽风；太大又会错过节奏点

#### `Audio IPAdapter Transitions`
- `blend_mode = linear` 先用最稳的
- `transitions_length = 4~6` 帧
- `min_IPA_weight = 0`
- 适合先做“平滑切图”，别一开始就做大跳变

---

## 五、官方/仓库工作流里值得抄的结构

### 1. 视频输入输出骨架
Yvann 的 Video-to-Video 示例里，核心骨架是：

```text
VHS_LoadVideo → VideoInfo(fps/frame_count/width)
VHS_LoadAudioUpload → Audio Analysis
→ Audio Peaks Detection
→ 图像/权重处理
→ AnimateDiff
→ SparseCtrl / Depth / AnyLine ControlNet
→ KSampler 首轮
→ KSampler 二轮
→ Upscale
→ VHS_VideoCombine
```

这个结构有三个高价值点：
1. **fps 直接喂给音频分析**，保证音画对齐。
2. **峰值数量反向决定图片重复/转场数量**，避免素材数量与节拍数量错位。
3. **两轮 KSampler + 最后统一 Upscale**，先抢节奏感，再补成片细节。

### 2. AnimateDiff 的稳妥搭法
在示例工作流里，AnimateDiff 不是裸跑，而是：
- `Load AnimateDiff Model`
- `Load AnimateDiff LoRA`
- `MultivalDynamic` 接收动态 float 作为 motion strength
- `Looped Uniform Context Options` 设上下文窗口

里面最值得抄的参数思想：
- `context length = 16`
- `context overlap = 4`
- motion strength 先小后大，不要一上来狂摇

这和我们昨天学到的视频时序一致性是完全一条线的：**音频驱动不该破坏连续性，而该在连续性内部制造节奏起伏。**

---

## 六、给《光与朽》的落地工作流

### 工作流 1：Boss 出场卡点宣传片
目标：做 10~15 秒高能短视频素材。

```text
挑一段鼓点明确的音乐
→ 用 Audio Analysis 只抓 Drums Only
→ Peaks Detection 找爆点
→ 实机录屏或静态概念图输入
→ 爆点时提高 AnimateDiff motion strength
→ 同时让 IP-Adapter 在“正常战斗图 / 过载激光图”之间转场
→ 高频段额外驱动 bloom/闪白/粒子权重
```

建议：
- 平时 motion strength 低
- 爆点时瞬时拉高 20%~40%
- 副歌第一拍触发一次大画面切换

### 工作流 2：激光炮塔脉冲感测试
目标：把“激光塔”做成更适合短视频传播的视觉节奏。

```text
低频振幅 → 激光粗细 / 发光强度
中频振幅 → 镜头轻微呼吸 / 画面缩放
高频峰值 → 命中闪烁 / 屏幕白闪 / Hit 粒子
```

这套东西哪怕先不进 ComfyUI，也能反向指导 Unity 里的 VFX 设计：说明你的宣传片爽点，最好和真实游戏反馈节奏一致。

### 工作流 3：概念图轮切买量片
目标：快速验证题材包装。

```text
4~6 张同题材概念图
→ Audio IPAdapter Transitions
→ 每个鼓点切一张图
→ Prompt Schedule 在副歌切换“腐朽 / 圣光 / 过载 / 终焉”等词组
→ 输出 8fps~12fps 低成本测试片
```

这比纯静态海报强太多，因为它能在极低成本下测“节奏 + 题材 + 光影”是否吸睛。

---

## 七、最容易翻车的点

1. **音频驱动不是越敏感越好**
   - 过度反应会让画面像癫痫，不是节奏，是噪音。

2. **先选频段，再谈视觉逻辑**
   - 低频管大动作，高频管小爆点，这是最稳的分工。

3. **音频数据必须先平滑再驱动模型**
   - 不做 attack/hold/release，画面会抽搐得像坏掉。

4. **fps 必须统一**
   - 音频分析 fps、AnimateDiff 上下文节奏、最终输出 fps 不统一，必炸。

5. **音频驱动最好只控制 1~2 个主变量**
   - 先只控 motion strength + 转场权重。
   - 一次同时控 prompt、ControlNet、IP-Adapter、镜头、颜色，八成会乱成屎。

---

## 八、我的结论
音频驱动节点不是花活，它是**把视频素材从“会动”升级到“会卡点、会带情绪、会打节奏”**的关键层。对主人现在最有价值的，不是拿它做艺术短片，而是把它接进《光与朽》的买量素材实验：

**先做 8~12fps、10 秒、鼓点驱动的 Boss/激光/清屏三类短片模板。**

这玩意儿如果跑通，后面我们做 AI 视频反向立项，筛题材、测首秒钩子、测高能点，效率会直接上一个台阶。

---

## 参考来源
- ComfyUI 官方文档：`https://docs.comfy.org/`
- ComfyUI-AudioScheduler 仓库：`https://github.com/a1lazydog/ComfyUI-AudioScheduler`
- ComfyUI_Yvann-Nodes 仓库：`https://github.com/yvann-ba/ComfyUI_Yvann-Nodes`
- Yvann 示例工作流：`AudioReactive_VideoToVideo_Yvann.json`
- ComfyUI-VideoHelperSuite 仓库：`https://github.com/Kosinkadink/ComfyUI-VideoHelperSuite`
- ComfyUI-AnimateDiff-Evolved 仓库：`https://github.com/Kosinkadink/ComfyUI-AnimateDiff-Evolved`
- Reddit 讨论：`https://www.reddit.com/r/comfyui/comments/17dc0e2/custom_nodes_to_let_audio_drive_your_animatediff/`
- Yvann YouTube 教程：`https://www.youtube.com/watch?v=BiQHWKP3q0c`、`https://www.youtube.com/watch?v=O2s6NseXlMc`
- Bilibili 参考：`https://www.bilibili.com/video/BV1N8411C7XE/`、`https://www.bilibili.com/video/BV1LMk9YxEHg/`
