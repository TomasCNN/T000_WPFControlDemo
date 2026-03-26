# 001001005_Application的生命周期																																																																																																																																																																																																																																												

​	Application（应用程序类）的生命周期，就是 WPF 程序从「启动→运行→退出」的完整过程，核心分为 **启动、运行、退出** 三大阶段，每个阶段对应专属事件和操作逻辑。下面用 “人话” 讲清生命周期，再结合工业场景案例说明用法。

## 一、Application 生命周期核心阶段（3 步核心 + 关键事件）

### 1. 启动阶段（Startup）

- **通俗解释**：程序 “刚睡醒”，还没显示任何窗口，是初始化的最佳时机；
- **核心事件**：`Startup`（程序启动时触发，优先级高于 `StartupUri`）；
- **典型操作**：初始化配置、检查权限、处理命令行参数、创建主窗口、单实例校验（互斥锁）。

### 2. 运行阶段（Running）

- **通俗解释**：程序 “正常工作”，用户操作窗口 / 控件，是程序的核心运行期；
- **核心事件**：无专属 “运行事件”，但可监听全局异常（`DispatcherUnhandledException`）、窗口激活 / 关闭等；
- **典型操作**：管理窗口、共享全局数据、响应用户操作、处理业务逻辑。

### 3. 退出阶段（Exit）

- **通俗解释**：程序 “准备关机”，所有窗口关闭前的收尾阶段；
- **核心事件**：`Exit`（程序退出时触发）；
- **典型操作**：保存配置、关闭数据库连接、释放硬件资源、记录退出日志。

### 生命周期完整流程（可视化）

```
flowchart TD
    A[程序启动] --> B[触发Startup事件<br/>（初始化/单实例校验/创建主窗口）]
    B --> C[运行阶段<br/>（用户操作/窗口管理/业务逻辑）]
    C --> D{程序退出触发条件?}
    D -->|主窗口关闭/Shutdown()/异常| E[触发Exit事件<br/>（清理资源/保存配置）]
    E --> F[程序进程结束]
```

## 二、核心生命周期案例（工业场景）

### 	案例 1：完整生命周期实现（含单实例 + 异常处理 + 资源清理）

​	这是工业软件（如数据采集系统）的标准生命周期实现，覆盖启动、运行、退出全流程：

csharp：

```c#
using System;
using System.Threading;
using System.Windows;

namespace IndustrialApp
{
    public partial class App : Application
    {
        // 1. 启动阶段：初始化+单实例校验
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // 步骤1：单实例校验（互斥锁）
            bool isNewInstance;
            using (Mutex mutex = new Mutex(true, "com.mycompany.IndustrialApp_Mutex", out isNewInstance))
            {
                if (isNewInstance)
                {
                    // 步骤2：初始化全局配置（工业场景：加载设备参数）
                    GlobalConfig.Load("config.json");

                    // 步骤3：处理命令行参数（比如双击数据文件打开程序）
                    string targetFile = e.Args.Length > 0 ? e.Args[0] : null;

                    // 步骤4：创建并显示主窗口
                    MainWindow = new MainWindow(targetFile);
                    MainWindow.Show();
                }
                else
                {
                    // 重复实例：提示并退出
                    MessageBox.Show("数据采集系统已在运行！", "提示", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Shutdown(); // 退出当前实例
                }
            }
        }

        // 2. 运行阶段：全局异常捕获（防止程序闪退）
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 工业场景：记录异常日志（方便排查设备通信问题）
            LogHelper.Error("程序运行异常", e.Exception);
            
            // 显示友好提示（不暴露技术细节）
            MessageBox.Show("数据采集异常，请检查设备连接！", "错误", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            
            // 标记异常已处理，避免程序崩溃
            e.Handled = true;
        }

        // 3. 退出阶段：资源清理（工业场景必备）
        private void App_Exit(object sender, ExitEventArgs e)
        {
            // 步骤1：保存用户配置（比如窗口大小、采集参数）
            GlobalConfig.Save("config.json");

            // 步骤2：关闭硬件/数据库连接（工业场景：断开传感器/PLC连接）
            DeviceHelper.DisconnectAll();
            DbHelper.CloseConnection();

            // 步骤3：记录退出日志（审计用）
            LogHelper.Info($"程序正常退出，退出码：{e.ApplicationExitCode}");
        }
    }
}
```

#### 配套 App.xaml 配置（绑定事件）

xaml:

```
<Application x:Class="IndustrialApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Startup="App_Startup"
             Exit="App_Exit"
             DispatcherUnhandledException="App_DispatcherUnhandledException">
    <Application.Resources>
        <!-- 全局资源 -->
    </Application.Resources>
</Application>
```

### 案例 2：启动阶段动态选择主窗口（按权限）

​	工业软件常按用户权限显示不同主窗口（比如管理员 / 普通操作员）：

csharp:

```
private void App_Startup(object sender, StartupEventArgs e)
{
    // 步骤1：校验用户权限（模拟从配置读取）
    string userRole = ConfigHelper.GetUserRole(); // "Admin" 或 "Operator"

    // 步骤2：按权限创建不同主窗口
    if (userRole == "Admin")
    {
        MainWindow = new AdminMainWindow(); // 管理员窗口（含配置/权限管理）
    }
    else
    {
        MainWindow = new OperatorMainWindow(); // 操作员窗口（仅数据采集）
    }

    // 步骤3：显示主窗口
    MainWindow.Show();
}
```

### 案例 3：退出阶段确认是否保存数据

​	工业软件退出前，确认用户是否保存未提交的采集数据：

csharp:

```c#
private void App_Exit(object sender, ExitEventArgs e)
{
    // 步骤1：检查是否有未保存数据
    if (DataCollector.HasUnsavedData)
    {
        // 步骤2：弹出确认框
        MessageBoxResult result = MessageBox.Show("有未保存的采集数据，是否保存？", "提示",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        // 步骤3：根据选择处理
        if (result == MessageBoxResult.Yes)
        {
            DataCollector.SaveUnsavedData(); // 保存数据
        }
        else if (result == MessageBoxResult.Cancel)
        {
            e.ApplicationExitCode = -1; // 标记退出取消
            ShutdownMode = ShutdownMode.OnExplicitShutdown; // 取消退出
        }
    }

    // 步骤4：通用清理
    DeviceHelper.DisconnectAll();
}
```

## 三、关键补充（生命周期易错点）

1. **ShutdownMode（退出触发规则）**：

   - 默认：`ShutdownMode.OnMainWindowClose`（主窗口关闭则退出程序）；
   - 常用配置：`ShutdownMode.OnExplicitShutdown`（仅调用 `Shutdown()` 才退出，适合多窗口场景）。

   

   csharp:

   ```c#
   // 在Startup事件中设置
   ShutdownMode = ShutdownMode.OnExplicitShutdown;
   ```

2. **异常退出处理**：

   程序崩溃时，

   ```c#
   DispatcherUnhandledException
   ```

    是最后一道防线，需记录日志并友好提示，避免直接闪退。

3. **启动参数（StartupEventArgs e）**：

   ```c#
   e.Args
   ```

    可获取命令行参数（比如用户双击 

   ```c#
   .dat
   ```

    文件打开程序，参数为文件路径）。

## 总结

### 生命周期核心要点

1. **启动阶段（Startup）**：初始化 + 校验 + 创建窗口，是程序 “开局准备”；
2. **运行阶段**：无专属事件，核心是窗口管理和业务逻辑，是程序 “核心工作期”；
3. **退出阶段（Exit）**：清理资源 + 保存数据，是程序 “收尾工作”；
4. **全局异常（DispatcherUnhandledException）**：运行阶段的 “安全兜底”，工业软件必须实现。

### 工业场景价值

​	规范的生命周期管理，能保证程序启动稳定（单实例、权限校验）、运行可靠（异常捕获）、退出安全（资源清理），是工业级 WPF 程序的基础要求。