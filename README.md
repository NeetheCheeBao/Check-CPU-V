# Check CPU-V

[![C#](https://img.shields.io/badge/C%23-.NET%20Framework%204.8-blue.svg)](#)
[![Platform](https://img.shields.io/badge/Platform-Windows-win.svg)](#)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

轻量检测 **CPU 虚拟化能力**（AMD-v / Intel VT-x）的 Windows 小工具

## 📸 工具截图

![img](/screenshot/demo1.png)

## ✨ 功能说明

| 功能 | 说明 |
| --- | --- |
| 处理器信息 | 显示 CPU 名称与处理器架构 |
| 系统架构 | 显示操作系统架构（X86 / X64） |
| 虚拟化支持 | 检测 AMD-v / Intel VT-x 是否支持 |
| 虚拟化启用 | 检测 BIOS 中虚拟化是否已开启 |
| 扩展特性 | 显示 DEP、SLAT、虚拟机监视器模式扩展状态 |

## 💻 系统兼容

| 系统 | 说明 |
| --- | --- |
| Windows 10 | 支持，无需额外安装运行库 |
| Windows 11 | 支持，无需额外安装运行库 |

## ⚙️ 原理说明

通过 WMI 与系统 API 读取本机 CPU 与虚拟化相关信息。

* **WMI**：`Win32_Processor`、`Win32_ComputerSystem`
* **API**：`GetSystemDEPPolicy`（DEP 状态）

## ⬇️ 下载使用

前往 [Releases](https://github.com/NeetheCheeBao/Check-CPU-V/releases) 页面下载

## 🛠️ 编译

```powershell
dotnet publish -c Release -o dist
```

或者

```bash
.\build.bat
```

## ⚖️ 许可证

本项目采用 MIT 许可证 - 详情请参阅 [LICENSE](LICENSE) 文件。
