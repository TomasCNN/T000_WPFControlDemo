# 004001002_IoIndicator (指示灯)自定义控件逐句深度解析

源码解析：

```c#
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace IndustrialVisionTemplate.Controls
{
    /// <summary>
    /// IO状态指示灯控件
    /// </summary>
    public class IoIndicator : Control
    {
        // 静态构造函数：指定默认样式
        static IoIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(IoIndicator), 
                new FrameworkPropertyMetadata(typeof(IoIndicator)));
        }

        // 依赖属性：IO状态（true=ON，false=OFF）
        public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
            nameof(IsOn), 
            typeof(bool), 
            typeof(IoIndicator), 
            new PropertyMetadata(false, OnIsOnChanged));

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        // 依赖属性：是否闪烁（报警状态）
        public static readonly DependencyProperty IsBlinkingProperty = DependencyProperty.Register(
            nameof(IsBlinking), 
            typeof(bool), 
            typeof(IoIndicator), 
            new PropertyMetadata(false, OnIsBlinkingChanged));

        public bool IsBlinking
        {
            get => (bool)GetValue(IsBlinkingProperty);
            set => SetValue(IsBlinkingProperty, value);
        }

        // 闪烁定时器
        private DispatcherTimer _blinkTimer;
        private bool _isBlinkOn;

        // 状态变化回调
        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IoIndicator)d;
            control.UpdateIndicatorColor();
        }

        private static void OnIsBlinkingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IoIndicator)d;
            if ((bool)e.NewValue)
            {
                control.StartBlinking();
            }
            else
            {
                control.StopBlinking();
            }
        }

        // 启动闪烁
        private void StartBlinking()
        {
            if (_blinkTimer == null)
            {
                _blinkTimer = new DispatcherTimer();
                _blinkTimer.Interval = TimeSpan.FromMilliseconds(500);
                _blinkTimer.Tick += (s, e) =>
                {
                    _isBlinkOn = !_isBlinkOn;
                    UpdateIndicatorColor();
                };
            }
            _blinkTimer.Start();
        }

        // 停止闪烁
        private void StopBlinking()
        {
            _blinkTimer?.Stop();
            _isBlinkOn = false;
            UpdateIndicatorColor();
        }

        // 更新指示灯颜色
        private void UpdateIndicatorColor()
        {
            if (IsBlinking && !_isBlinkOn)
            {
                // 闪烁时熄灭
                SetValue(IndicatorColorPropertyKey, Brushes.Gray);
            }
            else if (IsOn)
            {
                // ON状态绿色
                SetValue(IndicatorColorPropertyKey, Brushes.LimeGreen);
            }
            else
            {
                // OFF状态红色
                SetValue(IndicatorColorPropertyKey, Brushes.Red);
            }
        }

        // 只读依赖属性：指示灯颜色
        private static readonly DependencyPropertyKey IndicatorColorPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IndicatorColor), 
            typeof(Brush), 
            typeof(IoIndicator), 
            new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty IndicatorColorProperty = IndicatorColorPropertyKey.DependencyProperty;

        public Brush IndicatorColor
        {
            get => (Brush)GetValue(IndicatorColorProperty);
        }

        // 控件卸载时释放资源
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Unloaded += (s, e) =>
            {
                _blinkTimer?.Stop();
                _blinkTimer = null;
            };
        }
    }
}
```



这是一个**工业级标准的 IO 状态指示灯控件**，完全遵循 WPF 自定义控件最佳实践，针对工业上位机 7×24 小时运行、高频状态更新、高可靠性要求做了专门优化。下面逐句解析每一行代码的功能、作用和设计考量。

------

## 一、命名空间与引用

csharp：

```c#
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading
```

| 引用                       | 功能与作用                                       | 工业场景必要性                                             |
| :------------------------- | :----------------------------------------------- | :--------------------------------------------------------- |
| `System.Windows`           | WPF 核心基础库，提供依赖属性、路由事件等核心机制 | ✅ 所有 WPF 应用必须引用                                    |
| `System.Windows.Controls`  | 包含`Control`基类，所有 WPF 标准控件的父类       | ✅ 自定义控件必须继承自`Control`                            |
| `System.Windows.Media`     | 提供绘图相关类（如`Brush`、`Ellipse`）           | ✅ 用于绘制指示灯的圆形和颜色                               |
| `System.Windows.Threading` | 提供`DispatcherTimer`，UI 线程安全的定时器       | ✅ 工业场景高频状态更新必须用 UI 线程定时器，避免跨线程异常 |

csharp：

```c#
namespace IndustrialVisionTemplate.Controls
```

- 定义控件所在的命名空间，符合你之前的项目结构规范（自定义控件统一放在`Controls`文件夹下）
- 在 XAML 中使用时需要引入这个命名空间：`xmlns:controls="clr-namespace:IndustrialVisionTemplate.Controls"`

------

## 二、类定义与注释

csharp:

```c#
/// <summary>
/// IO状态指示灯控件
/// </summary>
public class IoIndicator : Control
```

- `/// <summary>`：XML 注释，用于生成文档和 IDE 智能提示，团队协作必备

- `public class IoIndicator`：定义公共类`IoIndicator`

- `: Control`：继承自`Control`基类，这是所有 WPF 标准控件的共同父类

  - 继承`Control`可以获得所有控件共有的属性（`Background`、`IsEnabled`、`Visibility`等）
  - 支持 WPF 的控件模板机制，实现逻辑与外观分离
  - 支持数据绑定、样式、动画等 WPF 核心特性

  

------

## 三、静态构造函数（WPF 自定义控件核心）

csharp:

```c#
// 静态构造函数：指定默认样式
static IoIndicator()
{
    DefaultStyleKeyProperty.OverrideMetadata(
        typeof(IoIndicator), 
        new FrameworkPropertyMetadata(typeof(IoIndicator)));
}
```

**这是 WPF 自定义控件最重要的一行代码，90% 的新手自定义控件显示不出来都是因为漏写了这行**。

### 逐句解析：

1. `static IoIndicator()`：静态构造函数，在类第一次被使用时执行，且只执行一次
2. `DefaultStyleKeyProperty`：`Control`类的一个依赖属性，用于指定控件的默认样式键
3. `OverrideMetadata`：重写依赖属性的元数据，为`IoIndicator`类设置新的默认值
4. `typeof(IoIndicator)`：指定默认样式的目标类型为`IoIndicator`

### 作用：

告诉 WPF 系统：**当没有为`IoIndicator`显式指定 Style 时，去`Themes/Generic.xaml`文件中查找 TargetType 为`IoIndicator`的 Style 作为默认样式**。

### 工业场景必要性：

- 保证控件在任何项目中使用时都有默认外观，不需要额外配置
- 支持样式重写，不同项目可以根据需要自定义外观，不改变控件逻辑

------

## 四、IsOn 依赖属性（IO 状态）

csharp:

```c#
// 依赖属性：IO状态（true=ON，false=OFF）
public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
    nameof(IsOn), 
    typeof(bool), 
    typeof(IoIndicator), 
    new PropertyMetadata(false, OnIsOnChanged));
```

### 逐句解析：

1. `public static readonly DependencyProperty IsOnProperty`：定义静态只读的依赖属性字段

   - 命名规范：属性名 + `Property`后缀
   - `static readonly`：依赖属性是类级别的静态字段，所有实例共享

   

2. `DependencyProperty.Register`：注册依赖属性的标准方法，四个参数分别是：

   - `nameof(IsOn)`：属性名称，使用`nameof`运算符避免硬编码错误

   - `typeof(bool)`：属性的数据类型

   - `typeof(IoIndicator)`：属性所属的类

   - `new PropertyMetadata(false, OnIsOnChanged)`：属性元数据

     - `false`：属性的默认值（IO 默认是 OFF 状态）
     - `OnIsOnChanged`：属性值变化时的回调方法

     

csharp:

```c#
public bool IsOn
{
    get => (bool)GetValue(IsOnProperty);
    set => SetValue(IsOnProperty, value);
}
```

- 依赖属性的 CLR 包装器，提供对依赖属性的强类型访问
- `GetValue(IsOnProperty)`：从依赖属性系统中获取值
- `SetValue(IsOnProperty, value)`：设置依赖属性的值

### 为什么必须用依赖属性而不是普通属性？

工业上位机中 IO 状态必须和 ViewModel 进行**双向数据绑定**，而普通属性不支持 WPF 的数据绑定、样式、动画等特性。只有依赖属性才能实现：

xaml:

```xaml
<!-- 直接绑定到ViewModel的Input1State属性 -->
<controls:IoIndicator IsOn="{Binding Input1State}"/>
```

------

## 五、IsBlinking 依赖属性（报警闪烁）

csharp:

```c#
// 依赖属性：是否闪烁（报警状态）
public static readonly DependencyProperty IsBlinkingProperty = DependencyProperty.Register(
    nameof(IsBlinking), 
    typeof(bool), 
    typeof(IoIndicator), 
    new PropertyMetadata(false, OnIsBlinkingChanged));
```

- 与`IsOn`属性结构完全相同，用于控制指示灯是否闪烁
- 默认值`false`：默认不闪烁
- 变更回调`OnIsBlinkingChanged`：当闪烁状态变化时启动或停止定时器

csharp:

```c#
public bool IsBlinking
{
    get => (bool)GetValue(IsBlinkingProperty);
    set => SetValue(IsBlinkingProperty, value);
}
```

- CLR 包装器，用法与`IsOn`相同
- 工业场景中用于报警状态：当 IO 点有报警时，设置`IsBlinking="True"`，指示灯会每秒闪烁一次

------

## 六、私有字段

csharp:

```c#
// 闪烁定时器
private DispatcherTimer _blinkTimer;
private bool _isBlinkOn;
```

| 字段          | 功能与作用                          | 工业场景必要性                                               |
| :------------ | :---------------------------------- | :----------------------------------------------------------- |
| `_blinkTimer` | WPF UI 线程定时器，用于控制闪烁频率 | ✅ `DispatcherTimer`的`Tick`事件在 UI 线程执行，可以直接修改 UI 属性，不需要跨线程调用`Dispatcher.Invoke`，避免高频更新时的性能问题和异常 |
| `_isBlinkOn`  | 闪烁状态标志位，记录当前是亮还是灭  | ✅ 用于在定时器回调中切换指示灯状态                           |

### 为什么不用 System.Timers.Timer？

`System.Timers.Timer`的回调是在后台线程执行的，如果直接修改 UI 属性会抛出`跨线程访问异常`。而工业场景中 IO 状态更新频率很高（每秒几十次），频繁的跨线程调用会严重影响性能，甚至导致程序崩溃。

------

## 七、依赖属性变更回调

csharp:

```c#
// 状态变化回调
private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var control = (IoIndicator)d;
    control.UpdateIndicatorColor();
}
```

### 逐句解析：

1. `private static void OnIsOnChanged`：静态方法，因为依赖属性是静态的，所以回调必须是静态的
2. `DependencyObject d`：触发属性变化的控件实例
3. `DependencyPropertyChangedEventArgs e`：包含属性的旧值和新值
4. `var control = (IoIndicator)d`：将通用的`DependencyObject`转换为具体的`IoIndicator`实例
5. `control.UpdateIndicatorColor()`：调用实例方法更新指示灯颜色

csharp:

```c#
private static void OnIsBlinkingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var control = (IoIndicator)d;
    if ((bool)e.NewValue)
    {
        control.StartBlinking();
    }
    else
    {
        control.StopBlinking();
    }
}
```

- 当`IsBlinking`属性变为`true`时，启动闪烁定时器
- 当`IsBlinking`属性变为`false`时，停止闪烁定时器

------

## 八、闪烁控制方法

csharp:

```c#
// 启动闪烁
private void StartBlinking()
{
    if (_blinkTimer == null)
    {
        _blinkTimer = new DispatcherTimer();
        _blinkTimer.Interval = TimeSpan.FromMilliseconds(500);
        _blinkTimer.Tick += (s, e) =>
        {
            _isBlinkOn = !_isBlinkOn;
            UpdateIndicatorColor();
        };
    }
    _blinkTimer.Start();
}
```

### 逐句解析：

1. `if (_blinkTimer == null)`：单例模式，确保只创建一个定时器实例

2. `new DispatcherTimer()`：创建 UI 线程定时器

3. `Interval = TimeSpan.FromMilliseconds(500)`：设置闪烁间隔为 500ms（每秒闪烁一次）

   - 这是工业现场标准的报警闪烁频率，人眼最敏感且不会太刺眼

   

4. `_blinkTimer.Tick += (s, e) =>`：注册定时器 Tick 事件的匿名方法

5. `_isBlinkOn = !_isBlinkOn`：切换闪烁状态

6. `UpdateIndicatorColor()`：更新指示灯颜色

7. `_blinkTimer.Start()`：启动定时器

csharp:

```c#
// 停止闪烁
private void StopBlinking()
{
    _blinkTimer?.Stop();
    _isBlinkOn = false;
    UpdateIndicatorColor();
}
```

- `_blinkTimer?.Stop()`：空值安全地停止定时器
- `_isBlinkOn = false`：重置闪烁状态为熄灭
- `UpdateIndicatorColor()`：更新指示灯为正常的 ON/OFF 颜色

------

## 九、指示灯颜色更新逻辑

csharp:

```c#
// 更新指示灯颜色
private void UpdateIndicatorColor()
{
    if (IsBlinking && !_isBlinkOn)
    {
        // 闪烁时熄灭
        SetValue(IndicatorColorPropertyKey, Brushes.Gray);
    }
    else if (IsOn)
    {
        // ON状态绿色
        SetValue(IndicatorColorPropertyKey, Brushes.LimeGreen);
    }
    else
    {
        // OFF状态红色
        SetValue(IndicatorColorPropertyKey, Brushes.Red);
    }
}
```

### 工业标准颜色逻辑：

| 状态组合     | 显示颜色 | 含义         |
| :----------- | :------- | :----------- |
| 闪烁中且熄灭 | 灰色     | 报警状态闪烁 |
| 不闪烁且 ON  | 亮绿色   | IO 点导通    |
| 不闪烁且 OFF | 亮红色   | IO 点断开    |

- 完全符合工业现场的颜色规范，操作人员已经形成条件反射
- `SetValue(IndicatorColorPropertyKey, ...)`：设置只读依赖属性的值，只能在类内部调用

------

## 十、IndicatorColor 只读依赖属性

csharp:

```c#
// 只读依赖属性：指示灯颜色
private static readonly DependencyPropertyKey IndicatorColorPropertyKey = DependencyProperty.RegisterReadOnly(
    nameof(IndicatorColor), 
    typeof(Brush), 
    typeof(IoIndicator), 
    new PropertyMetadata(Brushes.Gray));

public static readonly DependencyProperty IndicatorColorProperty = IndicatorColorPropertyKey.DependencyProperty;

public Brush IndicatorColor
{
    get => (Brush)GetValue(IndicatorColorProperty);
}
```

### 逐句解析：

1. `DependencyProperty.RegisterReadOnly`：注册只读依赖属性，返回`DependencyPropertyKey`

   - `DependencyPropertyKey`只能在类内部访问，用于设置属性值

   

2. `IndicatorColorPropertyKey.DependencyProperty`：获取公开的只读依赖属性

3. `public Brush IndicatorColor`：只读的 CLR 包装器，外部只能读取不能修改

### 为什么要用只读依赖属性？

1. **封装性**：指示灯的颜色应该由控件内部根据`IsOn`和`IsBlinking`自动计算，外部不能直接修改
2. **模板绑定**：需要将颜色绑定到控件模板中的`Ellipse.Fill`属性，只有依赖属性才能支持`TemplateBinding`：

xaml:

```xaml
<ControlTemplate TargetType="controls:IoIndicator">
    <Ellipse Fill="{TemplateBinding IndicatorColor}" Stroke="Black" StrokeThickness="1"/>
</ControlTemplate>
```

------

## 十一、资源释放（工业软件 7×24 小时运行必备）

csharp:

```c#
// 控件卸载时释放资源
public override void OnApplyTemplate()
{
    base.OnApplyTemplate();
    Unloaded += (s, e) =>
    {
        _blinkTimer?.Stop();
        _blinkTimer = null;
    };
}
```

### 逐句解析：

1. `public override void OnApplyTemplate()`：重写`Control`基类的方法，当控件的模板被应用时调用
2. `base.OnApplyTemplate()`：调用基类的实现，确保模板正常加载
3. `Unloaded += (s, e) =>`：订阅控件的`Unloaded`事件，当控件从可视化树中移除时触发
4. `_blinkTimer?.Stop()`：停止闪烁定时器
5. `_blinkTimer = null`：释放定时器引用，让 GC 可以回收

### 工业场景必要性：

如果不停止定时器，定时器会一直引用控件实例，导致 GC 无法回收控件，造成**内存泄漏**。对于 7×24 小时运行的工业软件来说，内存泄漏会导致程序运行几天后内存占用过高，最终崩溃。

------

## 总结：工业级控件的核心设计要点

1. **基于 Control 基类**：利用 WPF 成熟的控件体系和模板机制
2. **依赖属性驱动**：所有外部可配置的属性都用依赖属性，支持数据绑定
3. **UI 线程安全**：使用`DispatcherTimer`处理高频 UI 更新，避免跨线程异常
4. **工业标准规范**：颜色、闪烁频率都符合工业现场的操作习惯
5. **资源自动释放**：在`Unloaded`事件中释放所有非托管资源，防止内存泄漏
6. **逻辑与外观分离**：控件只负责逻辑，外观完全由`ControlTemplate`定义，支持自定义