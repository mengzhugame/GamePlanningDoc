# ComfyUI 深度搭建与学习指南 (游戏美术方向)

## 1. 为什么选择 ComfyUI 进行游戏美术工业化？
相比传统的 WebUI，ComfyUI 在游戏开发工作流中具有显著优势：
- **精准控制**：通过节点（Node）连接，可以精确控制图像生成的每一个环节（如多路 ControlNet 叠加、局部重绘、放大算法切换）。
- **工作流复用与自动化**：产品经理或主美搭建好 Pipeline 后，可以直接导出为 `.json` 格式的 API 或配置文件，供其他美术人员复用，极大地降低了试错成本，保证了项目整体风格的一致性。
- **性能优化**：底层显存管理更优秀，生成大尺寸游戏资产（如 4K 宣传图、高清立绘）时不易爆显存。

## 2. 核心工作流搭建基础 (文生图/图生图)
一个标准的 ComfyUI 工作流由以下基础节点构成：
1. **Load Checkpoint（加载大模型）**：输出 MODEL, CLIP, VAE。在游戏开发中，通常选择动漫风格或特定渲染风格的 Checkpoint（放在 `models/checkpoints` 目录下）。
2. **CLIP Text Encode（提示词输入）**：分为正向（Positive）和反向（Negative）提示词节点，将自然语言转换为模型可理解的特征向量。
3. **Empty Latent Image（空潜空间图像）**：定义生成图像的初始分辨率及批次大小（如 512x768 或 1024x1024）。如果是图生图，则由 **Load Image** 配合 **VAE Encode** 替代。
4. **KSampler（采样器）**：核心去噪模块，连接 MODEL、正负提示词以及 Latent。控制迭代步数（Steps）、CFG Scale、采样算法（Euler, DPM++ 等）和降噪强度（Denoise）。
5. **VAE Decode（解码器）**：将 KSampler 输出的 Latent 像素还原为实际图像。
6. **Save Image / Preview Image（保存与预览）**：输出最终生成的游戏美术资产。

## 3. 游戏美术高阶应用节点与技巧
为了在游戏开发中实现高标准的美术产出，必须掌握以下进阶技术：

### A. 风格与特征控制 (LoRA)
- 引入 **Load LoRA** 节点，串联在 Checkpoint 和 CLIP/Model 之间。
- **应用场景**：控制特定角色设计（IP 人物）、UI 按钮质感、材质风格（如赛博朋克、废土风）。可以同时叠加多个 LoRA 控制权重。

### B. 精准姿态与线稿控制 (ControlNet)
- 配合 **Apply ControlNet** 和 **Load ControlNet Model**，输入预处理后的图像（如骨骼图、线稿、深度图）。
- **应用场景**：角色动作拆分（2D角色动画预处理）、保持立绘结构精确、3D白模渲染优化。

### C. 高清化与细节重绘 (Hires. Fix & 放大算法)
- 使用 **Upscale Latent** (或者特定算法如 Ultimate SD Upscale) 进行两段式生成。第一阶段低分辨率生成大致构图，第二阶段配合较低的降噪强度（0.3-0.5）进行放大和细节丰富。
- **应用场景**：生成游戏登录界面大图、高质量宣发海报素材。

### D. 局部修缮 (Inpainting)
- 通过 **VAE Encode (for Inpainting)** 节点，结合遮罩（Mask），只对图像特定区域重绘。
- **应用场景**：修改角色不合理的肢体细节（如AI画错的手指）、替换 UI 上的特定图标或文字底板。

## 4. 落地建议 (针对《光与朽》及未来项目)
- **模块化思维**：将复杂的生成任务拆解，例如：生成底图 -> 角色ControlNet约束 -> 局部细节Inpaint -> 高清放大，每个环节独立为一个模块（Group）。
- **资产沉淀**：建立团队专属的 ComfyUI 工作流库（如 `UI_Button_Generator.json`, `Character_Sprite_Pipeline.json`），配合团队共享的 LoRA 模型库。
- **自动化预演**：在原型阶段，开发人员可以编写脚本直接调用 ComfyUI 后台 API，快速生成占位图供测试。

---
*更新时间：2026-03-04*