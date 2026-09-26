# 论文智作大师 (PaperMaster) - Windows 桌面版

> AI驱动的毕业论文自动生成工具，支持5步论文生成流程

## 功能特性

- **Step 1: 选题与参数设置** — 论文题目、层次、学科、专业、字数要求
- **Step 2: 论文大纲** — 自动生成三级目录大纲，支持重新生成
- **Step 3: 开题报告** — 基于大纲生成完整开题报告
- **Step 4: 论文正文** — 生成完整论文正文，支持降低AI检测率
- **Step 5: 答辩演讲稿 & PPT大纲** — 生成答辩演讲和PPT内容

## 技术栈

- **开发语言**: C# (.NET 8.0)
- **UI框架**: WPF (Windows Presentation Foundation)
- **JSON处理**: Newtonsoft.Json
- **文档导出**: OpenXML SDK (DOCX导出)

## 快速开始

### 构建

```bash
cd PaperMasterWin
dotnet build
dotnet run
```

### 配置

首次运行需要配置API信息：

1. 点击左侧 "⚙ 配置"
2. 填写 API 地址 (如 `https://aiapi.whsunshine.link/v1/chat/completions`)
3. 填写 API Key
4. 填写默认模型 (如 `qwen3.6-35b`)
5. 保存配置

## 发布日志

### v1.0.0 (2026-09-26)

- ✅ 首次发布 Windows 桌面版本
- ✅ 支持5步论文生成流程
- ✅ 对接 OpenAI 兼容 API
- ✅ 支持 DOCX 文档导出
- ✅ 历史记录管理
- ✅ 论文大纲/开题报告/正文/答辩稿/PPT大纲
- ✅ 降低AI检测率功能

## 相关链接

- **AI API**: aiapi.whsunshine.link
- **微信公众号**: ai_dxlw (备注体验加微信)
- **GitHub**: https://github.com/whsunshine8/papermaster-release/
- **闲鱼购买**: https://m.tb.cn/h.8E3c6Ov?tk=va4tTmOlu8K

## 依赖

在 `PaperMasterWin.csproj` 中引用：
- Newtonsoft.Json
- DocumentFormat.OpenXml

## 许可证

© 2026 whsunshine. All rights reserved.
