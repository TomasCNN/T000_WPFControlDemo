# 008004003_WPF `Trigger` 属性触发器官方类定义深度解析

`Trigger` 是 WPF 触发器体系中最基础、最常用的属性触发器，属于样式 / 模板体系的核心类。它通过监听**控件自身的依赖属性**，当属性值与指定值相等时，自动应用一组 UI 属性设置，条件消失后自动恢复原状，是实现控件交互状态的声明式核心工具，完全无需编写后台事件代码。

------

## 一、官方类定义完整解析

### 1. 基础元数据

| 项           | 官方值                                                       |
| :----------- | :----------------------------------------------------------- |
| **命名空间** | `System.Windows`                                             |
| **程序集**   | `PresentationFramework.dll`                                  |
| **继承链**   | `Object` → `DispatcherObject` → `DependencyObject` → `TriggerBase` → `Trigger` |
| **类性质**   | 密封类（不可继承），可直接实例化；属于依赖对象，自身属性也支持依赖属性系统 |

### 2. 继承体系

#### （1）抽象基类 `TriggerBase`

所有触发器（`Trigger`、`MultiTrigger`、`DataTrigger`、`MultiDataTrigger`、`EventTrigger`）的共同基类，定义了触发器的通用能力。

csharp:

```c#
public abstract class TriggerBase : DependencyObject
{
    // 进入触发状态时执行的动作集合（如播放动画）
    public TriggerActionCollection EnterActions { get; }
    // 退出触发状态时执行的动作集合
    public TriggerActionCollection ExitActions { get; }
    // 是否已密封（样式加载后自动密封，不可再修改）
    public bool IsSealed { get; }
    // 密封触发器，使其不可修改
    public void Seal();
}
```

#### （2）`Trigger` 类官方签名

csharp:

```c#
public class Trigger : TriggerBase, IAddChild
{
    // 要监听的依赖属性
    public DependencyProperty Property { get; set; }
    // 触发条件的目标值
    public object Value { get; set; }
    // 条件满足时应用的属性设置集合
    public SetterBaseCollection Setters { get; }
}
```

- 实现 `IAddChild` 接口：支持 XAML 中直接在 `<Trigger>` 标签内写 `<Setter>` 子元素，由 XAML 解析器自动添加到 `Setters` 集合。

------

### 3. 核心属性深度解析

#### （1）`Property`：监听的目标依赖属性

- **类型**：`DependencyProperty`
- **官方作用**：指定触发器要监听的控件依赖属性，比如 `UIElement.IsMouseOverProperty`、`Button.IsPressedProperty`。
- **硬性约束**：必须是**依赖属性**，普通 CLR 属性无法被 Trigger 监听，这是最基础的规则。
- **XAML 简写**：XAML 中直接写属性名（如 `Property="IsMouseOver"`），WPF 会自动转换为对应依赖属性字段。

#### （2）`Value`：触发条件的目标值

- **类型**：`object`
- **官方作用**：指定触发的匹配值，当 `Property` 对应的属性值等于该值时，触发器激活。
- **类型匹配**：WPF 会自动进行类型转换（如字符串 `"True"` 转布尔值），但严谨写法推荐使用 `{x:Static}` 显式指定类型，避免隐式转换失败。
- **注意**：仅支持**相等比较**，不支持大于、小于、包含等逻辑；需要范围判断时应使用数据触发器 + 转换器。

#### （3）`Setters`：条件满足时的属性设置集合

- **类型**：`SetterBaseCollection`
- **官方作用**：触发器激活时，将集合内所有 `Setter` 的属性值应用到目标控件。
- **内容**：集合元素为 `Setter` 对象，每个 Setter 指定一个属性名和对应的值。
- **生命周期**：触发器激活时应用，失活时自动撤销，不会永久修改属性的基础值。

#### （4）`EnterActions` / `ExitActions`：进入 / 退出动作

- **类型**：`TriggerActionCollection`
- **官方作用**：触发器进入激活状态时执行 `EnterActions` 中的动作，退出时执行 `ExitActions` 中的动作。
- **典型用法**：播放故事板动画（`BeginStoryboard`）、控制媒体播放等；属性赋值优先用 `Setters`，动作用动作集合。

------

### 4. 核心方法

- `Seal()`：继承自 `TriggerBase`，密封触发器使其不可修改。样式加载完成后 WPF 会自动调用，运行时再修改触发器属性会抛出异常。
- 常规业务开发几乎不会手动调用，均由框架自动处理。

------

## 二、核心功能与底层工作原理

### 1. 核心功能定位

`Trigger` 是**单条件属性触发器**，专门用于响应控件自身 UI 属性的变化，实现「状态变 → 外观自动变」的声明式交互，核心价值：

1. 纯 XAML 实现交互状态，无需后台 `MouseEnter`/`MouseLeave` 等事件代码；
2. 自动恢复原状，不需要编写反向逻辑，减少样板代码；
3. 统一样式内的交互行为，全项目体验一致。

### 2. 底层支撑：依赖属性系统

Trigger 的所有能力都建立在 WPF 依赖属性系统之上：

1. **监听机制**：触发器初始化时，会向目标属性注册变更回调，属性值变化时自动收到通知；
2. **优先级机制**：触发器设置的值属于「样式触发器」优先级，低于本地值、动画值，高于样式静态 Setter、默认值；
3. **自动恢复**：触发器失活时，只是移除自己设置的优先级值，依赖属性系统会自动回退到下一级的值，实现无代码恢复。

### 3. 完整触发与恢复流程

#### 激活流程（条件满足）

1. 监听的依赖属性值发生变化；
2. 触发器比较当前值与 `Value`，判断条件满足；
3. 将 `Setters` 中所有属性值，以「样式触发器」优先级写入目标元素的依赖属性系统；
4. 执行 `EnterActions` 中的所有动作；
5. UI 呈现触发后的效果。

#### 失活流程（条件消失）

1. 属性值变化，不再匹配 `Value`；
2. 执行 `ExitActions` 中的所有动作；
3. 移除本次触发器设置的所有属性值；
4. 依赖属性系统自动回退到下一级优先级的值（通常是样式静态 Setter 的值）；
5. UI 自动恢复到触发前的状态。

> 核心特性：**临时叠加、自动撤销**，触发器不会修改属性的基础值，只是在优先级层面叠加效果，这是它能自动恢复的根本原因。

### 4. 依赖属性值优先级（避坑核心）

优先级从高到低排序，这是所有「触发器不生效」问题的根源：

1. **本地值**（控件上直接写的属性、后台代码赋值）
2. 动画值
3. **触发器值**（Trigger / DataTrigger 等设置的值）
4. 样式静态 Setter 值
5. 属性值继承
6. 依赖属性默认值

> 90% 的「触发器写了没效果」，都是因为控件上写了同名本地值，本地值优先级高于触发器，直接覆盖了触发器的效果。

------

## 三、标准使用方法

### 1. 基础语法结构

Trigger 必须放在触发器集合中，最常用的宿主是样式：

xaml:

```xaml
<Style TargetType="目标控件类型">
    <!-- 静态默认属性 -->
    <Setter Property="属性名" Value="默认值"/>

    <Style.Triggers>
        <!-- 单属性触发器 -->
        <Trigger Property="监听的依赖属性" Value="触发值">
            <!-- 条件满足时应用的属性 -->
            <Setter Property="目标属性" Value="设置值"/>
            <!-- 可写多个Setter -->
        </Trigger>
    </Style.Triggers>
</Style>
```

### 2. 两个常用宿主位置

#### （1）样式触发器（最常用）

写在 `Style.Triggers` 中，作用于整个控件，只能设置控件自身的属性，适用于统一定义控件交互状态。

#### （2）模板触发器

写在 `ControlTemplate.Triggers` 或 `DataTemplate.Triggers` 中，可以通过 `TargetName` 修改模板内部指定元素的属性，适用于自定义控件、自定义数据项的精细控制。

### 3. 基本使用步骤

1. 在样式 / 模板的 `Triggers` 集合中添加 `<Trigger>`；
2. 通过 `Property` 指定要监听的依赖属性；
3. 通过 `Value` 指定触发的目标值；
4. 在 `<Trigger>` 内添加 `<Setter>`，定义条件满足时的 UI 变化；
5. （可选）添加 `EnterActions` / `ExitActions` 执行动画等动作。

------

## 四、实战实例（工业场景导向）

### 实例 1：按钮交互三态（入门必学）

#### 应用场景

工业操作按钮的基础交互：默认、悬停、按下、禁用四种状态，纯 XAML 实现，替代后台事件代码。

#### 完整代码

xaml:

```xaml
<Window x:Class="TriggerDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Trigger 按钮三态" Height="250" Width="450">

    <Window.Resources>
        <Style x:Key="OperateButtonStyle" TargetType="Button">
            <!-- 默认状态 -->
            <Setter Property="Width" Value="120"/>
            <Setter Property="Height" Value="36"/>
            <Setter Property="Background" Value="#2E7DFF"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Margin" Value="0 0 15 0"/>

            <Style.Triggers>
                <!-- 鼠标悬停 -->
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#5597FF"/>
                </Trigger>

                <!-- 鼠标按下 -->
                <Trigger Property="IsPressed" Value="True">
                    <Setter Property="Background" Value="#1A66E0"/>
                </Trigger>

                <!-- 控件禁用 -->
                <Trigger Property="IsEnabled" Value="False">
                    <Setter Property="Background" Value="#CCCCCC"/>
                    <Setter Property="Foreground" Value="#999999"/>
                    <Setter Property="Cursor" Value="No"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <StackPanel Orientation="Horizontal" Margin="50" VerticalAlignment="Center">
        <Button Content="启动设备" Style="{StaticResource OperateButtonStyle}"/>
        <Button Content="禁用状态" Style="{StaticResource OperateButtonStyle}" IsEnabled="False"/>
    </StackPanel>
</Window>
```

#### 核心要点

- 只需要写满足条件的触发器，条件消失自动恢复 Setter 的默认值，不需要写反向触发器；
- 多个触发器同时满足时，后定义的优先级更高。

------

### 实例 2：文本框聚焦与只读状态

#### 应用场景

参数输入框的交互：获得焦点时高亮边框、只读状态时灰底，统一所有输入框的交互体验。

#### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style TargetType="TextBox">
        <Setter Property="Width" Value="280"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Padding" Value="6 4"/>
        <Setter Property="BorderBrush" Value="#CCCCCC"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Background" Value="White"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>

        <Style.Triggers>
            <!-- 获得键盘焦点：边框变主题色 -->
            <Trigger Property="IsKeyboardFocused" Value="True">
                <Setter Property="BorderBrush" Value="#2E7DFF"/>
                <Setter Property="BorderThickness" Value="2"/>
            </Trigger>

            <!-- 只读状态：灰底，标识不可编辑 -->
            <Trigger Property="IsReadOnly" Value="True">
                <Setter Property="Background" Value="#F5F6FA"/>
                <Setter Property="Foreground" Value="#666"/>
            </Trigger>

            <!-- 禁用状态 -->
            <Trigger Property="IsEnabled" Value="False">
                <Setter Property="Background" Value="#F0F0F0"/>
                <Setter Property="Foreground" Value="#AAA"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="40" Spacing="15">
    <TextBox Text="可编辑参数：1500"/>
    <TextBox Text="只读参数：设备序列号" IsReadOnly="True"/>
    <TextBox Text="禁用参数：出厂配置" IsEnabled="False"/>
</StackPanel>
```

------

### 实例 3：控件模板内触发器（TargetName）

#### 应用场景

自定义按钮模板，通过触发器修改模板内部指定元素的属性，实现更精细的外观控制。

#### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style x:Key="CustomButtonStyle" TargetType="Button">
        <Setter Property="Width" Value="120"/>
        <Setter Property="Height" Value="36"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>

        <!-- 自定义控件模板 -->
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="BtnBorder" 
                            Background="#2E7DFF" 
                            CornerRadius="4"
                            Padding="15 0">
                        <ContentPresenter HorizontalAlignment="Center" 
                                          VerticalAlignment="Center"/>
                    </Border>

                    <!-- 模板内触发器 -->
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <!-- 通过TargetName修改模板内的Border -->
                            <Setter TargetName="BtnBorder" 
                                    Property="Background" 
                                    Value="#5597FF"/>
                        </Trigger>

                        <Trigger Property="IsPressed" Value="True">
                            <Setter TargetName="BtnBorder" 
                                    Property="Background" 
                                    Value="#1A66E0"/>
                        </Trigger>

                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="BtnBorder" 
                                    Property="Background" 
                                    Value="#CCC"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</Window.Resources>

<Button Content="自定义按钮" Style="{StaticResource CustomButtonStyle}" Margin="50"/>
```

#### 核心要点

- 模板内触发器通过 `TargetName` 指定要修改的元素，名称必须在模板内定义；
- 可以控制模板内部任意子元素，是自定义控件外观的核心手段。

------

### 实例 4：EnterActions 触发动画

#### 应用场景

鼠标悬停时播放平滑过渡动画，提升交互质感。

xaml:

```xaml
<Style TargetType="Button">
    <Setter Property="RenderTransformOrigin" Value="0.5 0.5"/>
    <Setter Property="RenderTransform">
        <Setter.Value>
            <ScaleTransform ScaleX="1" ScaleY="1"/>
        </Setter.Value>
    </Setter>

    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <!-- 进入悬停时播放放大动画 -->
            <Trigger.EnterActions>
                <BeginStoryboard>
                    <Storyboard Duration="0:0:0.2">
                        <DoubleAnimation To="1.05" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleX"/>
                        <DoubleAnimation To="1.05" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleY"/>
                    </Storyboard>
                </BeginStoryboard>
            </Trigger.EnterActions>

            <!-- 退出悬停时播放还原动画 -->
            <Trigger.ExitActions>
                <BeginStoryboard>
                    <Storyboard Duration="0:0:0.2">
                        <DoubleAnimation To="1" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleX"/>
                        <DoubleAnimation To="1" 
                                         Storyboard.TargetProperty="RenderTransform.ScaleY"/>
                    </Storyboard>
                </BeginStoryboard>
            </Trigger.ExitActions>
        </Trigger>
    </Style.Triggers>
</Style>
```

------

## 五、官方注意事项与最佳实践

### 1. 仅支持依赖属性

`Property` 必须是依赖属性，普通 CLR 属性无法被监听，也无法触发 UI 更新。

- 验证方式：属性字段名通常以 `Property` 结尾（如 `IsMouseOverProperty`），是 `public static readonly DependencyProperty` 类型。

### 2. 自动恢复，不要画蛇添足

Trigger 条件消失后会自动撤销所有设置，不需要再写一个「值不相等」的反向触发器。

- ❌ 错误：同时写 `IsMouseOver=True` 和 `IsMouseOver=False` 两个触发器；
- ✅ 正确：只写满足条件的触发器，默认值写在样式静态 Setter 中。

### 3. 本地值覆盖陷阱

控件上直接赋值的本地值，优先级高于触发器，会导致 Trigger 完全失效。

xaml:

```xaml
<!-- ❌ 错误：本地写了Background，触发器改不动 -->
<Button Content="按钮" Background="Red" Style="{StaticResource BtnStyle}"/>

<!-- ✅ 正确：移除本地值，完全由样式+触发器控制 -->
<Button Content="按钮" Style="{StaticResource BtnStyle}"/>
```

### 4. 触发器顺序决定优先级

同一个样式中，多个触发器同时满足条件时，**后定义的优先级更高**。

- 最佳实践：按优先级从低到高排列，高优先级状态（如禁用、故障）写在最后。

### 5. 仅支持相等比较

Trigger 只能做「等于」判断，不支持大于、小于、区间、包含等逻辑。

- 需要范围判断：使用 `DataTrigger` + 值转换器，或在 ViewModel 中新增布尔属性。

### 6. 选型边界

| 场景                                     | 推荐触发器     |
| :--------------------------------------- | :------------- |
| 控件自身 UI 属性变化（悬停、禁用、聚焦） | `Trigger`      |
| 多个控件属性同时满足才触发               | `MultiTrigger` |
| 业务数据驱动 UI 变化                     | `DataTrigger`  |
| 播放动画、事件驱动动效                   | `EventTrigger` |

### 7. 性能注意

- 单个控件的触发器数量建议控制在合理范围，过多触发器会增加属性变更时的校验开销；
- 大数据量列表的行模板中，尽量减少复杂触发器，避免影响滚动流畅度。