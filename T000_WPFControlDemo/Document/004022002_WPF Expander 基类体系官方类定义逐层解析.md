# 004022002_WPF Expander 基类体系官方类定义逐层解析

`System.Windows.Controls.Expander` 是 WPF 典型的分层继承控件，它的所有能力并非全部自身实现，而是由下到上 8 层基类逐步累积而来，每一层负责一个维度的底层能力。完整继承链如下：

plaintext:

```tex
System.Windows.Threading.DispatcherObject
  → System.Windows.DependencyObject
    → System.Windows.Media.Visual
      → System.Windows.UIElement
        → System.Windows.FrameworkElement
          → System.Windows.Controls.Control
            → System.Windows.Controls.ContentControl
              → System.Windows.Controls.HeaderedContentControl
                → System.Windows.Controls.Expander
```

下面从最底层基类开始，逐层解析**官方类定义签名、核心职责、关键成员**，以及每一层为 `Expander` 赋予的核心能力。

------

## 第 1 层：DispatcherObject（线程模型基石）

**命名空间**：`System.Windows.Threading`

**官方类定义**：

csharp:

```c#
public abstract class DispatcherObject
{
    protected DispatcherObject();
    
    // 获取当前对象绑定的 UI 线程调度器
    public Dispatcher Dispatcher { get; }
    
    // 检查当前线程是否有权访问该对象
    public bool CheckAccess();
    // 无权限时直接抛出异常，强制线程安全
    public void VerifyAccess();
}
```

### 核心定位

WPF 单线程模型的底层基石：所有 WPF 对象都必须绑定到创建它的 UI 线程（Dispatcher 线程），禁止跨线程直接操作 UI 对象。

### 对 Expander 的意义

Expander 作为标准 WPF 控件，天然遵循线程安全规则：

- 展开 / 折叠状态修改、内容更新等所有 UI 操作，必须在 UI 线程执行
- 工业场景中后台线程读取 PLC 数据后，必须通过 `Dispatcher.Invoke` 才能修改 `Expander.IsExpanded` 或内部内容

------

## 第 2 层：DependencyObject（属性系统核心）

**命名空间**：`System.Windows`

**官方类定义**：

csharp:

```c#
public abstract class DependencyObject : DispatcherObject
{
    public DependencyObject();
    
    // 对象是否已密封（密封后不可修改依赖属性）
    public bool IsSealed { get; }
    
    // 依赖属性核心读写方法
    public object GetValue(DependencyProperty dp);
    public void SetValue(DependencyProperty dp, object value);
    // 清除本地赋值，恢复为默认/样式/继承值
    public void ClearValue(DependencyProperty dp);
    // 读取本地赋值（不包含样式、默认值）
    public object ReadLocalValue(DependencyProperty dp);
    
    public LocalValueEnumerator GetLocalValueEnumerator();
    public void CoerceValue(DependencyProperty dp);
    public void InvalidateProperty(DependencyProperty dp);
}
```

### 核心定位

WPF 依赖属性系统的底层载体，替代传统 CLR 属性，支持数据绑定、样式、动画、属性值继承、默认值回调等高级特性，是 WPF 控件的核心基础设施。

### 对 Expander 的意义

Expander 的所有核心属性（`IsExpanded`、`ExpandDirection`、`Header`、`Content`、`Background` 等）都是依赖属性，全部基于这一层实现：

- 支持 `IsExpanded` 双向绑定到 ViewModel
- 支持样式触发器控制展开 / 折叠状态
- 支持展开 / 折叠的平滑过渡动画

------

## 第 3 层：Visual（可视化树最小单元）

**命名空间**：`System.Windows.Media`

**官方类定义**：

csharp:

```c#
public abstract class Visual : DependencyObject
{
    protected Visual();
    
    // 可视化子元素数量
    protected virtual int VisualChildrenCount { get; }
    // 获取指定索引的可视化子元素
    protected virtual Visual GetVisualChild(int index);
    
    // 命中测试底层实现
    protected internal virtual HitTestResult HitTestCore(PointHitTestParameters hitTestParameters);
    
    // 受保护的渲染属性：VisualOffset、VisualTransform、VisualClip、VisualOpacity 等
}
```

### 核心定位

WPF 可视化树的最小单位，负责低级别渲染、坐标变换、区域裁剪、命中测试，是所有能显示在界面上的对象的基础。绝大多数成员为 `protected`，仅供子类内部使用。

### 对 Expander 的意义

Expander 的标题箭头、边框、内容区域等视觉元素，都是作为 Visual 子元素挂载在可视化树上；点击标题栏触发展开 / 折叠的交互，底层也依赖 Visual 层的命中测试能力。

------

## 第 4 层：UIElement（核心交互基类）

**命名空间**：`System.Windows`

**官方类定义（核心签名）**：

csharp:

```c#
public class UIElement : Visual, IInputElement
{
    // 核心依赖属性
    public static readonly DependencyProperty VisibilityProperty;
    public static readonly DependencyProperty IsEnabledProperty;
    public static readonly DependencyProperty IsFocusedProperty;
    
    public Visibility Visibility { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsFocused { get; }
    public bool IsMouseOver { get; }
    public bool IsKeyboardFocusWithin { get; }
    
    // 路由事件（鼠标、键盘等输入事件）
    public event MouseButtonEventHandler MouseDown;
    public event KeyEventHandler KeyDown;
    public event RoutedEventHandler GotFocus;
    
    // 布局系统两步核心方法
    public void Measure(Size availableSize);
    public void Arrange(Rect finalRect);
    
    // 焦点控制
    public bool Focus();
    public void UpdateLayout();
}
```

### 核心定位

WPF 所有可交互 UI 元素的核心基类，提供**布局系统入口、输入事件体系、焦点管理、路由事件、可见性控制**五大核心能力。

### 对 Expander 的意义

- 支持禁用（`IsEnabled="False"`）、隐藏（`Visibility="Collapsed"`）等状态控制
- 鼠标点击标题、键盘快捷键操作等交互能力，全部来自 UIElement 的输入事件体系
- `Expanded` / `Collapsed` 状态事件，也是基于 UIElement 的路由事件机制实现的冒泡事件
- 展开 / 折叠时的尺寸变化，依赖 UIElement 的 Measure/Arrange 布局流程

------

## 第 5 层：FrameworkElement（框架级扩展）

**命名空间**：`System.Windows`

**官方类定义（核心签名）**：

csharp:

```c#
public class FrameworkElement : UIElement
{
    // 布局属性
    public static readonly DependencyProperty WidthProperty;
    public static readonly DependencyProperty HeightProperty;
    public static readonly DependencyProperty MarginProperty;
    public static readonly DependencyProperty HorizontalAlignmentProperty;
    public static readonly DependencyProperty VerticalAlignmentProperty;
    
    // 框架特性属性
    public static readonly DependencyProperty StyleProperty;
    public static readonly DependencyProperty DataContextProperty;
    public static readonly DependencyProperty NameProperty;
    
    public double Width { get; set; }
    public double Height { get; set; }
    public Thickness Margin { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public Style Style { get; set; }
    public object DataContext { get; set; }
    public string Name { get; set; }
    public ResourceDictionary Resources { get; set; }
    
    // 自定义布局重写入口
    protected virtual Size MeasureOverride(Size availableSize);
    protected virtual Size ArrangeOverride(Size finalSize);
    
    // 控件模板加载完成回调
    public virtual void OnApplyTemplate();
    // 按名称查找子元素
    public object FindName(string name);
}
```

### 核心定位

WPF 框架级功能扩展，在 UIElement 基础上增加了**常用布局属性、样式体系、数据绑定、资源管理、命名作用域、模板生命周期**等高级框架特性，是绝大多数 WPF 控件的实际功能基类。

### 对 Expander 的意义

- 日常开发中设置的 `Width`、`Margin`、`HorizontalAlignment` 等布局属性，全部继承自 FrameworkElement
- 支持 Style 统一样式、DataContext 数据绑定，是 MVVM 开发的核心基础
- 自定义 Expander 外观时，`OnApplyTemplate()` 是获取模板内部子元素的核心入口

------

## 第 6 层：Control（标准控件基类）

**命名空间**：`System.Windows.Controls`

**官方类定义（核心签名）**：

csharp:

```c#
public class Control : FrameworkElement
{
    // 外观属性
    public static readonly DependencyProperty BackgroundProperty;
    public static readonly DependencyProperty ForegroundProperty;
    public static readonly DependencyProperty BorderBrushProperty;
    public static readonly DependencyProperty BorderThicknessProperty;
    
    // 文本属性
    public static readonly DependencyProperty FontFamilyProperty;
    public static readonly DependencyProperty FontSizeProperty;
    public static readonly DependencyProperty FontWeightProperty;
    
    // 控件模板核心
    public static readonly DependencyProperty TemplateProperty;
    
    // Tab 导航属性
    public static readonly DependencyProperty TabIndexProperty;
    public static readonly DependencyProperty IsTabStopProperty;

    public Brush Background { get; set; }
    public Brush Foreground { get; set; }
    public Thickness BorderThickness { get; set; }
    public Brush BorderBrush { get; set; }
    public FontFamily FontFamily { get; set; }
    public double FontSize { get; set; }
    public ControlTemplate Template { get; set; }
    public int TabIndex { get; set; }
    public bool IsTabStop { get; set; }

    protected virtual void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate);
}
```

### 核心定位

所有标准可交互控件的基类，定义了控件通用的外观属性、文本属性、控件模板、键盘导航等通用能力，是 WPF「逻辑与外观分离」设计的核心载体。

### 对 Expander 的意义

- 直接给 Expander 设置背景色、边框、字体大小等外观，属性全部来自 Control 基类
- 完全重写 Expander 的箭头样式、标题栏外观，本质就是重写 `Template` 控件模板
- 支持 Tab 键聚焦切换，符合工业软件键盘操作的交互习惯

------

## 第 7 层：ContentControl（单内容模型）

**命名空间**：`System.Windows.Controls`

**官方类定义（核心签名）**：

csharp:

```c#
public class ContentControl : Control
{
    public static readonly DependencyProperty ContentProperty;
    public static readonly DependencyProperty ContentTemplateProperty;
    public static readonly DependencyProperty ContentTemplateSelectorProperty;
    public static readonly DependencyProperty ContentStringFormatProperty;

    public object Content { get; set; }
    public DataTemplate ContentTemplate { get; set; }
    public DataTemplateSelector ContentTemplateSelector { get; set; }
    public string ContentStringFormat { get; set; }
}
```

### 核心定位

单内容模型控件的基类，代表「包含一个任意内容的控件」。`Content` 属性类型为 `object`，支持文本、图片、布局面板、其他控件等任意 WPF 元素。`Button`、`Label`、`CheckBox` 等常用控件均继承自该类。

### 对 Expander 的意义

Expander 折叠区域的主体内容，完全继承自 ContentControl 的 `Content` 属性。这也是为什么 Expander 内部可以放置 StackPanel、DataGrid、图表等任意内容 —— 本质是复用了 WPF 的单内容模型。

------

## 第 8 层：HeaderedContentControl（双内容模型）

**命名空间**：`System.Windows.Controls`

**官方类定义（核心签名）**：

csharp:

```c#
public class HeaderedContentControl : ContentControl
{
    public static readonly DependencyProperty HeaderProperty;
    public static readonly DependencyProperty HeaderTemplateProperty;
    public static readonly DependencyProperty HeaderTemplateSelectorProperty;
    public static readonly DependencyProperty HeaderStringFormatProperty;

    public object Header { get; set; }
    public DataTemplate HeaderTemplate { get; set; }
    public DataTemplateSelector HeaderTemplateSelector { get; set; }
    public string HeaderStringFormat { get; set; }
}
```

### 核心定位

带标题的内容控件基类，在 ContentControl 单内容基础上，新增了 `Header` 标题内容，形成「标题 + 主体」的双内容结构。`GroupBox`、`TabItem` 等控件也继承自该类。

### 对 Expander 的意义

这是 **Expander 最直接的父类**，Expander 的核心结构「标题栏（箭头 + 标题文字） + 折叠内容区」完全复用了该基类的双内容模型：

- `Header` 对应 Expander 的标题栏部分
- `Content` 对应 Expander 折叠展开的内容区域

Expander 自身没有重新定义标题和内容的存储逻辑，仅在该基类基础上增加了展开 / 折叠的交互逻辑。

------

## 总结：基类能力叠加与 Expander 自身扩展

8 层基类为 Expander 提供了完整的底层基础设施，Expander 自身仅在 `HeaderedContentControl` 基础上扩展了 3 个核心特性：

1. **状态属性**：`IsExpanded` 依赖属性，控制内容的展开 / 折叠状态
2. **方向属性**：`ExpandDirection` 依赖属性，控制内容展开的方向（上 / 下 / 左 / 右）
3. **状态事件**：`Expanded` / `Collapsed` 路由事件，状态变更时通知外部