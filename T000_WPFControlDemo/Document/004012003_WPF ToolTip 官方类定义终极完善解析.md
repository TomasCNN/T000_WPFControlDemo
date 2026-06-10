# 004012003_WPF ToolTip 官方类定义终极完善解析

## 一、类定义与特性补充解析

### 1.1 完整官方元数据（含隐藏特性）

csharp:

```c#
// 微软官方源代码完整签名（PresentationFramework.dll v8.0.0）
[System.Windows.LocalizabilityAttribute(System.Windows.LocalizationCategory.ToolTip)]
[System.Windows.StyleTypedPropertyAttribute(Property = "Style", StyleTargetType = typeof(ToolTip))]
[System.Windows.TemplatePartAttribute(Name = "PART_Popup", Type = typeof(System.Windows.Controls.Primitives.Popup))]
[System.Windows.DefaultEventAttribute("Opened")]
[System.Windows.ContentPropertyAttribute("Content")]
[System.Windows.ThemeInfoAttribute(System.Windows.ResourceDictionaryLocation.None, System.Windows.ResourceDictionaryLocation.SourceAssembly)]
public class ToolTip : System.Windows.Controls.ContentControl
{
    // 静态构造函数：注册依赖属性和元数据
    static ToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolTip), 
            new FrameworkPropertyMetadata(typeof(ToolTip)));
        
        // 注册路由事件
        OpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), typeof(ToolTip));
        ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), typeof(ToolTip));
        
        // 键盘输入处理：按ESC键关闭ToolTip
        EventManager.RegisterClassHandler(typeof(ToolTip), KeyDownEvent, 
            new KeyEventHandler(OnKeyDown), true);
    }
}
```

### 1.2 特性深度补充

#### `ThemeInfoAttribute`（隐藏特性）

- **作用**：指定控件主题资源的位置
- **官方行为**：ToolTip 的默认样式存储在`PresentationFramework.Aero2.dll`等主题程序集中
- **工业场景意义**：当自定义 ToolTip 样式时，需要显式覆盖默认样式，否则会继承系统主题样式

#### 类级键盘事件处理

- **官方隐藏行为**：ToolTip 内部注册了类级`KeyDown`事件处理
- **默认行为**：当 ToolTip 显示时，按下**ESC 键**会自动关闭它
- **扩展点**：可以通过重写`OnKeyDown`方法添加自定义键盘快捷键

------

## 二、依赖属性完整解析（含元数据和变更回调）

每个依赖属性都有对应的元数据和变更回调，这是理解 ToolTip 内部行为的关键。

### 2.1 `IsOpen` 属性（核心中的核心）

csharp:

```c#
// 官方依赖属性注册代码
public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
    nameof(IsOpen), typeof(bool), typeof(ToolTip),
    new FrameworkPropertyMetadata(false,
        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        OnIsOpenChanged));

// 属性变更回调（内部实现）
private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var toolTip = (ToolTip)d;
    bool newValue = (bool)e.NewValue;
    
    if (newValue)
    {
        toolTip.OnOpened(new RoutedEventArgs(OpenedEvent));
    }
    else
    {
        toolTip.OnClosed(new RoutedEventArgs(ClosedEvent));
    }
}
```

**隐藏行为**：

- `IsOpen`是**双向绑定**的依赖属性（`BindsTwoWayByDefault`）
- 当用户点击鼠标、按下键盘或鼠标移开时，ToolTip 会**自动将 IsOpen 设置为 false**
- 即使你通过绑定将 IsOpen 强制设为 true，用户操作仍然可以关闭它

### 2.2 `StaysOpen` 属性

csharp:

```c#
public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register(
    nameof(StaysOpen), typeof(bool), typeof(ToolTip),
    new PropertyMetadata(false, OnStaysOpenChanged));

private static void OnStaysOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var toolTip = (ToolTip)d;
    if (toolTip._popup != null)
    {
        toolTip._popup.StaysOpen = (bool)e.NewValue;
    }
}
```

**关键实现细节**：

- `StaysOpen`属性直接透传给内部`PART_Popup`控件的同名属性
- 当`StaysOpen="True"`时，ToolTip 不会响应鼠标移开事件，只有以下情况会关闭：
  1. 手动设置`IsOpen="False"`
  2. 按下 ESC 键
  3. 点击 ToolTip 外部的任意位置

### 2.3 `Placement` 属性（完整枚举解析）

| 枚举值          | 官方行为细节                         | 工业场景适用           |
| :-------------- | :----------------------------------- | :--------------------- |
| `Mouse`         | 显示在鼠标指针右下角，随鼠标移动     | 通用按钮提示           |
| `Bottom`        | 显示在目标控件底部中央，水平居中对齐 | 输入框提示、工具栏按钮 |
| `Top`           | 显示在目标控件顶部中央               | 底部状态栏图标         |
| `Left`          | 显示在目标控件左侧中央，垂直居中对齐 | 右侧边栏按钮           |
| `Right`         | 显示在目标控件右侧中央               | 左侧工具栏按钮         |
| `Center`        | 显示在目标控件中心                   | 全屏提示、引导提示     |
| `Relative`      | 相对于目标控件左上角 (0,0) 点        | 精确位置控制           |
| `Absolute`      | 相对于屏幕左上角 (0,0) 点            | 全局提示               |
| `RelativePoint` | 相对于目标控件的指定比例点 (0-1)     | 自定义位置             |
| `AbsolutePoint` | 相对于屏幕的指定比例点 (0-1)         | 全局自定义位置         |

**高级用法示例**：

xaml:

```xaml
<!-- 显示在按钮右上角 -->
<Button Content="按钮">
    <Button.ToolTip>
        <ToolTip Content="提示"
                 Placement="Relative"
                 HorizontalOffset="100"
                 VerticalOffset="-20"/>
    </Button.ToolTip>
</Button>
```

### 2.4 `CustomPopupPlacementCallback` 属性（高级位置控制）

这是最强大但最少被使用的属性，允许完全自定义 ToolTip 的位置计算逻辑。

**官方定义**：

csharp:

```c#
public delegate CustomPopupPlacement[] CustomPopupPlacementCallback(
    Size popupSize, Size targetSize, Point offset);
```

**工业场景应用**：确保 ToolTip 不会超出屏幕边界

csharp:

```c#
// 自定义位置回调：自动调整位置避免超出屏幕
private CustomPopupPlacement[] OnCustomPopupPlacement(Size popupSize, Size targetSize, Point offset)
{
    var screenBounds = SystemParameters.WorkArea;
    var targetBounds = PlacementTarget.TransformToAncestor(Window.GetWindow(this))
        .TransformBounds(new Rect(0, 0, targetSize.Width, targetSize.Height));
    
    // 计算右下角位置
    double x = targetBounds.Right + 5;
    double y = targetBounds.Top;
    
    // 如果超出右边界，显示在左侧
    if (x + popupSize.Width > screenBounds.Right)
    {
        x = targetBounds.Left - popupSize.Width - 5;
    }
    
    // 如果超出下边界，向上调整
    if (y + popupSize.Height > screenBounds.Bottom)
    {
        y = screenBounds.Bottom - popupSize.Height;
    }
    
    return new[]
    {
        new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Horizontal)
    };
}

// 使用回调
toolTip.CustomPopupPlacementCallback = OnCustomPopupPlacement;
```

------

## 三、内部机制深度解析

### 3.1 ToolTip 生命周期

ToolTip 的生命周期完全由内部 Popup 控件控制：

1. **创建阶段**：
   - 当`ToolTipService`检测到鼠标悬浮时，创建 ToolTip 实例
   - 调用`OnApplyTemplate()`方法，加载模板中的`PART_Popup`控件
   - 将 Content 赋值给 Popup 的 Child 属性
2. **显示阶段**：
   - 设置`IsOpen="True"`，触发`Opened`事件
   - Popup 控件计算位置并显示
3. **运行阶段**：
   - 监听鼠标和键盘事件
   - 根据`StaysOpen`属性决定是否自动关闭
4. **销毁阶段**：
   - 设置`IsOpen="False"`，触发`Closed`事件
   - Popup 控件隐藏
   - 如果没有被其他引用持有，ToolTip 实例会被 GC 回收

### 3.2 与 Popup 控件的关系

ToolTip 的所有弹出功能都委托给内部的`PART_Popup`控件：

csharp:

```c#
// 官方内部代码片段
private Popup _popup;

public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    _popup = GetTemplateChild("PART_Popup") as Popup;
    if (_popup != null)
    {
        _popup.Child = this;
        _popup.StaysOpen = StaysOpen;
        _popup.Placement = Placement;
        _popup.PlacementTarget = PlacementTarget;
        // ... 其他属性同步
    }
}
```

**关键结论**：

- ToolTip 本身是 Popup 的 Child 元素
- ToolTip 的所有位置属性都直接透传给 Popup
- 任何 Popup 控件的特性都适用于 ToolTip（如`AllowsTransparency`、`PopupAnimation`）

### 3.3 ToolTipService 内部工作原理

ToolTipService 是一个静态服务类，它通过**附加属性**和**类级事件处理**来管理所有 ToolTip 的显示：

1. **事件监听**：在静态构造函数中注册了所有`UIElement`的`MouseEnter`和`MouseLeave`事件
2. **计时器管理**：内部维护一个全局计时器，用于处理`InitialShowDelay`和`ShowDuration`
3. **实例管理**：同一时间只能显示一个 ToolTip，显示新的 ToolTip 时会自动关闭之前的
4. **优先级处理**：子控件的 ToolTip 会覆盖父控件的 ToolTip

------

## 四、工业场景高级扩展实例

### 4.1 工业风格 ToolTip（带箭头）

工业界面最常用的带箭头提示样式，支持上下左右四个方向的箭头。

xaml:

```xaml
<!-- 带箭头的工业ToolTip样式 -->
<Style TargetType="ToolTip">
    <Setter Property="Background" Value="#FF2D2D30"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="BorderBrush" Value="#FF3E3E42"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="10,8"/>
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="HasDropShadow" Value="False"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ToolTip">
                <Grid>
                    <!-- 箭头 -->
                    <Polygon x:Name="Arrow"
                             Points="0,0 10,0 5,6"
                             Fill="{TemplateBinding Background}"
                             Stroke="{TemplateBinding BorderBrush}"
                             StrokeThickness="1"
                             HorizontalAlignment="Left"
                             Margin="10,-6,0,0"/>
                    
                    <!-- 内容区域 -->
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="3"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter/>
                    </Border>
                </Grid>
                
                <ControlTemplate.Triggers>
                    <!-- 根据Placement自动调整箭头位置 -->
                    <Trigger Property="Placement" Value="Top">
                        <Setter TargetName="Arrow" Property="Points" Value="5,0 0,6 10,6"/>
                        <Setter TargetName="Arrow" Property="Margin" Value="10,0,0,-6"/>
                        <Setter TargetName="Arrow" Property="VerticalAlignment" Value="Bottom"/>
                    </Trigger>
                    <Trigger Property="Placement" Value="Left">
                        <Setter TargetName="Arrow" Property="Points" Value="0,5 6,0 6,10"/>
                        <Setter TargetName="Arrow" Property="Margin" Value="-6,10,0,0"/>
                        <Setter TargetName="Arrow" Property="HorizontalAlignment" Value="Right"/>
                    </Trigger>
                    <Trigger Property="Placement" Value="Right">
                        <Setter TargetName="Arrow" Property="Points" Value="6,0 0,5 6,10"/>
                        <Setter TargetName="Arrow" Property="Margin" Value="0,10,-6,0"/>
                        <Setter TargetName="Arrow" Property="HorizontalAlignment" Value="Left"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### 4.2 实时数据 ToolTip

工业场景中经常需要显示实时更新的设备状态数据。

csharp:

```c#
// 实时设备状态ToolTip
public class DeviceStatusToolTip : ToolTip
{
    private DispatcherTimer _updateTimer;

    public DeviceStatusToolTip()
    {
        _updateTimer = new DispatcherTimer();
        _updateTimer.Interval = TimeSpan.FromSeconds(1);
        _updateTimer.Tick += UpdateTimer_Tick;
    }

    // 设备ID依赖属性
    public static readonly DependencyProperty DeviceIdProperty = DependencyProperty.Register(
        nameof(DeviceId), typeof(string), typeof(DeviceStatusToolTip),
        new PropertyMetadata(null, OnDeviceIdChanged));

    public string DeviceId
    {
        get => (string)GetValue(DeviceIdProperty);
        set => SetValue(DeviceIdProperty, value);
    }

    private static void OnDeviceIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var toolTip = (DeviceStatusToolTip)d;
        toolTip.UpdateStatus();
    }

    protected override void OnOpened(RoutedEventArgs e)
    {
        base.OnOpened(e);
        _updateTimer.Start();
    }

    protected override void OnClosed(RoutedEventArgs e)
    {
        base.OnClosed(e);
        _updateTimer.Stop();
    }

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (string.IsNullOrEmpty(DeviceId)) return;
        
        // 从PLC读取实时数据
        var device = DeviceManager.GetDevice(DeviceId);
        Content = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = $"设备名称：{device.Name}", FontWeight = FontWeights.Bold },
                new TextBlock { Text = $"运行状态：{device.Status}" },
                new TextBlock { Text = $"当前温度：{device.Temperature:F1} ℃" },
                new TextBlock { Text = $"当前速度：{device.Speed:F1} m/s" },
                new TextBlock { Text = $"运行时间：{device.RunTime:hh\\:mm\\:ss}" }
            }
        };
    }
}
```

**使用方法**：

xaml:

```xaml
<Ellipse Fill="Green" Width="16" Height="16">
    <Ellipse.ToolTip>
        <local:DeviceStatusToolTip DeviceId="PLC-001" Placement="Right"/>
    </Ellipse.ToolTip>
</Ellipse>
```

### 4.3 错误提示 ToolTip（带错误图标和详细信息）

用于参数输入验证和错误提示。

xaml:

```xaml
<Style x:Key="ErrorToolTipStyle" TargetType="ToolTip">
    <Setter Property="Background" Value="#FFFDEDED"/>
    <Setter Property="Foreground" Value="#FFC82333"/>
    <Setter Property="BorderBrush" Value="#FFF5C6CB"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="10,8"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ToolTip">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="3"
                        Padding="{TemplateBinding Padding}">
                    <StackPanel Orientation="Horizontal">
                        <TextBlock Text="⚠️" FontSize="16" Margin="0,0,8,0"/>
                        <ContentPresenter/>
                    </StackPanel>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

**使用方法**：

xaml:

```xaml
<TextBox x:Name="txtTemperature" Text="{Binding Temperature}">
    <TextBox.ToolTip>
        <ToolTip Style="{StaticResource ErrorToolTipStyle}"
                 Content="温度值超出范围（0-100℃）"
                 Placement="Bottom"
                 IsOpen="{Binding IsTemperatureInvalid}"/>
    </TextBox.ToolTip>
</TextBox>
```

------

## 五、常见问题与终极解决方案

### 5.1 ToolTip 不显示的 10 种原因及解决方法

| 问题原因                                        | 解决方案                                   |
| :---------------------------------------------- | :----------------------------------------- |
| 控件`IsEnabled="False"`且未设置`ShowOnDisabled` | 添加`ToolTipService.ShowOnDisabled="True"` |
| `PlacementTarget`未设置                         | 显式设置`PlacementTarget`属性              |
| `IsOpen`被绑定为 false                          | 检查绑定源的值                             |
| `ToolTipService.IsEnabled="False"`              | 确保该属性为 true                          |
| 控件被其他控件遮挡                              | 调整 ZIndex 或使用`Popup`的`Topmost`属性   |
| 自定义模板缺少`PART_Popup`                      | 在模板中添加名为`PART_Popup`的 Popup 控件  |
| 内容为 null                                     | 确保 Content 属性有值                      |
| `InitialShowDelay`设置太长                      | 调整为合适的值（如 200ms）                 |
| `ShowDuration`设置为 0                          | 设置为正数（默认 5000ms）                  |
| 线程问题                                        | 确保在 UI 线程上设置 ToolTip 属性          |

### 5.2 ToolTip 显示位置不正确

- **问题**：ToolTip 显示在屏幕左上角

- **根本原因**：`PlacementTarget`未设置或为 null

- **解决方案**：

  csharp:

  ```c#
  // 手动设置PlacementTarget
  var toolTip = new ToolTip { Content = "提示" };
  toolTip.PlacementTarget = button;
  toolTip.IsOpen = true;
  ```

### 5.3 ToolTip 内存泄漏问题

- **问题**：频繁显示 ToolTip 导致内存泄漏
- **根本原因**：ToolTipService 会缓存 ToolTip 实例
- **解决方案**：
  1. 不要在代码中频繁创建新的 ToolTip 实例
  2. 使用数据模板而不是动态创建 ToolTip
  3. 手动关闭不再需要的 ToolTip：`toolTip.IsOpen = false;`

### 5.4 ToolTip 与其他弹出控件冲突

- **问题**：ToolTip 显示在 ComboBox、Menu 等弹出控件下方

- **根本原因**：Popup 的 ZIndex 顺序问题

- **解决方案**：设置 ToolTip 的`Popup.Topmost="True"`

  xaml:

  ```xaml
  <Style TargetType="ToolTip">
      <Setter Property="Template">
          <Setter.Value>
              <ControlTemplate TargetType="ToolTip">
                  <Popup x:Name="PART_Popup" Topmost="True">
                      <!-- 其他内容 -->
                  </Popup>
              </ControlTemplate>
          </Setter.Value>
      </Setter>
  </Style>
  ```

------

## 六、工业场景最佳实践总结

### 6.1 强制规范

1. **所有禁用控件必须加 ToolTip**：说明禁用原因，如 "设备未连接"、"权限不足"
2. **统一全局样式**：在 App.xaml 中定义全局 ToolTip 样式，确保整个应用风格一致
3. **提示内容规范**：
   - 不超过 3 行，每行不超过 20 个字
   - 参数提示必须包含：范围、单位、精度
   - 操作提示必须说明：后果、注意事项
4. **工业配色**：使用深色背景 (#2D2D30)、白色文字 (#FFFFFF)、高对比度边框 (#3E3E42)
5. **禁用阴影效果**：工业现场光线复杂，阴影会降低可读性

### 6.2 性能优化

1. **避免复杂内容**：ToolTip 中不要包含 DataGrid、Chart 等复杂控件
2. **延迟加载内容**：在`Opened`事件中动态加载内容，而不是预先创建
3. **使用数据模板**：复用相同结构的提示内容，减少内存占用
4. **及时关闭**：设置合理的`ShowDuration`（建议 3-8 秒），避免长时间显示

### 6.3 可访问性

1. **支持键盘访问**：确保所有控件的 ToolTip 可以通过键盘焦点触发
2. **屏幕阅读器支持**：确保提示内容对屏幕阅读器友好
3. **高对比度支持**：在高对比度模式下仍然清晰可见