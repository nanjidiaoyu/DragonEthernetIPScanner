# DragonEthernetIPScanner

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](https://github.com/dotnet/core)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

**DragonEthernetIPScanner** 是一个使用 C# / .NET 8 编写的开源 **EtherNet/IP**（EtherNet Industrial Protocol）**Scanner（主站/客户端）** 通信库，实现了 ODVA **CIP**（Common Industrial Protocol）协议的核心功能，可用于与罗克韦尔（Allen-Bradley）、欧姆龙等支持 EtherNet/IP 的 PLC 及工业设备进行通信。

> 协议实现参考了 ODVA 发布的 CIP 规范：
> *The CIP Networks Library Vol.1 — Common Industrial Protocol*、
> *The CIP Networks Library Vol.2 — EtherNet/IP Adaptation of CIP*，
> 规范原文可在 [ODVA 官网](https://www.odva.org) 获取。

## ✨ 功能特性

- **会话管理**：基于 TCP 的 Encapsulation 会话注册 / 注销（Register Session / UnRegister Session）
- **显式消息（Explicit Messaging）**：通过 Message Router 发送 CIP 请求
  - 支持全部 CIP 服务码（如 `GET_ATTRIBUTE_SINGLE`、`SET_ATTRIBUTE_SINGLE` 等）
  - 支持完整 EPath 路径寻址（类 / 实例 / 属性）
- **隐式消息（Implicit Messaging / I/O 连接）**：基于 UDP 实时 I/O
  - `Forward Open` / `Large Forward Open` / `Forward Close`
  - O2T（Output to Target）与 T2O（Target to Output）双向数据交换
  - 支持多种连接类型（Point-to-Point / Multicast）、优先级与实时格式（Real Time Format）
  - 支持连接超时监控与**断线自动重连**
- **标准 CIP 对象**：内置 Identity 对象（0x01）解析，读取设备厂商 ID、设备类型、产品代码、序列号等
- **异步 API**：全异步（`async/await`）实现，适用于高并发场景
- **结构化日志**：集成 `Microsoft.Extensions.Logging.Abstractions`，可对接任意日志框架（控制台、Serilog、NLog 等）
- **零外部依赖**：除日志抽象包外无其他第三方依赖，全部报文手工编解码

## 📦 项目结构

```
DragonEthernetIPScanner/
├── src/
│   └── DragonEthernetIP/            # 核心库
│       ├── SessionInfo.cs           # TCP 会话管理（注册/注销/心跳）
│       ├── MessageRouter.cs         # CIP 显式消息路由
│       ├── ConnectionManager.cs     # I/O 连接管理（Forward Open/Close、重连）
│       ├── IOConnection.cs          # 单条 I/O 连接（UDP 实时数据收发）
│       ├── IdentityObject.cs        # CIP Identity 对象（0x01）
│       ├── CIP/                     # CIP 协议层
│       │   ├── ConnectionManager/   # Forward Open/Close 请求与响应
│       │   ├── EPath.cs             # CIP 路径寻址
│       │   ├── MessageRouterRequest/Response.cs
│       │   └── ServiceCodes / GeneralStatusCodes 等
│       ├── EIP/                     # EtherNet/IP 封装层
│       │   ├── EncapsPacket.cs      # Encapsulation 报文
│       │   ├── CommonPacket.cs      # CPF（Common Packet Format）
│       │   └── ConnectionType / NetworkType 等
│       └── Utils/
│           └── DragonBuffer.cs      # 字节缓冲区读写工具
├── example/                         # 示例程序
│   ├── ExplicitMessagingExample/    # 显式消息示例
│   ├── ImplicitMessagingExample/    # 隐式 I/O 连接示例
│   └── IdentityObjectExample/       # 读取设备 Identity 示例
└── doc/                             # ODVA 协议规范（仅本地保留，不入库）
```

## 🚀 快速开始

### 环境要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本
- 支持 EtherNet/IP 的目标设备（PLC、变频器、远程 I/O 等），默认 TCP 端口 `0xAF12`（44818），UDP 实时端口 `0x08AE`（2222）

### 安装

发布到 NuGet 后（或本地编译）：

```bash
dotnet add package DragonEthernetIP
```

或直接将 `src/DragonEthernetIP` 项目加入解决方案引用。

### 示例 1：读取设备 Identity（识别设备信息）

```csharp
using DragonEthernetIP;
using DragonEthernetIP.CIP;

// 建立会话
var sessionInfo = new SessionInfo("192.168.0.10", DragonEndPoint.EIP_DEFAULT_EXPLICIT_PORT);
await sessionInfo.RegisterSessionAsync();

// 读取 Identity 对象（类 0x01）
var messageRouter = new MessageRouter();
var identityObject = new IdentityObject(1, sessionInfo, messageRouter);
await identityObject.ReadIdentityAsync();

Console.WriteLine($"厂商 ID:    {identityObject.VendorId}");
Console.WriteLine($"设备类型:   {identityObject.DeviceType}");
Console.WriteLine($"产品代码:   {identityObject.ProductCode}");
Console.WriteLine($"版本号:     {identityObject.Revision}");
Console.WriteLine($"序列号:     {identityObject.SerialNumber}");
Console.WriteLine($"产品名称:   {identityObject.ProductName}");
```

### 示例 2：显式消息（读写 CIP 属性）

```csharp
using DragonEthernetIP;
using DragonEthernetIP.CIP;
using DragonEthernetIP.Utils;

var sessionInfo = new SessionInfo("192.168.0.10", DragonEndPoint.EIP_DEFAULT_EXPLICIT_PORT);
await sessionInfo.RegisterSessionAsync();

var messageRouter = new MessageRouter();

// GET_ATTRIBUTE_SINGLE：读取 Identity 对象（0x01）实例 1 的属性 1（Vendor ID）
var response = await messageRouter.SendRequest(
    sessionInfo,
    (byte)ServiceCodes.GET_ATTRIBUTE_SINGLE,
    new EPath(0x01, 1, 1));

if (response.GetGeneralStatusCode() == GeneralStatusCodes.SUCCESS)
{
    var buffer = new DragonBuffer(response.GetData());
    ushort vendorId = buffer.ReadUshort();
    Console.WriteLine($"Vendor ID: {vendorId}");
}
else
{
    Console.WriteLine($"错误: 0x{response.GetGeneralStatusCode():X}");
}
```

### 示例 3：隐式消息（建立实时 I/O 连接）

```csharp
using DragonEthernetIP;
using DragonEthernetIP.CIP;
using DragonEthernetIP.CIP.ConnectionManager;

var sessionInfo = new SessionInfo("192.168.0.10", DragonEndPoint.EIP_DEFAULT_EXPLICIT_PORT);
await sessionInfo.RegisterSessionAsync();

// ConnectionManager 需要传入本机 IP 地址（用于 UDP 收发）
var connectionManager = new ConnectionManager("192.168.0.100");

var parameters = new ConnectionParameters
{
    // 连接路径：目标设备中的 Input/Output 组合实例（按实际设备填写）
    ConnectionPath = new List<byte> { 0x20, 0x04, 0x24, 0x64, /* ... */ },
    O2TRealTimeFormat = 1,
    T2ORealTimeFormat = 1,
    OriginatorVendorId = 170,
    OriginatorSerialNumber = 0x504D4153,
    O2TRPI = 1000000,   // 请求包间隔（微秒）
    T2ORPI = 1000000,
    TransportTypeTrigger = (byte)NetworkConnectionParams.CLASS1,
    PriorityTimeTick = 10,
    TimeoutTicks = 240,
    ConnectionTimeoutMultiplier = 2,
};
parameters.O2TNetworkConnectionParams |= (uint)NetworkConnectionParams.P2P;
parameters.T2ONetworkConnectionParams |= (uint)NetworkConnectionParams.MULTICAST | 4;

// Forward Open，建立 I/O 连接（连接断开后自动重连）
await connectionManager.ForwardOpen(sessionInfo, parameters, isLarge: false);

Console.ReadKey(); // 保持程序运行以持续收发 I/O 数据
```

> 完整可运行示例见 [example](example) 目录。

### 启用日志

库使用 `Microsoft.Extensions.Logging.Abstractions`，传入任意 `ILogger` 即可输出调试信息：

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "HH:mm:ss.fff "; });
});

var sessionInfo = new SessionInfo(
    "192.168.0.10",
    DragonEndPoint.EIP_DEFAULT_EXPLICIT_PORT,
    loggerFactory.CreateLogger<SessionInfo>());
```

## 📚 核心概念

| 概念 | 说明 |
|------|------|
| **显式消息**（Explicit Messaging） | 基于 TCP 的请求/响应式通信，用于读写参数、诊断等非实时数据 |
| **隐式消息**（Implicit Messaging） | 基于 UDP 的实时 I/O 数据交换，数据含义由 I/O 连接预先定义，无需每次解析协议 |
| **CIP** | Common Industrial Protocol，ODVA 定义的工业协议族公共应用层 |
| **EPath** | CIP 中的对象寻址路径，格式为 `类 ID / 实例 ID / 属性 ID` |
| **RPI** | Requested Packet Interval，请求的 I/O 报文发送间隔（微秒） |
| **O2T / T2O** | Originator to Target（Scanner → 设备，输出数据）/ Target to Originator（设备 → Scanner，输入数据） |

## 🛠️ 开发与调试

```bash
# 克隆仓库
git clone https://github.com/yourname/DragonEthernetIPScanner.git
cd DragonEthernetIPScanner

# 编译
dotnet build

# 运行示例（修改 Program.cs 中的 IP 地址后）
dotnet run --project example/IdentityObjectExample
```

### 测试建议

本仓库不包含硬件仿真，建议使用以下软件模拟 EtherNet/IP 设备进行测试：

- [SCADA/HMI 支持 EtherNet/IP 适配器的仿真软件]
- SoftLogix / RSEmulate（罗克韦尔）
- 或直接连接真实 PLC

## 🗺️ 路线图

- [ ] Tag 层读写（ControlLogix 符号寻址）
- [ ] 连接管理器多设备并发支持
- [ ] 更完善的 Forward Close / 生命周期管理
- [ ] NuGet 包发布
- [ ] 单元测试与协议一致性测试

## 🤝 参与贡献

欢迎提交 Issue 与 Pull Request！

1. Fork 本仓库
2. 创建特性分支：`git checkout -b feature/your-feature`
3. 提交更改：`git commit -m "feat: add some feature"`
4. 推送分支：`git push origin feature/your-feature`
5. 提交 Pull Request

## 📄 许可证

本项目采用 [MIT License](LICENSE) 开源。

## 🙏 致谢

- 协议规范版权归 [ODVA](https://www.odva.org) 所有，本仓库不含规范原文，仅参考其公开规范实现
- 灵感来源于开源 EtherNet/IP 实现（如 OpENer、pycomm3 等）
