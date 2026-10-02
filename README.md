# 📄 论文智作大师 (PaperMaster)

[中文](#-中文介绍) | [English](#-english-description)

---

## 🇨🇳 中文介绍

> 全自动 AI 学术论文 / 研究报告智能创作助手  
> 支持 **Windows 原生桌面端** 与 **Android 移动端** 双平台，统一采用 `v1.1.9` 版本规范；提供学术大纲推理、分章节万字长文连贯生成、开题报告/文献综述/答辩自述稿/PPT全流程管线、四大系统（维普/格子达/PaperPass/知网）AIGC 深度防红重构及标准 Word (.docx) 排版导出。

[![Release](https://img.shields.io/github/v/release/whsunshine8/papermaster-release?color=blue&label=Latest%20Version)](https://github.com/whsunshine8/papermaster-release/releases/latest)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Android-brightgreen)](https://github.com/whsunshine8/papermaster-release)
[![License](https://img.shields.io/badge/License-MIT-orange)](#)

### 📥 软件下载 (v1.2.0 双端统一发布)

| 平台 / 版本 | 文件类型 | 架构 / 系统要求 | 官方下载直链 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| **Windows v1.2.0 (桌面版)** | 绿色版压缩包 (`.zip`) | Windows 10 / 11 64位 (x64) | [⬇️ 下载 Windows 绿色版](https://github.com/whsunshine8/papermaster-release/releases/download/v1.2.0/PaperMaster-Windows-v1.2.0.zip) | 单文件独立免安装，内含 `PaperMasterWin.exe` |
| **Android v1.2.0 (移动版)** | Android 安装包 (`.apk`) | Android 8.0 及以上 (全架构通用) | [⬇️ 下载 Android 安装包](https://github.com/whsunshine8/papermaster-release/releases/download/v1.2.0/PaperMaster_v1.2.0.apk) | 支持答辩PPTX生成、精美模板选择与深色模式自适应 |

> 🌐 **在线体验站**：你可以访问 [https://papermaster.whsunshine.link/](https://papermaster.whsunshine.link/) 免费在线体验学术大纲规划。  
> 💡 **版本归档**：可随时访问 [Releases 官方发布页](https://github.com/whsunshine8/papermaster-release/releases) 查看详细更新日志。

---

### 🛒 官方交流与卡密购买

- 🔗 **AI API 服务网关**：[aiapi.whsunshine.link](https://aiapi.whsunshine.link)
- 🐟 **官方闲鱼直营店 (自动发卡/额度充值)**：[点击前往闲鱼选购](https://m.tb.cn/h.8E3c6Ov?tk=va4tTmOlu8K)
- 💬 **客服微信**：`ai_dxlw` (备注体验加微信)

---

### 🛡️ 核心特性：四大主流系统 AIGC 深度防红算法 (双端通用)

针对高校与期刊普遍采用的 **维普 (VP)、格子达 (Gocheck)、PaperPass、知网 (CNKI)** 四大检测系统底层特征，双端全面部署统一的防红生成与重构算法：

1. **击穿维普“词汇丰富度与句式规整度”算法 (权重65%)**：
   - 彻底封杀“随着……的深入推进”、“本文旨在……”等八股开头与“首先…其次…最后…”等模式化连接词；
   - 融入中文学者高阶情态助词（“质言之”、“诚然”、“毋庸讳言”、“进而言之”、“揆诸现实”、“殊途同归”），提升专业词汇密度。
2. **击穿格子达“语言熵值与突发度 (Burstiness)”算法**：
   - 故意拉开句长方差，采用 **45-65 字复杂多重限定长复合句** 与 **8-12 字短促论断短句** 的剧烈对撞，打破大模型均匀平滑的低熵均值。
3. **彻底根治 PaperPass 75% 集中报红**：
   - 坚决杜绝“值得注意的是”、“不可否认”、“由此可见”、“总而言之”、“综上所述”等机器高频套话，开篇直接从具体学术矛盾与实证数据切入。
4. **知网学术语态客观化**：
   - 规避第一人称（禁止口语化“我们”、“本文”），全面转化为“经由……之实测”、“基于对……之溯源”、“在……视阈下”等规范学术语态。

---

### 💡 论文全流程 5 步管线 (双端通用)

1. 📋 **任务书规划**：全自动提炼研究背景、研究目标、核心技术路线与时间进度安排；
2. 📖 **开题报告撰写**：包含研究意义、国内外现状、研究方案、拟解决关键问题与预期成果；
3. 📚 **文献综述推演**：学术观点对比、研究空白分析与理论支撑；
4. 📝 **分章节长篇正文流水线**：突破 Token 截断，按大纲拆解逐章深入展开，保质保量轻松达标万字长文，文末自动编制真实 GB/T 7714 参考文献与可查检索表；
5. 🎤 **答辩自述稿与PPT**：5-8分钟自述演讲稿、专家高频追问 TOP 5 应对策略与标准 Markdown 格式答辩 PPT。
6. 📑 **标准 Word (.docx) 安全排版导出**：每个模块均配备独立导出引擎，正文四号字规范排版，无文件锁冲突或闪退问题。

---

### 📝 最近更新日志

#### v1.2.0 (2026-10-02) - 双端统一正式版
- **【AI 智能 PPTX 生成】**：深度打通答辩 PPTX 制作链路，自动将答辩自述稿作为正文内容，论文题目作为主题；
- **【学术模板在线选择】**：客户端集成科技蓝、严谨灰、学术绿、工科蓝等多种精美学术排版模板；
- **【实时动态进度感知】**：大纲设计、模版绑定、编译渲染、打包下载全流程状态实时反馈；
- **【暗黑/深色模式适配】**：移动端全面深度适配夜间模式自适应跟随系统，夜晚学术创作输入更清晰护眼；
- **【双端统一发布】**：Windows 桌面端与 Android 移动端统一升级至 **v1.2.0**，统一构建与同步发版。

#### v1.1.9 (2026-09-27) - 双端统一正式版
- **版本号统一**：Windows 桌面端与 Android 移动端版本号统一定制为 **v1.1.9**，合并为单一统一说明与联合发布页；
- **AIGC 四大平台防红重构**：全面落地维普、格子达、PaperPass、知网底层特征对抗生成逻辑；
- **全模块 Word 导出**：大纲、开题报告、正文、答辩自述稿、PPT大纲每个模块均支持独立导出标准 Word (.docx)；
- **桌面端体验升级**：独立单文件绿色运行免安装，新增本地工程历史记录管理，支持断点续写；
- **移动端持续保活**：前台服务与 WakeLock 双重保障，锁屏与切后台不中断论文长文推演。

---

## 🌐 English Description

> An automated AI-powered academic thesis and research report creation assistant.  
> Supporting both **Windows Desktop** and **Android Mobile** platforms unified at **v1.1.9**. Features deep outline reasoning, coherent long-form multi-chapter generation, proposal/review/defense pipeline, anti-AIGC detection algorithms (CQVIP, Gocheck, PaperPass, CNKI), and standard Word (.docx) export.

### 📥 Download (v1.1.9)

| Platform / Version | File Type | System Requirements | Download Link |
| :--- | :--- | :--- | :--- |
| **Windows v1.1.9 (Desktop)** | Standalone Archive (`.zip`) | Windows 10/11 64-bit | [⬇️ Download Windows (.zip)](https://github.com/whsunshine8/papermaster-release/releases/download/v1.1.9/PaperMaster-Windows-v1.1.9.zip) |
| **Android v1.1.9 (Mobile)** | Android Package (`.apk`) | Android 8.0+ | [⬇️ Download Android (.apk)](https://github.com/whsunshine8/papermaster-release/releases/download/v1.1.9/PaperMaster_v1.1.9.apk) |

---

## 📬 Contact & Support

- **Online Trial**: [https://papermaster.whsunshine.link/](https://papermaster.whsunshine.link/)
- **API Service**: [https://aiapi.whsunshine.link](https://aiapi.whsunshine.link)
- **Official Store**: [Goofish Store](https://m.tb.cn/h.8E3c6Ov?tk=va4tTmOlu8K)
- **WeChat Support**: `ai_dxlw`
