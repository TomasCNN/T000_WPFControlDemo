# 004005002_WPF ToggleButton 基类官方类定义逐行深度解析（.NET 8 最新版）

基于 **.NET 8 官方开源源码** 完整解析，包含所有公共 / 受保护成员、特性、设计意图和工业场景应用。`ToggleButton`是 WPF**所有开关式控件的抽象基类**，在`ButtonBase`的基础上增加了**状态保持能力**，是`CheckBox`、`RadioButton`等控件的共同父类。

------

## 一、ToggleButton 在 WPF 类层次结构中的位置

plaintext：

```tex
System.Object
  ↳ System.Windows.Threading.DispatcherObject
    ↳ System.Windows.DependencyObject
      ↳ System.Windows.Media.Visual
        ↳ System.Windows.UIElement
          ↳ System.Windows.FrameworkElement
            ↳ System.Windows.Controls.Control
              ↳ System.Windows.Controls.ContentControl
                ↳ System.Windows.Controls.Primitives.ButtonBase
                  ↳ System.Windows.Controls.Primitives.ToggleButton  ← 核心基类
                    ↳ System.Windows.Controls.CheckBox
                    ↳ System.Windows.Controls.RadioButton
```

**核心继承关系**：

- 继承`ContentControl`：支持任意内容（文本、图标、复杂布局）
- 继承`ButtonBase`：拥有完整的点击逻辑、命令系统和键盘支持
- 新增特性：`IsChecked`状态保持、三状态支持、状态变化路由事件

------

## 二、完整官方类定义（.NET 8 源码级）

csharp:

```c#
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Markup;

namespace System.Windows.Controls.Primitives
{
    /// <summary>
    /// 表示可以切换选中/未选中状态的按钮基类
    /// </summary>
    /// <remarks>
    /// ToggleButton 是所有开关式控件的基类，支持双状态（选中/未选中）和三状态（增加不确定状态）。
    /// 每次点击会切换 IsChecked 属性的值，并触发对应的路由事件。
    /// </remarks>
    [DefaultEvent("Checked")]
    [Localizability(LocalizationCategory.Button)]
    public class ToggleButton : ButtonBase
    {
        // ==============================================
        // 依赖属性定义（ToggleButton特有）
        // ==============================================
        public static readonly DependencyProperty IsCheckedProperty;
        public static readonly DependencyProperty IsThreeStateProperty;

        // ==============================================
        // 静态构造函数
        // ==============================================
        static ToggleButton()
        {
            // 注册依赖属性
            IsCheckedProperty = DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool?),
                typeof(ToggleButton),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnIsCheckedChanged));

            IsThreeStateProperty = DependencyProperty.Register(
                nameof(IsThreeState),
                typeof(bool),
                typeof(ToggleButton),
                new FrameworkPropertyMetadata(false));

            // 重写默认样式键
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToggleButton),
                new FrameworkPropertyMetadata(typeof(ToggleButton)));

            // 注册键盘快捷键：空格键和回车键触发点击
            KeyboardNavigation.AcceptsReturnProperty.OverrideMetadata(
                typeof(ToggleButton),
                new FrameworkPropertyMetadata(true));
        }

        // ==============================================
        // 公共构造函数
        // ==============================================
        public ToggleButton();

        // ==============================================
        // 公共属性
        // ==============================================
        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsChecked { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        public bool IsThreeState { get; set; }

        // ==============================================
        // 核心路由事件
        // ==============================================
        public static readonly RoutedEvent CheckedEvent;
        public static readonly RoutedEvent UncheckedEvent;
        public static readonly RoutedEvent IndeterminateEvent;

        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }

        public event RoutedEventHandler Unchecked
        {
            add => AddHandler(UncheckedEvent, value);
            remove => RemoveHandler(UncheckedEvent, value);
        }

        public event RoutedEventHandler Indeterminate
        {
            add => AddHandler(IndeterminateEvent, value);
            remove => RemoveHandler(IndeterminateEvent, value);
        }

        // ==============================================
        // 受保护方法（派生类可重写）
        // ==============================================
        protected override AutomationPeer OnCreateAutomationPeer();
        protected override void OnClick();
        protected virtual void OnChecked(RoutedEventArgs e);
        protected virtual void OnUnchecked(RoutedEventArgs e);
        protected virtual void OnIndeterminate(RoutedEventArgs e);
        protected internal virtual void OnToggle();
    }
}
```

------

## 三、类级特性逐行解析

### 1. `[DefaultEvent("Checked")]`

csharp:

```c#
[DefaultEvent("Checked")]
```

- **作用**：指定控件的默认事件
- **设计意图**：在 Visual Studio 设计器中双击 ToggleButton 时，自动生成`Checked`事件的处理方法
- **工业场景意义**：符合开发人员直觉，因为 ToggleButton 最常用的操作是响应状态变化，而不是点击事件

### 2. `[Localizability(LocalizationCategory.Button)]`

csharp:

```c#
[Localizability(LocalizationCategory.Button)]
```

- **作用**：本地化特性，告诉本地化工具该类属于按钮类别
- **设计意图**：本地化工具会自动将 ToggleButton 的`Content`属性作为可本地化内容处理
- **注意**：不会本地化状态相关的属性，只本地化显示内容

------

## 四、静态构造函数解析（核心初始化逻辑）

静态构造函数是 ToggleButton 最关键的部分，负责所有依赖属性的注册和默认行为的设置。

### 1. `IsCheckedProperty` 注册

csharp:

```c#
IsCheckedProperty = DependencyProperty.Register(
    nameof(IsChecked),
    typeof(bool?),
    typeof(ToggleButton),
    new FrameworkPropertyMetadata(
        false,
        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
        OnIsCheckedChanged));
```

- **类型**：`bool?`（可空布尔值），这是 ToggleButton 最特殊的设计

- **默认值**：`false`（未选中状态）

- **元数据标志**：

  - `BindsTwoWayByDefault`：默认双向绑定，这是 WPF 中少数默认双向绑定的属性之一
  - `Journal`：支持导航日志，在页面导航时会保存和恢复状态

  

- **属性变更回调**：`OnIsCheckedChanged`，当`IsChecked`值变化时调用，负责触发对应的路由事件

### 2. `IsThreeStateProperty` 注册

csharp:

```c#
IsThreeStateProperty = DependencyProperty.Register(
    nameof(IsThreeState),
    typeof(bool),
    typeof(ToggleButton),
    new FrameworkPropertyMetadata(false));
```

- **类型**：`bool`
- **默认值**：`false`（仅支持双状态）
- **作用**：控制是否启用第三种不确定状态（`IsChecked = null`）

### 3. 其他初始化

csharp:

```c#
// 重写默认样式键
DefaultStyleKeyProperty.OverrideMetadata(
    typeof(ToggleButton),
    new FrameworkPropertyMetadata(typeof(ToggleButton)));

// 注册键盘快捷键：空格键和回车键触发点击
KeyboardNavigation.AcceptsReturnProperty.OverrideMetadata(
    typeof(ToggleButton),
    new FrameworkPropertyMetadata(true));
```

- **默认样式键**：指定 ToggleButton 的默认样式类型
- **键盘支持**：默认支持空格键和回车键触发点击，符合 Windows 标准

------

## 五、核心依赖属性逐行解析

### 1. `IsChecked` 属性（最核心）

csharp:

```c#
[Bindable(true)]
[Category("Appearance")]
[TypeConverter(typeof(NullableBoolConverter))]
public bool? IsChecked { get; set; }
```

#### 逐句解析：

- **`[Bindable(true)]`**：标记该属性支持数据绑定

- **`[Category("Appearance")]`**：在 Visual Studio 属性窗口中归类到 "外观" 组

- **`[TypeConverter(typeof(NullableBoolConverter))]`**：指定类型转换器，支持在 XAML 中直接写`True`、`False`或`{x:Null}`

- **`bool?`类型**：可空布尔值，支持三种状态：

  - `true`：选中状态
  - `false`：未选中状态（默认）
  - `null`：不确定状态（仅当`IsThreeState="True"`时有效）

  

#### 工业场景应用：

- **双状态**：设备启停开关（true = 运行，false = 停止）、自动 / 手动模式（true = 自动，false = 手动）、报警静音开关
- **三状态**：多设备总开关（true = 全部运行，false = 全部停止，null = 部分运行）、批量选择（true = 全选，false = 全不选，null = 部分选）

### 2. `IsThreeState` 属性

csharp:

```c#
[Bindable(true)]
[Category("Behavior")]
public bool IsThreeState { get; set; }
```

#### 逐句解析：

- **`[Category("Behavior")]`**：在属性窗口中归类到 "行为" 组
- **默认值**：`false`，默认只支持双状态
- **作用**：启用后，`IsChecked`可以取`null`值，点击时会在`false`→`true`→`null`→`false`之间循环切换

#### 工业场景应用：

xaml:

```xaml
<!-- 多相机总开关，支持三状态 -->
<ToggleButton Content="全部相机"
              IsThreeState="True"
              IsChecked="{Binding AllCamerasStatus}"
              Checked="AllCameras_Checked"
              Unchecked="AllCameras_Unchecked"
              Indeterminate="AllCameras_Indeterminate"/>
```

------

## 六、核心路由事件逐行解析

ToggleButton 有三个专属路由事件，分别对应三种状态变化，这是它与普通 Button 最本质的区别。

### 1. `CheckedEvent`

csharp:

```c#
public static readonly RoutedEvent CheckedEvent;
public event RoutedEventHandler Checked;
```

- **触发时机**：当`IsChecked`从`false`或`null`变为`true`时触发
- **路由策略**：冒泡路由事件
- **工业场景**：设备启动、自动模式开启、报警静音开启

### 2. `UncheckedEvent`

csharp:

```c#
public static readonly RoutedEvent UncheckedEvent;
public event RoutedEventHandler Unchecked;
```

- **触发时机**：当`IsChecked`从`true`或`null`变为`false`时触发
- **路由策略**：冒泡路由事件
- **工业场景**：设备停止、手动模式开启、报警静音关闭

### 3. `IndeterminateEvent`

csharp:

```c#
public static readonly RoutedEvent IndeterminateEvent;
public event RoutedEventHandler Indeterminate;
```

- **触发时机**：当`IsChecked`从`true`或`false`变为`null`时触发
- **路由策略**：冒泡路由事件
- **工业场景**：多设备部分运行、批量部分选择

### 关键区别：Click 事件 vs 状态事件

| 事件                              | 触发时机                               | 适用场景             |
| :-------------------------------- | :------------------------------------- | :------------------- |
| `Click`                           | **每次点击都会触发**，无论状态是否变化 | 需要响应点击动作本身 |
| `Checked/Unchecked/Indeterminate` | **只有当状态发生变化时才会触发**       | 需要响应状态变化     |

**工业最佳实践**：永远优先使用状态事件，而不是 Click 事件。因为 Click 事件可能会在状态没有变化时触发（比如点击已经选中的 ToggleButton），导致逻辑错误。

------

## 七、受保护方法逐行解析（派生类开发必备）

这些方法是自定义 ToggleButton 派生类时必须掌握的，ToggleButton 的所有核心逻辑都在这里实现。

### 1. `OnClick()` 方法

csharp:

```c#
protected override void OnClick();
```

- **重写自**：`ButtonBase.OnClick()`

- **核心逻辑**：

  csharp:

  ```c#
  protected override void OnClick()
  {
      base.OnClick(); // 触发Click事件和执行Command
      OnToggle(); // 切换状态
  }
  ```

  

- **设计意图**：点击按钮时自动切换状态

- **自定义注意事项**：重写时必须调用`base.OnClick()`，否则 Click 事件、Command 和状态切换都不会执行

### 2. `OnToggle()` 方法（核心中的核心）

csharp:

```c#
protected internal virtual void OnToggle();
```

- **作用**：负责切换`IsChecked`属性的值，是 ToggleButton 的灵魂方法

- **默认实现逻辑**：

  csharp:

  ```c#
  protected internal virtual void OnToggle()
  {
      if (IsChecked == false)
      {
          IsChecked = true;
      }
      else if (IsChecked == true)
      {
          IsChecked = IsThreeState ? null : false;
      }
      else // IsChecked == null
      {
          IsChecked = false;
      }
  }
  ```

  

- **切换顺序**：

  - 双状态（`IsThreeState=false`）：`false` → `true` → `false` → ...
  - 三状态（`IsThreeState=true`）：`false` → `true` → `null` → `false` → ...

  

- **派生类重写**：`CheckBox`和`RadioButton`都重写了这个方法来实现自己的切换逻辑

### 3. 状态变化通知方法

csharp:

```c#
protected virtual void OnChecked(RoutedEventArgs e);
protected virtual void OnUnchecked(RoutedEventArgs e);
protected virtual void OnIndeterminate(RoutedEventArgs e);
```

- **触发时机**：当对应的状态变化时，由`OnIsCheckedChanged`回调调用

- **作用**：触发对应的路由事件

- **默认实现**：

  csharp:

  ```c#
  protected virtual void OnChecked(RoutedEventArgs e)
  {
      RaiseEvent(e);
  }
  ```

  

- **自定义注意事项**：重写时必须调用基类方法，否则路由事件不会触发

### 4. `OnCreateAutomationPeer()` 方法

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- **作用**：创建自动化对等类，支持 UI 自动化测试和辅助功能
- **返回值**：`ToggleButtonAutomationPeer`实例
- **工业场景意义**：支持设备的自动化测试和远程监控

------

## 八、ToggleButton 核心工作流程

当用户点击 ToggleButton 时，WPF 会按照以下顺序执行：

1. 触发`MouseLeftButtonDown`事件
2. 设置`IsPressed`为`true`
3. 捕获鼠标
4. 触发`MouseLeftButtonUp`事件
5. 根据`ClickMode`设置触发`Click`事件
6. 调用`OnClick()`方法
7. 调用`OnToggle()`方法切换`IsChecked`属性的值
8. 触发`OnIsCheckedChanged`回调
9. 根据新的`IsChecked`值调用对应的`OnChecked/OnUnchecked/OnIndeterminate`方法
10. 触发对应的路由事件
11. 执行`Command`命令（如果设置了）
12. 释放鼠标捕获
13. 设置`IsPressed`为`false`

------

## 九、派生类实现原理

### 1. CheckBox 的实现

CheckBox 重写了`OnToggle()`方法，保持了 ToggleButton 的默认切换逻辑，但提供了不同的默认样式和模板。

### 2. RadioButton 的实现

RadioButton 重写了`OnToggle()`方法，实现了互斥单选逻辑：

csharp:

```c#
protected internal override void OnToggle()
{
    if (IsChecked == false)
    {
        IsChecked = true;
    }
    // 选中状态下点击不会取消选中
}
```

同时，RadioButton 会在同一个父容器内维护互斥关系，确保同一时间只有一个 RadioButton 被选中。

------

## 十、官方设计意图总结

微软设计 ToggleButton 的核心目标是：

1. **统一开关式控件的模型**：所有开关式控件都继承自 ToggleButton，共享相同的状态处理逻辑
2. **支持双状态和三状态**：满足不同的业务需求
3. **默认双向绑定**：简化 MVVM 模式下的状态绑定
4. **完整的路由事件支持**：提供细粒度的状态变化通知
5. **易于扩展**：通过重写受保护方法，可以轻松实现自定义的开关逻辑

------

## 十一、工业场景最佳实践

1. **优先使用状态事件**：使用`Checked/Unchecked`而不是`Click`事件响应状态变化
2. **明确状态含义**：每个状态的含义必须清晰，特别是三状态的不确定状态
3. **记录状态变化日志**：所有 ToggleButton 的状态变化都必须记录日志，便于问题追溯
4. **危险操作禁用 ToggleButton**：急停、删除等危险操作必须使用带二次确认的 Button，不要使用 ToggleButton
5. **使用合适的样式**：工业场景推荐使用开关样式或明确的文字提示，避免用户混淆