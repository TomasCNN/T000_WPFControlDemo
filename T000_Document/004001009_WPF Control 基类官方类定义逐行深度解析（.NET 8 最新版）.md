# 004001009_WPF Control 基类官方类定义逐行深度解析（.NET 8 最新版）

基于 **.NET 8 官方开源源码** 完整解析，包含所有公共 / 受保护成员、特性、设计意图和工业场景应用。`Control`是 WPF**所有用户交互控件的最底层基类**，定义了所有控件共有的外观属性、字体属性、布局属性、模板系统和视觉状态管理，是`ContentControl`、`ItemsControl`、`Panel`等几乎所有 WPF 控件的共同父类。

------

## 一、Control 在 WPF 类层次结构中的位置

plaintext:

```tex
System.Object
  ↳ System.Windows.Threading.DispatcherObject
    ↳ System.Windows.DependencyObject
      ↳ System.Windows.Media.Visual
        ↳ System.Windows.UIElement
          ↳ System.Windows.FrameworkElement
            ↳ System.Windows.Controls.Control  ← 所有控件的最底层基类
              ↳ System.Windows.Controls.ContentControl
              ↳ System.Windows.Controls.ItemsControl
              ↳ System.Windows.Controls.Panel
              ↳ System.Windows.Controls.TextBoxBase
              ↳ System.Windows.Controls.Primitives.RangeBase
```

**核心设计意义**：

- 统一所有控件的基础属性模型（背景、前景、边框、字体、对齐方式等）
- 实现**控件逻辑与外观的完全分离**（通过`Template`属性）
- 引入**视觉状态管理系统**，统一控件的状态切换逻辑
- 提供键盘导航、焦点管理、Tab 键顺序等基础交互能力
- 作为所有自定义控件的最终基类，提供可扩展的控件开发框架

------

## 二、完整官方类定义（.NET 8 源码级）

csharp:

```c#
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls
{
    /// <summary>
    /// 表示所有用户交互控件的基类
    /// </summary>
    /// <remarks>
    /// Control 是一个抽象类，不能直接实例化。
    /// 它定义了所有控件共有的属性、事件和方法，包括外观属性、字体属性、模板系统和视觉状态管理。
    /// </remarks>
    [DefaultProperty("Template")]
    [Localizability(LocalizationCategory.Control)]
    public abstract class Control : FrameworkElement
    {
        // ==============================================
        // 依赖属性定义（所有控件共有）
        // ==============================================
        public static readonly DependencyProperty BackgroundProperty;
        public static readonly DependencyProperty BorderBrushProperty;
        public static readonly DependencyProperty BorderThicknessProperty;
        public static readonly DependencyProperty ForegroundProperty;
        public static readonly DependencyProperty FontFamilyProperty;
        public static readonly DependencyProperty FontSizeProperty;
        public static readonly DependencyProperty FontWeightProperty;
        public static readonly DependencyProperty FontStyleProperty;
        public static readonly DependencyProperty FontStretchProperty;
        public static readonly DependencyProperty HorizontalContentAlignmentProperty;
        public static readonly DependencyProperty VerticalContentAlignmentProperty;
        public static readonly DependencyProperty PaddingProperty;
        public static readonly DependencyProperty TemplateProperty;
        public static readonly DependencyProperty IsTabStopProperty;
        public static readonly DependencyProperty TabIndexProperty;
        public static readonly DependencyProperty TabNavigationProperty;
        public static readonly DependencyProperty FocusVisualStyleProperty;

        // ==============================================
        // 路由事件定义（所有控件共有）
        // ==============================================
        public static readonly RoutedEvent MouseDoubleClickEvent;
        public static readonly RoutedEvent PreviewMouseDoubleClickEvent;
        public static readonly RoutedEvent GotFocusEvent;
        public static readonly RoutedEvent LostFocusEvent;
        public static readonly RoutedEvent IsKeyboardFocusWithinChangedEvent;

        // ==============================================
        // 静态构造函数
        // ==============================================
        static Control()
        {
            // 注册外观属性
            BackgroundProperty = DependencyProperty.Register(
                nameof(Background),
                typeof(Brush),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    Brushes.Transparent,
                    FrameworkPropertyMetadataOptions.AffectsRender));

            BorderBrushProperty = DependencyProperty.Register(
                nameof(BorderBrush),
                typeof(Brush),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    Brushes.Transparent,
                    FrameworkPropertyMetadataOptions.AffectsRender));

            BorderThicknessProperty = DependencyProperty.Register(
                nameof(BorderThickness),
                typeof(Thickness),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    new Thickness(0),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender));

            // 注册字体属性
            ForegroundProperty = DependencyProperty.Register(
                nameof(Foreground),
                typeof(Brush),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits));

            FontFamilyProperty = DependencyProperty.Register(
                nameof(FontFamily),
                typeof(FontFamily),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    SystemFonts.MessageFontFamily,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits));

            FontSizeProperty = DependencyProperty.Register(
                nameof(FontSize),
                typeof(double),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    SystemFonts.MessageFontSize,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits),
                new ValidateValueCallback(IsValidFontSize));

            FontWeightProperty = DependencyProperty.Register(
                nameof(FontWeight),
                typeof(FontWeight),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    FontWeights.Normal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits));

            FontStyleProperty = DependencyProperty.Register(
                nameof(FontStyle),
                typeof(FontStyle),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    FontStyles.Normal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits));

            FontStretchProperty = DependencyProperty.Register(
                nameof(FontStretch),
                typeof(FontStretch),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    FontStretches.Normal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.Inherits));

            // 注册内容对齐属性
            HorizontalContentAlignmentProperty = DependencyProperty.Register(
                nameof(HorizontalContentAlignment),
                typeof(HorizontalAlignment),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    HorizontalAlignment.Left,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

            VerticalContentAlignmentProperty = DependencyProperty.Register(
                nameof(VerticalContentAlignment),
                typeof(VerticalAlignment),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    VerticalAlignment.Top,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

            PaddingProperty = DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    new Thickness(0),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

            // 注册模板属性（核心）
            TemplateProperty = DependencyProperty.Register(
                nameof(Template),
                typeof(ControlTemplate),
                typeof(Control),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    new PropertyChangedCallback(OnTemplateChanged)));

            // 注册键盘导航属性
            IsTabStopProperty = DependencyProperty.Register(
                nameof(IsTabStop),
                typeof(bool),
                typeof(Control),
                new FrameworkPropertyMetadata(true));

            TabIndexProperty = DependencyProperty.Register(
                nameof(TabIndex),
                typeof(int),
                typeof(Control),
                new FrameworkPropertyMetadata(Int32.MaxValue));

            TabNavigationProperty = DependencyProperty.Register(
                nameof(TabNavigation),
                typeof(KeyboardNavigationMode),
                typeof(Control),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));

            // 注册焦点样式属性
            FocusVisualStyleProperty = DependencyProperty.Register(
                nameof(FocusVisualStyle),
                typeof(Style),
                typeof(Control),
                new FrameworkPropertyMetadata(null));

            // 注册路由事件
            MouseDoubleClickEvent = EventManager.RegisterRoutedEvent(
                nameof(MouseDoubleClick),
                RoutingStrategy.Bubble,
                typeof(MouseButtonEventHandler),
                typeof(Control));

            PreviewMouseDoubleClickEvent = EventManager.RegisterRoutedEvent(
                nameof(PreviewMouseDoubleClick),
                RoutingStrategy.Tunnel,
                typeof(MouseButtonEventHandler),
                typeof(Control));

            // 重写默认样式键
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Control),
                new FrameworkPropertyMetadata(typeof(Control)));
        }

        // ==============================================
        // 受保护构造函数（抽象类不能直接实例化）
        // ==============================================
        protected Control();

        // ==============================================
        // 公共属性
        // ==============================================
        [Bindable(true)]
        [Category("Appearance")]
        public Brush Background { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public Brush BorderBrush { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public Thickness BorderThickness { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public Brush Foreground { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(FontSizeConverter))]
        [Localizability(LocalizationCategory.None)]
        public double FontSize { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public FontWeight FontWeight { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public FontStyle FontStyle { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public FontStretch FontStretch { get; set; }

        [Bindable(true)]
        [Category("Layout")]
        public HorizontalAlignment HorizontalContentAlignment { get; set; }

        [Bindable(true)]
        [Category("Layout")]
        public VerticalAlignment VerticalContentAlignment { get; set; }

        [Bindable(true)]
        [Category("Layout")]
        public Thickness Padding { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public ControlTemplate Template { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public bool IsTabStop { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public int TabIndex { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public KeyboardNavigationMode TabNavigation { get; set; }

        [Bindable(true)]
        [Category("Appearance")]
        public Style FocusVisualStyle { get; set; }

        // ==============================================
        // 公共事件
        // ==============================================
        public event MouseButtonEventHandler MouseDoubleClick
        {
            add => AddHandler(MouseDoubleClickEvent, value);
            remove => RemoveHandler(MouseDoubleClickEvent, value);
        }

        public event MouseButtonEventHandler PreviewMouseDoubleClick
        {
            add => AddHandler(PreviewMouseDoubleClickEvent, value);
            remove => RemoveHandler(PreviewMouseDoubleClickEvent, value);
        }

        public event RoutedEventHandler GotFocus
        {
            add => AddHandler(GotFocusEvent, value);
            remove => RemoveHandler(GotFocusEvent, value);
        }

        public event RoutedEventHandler LostFocus
        {
            add => AddHandler(LostFocusEvent, value);
            remove => RemoveHandler(LostFocusEvent, value);
        }

        public event DependencyPropertyChangedEventHandler IsKeyboardFocusWithinChanged
        {
            add => AddHandler(IsKeyboardFocusWithinChangedEvent, value);
            remove => RemoveHandler(IsKeyboardFocusWithinChangedEvent, value);
        }

        // ==============================================
        // 受保护方法（自定义控件必备）
        // ==============================================
        protected override AutomationPeer OnCreateAutomationPeer();
        public override void OnApplyTemplate();
        protected virtual void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate);
        protected virtual DependencyObject GetTemplateChild(string childName);
        protected override void OnGotFocus(RoutedEventArgs e);
        protected override void OnLostFocus(RoutedEventArgs e);
        protected virtual void OnMouseDoubleClick(MouseButtonEventArgs e);
        protected virtual void OnPreviewMouseDoubleClick(MouseButtonEventArgs e);
    }
}
```

------

## 三、类级特性逐行解析

### 1. `[DefaultProperty("Template")]`

csharp:

```c#
[DefaultProperty("Template")]
```

- **作用**：指定控件的默认属性
- **设计意图**：在 XAML 中可以直接编写控件模板，而不需要显式指定`Template`属性名
- **核心意义**：体现了 WPF"外观与逻辑分离" 的设计哲学，模板是控件最重要的属性

### 2. `[Localizability(LocalizationCategory.Control)]`

csharp:

```c#
[Localizability(LocalizationCategory.Control)]
```

- **作用**：本地化特性，告诉本地化工具该类属于控件类别
- **设计意图**：本地化工具会自动处理控件的所有可本地化属性（如`Content`、`Header`等）

------

## 四、静态构造函数解析（核心初始化逻辑）

静态构造函数是 Control 最关键的部分，负责所有依赖属性和路由事件的注册。我将按功能分组解析最重要的属性。

### 4.1 外观属性组

这些属性控制控件的基本外观，所有控件都继承这些属性。

#### 1. `BackgroundProperty`

csharp:

```c#
BackgroundProperty = DependencyProperty.Register(
    nameof(Background),
    typeof(Brush),
    typeof(Control),
    new FrameworkPropertyMetadata(
        Brushes.Transparent,
        FrameworkPropertyMetadataOptions.AffectsRender));
```

- **类型**：`Brush`
- **默认值**：`Brushes.Transparent`（透明）
- **元数据标志**：`AffectsRender`（属性变化会影响渲染）
- **作用**：设置控件的背景色

#### 2. `BorderBrushProperty` 和 `BorderThicknessProperty`

csharp:

```c#
BorderBrushProperty = DependencyProperty.Register(
    nameof(BorderBrush),
    typeof(Brush),
    typeof(Control),
    new FrameworkPropertyMetadata(
        Brushes.Transparent,
        FrameworkPropertyMetadataOptions.AffectsRender));

BorderThicknessProperty = DependencyProperty.Register(
    nameof(BorderThickness),
    typeof(Thickness),
    typeof(Control),
    new FrameworkPropertyMetadata(
        new Thickness(0),
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsRender));
```

- **作用**：设置控件的边框颜色和边框厚度
- **工业场景应用**：为按钮、输入框等控件添加边框，区分不同的功能区域

### 4.2 字体属性组

所有字体属性都带有`Inherits`标志，这意味着子控件会继承父控件的字体设置，这是 WPF 非常重要的特性。

#### 1. `ForegroundProperty`

csharp:

```c#
ForegroundProperty = DependencyProperty.Register(
    nameof(Foreground),
    typeof(Brush),
    typeof(Control),
    new FrameworkPropertyMetadata(
        Brushes.Black,
        FrameworkPropertyMetadataOptions.AffectsRender |
        FrameworkPropertyMetadataOptions.Inherits));
```

- **类型**：`Brush`
- **默认值**：`Brushes.Black`
- **元数据标志**：`Inherits`（子控件继承）
- **作用**：设置控件的文本颜色

#### 2. `FontSizeProperty`

csharp:

```c#
FontSizeProperty = DependencyProperty.Register(
    nameof(FontSize),
    typeof(double),
    typeof(Control),
    new FrameworkPropertyMetadata(
        SystemFonts.MessageFontSize,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsRender |
        FrameworkPropertyMetadataOptions.Inherits),
    new ValidateValueCallback(IsValidFontSize));
```

- **类型**：`double`
- **默认值**：系统消息字体大小
- **验证回调**：`IsValidFontSize`，确保字体大小大于 0
- **工业场景应用**：在窗口级别设置统一的字体大小，所有子控件自动继承

### 4.3 内容对齐与内边距属性

csharp:

```c#
HorizontalContentAlignmentProperty = DependencyProperty.Register(
    nameof(HorizontalContentAlignment),
    typeof(HorizontalAlignment),
    typeof(Control),
    new FrameworkPropertyMetadata(
        HorizontalAlignment.Left,
        FrameworkPropertyMetadataOptions.AffectsArrange));

VerticalContentAlignmentProperty = DependencyProperty.Register(
    nameof(VerticalContentAlignment),
    typeof(VerticalAlignment),
    typeof(Control),
    new FrameworkPropertyMetadata(
        VerticalAlignment.Top,
        FrameworkPropertyMetadataOptions.AffectsArrange));

PaddingProperty = DependencyProperty.Register(
    nameof(Padding),
    typeof(Thickness),
    typeof(Control),
    new FrameworkPropertyMetadata(
        new Thickness(0),
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsArrange));
```

- **`HorizontalContentAlignment`/`VerticalContentAlignment`**：设置控件内容的水平和垂直对齐方式
- **`Padding`**：设置控件内容与边框之间的内边距
- **工业场景应用**：统一设置按钮的内边距和内容对齐方式，确保界面美观

### 4.4 核心属性：`TemplateProperty`（控件模板）

csharp:

```c#
TemplateProperty = DependencyProperty.Register(
    nameof(Template),
    typeof(ControlTemplate),
    typeof(Control),
    new FrameworkPropertyMetadata(
        null,
        FrameworkPropertyMetadataOptions.AffectsMeasure |
        FrameworkPropertyMetadataOptions.AffectsArrange,
        new PropertyChangedCallback(OnTemplateChanged)));
```

- **类型**：`ControlTemplate`
- **默认值**：`null`
- **元数据标志**：`AffectsMeasure`和`AffectsArrange`（模板变化会影响控件的测量和排列）
- **属性变更回调**：`OnTemplateChanged`，当模板变化时调用
- **核心设计意义**：实现了**控件逻辑与外观的完全分离**，这是 WPF 与传统 WinForms 最本质的区别之一

### 4.5 键盘导航与焦点属性

csharp:

```c#
IsTabStopProperty = DependencyProperty.Register(
    nameof(IsTabStop),
    typeof(bool),
    typeof(Control),
    new FrameworkPropertyMetadata(true));

TabIndexProperty = DependencyProperty.Register(
    nameof(TabIndex),
    typeof(int),
    typeof(Control),
    new FrameworkPropertyMetadata(Int32.MaxValue));

TabNavigationProperty = DependencyProperty.Register(
    nameof(TabNavigation),
    typeof(KeyboardNavigationMode),
    typeof(Control),
    new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));

FocusVisualStyleProperty = DependencyProperty.Register(
    nameof(FocusVisualStyle),
    typeof(Style),
    typeof(Control),
    new FrameworkPropertyMetadata(null));
```

- **`IsTabStop`**：指示控件是否可以通过 Tab 键获得焦点
- **`TabIndex`**：设置 Tab 键导航的顺序
- **`TabNavigation`**：设置控件内部的 Tab 导航模式
- **`FocusVisualStyle`**：设置控件获得焦点时的视觉样式
- **工业场景应用**：为工业界面设置合理的 Tab 键顺序，方便操作人员使用键盘操作

### 4.6 路由事件注册

csharp:

```c#
MouseDoubleClickEvent = EventManager.RegisterRoutedEvent(
    nameof(MouseDoubleClick),
    RoutingStrategy.Bubble,
    typeof(MouseButtonEventHandler),
    typeof(Control));

PreviewMouseDoubleClickEvent = EventManager.RegisterRoutedEvent(
    nameof(PreviewMouseDoubleClick),
    RoutingStrategy.Tunnel,
    typeof(MouseButtonEventHandler),
    typeof(Control));
```

- **`MouseDoubleClickEvent`**：鼠标双击事件，冒泡路由
- **`PreviewMouseDoubleClickEvent`**：鼠标双击预览事件，隧道路由
- **工业场景应用**：处理表格行双击、设备图标双击等操作

------

## 五、核心依赖属性逐行解析

### 1. `Template` 属性（灵魂属性）

csharp:

```c#
[Bindable(true)]
[Category("Appearance")]
public ControlTemplate Template { get; set; }
```

#### 逐句解析：

- **`[Category("Appearance")]`**：在属性窗口中归类到 "外观" 组
- **类型**：`ControlTemplate`（控件模板）
- **核心作用**：定义控件的视觉结构和外观
- **设计意图**：将控件的逻辑与外观完全分离，同一逻辑可以有多种不同的外观
- **工业场景意义**：可以为工业界面设计统一的控件样式，确保所有设备的界面风格一致

#### 示例：自定义工业按钮模板

xaml:

```xaml
<Style TargetType="Button">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="Border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4">
                    <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                      VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                                      Margin="{TemplateBinding Padding}"/>
                </Border>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="Border" Property="Background" Value="#E0E0E0"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### 2. 字体属性（继承特性）

所有字体属性都带有`Inherits`标志，这意味着：

- 子控件会自动继承父控件的字体设置
- 在窗口级别设置一次字体，所有子控件都会自动应用
- 可以在子控件级别覆盖父控件的字体设置

#### 示例：全局字体设置

xaml:

```xaml
<Window x:Class="IndustrialVisionTemplate.MainWindow"
        FontFamily="Microsoft YaHei"
        FontSize="14">
    <!-- 窗口内所有控件都会继承这个字体设置 -->
    <Grid>
        <Button Content="启动设备"/>
        <Label Content="设备状态"/>
        <TextBox Text="1234"/>
    </Grid>
</Window>
```

### 3. `Padding` 属性

csharp:

```xaml
[Bindable(true)]
[Category("Layout")]
public Thickness Padding { get; set; }
```

- **作用**：设置控件内容与边框之间的内边距

- **与`Margin`的区别**：

  - `Margin`：控件与其他控件之间的外边距
  - `Padding`：控件内部内容与边框之间的内边距

  

- **工业场景应用**：为按钮、输入框等控件设置合适的内边距，提高界面的美观性和易用性

------

## 六、核心路由事件逐行解析

### 1. `MouseDoubleClickEvent` 和 `PreviewMouseDoubleClickEvent`

csharp:

```c#
public event MouseButtonEventHandler MouseDoubleClick;
public event MouseButtonEventHandler PreviewMouseDoubleClick;
```

- **触发时机**：当用户双击鼠标左键时触发

- **路由策略**：

  - `PreviewMouseDoubleClick`：隧道路由（从根元素向下传播到触发元素）
  - `MouseDoubleClick`：冒泡路由（从触发元素向上传播到根元素）

  

- **工业场景应用**：

  - 双击表格行查看详细信息
  - 双击设备图标打开设备详情窗口
  - 双击生产记录修改数据

  

### 2. `GotFocusEvent` 和 `LostFocusEvent`

csharp:

```c#
public event RoutedEventHandler GotFocus;
public event RoutedEventHandler LostFocus;
```

- **触发时机**：

  - `GotFocus`：当控件获得键盘焦点时触发
  - `LostFocus`：当控件失去键盘焦点时触发

  

- **工业场景应用**：

  - 输入框获得焦点时自动选中所有文本
  - 输入框失去焦点时验证输入数据
  - 控件获得焦点时显示帮助信息

  

------

## 七、受保护方法逐行解析（自定义控件必备）

这些方法是开发自定义 WPF 控件时必须掌握的核心方法。

### 1. `OnApplyTemplate()` 方法（最重要）

csharp:

```c#
public override void OnApplyTemplate();
```

- **触发时机**：当控件模板被应用到控件时调用

- **核心作用**：获取模板中的命名元素，初始化事件处理程序

- **自定义注意事项**：

  - 重写时必须调用`base.OnApplyTemplate()`
  - 使用`GetTemplateChild()`方法获取模板中的元素
  - 总是检查元素是否为 null，因为模板可能被用户替换

  

#### 示例：自定义控件中的 OnApplyTemplate

csharp:

```c#
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    
    // 获取模板中的按钮元素
    _startButton = GetTemplateChild("PART_StartButton") as Button;
    
    if (_startButton != null)
    {
        // 注册按钮点击事件
        _startButton.Click += StartButton_Click;
    }
}
```

### 2. `GetTemplateChild()` 方法

csharp:

```c#
protected virtual DependencyObject GetTemplateChild(string childName);
```

- **作用**：根据名称获取控件模板中的子元素
- **参数**：`childName` - 模板中元素的`x:Name`
- **返回值**：找到的元素，如果找不到则返回 null
- **命名约定**：模板中的命名元素通常以`PART_`开头，如`PART_StartButton`、`PART_ContentPresenter`

### 3. `OnTemplateChanged()` 方法

csharp:

```c#
protected virtual void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate);
```

- **触发时机**：当`Template`属性的值发生变化时调用
- **默认实现**：调用`OnApplyTemplate()`方法应用新模板
- **自定义注意事项**：重写时必须调用基类方法

### 4. `OnMouseDoubleClick()` 和 `OnPreviewMouseDoubleClick()` 方法

csharp:

```c#
protected virtual void OnMouseDoubleClick(MouseButtonEventArgs e);
protected virtual void OnPreviewMouseDoubleClick(MouseButtonEventArgs e);
```

- **触发时机**：当鼠标双击事件发生时调用
- **默认实现**：触发对应的路由事件
- **自定义注意事项**：重写时必须调用基类方法，否则路由事件不会触发

------

## 八、Control 核心工作原理

### 8.1 控件模板应用流程

当 Control 的 Template 属性被设置时，WPF 会按照以下流程应用模板：

1. 调用`OnTemplateChanged()`方法
2. 卸载旧模板中的所有元素
3. 加载新模板的可视化树
4. 调用`OnApplyTemplate()`方法
5. 开发者在`OnApplyTemplate()`中获取模板中的命名元素并注册事件
6. 控件开始使用新模板进行渲染

### 8.2 视觉状态管理

Control 引入了**视觉状态管理系统**，这是 WPF 控件的重要特性。视觉状态定义了控件在不同状态下的外观，如正常、悬停、按下、禁用等。

#### 示例：按钮的视觉状态

xaml:

```xaml
<ControlTemplate TargetType="Button">
    <Border x:Name="Border">
        <ContentPresenter/>
    </Border>
    
    <VisualStateManager.VisualStateGroups>
        <VisualStateGroup x:Name="CommonStates">
            <VisualState x:Name="Normal"/>
            <VisualState x:Name="MouseOver">
                <Storyboard>
                    <ColorAnimation Storyboard.TargetName="Border"
                                    Storyboard.TargetProperty="Background.Color"
                                    To="#E0E0E0" Duration="0:0:0.1"/>
                </Storyboard>
            </VisualState>
            <VisualState x:Name="Pressed">
                <Storyboard>
                    <ColorAnimation Storyboard.TargetName="Border"
                                    Storyboard.TargetProperty="Background.Color"
                                    To="#BDBDBD" Duration="0:0:0.1"/>
                </Storyboard>
            </VisualState>
            <VisualState x:Name="Disabled">
                <Storyboard>
                    <ColorAnimation Storyboard.TargetName="Border"
                                    Storyboard.TargetProperty="Background.Color"
                                    To="#F5F5F5" Duration="0:0:0.1"/>
                </Storyboard>
            </VisualState>
        </VisualStateGroup>
    </VisualStateManager.VisualStateGroups>
</ControlTemplate>
```

------

## 九、派生类实现原理

所有 WPF 控件都继承自 Control，只需要重写少量方法即可实现自己的特殊行为：

- **`ContentControl`**：继承自 Control，增加了`Content`属性和内容呈现逻辑
- **`ItemsControl`**：继承自 Control，增加了`ItemsSource`属性和集合呈现逻辑
- **`TextBoxBase`**：继承自 Control，增加了文本编辑逻辑
- **`RangeBase`**：继承自 Control，增加了范围值逻辑（如 Slider、ProgressBar）

------

## 十、工业上位机典型应用实例

### 实例 1：全局工业控件样式

xaml:

```xaml
<!-- App.xaml 中的全局样式 -->
<Application.Resources>
    <!-- 全局按钮样式 -->
    <Style TargetType="Button">
        <Setter Property="MinWidth" Value="100"/>
        <Setter Property="MinHeight" Value="40"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Padding" Value="12,6"/>
        <Setter Property="HorizontalContentAlignment" Value="Center"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
        <Setter Property="Background" Value="#F5F5F5"/>
        <Setter Property="BorderBrush" Value="#E0E0E0"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Template">
            <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border x:Name="Border"
                        Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4">
                    <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                      VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                                      Margin="{TemplateBinding Padding}"/>
                </Border>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter TargetName="Border" Property="Background" Value="#E0E0E0"/>
                    </Trigger>
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter TargetName="Border" Property="Background" Value="#BDBDBD"/>
                        <Setter Property="Foreground" Value="#757575"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- 全局输入框样式 -->
    <Style TargetType="TextBox">
        <Setter Property="MinHeight" Value="32"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="Padding" Value="5,3"/>
        <Setter Property="BorderBrush" Value="#E0E0E0"/>
        <Setter Property="BorderThickness" Value="1"/>
    </Style>
</Application.Resources>
```

### 实例 2：自定义工业状态指示灯控件

csharp:

```c#
public class StatusIndicator : Control
{
    public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
        nameof(IsOn),
        typeof(bool),
        typeof(StatusIndicator),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool IsOn
    {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    static StatusIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StatusIndicator),
            new FrameworkPropertyMetadata(typeof(StatusIndicator)));
    }
}
```

xaml:

```xaml
<Style TargetType="local:StatusIndicator">
    <Setter Property="Width" Value="20"/>
    <Setter Property="Height" Value="20"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="local:StatusIndicator">
                <Ellipse x:Name="Indicator"
                         Fill="#F44336"
                         Stroke="#E0E0E0"
                         StrokeThickness="1">
                    <Ellipse.Effect>
                        <DropShadowEffect ShadowDepth="0" BlurRadius="5" Color="#F44336" Opacity="0.5"/>
                    </Ellipse.Effect>
                </Ellipse>
                
                <ControlTemplate.Triggers>
                    <Trigger Property="IsOn" Value="True">
                        <Setter TargetName="Indicator" Property="Fill" Value="#4CAF50"/>
                        <Setter TargetName="Indicator" Property="Effect">
                            <Setter.Value>
                                <DropShadowEffect ShadowDepth="0" BlurRadius="5" Color="#4CAF50" Opacity="0.5"/>
                            </Setter.Value>
                        </Setter>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

------

## 十一、最佳实践与常见问题

### 11.1 最佳实践

1. **使用全局样式统一控件外观**：在 App.xaml 中定义全局样式，确保所有控件风格一致
2. **利用字体继承特性**：在窗口级别设置字体，所有子控件自动继承
3. **正确重写 OnApplyTemplate**：总是检查 GetTemplateChild 的返回值是否为 null
4. **使用视觉状态管理**：通过 VisualStateManager 实现控件的状态切换
5. **合理设置 Tab 键顺序**：为工业界面设置合理的 Tab 键顺序，方便键盘操作
6. **避免在控件模板中编写业务逻辑**：控件模板只负责外观，业务逻辑应该放在 ViewModel 中

### 11.2 常见问题与解决方案

#### 问题 1：自定义控件的模板不生效

**可能原因**：

1. 没有在静态构造函数中重写 DefaultStyleKeyProperty
2. 样式文件没有被正确引用
3. 模板中的元素名称与代码中的不一致

**解决方案**：

1. 在静态构造函数中添加`DefaultStyleKeyProperty.OverrideMetadata`
2. 确保样式文件在 App.xaml 中被正确引用
3. 检查模板中的元素名称与代码中 GetTemplateChild 的参数是否一致

#### 问题 2：字体属性不继承

**可能原因**：子控件显式设置了字体属性，覆盖了父控件的继承值

**解决方案**：移除子控件的显式字体设置，让它继承父控件的字体

#### 问题 3：GetTemplateChild 返回 null

**可能原因**：

1. 模板中没有对应的命名元素
2. 元素名称拼写错误
3. 没有调用 base.OnApplyTemplate ()

**解决方案**：

1. 确保模板中有对应的命名元素
2. 检查元素名称的拼写
3. 重写 OnApplyTemplate 时必须调用 base.OnApplyTemplate ()

------

## 十二、官方设计意图总结

微软设计 Control 的核心目标是：

1. **统一所有控件的基础模型**：定义所有控件共有的属性和行为
2. **实现外观与逻辑的完全分离**：通过控件模板系统，将控件的视觉外观与业务逻辑完全分离
3. **提供可扩展的控件开发框架**：通过重写受保护方法，可以轻松开发自定义控件
4. **支持继承特性**：字体、前景色等属性支持继承，简化全局样式设置
5. **符合 Windows 标准**：提供符合 Windows 用户习惯的键盘导航和焦点管理

------

## 总结

`Control`是 WPF 所有控件的最底层基类，它定义了 WPF 控件的基础模型。它的核心特性包括：

- 统一的外观属性（背景、前景、边框、字体等）
- 强大的控件模板系统，实现外观与逻辑的分离
- 视觉状态管理系统，统一控件的状态切换逻辑
- 键盘导航和焦点管理
- 属性继承特性，简化全局样式设置

掌握 Control 基类的核心原理，是理解 WPF 整个控件体系的基础，也是开发高质量自定义控件的必备技能。

