# MyCTP (CTP v6.6.9 C# 客户端示例)

该项目提供一个基于 **C# + ASP.NET Core + SignalR** 的 CTP 客户端骨架，覆盖以下能力：

1. 登录后读取并展示账户资金、持仓（资金栏 + 持仓表格）
2. 实时接收委托与成交，并根据成交自动更新持仓
3. 实时接收订阅合约行情
4. 支持多账户登录与切换查看

> 当前仓库内为 `MockCtpTradingGateway / MockCtpMarketDataGateway` 模拟实现，接口设计已经对齐真实 CTP 接入方式，替换为你们现有的 `CTP TraderApi / MdApi` 封装即可。

## 目录

- `src/CtpClientWeb/Services/Abstractions.cs`：交易、行情网关接口 + 持仓服务接口
- `src/CtpClientWeb/Services/MockCtpTradingGateway.cs`：资金/委托/成交模拟推送
- `src/CtpClientWeb/Services/MockCtpMarketDataGateway.cs`：订阅行情模拟推送
- `src/CtpClientWeb/Services/PositionService.cs`：成交驱动持仓更新
- `src/CtpClientWeb/Services/GatewaySubscriptionService.cs`：事件汇总并广播给前端
- `src/CtpClientWeb/Pages/Index.cshtml`：资金栏、持仓表格、委托/成交/行情实时展示
- `src/CtpClientWeb/wwwroot/js/app.js`：多账户登录、SignalR 实时更新、页面渲染

## 如何替换为真实 CTP 6.6.9

1. 新建 `RealCtpTradingGateway : ITradingGateway`
   - 在 `LoginAsync` 中调用 `ReqAuthenticate / ReqUserLogin / ReqQryTradingAccount / ReqQryInvestorPosition`
   - 把 `OnRtnOrder / OnRtnTrade / OnRspQryTradingAccount` 映射到接口事件
2. 新建 `RealCtpMarketDataGateway : IMarketDataGateway`
   - 登录行情前置后执行 `SubscribeMarketData`
   - 把 `OnRtnDepthMarketData` 映射到 `TickReceived`
3. 在 `Program.cs` 中把 `Mock...Gateway` 注入替换为 `Real...Gateway`
4. 若你们是 WinForms/WPF 客户端，也可复用 `Services` 层逻辑，将 SignalR 部分替换为 UI 线程事件分发

## 启动

```bash
dotnet run --project src/CtpClientWeb/CtpClientWeb.csproj
```

打开 `http://localhost:5000` 或启动日志中的地址。
