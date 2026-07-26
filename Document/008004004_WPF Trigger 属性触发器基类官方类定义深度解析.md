# 008004004_WPF `Trigger` 属性触发器基类官方类定义深度解析

WPF 中属性触发器的核心实现类是 `Trigger`，其通用能力全部继承自抽象基类 `TriggerBase`。`TriggerBase` 是所有触发器（属性触发器、多条件触发器、数据触发器、事件触发器）的共同基类，定义了触发器的通用机制；`Trigger` 则是针对「控件自身依赖属性」的单条件触发器实现。

------

## 一、基础元数据与完整继承体系

### 1. 官方基础信息

| 项           | 官方值                                                       |
| :----------- | :----------------------------------------------------------- |
| **命名空间** | `System.Windows`                                             |
| **程序集**   | `PresentationFramework.dll`                                  |
| **类性质**   | `TriggerBase` 为抽象类，`Trigger` 为密封类（`sealed`，不可被第三方继承） |
| **底层定位** | 均继承自 `DependencyObject`，属于依赖对象，深度绑定 WPF 依赖属性系统 |

### 2. 完整继承链

plaintext:

```tex
System.Object
  └─ System.Windows.Threading.DispatcherObject
      └─ System.Windows.DependencyObject
          └─ System.Windows.TriggerBase （所有触发器的抽象基类）
              └─ System.Windows.Trigger （属性触发器的具体实现）
```

- `DispatcherObject`：提供线程调度能力，保证触发器操作在 UI 线程执行；
- `DependencyObject`：提供依赖属性基础能力，支撑触发器的属性监听与值优先级机制；
- `TriggerBase`：抽象所有触发器的通用行为（进入 / 退出动作、密封机制）；
- `Trigger`：实现「单依赖属性相等比较」的具体触发逻辑。

------

## 二、抽象基类 `TriggerBase` 官方定义详解

`TriggerBase` 是所有触发器的根基类，定义了 WPF 触发器的通用能力框架，所有派生触发器（`Trigger`、`MultiTrigger`、`DataTrigger`、`MultiDataTrigger`、`EventTrigger`）都继承并复用这套机制。

### 官方类签名

csharp:

```c#
public abstract class TriggerBase : DependencyObject
```

### 核心成员完整解析

#### 1. `EnterActions` 属性

csharp:

```c#
public TriggerActionCollection EnterActions { get; }
```

- **官方定义**：获取触发器进入激活状态时要执行的操作集合。
- **类型**：`TriggerActionCollection`，元素为 `TriggerAction` 派生类，最常用的是 `BeginStoryboard`（播放动画）。
- **触发时机**：当触发器条件从「不满足」变为「满足」的瞬间，按顺序执行集合内所有动作，仅执行一次。
- **设计定位**：专门用于处理动画、音效等瞬时动作；属性赋值类场景优先使用 `Setters`，而非动作集合。

#### 2. `ExitActions` 属性

csharp:

```c#
public TriggerActionCollection ExitActions { get; }
```

- **官方定义**：获取触发器退出激活状态时要执行的操作集合。
- **触发时机**：当触发器条件从「满足」变为「不满足」的瞬间执行，与 `EnterActions` 成对对应，用于执行反向动作（如停止动画、播放还原动画）。
- **关键特性**：属性触发器、数据触发器的 `Setters` 会自动恢复，无需在 `ExitActions` 中写反向赋值；只有动画等不会自动还原的动作，才需要在退出动作中处理。

#### 3. `IsSealed` 属性

csharp:

```c#
public bool IsSealed { get; }
```

- **官方定义**：获取一个值，指示此对象是否处于不可修改的密封状态。
- **设计作用**：
  1. 当样式、模板加载完成并首次应用到控件后，WPF 会自动调用 `Seal()` 方法密封所有触发器；
  2. 密封后触发器的所有集合（`Setters`、`EnterActions`、`ExitActions`）都会变为只读，强行修改会抛出异常；
  3. 保证运行时触发器的不可变性，避免意外修改导致的 UI 异常，同时便于框架做渲染性能优化。

#### 4. `Seal()` 方法

csharp:

```c#
public void Seal();
```

- **官方定义**：将此触发器标记为不可修改的密封状态。
- **内部执行逻辑**：
  1. 标记 `IsSealed = true`；
  2. 将 `Setters`、`EnterActions`、`ExitActions` 等所有集设置为只读；
  3. 冻结集合内所有可冻结对象（如 `Freezable` 类型的画刷、动画），关闭变更通知，提升渲染性能。
- **调用时机**：由 WPF 框架内部在样式、模板解析完成后自动调用，业务开发几乎不会手动调用。

------

## 三、`Trigger` 属性触发器官方成员完整解析

`Trigger` 是 `TriggerBase` 的派生类，专门实现「监听单个依赖属性，值相等时触发」的逻辑，是最基础、使用最频繁的触发器类型。

### 官方类签名

csharp:

```c#
public sealed class Trigger : TriggerBase, IAddChild
```

- 密封类：不允许外部继承，微软官方不推荐自定义触发器；复杂场景优先通过附加属性、数据触发器扩展。
- 实现 `IAddChild`：支撑 XAML 解析语法，让 `<Trigger>` 标签内的 `<Setter>` 子元素能自动加入 `Setters` 集合。

### 核心属性详解

#### 1. `Property` 属性

csharp:

```c#
public DependencyProperty Property { get; set; }
```

- **官方定义**：获取或设置触发器要监听的目标依赖属性。
- **核心硬性约束**：必须是**依赖属性（`DependencyProperty`）**，普通 CLR 属性无法被触发器监听。
  - 原因：只有依赖属性才具备内置的值变更通知机制，触发器才能感知属性变化并重新判断条件。
- **XAML 语法糖**：XAML 中直接写属性名字符串（如 `Property="IsMouseOver"`），WPF 会通过类型转换器自动映射到对应控件的依赖属性字段。
- **常见使用场景**：`UIElement.IsMouseOver`、`ButtonBase.IsPressed`、`UIElement.IsEnabled`、`TextBox.IsReadOnly`、`UIElement.IsFocused` 等控件自身的交互状态属性。

#### 2. `Value` 属性

csharp:

```c#
public object Value { get; set; }
```

- **官方定义**：获取或设置触发条件的目标比较值。
- **触发逻辑**：当 `Property` 指定的依赖属性的当前值，与 `Value` 相等时，触发器激活。
- **类型转换规则**：
  - XAML 中写入的字符串值，WPF 会自动通过对应类型的 `TypeConverter` 转换为目标类型（如 `"True"` 转布尔值、`"Red"` 转颜色）；
  - 枚举、布尔类型推荐使用 `{x:Static}` 显式指定类型，避免隐式转换失败导致触发器不生效。
- **能力边界**：仅支持**相等比较**，不支持大于、小于、区间、包含等逻辑；复杂判断需要配合值转换器，或改用 `DataTrigger`。

#### 3. `Setters` 属性

csharp:

```c#
public SetterBaseCollection Setters { get; }
```

- **官方定义**：获取触发器激活时要应用的属性设置器集合。
- **核心作用**：触发器激活时，将集合中所有 `Setter` 的属性值，以「样式触发器」优先级写入目标控件的依赖属性系统；触发器失活时自动移除这些值，UI 自动恢复原状。
- **集合特性**：
  - 属性本身为只读（不能重新赋值集合对象），但可以向集合内添加、移除 `Setter` 元素；
  - 触发器密封后，集合自动变为只读，无法再修改。
- **元素类型**：集合元素为 `SetterBase` 派生类，绝大多数场景使用 `Setter`，也支持 `EventSetter` 等特殊设置器。

#### 4. `SourceName` 属性

csharp:

```c#
public string SourceName { get; set; }
```

- **官方定义**：获取或设置触发器所监听的目标对象的名称。

- **核心用途**：在 `ControlTemplate`（控件模板）或 `DataTemplate`（数据模板）内部使用时，指定模板内某个已命名元素的 `x:Name`，让触发器监听模板内子元素的属性，而非模板根控件本身。

- **默认行为**：不设置 `SourceName` 时，默认监听样式 / 模板的目标控件本身。

- **典型示例**：

  xaml:

  ```xaml
  <ControlTemplate TargetType="Button">
      <Border x:Name="BtnBorder" Background="Blue">
          <ContentPresenter/>
      </Border>
      <ControlTemplate.Triggers>
          <!-- 监听模板内名为 BtnBorder 的元素的 IsMouseOver 属性 -->
          <Trigger Property="IsMouseOver" Value="True" SourceName="BtnBorder">
              <Setter TargetName="BtnBorder" Property="Background" Value="Red"/>
          </Trigger>
      </ControlTemplate.Triggers>
  </ControlTemplate>
  ```

### `IAddChild` 接口实现

`Trigger` 实现了 `IAddChild` 接口，包含两个方法：

- `AddChild(object child)`：XAML 解析时，将 `<Trigger>` 内的子元素（如 `<Setter>`）自动添加到 `Setters` 集合；
- `AddText(string text)`：处理标签内的文本内容，触发器场景几乎不使用。

这是 WPF XAML 声明式语法的底层支撑，业务开发无需手动调用。

------

## 四、官方设计核心原理总结

### 1. 深度绑定依赖属性系统

`Trigger` 的所有能力都建立在 WPF 依赖属性机制之上：

- **监听能力**：依赖属性的变更回调机制，让触发器可以感知属性值变化；
- **自动恢复**：依赖属性的多优先级值机制，让触发器设置的值只是临时叠加，移除后自动回退到下一级优先级的值，无需手动写反向逻辑。

### 2. 声明式状态机设计

每个 `Trigger` 本质是一个「条件 → 动作」的声明式状态节点：

- 无需编写后台事件代码，纯 XAML 即可实现 UI 状态切换；
- 条件与动作集中定义，代码可读性、可维护性远优于分散的事件处理函数。

### 3. 密封与不可变性

`Trigger` 设计为密封类，且运行时自动密封：

- 避免第三方自定义触发器破坏框架的统一行为；
- 不可变性保证了渲染线程的安全性，便于框架做批量渲染优化。

### 4. 统一的基类抽象

`TriggerBase` 统一了所有触发器的进入 / 退出动作、密封机制，保证了属性触发器、数据触发器、事件触发器的行为一致性，降低学习与维护成本。