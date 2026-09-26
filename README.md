# 📄 论文智作大师 (PaperMaster)

[中文](#-中文介绍) | [English](#-english-description)

---

## 🇨🇳 中文介绍

> 全自动 AI 学术论文 / 研究报告智能创作助手  
> 支持深度大纲推理、多章节长文本连贯生成、后台保活推演与标准 Word (.docx) 排版导出。

[![Release](https://img.shields.io/github/v/release/whsunshine8/papermaster-release?color=blue&label=Latest%20Version)](https://github.com/whsunshine8/papermaster-release/releases/latest)
[![Platform](https://img.shields.io/badge/Platform-Android%208.0%2B-brightgreen)](https://github.com/whsunshine8/papermaster-release)
[![License](https://img.shields.io/badge/License-MIT-orange)](#)

### 📥 软件下载

| 版本 | 文件类型 | 适用架构 | 下载方式 |
| :--- | :--- | :--- | :--- |
| **v1.1.5 (最新正式版)** | Android APK (`.apk`) | 兼容全部主流 Android 设备 (armeabi-v7a / arm64-v8a) | [⬇️ GitHub 极速下载直链](https://github.com/whsunshine8/papermaster-release/releases/download/v1.1.5/PaperMaster_v1.1.5.apk) |

> 💡 **历史版本**：你可以随时访问 [Releases 官方发布页](https://github.com/whsunshine8/papermaster-release/releases) 查看所有历史版本与详细发布说明。

### 💡 核心功能特性

- 🧠 **学术大纲智能推理**：仅需输入论文题目、专业领域与核心要求，即可一键生成逻辑严密、层级分明（一至三级标题）的标准学术大纲。
- 📖 **长篇章节流式生成**：独创的多章节连贯生成算法，有效克服传统大语言模型的单次 Token 长度截断瓶颈，保障数十页学术长文的行文连贯性。
- 🎯 **字数精准约束机制**：内置严格的三层字数控制与句末收口算法，确保正文字数稳定控制在目标设定值的 1.0 ~ 1.2 倍之间，杜绝腰斩或无序膨胀。
- 📑 **标准 Docx 排版导出**：一键导出符合官方规范的 Word (`.docx`) 示例文档，统一标准学术字号排版（正文四号字、无冗余样式），可直接分享或导入电脑编辑。
- 🔋 **后台任务持续保活**：针对移动端长耗时任务，集成 WakeLock 与前台任务保活机制，锁屏、切换应用窗口均不会中断论文推演生成。
- 🔄 **自动静默增量更新**：内置高可用版本检测服务，启动时自动比对最新版本，提供一键免跳转直接升级体验。

### 📱 系统环境要求

- **操作系统**：Android 8.0 (API Level 26) 及更高版本。
- **存储权限**：支持直接导出 `.docx` 文档至手机系统「下载」或「文档」目录。
- **网络连接**：支持通过主流大语言模型 API（内置通道或局域网直连）进行流式推算。

### 🚀 快速上手使用

1. **下载安装**：通过上方链接下载 `PaperMaster_v1.1.5.apk` 并安装到手机（若系统提示请允许“安装未知来源应用”）。
2. **设定题目与需求**：打开应用，输入论文题目、字数预期（如 8000 ~ 15000 字）及研究要点。
3. **确认/调整大纲**：点击「生成大纲」，系统将自动推理多级目录结构，可在大纲编辑区手动调整。
4. **一键生成全文**：点击「开始生成」，系统将逐章有序推进，支持随时暂停或继续。
5. **导出并分享**：生成完成后，点击「导出为 Word」，即可直接调用系统分享或在 WPS/Office 中打开。

### 📝 最近更新日志

#### v1.1.5 (2026-09-26)
- **检测通道升级**：接入高可用 GitHub 极速更新检测接口，告别网络拦截与卡顿。
- **签名兼容重构**：全量补齐 Android 系统级 v2/v3 签名校验，彻底解决部分设备提示“无效安装包”的问题。
- **长文本保活优化**：增强生成进程后台 CPU 锁与内存保活能力，长篇大作生成更稳定。

---

## 🌐 English Description

> An automated AI-powered academic paper and research report generation assistant.  
> Featuring deep outline reasoning, coherent long-form multi-chapter generation, background execution persistence, and standardized Word (.docx) export.

[![Release](https://img.shields.io/github/v/release/whsunshine8/papermaster-release?color=blue&label=Latest%20Version)](https://github.com/whsunshine8/papermaster-release/releases/latest)
[![Platform](https://img.shields.io/badge/Platform-Android%208.0%2B-brightgreen)](https://github.com/whsunshine8/papermaster-release)
[![License](https://img.shields.io/badge/License-MIT-orange)](#)

### 📥 Download

| Version | File Type | Architecture | Download Link |
| :--- | :--- | :--- | :--- |
| **v1.1.5 (Latest)** | Android APK (`.apk`) | Universal Android (armeabi-v7a / arm64-v8a) | [⬇️ Direct Download (.apk)](https://github.com/whsunshine8/papermaster-release/releases/download/v1.1.5/PaperMaster_v1.1.5.apk) |

> 💡 **Release Archive**: Visit the [Releases Page](https://github.com/whsunshine8/papermaster-release/releases) for historical builds and release notes.

### 💡 Key Features

- 🧠 **Intelligent Outline Reasoning**: Generates well-structured, multi-level academic outlines (Chapters, Sections, Subsections) based on research topics and requirements.
- 📖 **Long-Form Multi-Chapter Generation**: Overcomes LLM single-request context and token output limits through seamless multi-stage chapter synthesis.
- 🎯 **Precise Length Control**: Multi-layer constraint system with natural sentence boundary detection keeps output within 1.0x - 1.2x of the target word count.
- 📑 **Standard Word (.docx) Export**: Exports formatted `.docx` files adhering to formal academic typography standards, ready for desktop editing.
- 🔋 **Background Task Persistence**: Integrated WakeLock and foreground task scheduling ensures uninterrupted generation even when the screen is locked or apps are switched.
- 🔄 **Automatic Update Check**: High-availability version checking on startup with one-click direct update downloading.

### 📱 Requirements

- **OS**: Android 8.0 (API Level 26) or higher.
- **Permissions**: Storage/file management for exporting documents.
- **Network**: Internet connectivity for LLM API streaming inference.

### 🚀 Getting Started

1. **Install**: Download `PaperMaster_v1.1.5.apk` from the link above and install on your Android device.
2. **Setup**: Enter your paper title, target word count (e.g. 8,000–15,000 words), and key research points.
3. **Outline**: Generate and customize your chapter outline structure.
4. **Generate**: Tap "Start Generation" to produce chapters sequentially with live progress tracking.
5. **Export**: Export as `.docx` to open in Microsoft Word or WPS Office.

---

## 📬 Contact & Support

- **Official Website**: [https://aidxlw.my-place.us/software/](https://aidxlw.my-place.us/software/)
- **Feedback & Issues**: [GitHub Issues](https://github.com/whsunshine8/papermaster-release/issues)
