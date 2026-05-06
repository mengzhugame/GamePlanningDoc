# API成本优化方案报告

**问题**: 当前使用Kimi API，每天约30元（约900元/月）  
**目标**: 寻找不降智的免费/低价替代方案

---

## 🏆 推荐方案（按优先级排序）

### 方案1: 硅基流动 SiliconFlow（强烈推荐）
**官网**: https://siliconflow.cn

| 优势 | 说明 |
|------|------|
| ✅ 新用户免费额度 | 注册送 **2000万 Token** |
| ✅ 多款永久免费模型 | DeepSeek-R1、Qwen2.5、GLM-4等 |
| ✅ 价格便宜 | 付费模型也比Kimi便宜50-70% |
| ✅ 国内访问快 | 无需翻墙 |
| ✅ 支持OpenAI格式 | 迁移成本低 |

**免费模型推荐**:
- `deepseek-ai/DeepSeek-R1-Distill-Qwen-7B` - **完全免费**
- `Qwen/Qwen2.5-7B-Instruct` - **完全免费**
- `THUDM/GLM-4-9B-0414` - **完全免费**
- `Qwen/Qwen2.5-Coder-7B-Instruct` - **完全免费**（代码能力强）

**价格对比**:
| 服务 | 输入价格 | 输出价格 |
|------|----------|----------|
| Kimi K2.5 | ~12元/M tokens | ~12元/M tokens |
| SiliconFlow免费版 | **0元** | **0元** |
| SiliconFlow付费版 | 0.35-1.89元/M tokens | 0.35-2.8元/M tokens |

**预计节省**: 使用免费模型可降至 **0元/天**

---

### 方案2: OpenRouter（国际平台）
**官网**: https://openrouter.ai

| 优势 | 说明 |
|------|------|
| ✅ 免费模型多 | 300+模型，部分免费 |
| ✅ 统一API接口 | 一次接入，多模型切换 |
| ✅ 支持主流模型 | Claude、GPT、Llama等 |

**缺点**:
- 需要海外支付
- 国内访问可能不稳定
- 免费模型有速率限制

**适用场景**: 需要Claude/GPT等国外模型时

---

### 方案3: Google AI Studio（你试过）
**官网**: https://aistudio.google.com

| 优势 | 说明 |
|------|------|
| ✅ Gemini 2.0 Flash免费 | 每天1500次请求 |
| ✅ 速度快 | Google基础设施 |

**缺点**:
- 你提到已限额
- 国内访问需翻墙
- 代码能力不如Claude

---

### 方案4: Groq
**官网**: https://groq.com

| 优势 | 说明 |
|------|------|
| ✅ 速度极快 | 世界最快推理速度 |
| ✅ Llama/Mixtral免费层 |  generous免费额度 |

**缺点**:
- 主要支持开源模型
- 国内访问不稳定

---

## 💡 立即可执行的方案

### 第一步: 注册 SiliconFlow（5分钟）
```
1. 访问 https://siliconflow.cn
2. 注册账号（支持手机号）
3. 获取 API Key
4. 获得 2000万 Token 免费额度
```

### 第二步: 配置 OpenClaw 使用 SiliconFlow

修改配置文件 `~/.openclaw/openclaw.json`:

```json
{
  "models": {
    "default": "siliconflow/deepseek-ai/DeepSeek-R1-Distill-Qwen-7B",
    "aliases": {
      "default": "siliconflow/deepseek-ai/DeepSeek-R1-Distill-Qwen-7B",
      "coding": "siliconflow/Qwen/Qwen2.5-Coder-7B-Instruct"
    }
  },
  "providers": {
    "siliconflow": {
      "apiKey": "YOUR_SILICONFLOW_API_KEY",
      "baseUrl": "https://api.siliconflow.cn/v1"
    }
  }
}
```

### 第三步: 测试运行
使用免费模型测试一天，观察效果是否满足需求。

---

## 📊 成本对比预估

| 方案 | 日成本 | 月成本 | 年成本 | 效果 |
|------|--------|--------|--------|------|
| 当前Kimi | 30元 | 900元 | 10,800元 | ⭐⭐⭐⭐⭐ |
| SiliconFlow免费 | **0元** | **0元** | **0元** | ⭐⭐⭐⭐ |
| SiliconFlow付费 | 5-10元 | 150-300元 | 1800-3600元 | ⭐⭐⭐⭐⭐ |
| OpenRouter免费 | 0元 | 0元 | 0元 | ⭐⭐⭐ |

**建议**: 先试用 SiliconFlow 免费模型，如果质量不满意再考虑付费档位。

---

## ⚠️ 注意事项

1. **免费模型质量**: 
   - DeepSeek-R1-Distill-7B 推理能力不错
   - Qwen2.5-7B 日常任务够用
   - 复杂代码任务可能不如Kimi K2.5

2. **免费额度用完后的策略**:
   - 2000万Token约能用1-2个月
   - 用完后可开新账号或转付费（仍比Kimi便宜）

3. **多Provider策略**:
   - 简单任务用SiliconFlow免费
   - 复杂代码任务用Kimi（少量）
   - 最大限度节省成本

---

## 🎯 推荐配置

**阶段1（立即执行）**:
- 注册 SiliconFlow
- 配置使用 `DeepSeek-R1-Distill-Qwen-7B` 免费模型
- 日成本: **0元**

**阶段2（免费额度用完）**:
- 切换到 SiliconFlow 付费（0.35元/M tokens）
- 或开新账号继续用免费
- 日成本: **5-10元**

**阶段3（稳定期）**:
- 根据任务复杂度自动切换模型
- 简单任务用免费，复杂用付费
- 日成本控制在 **10元以内**

---

## 🔧 配置示例

如果需要我帮你配置，请提供：
1. SiliconFlow API Key
2. 当前 OpenClaw 配置文件路径

我可以立即帮你修改配置并测试。

---

*报告生成时间: 2026-02-09*  
*数据来源: SiliconFlow官网、OpenRouter文档*
