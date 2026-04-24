# Unity 核心模块底层知识（辅助排查Bug）

本篇记录 Unity 游戏开发中涉及核心模块的底层运作原理，帮助分析及解决各类Bug。

## 1. 碰撞体与可视范围的同步 (LineRenderer与BoxCollider2D)

在开发如激光、射线等效果时，经常使用 `LineRenderer` 作为视觉表现，而物理检测则需要 `BoxCollider2D`。它们的同步是常见痛点：

*   **痛点**：LineRenderer 是基于顶点（Positions）绘制的，而 BoxCollider2D 是基于 Size 和 Offset 的。
*   **同步方法**：
    *   **长度与角度**：计算 LineRenderer 首尾两个顶点的距离 `distance = Vector2.Distance(startPos, endPos)`，将 BoxCollider2D 的 `size.x` 或 `size.y` 设为该距离。
    *   **中心点 (Offset)**：Collider 的中心应为两顶点的中点 `offset = (startPos + endPos) / 2`。
    *   **旋转**：计算两顶点连线的角度 `angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg`，并赋值给挂载 Collider 的 GameObject 的 `transform.rotation`。
*   **最佳实践**：不要把 BoxCollider2D 直接挂在 LineRenderer 所在的物体上，最好作为它的子物体，这样可以直接修改子物体的 Transform，避免影响 LineRenderer 的世界坐标点位。

## 2. 物理系统深入 (Rigidbody2D 与 AddForce)

打击感与击退效果（Knockback）高度依赖 Rigidbody2D 的力学计算。

*   **质量 (Mass) 的影响**：`Force = Mass * Acceleration`。如果质量为 1，施加 10 的力；如果质量为 10，施加 10 的力几乎没有明显位移。因此在处理击退时，可以考虑 `force / rigidbody2D.mass` 或使用 `ForceMode2D.Impulse`（瞬间施加冲量，忽略时间但仍受质量影响）。
*   **AddForce 参数**：
    *   `ForceMode2D.Force`：持续力，受质量影响，通常在 `FixedUpdate` 中调用，适合做风场、引力。
    *   `ForceMode2D.Impulse`：瞬间冲量，受质量影响，适合做爆炸击退、跳跃。
*   **摩擦力 (Linear Drag & Friction)**：击退距离不仅与初始力有关，还受材质的 `Friction` 和 Rigidbody 的 `Linear Drag` 影响。Linear Drag 会随时间让物体减速，是控制击退手感（Hitstop/停顿）的重要参数。

## 3. 高阶对象池管理 (VFX与嵌套对象池)

对象池 (Object Pool) 是解决 Instantiate/Destroy 性能瓶颈的核心机制。

*   **VFX 的生命周期**：特效播放完毕后需要回收到池中，而不是直接 Destroy。常见做法：监听 `ParticleSystem.IsAlive()`，或者使用协程/UniTask 等待 `duration` 后自动 `ReturnToPool`。
*   **嵌套对象池问题**：例如敌人死亡（敌人回池）时，播放死亡特效（从特效池拿）。如果敌人物体被 SetActive(false)，其子物体（可能挂载了某些未回收的子特效或血条）也会被隐藏，导致下次从池中取出时状态错误。
*   **解决方案**：
    *   在 `OnDisable` 或回池前，将自身的子特效/附属物体解绑（`transform.SetParent(null)`）并归还到它们各自的池中。
    *   池化物体必须有严格的 `Init()` 和 `Reset()` 接口，每次从池中取出必须全量覆盖旧数据。