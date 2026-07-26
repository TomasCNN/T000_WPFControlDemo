# 004023003_WPF `MediaElement` 基类体系官方类定义逐层解析

**源码：**

```c#
public class MediaElement : FrameworkElement, IUriContext
{
    public static readonly DependencyProperty SourceProperty;
    public static readonly RoutedEvent ScriptCommandEvent;
    public static readonly RoutedEvent BufferingEndedEvent;
    public static readonly RoutedEvent BufferingStartedEvent;
    public static readonly RoutedEvent MediaOpenedEvent;
    public static readonly RoutedEvent MediaFailedEvent;
    public static readonly DependencyProperty StretchDirectionProperty;
    public static readonly RoutedEvent MediaEndedEvent;
    public static readonly DependencyProperty LoadedBehaviorProperty;
    public static readonly DependencyProperty UnloadedBehaviorProperty;
    public static readonly DependencyProperty ScrubbingEnabledProperty;
    public static readonly DependencyProperty IsMutedProperty;
    public static readonly DependencyProperty BalanceProperty;
    public static readonly DependencyProperty VolumeProperty;
    public static readonly DependencyProperty StretchProperty;
 
    public MediaElement();
 
    public MediaState LoadedBehavior { get; set; }
    public bool CanPause { get; }
    public bool IsBuffering { get; }
    public double DownloadProgress { get; }
    public double BufferingProgress { get; }
    public int NaturalVideoHeight { get; }
    public Duration NaturalDuration { get; }
    public bool HasAudio { get; }
    public bool HasVideo { get; }
    public TimeSpan Position { get; set; }
    public double SpeedRatio { get; set; }
    public MediaState UnloadedBehavior { get; set; }
    public int NaturalVideoWidth { get; }
    public bool ScrubbingEnabled { get; set; }
    public MediaClock Clock { get; set; }
    public double Balance { get; set; }
    public double Volume { get; set; }
    public StretchDirection StretchDirection { get; set; }
    public Stretch Stretch { get; set; }
    public Uri Source { get; set; }
    public bool IsMuted { get; set; }
 
    public event RoutedEventHandler BufferingEnded;
    public event RoutedEventHandler BufferingStarted;
    public event RoutedEventHandler MediaOpened;
    public event EventHandler<ExceptionRoutedEventArgs> MediaFailed;
    public event RoutedEventHandler MediaEnded;
    public event EventHandler<MediaScriptCommandRoutedEventArgs> ScriptCommand;
 
    public void Close();
    public void Pause();
    public void Play();
    public void Stop();
    protected override Size ArrangeOverride(Size finalSize);
    protected override Size MeasureOverride(Size availableSize);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected override void OnRender(DrawingContext drawingContext);
 
}
```

这是 WPF 原生 `System.Windows.Controls.MediaElement` 的完整核心类签名，定义于程序集 `PresentationFramework.dll`，直接继承自 `FrameworkElement` 并实现 `IUriContext` 接口，是**无控件模板的纯渲染型媒体元素**，底层封装 Windows Media Player COM 组件，专注于音视频解码、播放控制与画面渲染。

------

## 一、类整体定位与继承关系

### 完整签名

csharp：

```c#
public class MediaElement : FrameworkElement, IUriContext
```

- **继承链**：`DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → MediaElement`
- **实现接口**：`IUriContext` 提供 `BaseUri` 基准地址能力，用于解析相对路径的媒体源 `Uri`
- **设计定位**：轻量级渲染元素，无 `ControlTemplate` 控件模板，不具备通用控件的边框、字体、标题等外观属性，所有能力围绕媒体播放与画面渲染展开
- **底层依赖**：基于 Windows Media Player 核心组件实现解码，格式支持能力与系统安装的解码器直接相关

------

## 二、静态成员深度解析

所有 `static readonly` 字段均为 WPF 属性系统与事件系统的唯一标识符，是数据绑定、样式、动画、路由事件的底层基础设施。

### 2.1 依赖属性标识符

按功能维度分组解析如下：

#### 🔹 媒体源与生命周期行为组

| 依赖属性标识符             | 对应 CLR 属性      | 属性类型          | 默认值  | 核心说明                                                     |
| :------------------------- | :----------------- | :---------------- | :------ | :----------------------------------------------------------- |
| `SourceProperty`           | `Source`           | `Uri`             | `null`  | 媒体文件地址，支持本地绝对 / 相对路径、HTTP 网络流；依赖 `IUriContext` 解析相对路径 |
| `LoadedBehaviorProperty`   | `LoadedBehavior`   | `MediaState` 枚举 | `Play`  | **核心控制属性**，媒体加载完成后的默认行为：`Play`自动播放 / `Pause`加载后暂停 / `Stop`加载后停止 / `Manual`手动控制 / `Close`加载后关闭自定义播放逻辑必须设为 `Manual` |
| `UnloadedBehaviorProperty` | `UnloadedBehavior` | `MediaState` 枚举 | `Close` | 控件从可视化树卸载时的行为，默认 `Close` 自动释放底层媒体资源，防止内存泄漏 |

#### 🔹 播放控制属性组

| 依赖属性标识符             | 对应 CLR 属性      | 属性类型 | 默认值  | 核心说明                                                     |
| :------------------------- | :----------------- | :------- | :------ | :----------------------------------------------------------- |
| `VolumeProperty`           | `Volume`           | `double` | `0.5`   | 音量大小，取值范围 `0.0 ~ 1.0`，支持绑定与动画               |
| `IsMutedProperty`          | `IsMuted`          | `bool`   | `false` | 是否静音；静音不修改 `Volume` 值，取消静音自动恢复原音量     |
| `BalanceProperty`          | `Balance`          | `double` | `0.0`   | 左右声道平衡，取值 `-1.0（全左） ~ 1.0（全右）`，默认居中    |
| `ScrubbingEnabledProperty` | `ScrubbingEnabled` | `bool`   | `false` | 是否启用拖拽进度时即时预览画面；工业录像回放场景建议开启，拖动进度条同步显示对应帧 |

#### 🔹 渲染布局属性组

| 依赖属性标识符             | 对应 CLR 属性      | 属性类型                | 默认值    | 核心说明                                                     |
| :------------------------- | :----------------- | :---------------------- | :-------- | :----------------------------------------------------------- |
| `StretchProperty`          | `Stretch`          | `Stretch` 枚举          | `Uniform` | 画面填充模式：`None`原始尺寸 / `Uniform`等比完整显示 / `Fill`拉伸填满 / `UniformToFill`等比填满裁剪工业视觉检测推荐 `Uniform` 保证画面不变形 |
| `StretchDirectionProperty` | `StretchDirection` | `StretchDirection` 枚举 | `Both`    | 拉伸方向限制：`UpOnly`仅放大 / `DownOnly`仅缩小 / `Both`双向均可 |

### 2.2 路由事件标识符

所有媒体事件均为**冒泡路由事件**，可在父容器统一监听，无需逐个控件订阅。

| 路由事件标识符          | 对应 CLR 事件      | 委托类型                                          | 核心说明                                                     |
| :---------------------- | :----------------- | :------------------------------------------------ | :----------------------------------------------------------- |
| `MediaOpenedEvent`      | `MediaOpened`      | `RoutedEventHandler`                              | 媒体加载完成、元数据解析完毕时触发；此时才能读取总时长、分辨率等信息 |
| `MediaEndedEvent`       | `MediaEnded`       | `RoutedEventHandler`                              | 播放到媒体末尾时触发；常用于循环播放、播放完成回调           |
| `MediaFailedEvent`      | `MediaFailed`      | `EventHandler<ExceptionRoutedEventArgs>`          | 媒体加载失败、解码错误、文件损坏时触发；工业场景必须处理，避免程序无响应 |
| `BufferingStartedEvent` | `BufferingStarted` | `RoutedEventHandler`                              | 网络流缓冲开始时触发，可用于显示加载动画                     |
| `BufferingEndedEvent`   | `BufferingEnded`   | `RoutedEventHandler`                              | 网络流缓冲结束、恢复播放时触发                               |
| `ScriptCommandEvent`    | `ScriptCommand`    | `EventHandler<MediaScriptCommandRoutedEventArgs>` | 媒体流中遇到脚本命令时触发；可用于 ASF/WMV 文件内嵌标记点，工业场景可用于录像缺陷点位自动跳转 |

------

## 三、构造函数

csharp:

```c#
public MediaElement();
```

构造函数内部完成三项核心初始化：

1. 注册所有依赖属性的默认元数据与变更回调
2. 初始化底层 Windows Media Player COM 组件的包装器
3. 设置默认渲染参数：`Stretch=Uniform`、`Volume=0.5`、`LoadedBehavior=Play`

------

## 四、实例属性逐字段解析

### 4.1 生命周期行为属性

csharp:

```c#
public MediaState LoadedBehavior { get; set; }
public MediaState UnloadedBehavior { get; set; }
```

- `LoadedBehavior`：控制媒体加载后的自动行为，**自定义播放控制必须设为 `Manual`**，否则 `Play/Pause` 方法不生效
- `UnloadedBehavior`：控件卸载时的资源回收策略，默认 `Close` 会自动释放媒体资源；若需后台继续播放音频，可设为 `Play`

### 4.2 播放控制属性（可读写）

csharp:

```c#
public TimeSpan Position { get; set; }
public double SpeedRatio { get; set; }
public double Volume { get; set; }
public bool IsMuted { get; set; }
public double Balance { get; set; }
public bool ScrubbingEnabled { get; set; }
public MediaClock Clock { get; set; }
```



| 属性         | 核心说明与工业场景用法                                       |
| :----------- | :----------------------------------------------------------- |
| `Position`   | 当前播放进度，可读写；用于跳转指定时间点（如缺陷录像一键定位）。⚠️ 注意：无 `PositionChanged` 事件，UI 进度条需通过 `DispatcherTimer` 轮询更新 |
| `SpeedRatio` | 播放倍速，默认 `1.0`；支持 0.5~2.0 倍速，工业场景常用于快速回看检测录像、定位缺陷 |
| `Clock`      | WPF 动画计时系统的媒体时钟，用于基于 `MediaTimeline` 的时间线驱动播放；MVVM 动画场景使用，常规手动控制极少用到 |

### 4.3 媒体元数据属性（只读）

csharp:

```c#
public Duration NaturalDuration { get; }
public int NaturalVideoWidth { get; }
public int NaturalVideoHeight { get; }
public bool HasAudio { get; }
public bool HasVideo { get; }
public double BufferingProgress { get; }
public double DownloadProgress { get; }
public bool IsBuffering { get; }
public bool CanPause { get; }
```

> ⚠️ 重要：所有元数据属性**仅在 `MediaOpened` 事件触发后才有有效值**，加载过程中访问会得到默认空值。

表格







| 属性                       | 核心说明                                                     |
| :------------------------- | :----------------------------------------------------------- |
| `NaturalDuration`          | 媒体总时长，`Duration` 类型；需判断 `HasTimeSpan` 后再转为 `TimeSpan` 使用 |
| `NaturalVideoWidth/Height` | 视频原始分辨率，用于计算画面比例、自适应布局                 |
| `HasAudio/HasVideo`        | 当前媒体是否包含音频 / 视频流；纯音频播报场景可判断 `HasVideo=false` 隐藏渲染区域 |
| `BufferingProgress`        | 缓冲进度，`0.0~1.0`；网络流、远程录像场景用于显示缓冲百分比  |
| `DownloadProgress`         | 下载进度，`0.0~1.0`；HTTP 网络文件场景使用                   |
| `IsBuffering`              | 是否正在缓冲；可联动 UI 显示加载转圈动画                     |
| `CanPause`                 | 当前媒体是否支持暂停；直播流、实时流通常不支持暂停           |

### 4.4 渲染与源属性

csharp:

```c#
public Stretch Stretch { get; set; }
public StretchDirection StretchDirection { get; set; }
public Uri Source { get; set; }
```

- `Stretch` / `StretchDirection`：控制视频画面在容器内的拉伸方式，直接影响渲染效果
- `Source`：媒体源地址，赋值后自动触发加载流程；支持本地文件、UNC 路径、HTTP URL

------

## 五、事件体系详解

csharp:

```c#
public event RoutedEventHandler BufferingEnded;
public event RoutedEventHandler BufferingStarted;
public event RoutedEventHandler MediaOpened;
public event EventHandler<ExceptionRoutedEventArgs> MediaFailed;
public event RoutedEventHandler MediaEnded;
public event EventHandler<MediaScriptCommandRoutedEventArgs> ScriptCommand;
```

### 1. MediaOpened（核心事件）

- **触发时机**：媒体文件加载完成、解码初始化成功、元数据全部可用时

- **典型用法**：获取总时长、初始化进度条最大值、设置初始音量、读取分辨率

  csharp:

  ```c#
  private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
  {
      if (mediaPlayer.NaturalDuration.HasTimeSpan)
      {
          progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
      }
  }
  ```

### 2. MediaEnded

- **触发时机**：播放进度到达媒体末尾时

- **典型用法**：循环播放、播放完成日志记录、自动播放下一段录像

  csharp:

  ```c#
  private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
  {
      // 循环播放：回到开头重新播放
      mediaPlayer.Position = TimeSpan.Zero;
      mediaPlayer.Play();
  }
  ```

### 3. MediaFailed（必须处理）

- **触发时机**：文件不存在、格式不支持、解码失败、网络中断等所有播放错误时

- **事件参数**：`ExceptionRoutedEventArgs` 包含 `ErrorException` 异常详情

- **典型用法**：记录错误日志、弹窗提示、降级播放备用视频，避免程序静默失败

  csharp:

  ```c#
  private void MediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
  {
      UpdateProcess($"视频加载失败：{e.ErrorException.Message}");
  }
  ```

### 4. BufferingStarted / BufferingEnded

- **触发时机**：网络播放数据不足开始缓冲、缓冲完成恢复播放时
- **典型用法**：控制加载动画的显示与隐藏，优化用户体验

### 5. ScriptCommand

- **触发时机**：媒体流中遇到内嵌脚本命令标记时
- **事件参数**：包含 `ParameterType`（命令类型）和 `Parameter`（参数）
- **工业场景用法**：在检测录像中嵌入缺陷时间点标记，播放到对应位置自动弹出缺陷详情

------

## 六、核心方法详解

### 6.1 公共播放控制方法

csharp:

```
public void Play();
public void Pause();
public void Stop();
public void Close();
```

| 方法      | 作用                                 | 注意事项                                                     |
| :-------- | :----------------------------------- | :----------------------------------------------------------- |
| `Play()`  | 从当前 `Position` 开始播放           | 仅 `LoadedBehavior=Manual` 时生效；媒体未加载时自动触发加载  |
| `Pause()` | 暂停播放，保留当前进度               | 仅 `CanPause=true` 的媒体支持；直播流可能无效                |
| `Stop()`  | 停止播放，进度重置为 `TimeSpan.Zero` | 停止后媒体资源仍保持加载状态，可再次 `Play()`                |
| `Close()` | 关闭媒体，释放底层所有资源           | 释放后需重新赋值 `Source` 才能再次播放；页面关闭时建议手动调用 |

### 6.2 重写的基类方法

csharp:

```c#
protected override Size MeasureOverride(Size availableSize);
protected override Size ArrangeOverride(Size finalSize);
protected override void OnRender(DrawingContext drawingContext);
protected override AutomationPeer OnCreateAutomationPeer();
```

#### MeasureOverride / ArrangeOverride

- 重写自 `FrameworkElement`，实现 WPF 布局两步法
- **内部逻辑**：结合视频原始分辨率、`Stretch`、`StretchDirection` 计算视频画面的最终渲染尺寸，保证按比例正确显示

#### OnRender

- 重写自 `UIElement`，是画面渲染的核心入口
- **内部逻辑**：通过 `DrawingContext` 将解码后的视频帧绘制到渲染表面，是视频画面最终显示在界面上的底层实现

#### OnCreateAutomationPeer

- 创建 `MediaElementAutomationPeer` 自动化对等类
- 作用：为屏幕阅读器、UI 自动化测试、无障碍访问提供播放状态、控制接口

------

## 七、工业场景关键避坑与最佳实践

1. **手动控制必设配置**

   自定义播放逻辑必须显式设置 `LoadedBehavior="Manual"`，否则 `Play/Pause` 方法不生效，这是最常见的入门坑。

2. **元数据读取时机**

   禁止在构造函数、`Loaded` 事件中直接读取 `NaturalDuration`、分辨率，必须在 `MediaOpened` 事件触发后访问。

3. **进度更新方式**

   无原生进度变更事件，需使用 `DispatcherTimer` 以 200~500ms 间隔轮询 `Position` 更新进度条，避免高频刷新影响性能。

4. **资源释放规范**

   页面关闭时确保 `UnloadedBehavior="Close"`，或手动调用 `Close()`，避免 WMP 组件残留、内存泄漏；频繁切换视频时，先 `Stop()` 再赋值新 `Source`。

5. **线程安全约束**

   所有播放控制、属性赋值必须在 UI 线程执行；后台报警线程触发语音播报时，需通过 `Dispatcher.Invoke` 切换线程。

6. **格式兼容建议**

   工业场景统一使用 H.264 编码的 MP4 格式，兼容性最好；H.265、MKV 等格式依赖系统解码器，跨机器易出现播放失败。