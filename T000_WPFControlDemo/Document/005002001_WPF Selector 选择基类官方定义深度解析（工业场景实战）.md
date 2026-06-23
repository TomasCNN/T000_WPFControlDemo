# 005002001_WPF `Selector` 选择基类官方定义深度解析（工业场景实战）

`Selector` 是 WPF 所有具备**选择能力**的集合控件的**抽象基类**，直接继承自 `ItemsControl`，在「集合数据可视化」的基础上，新增了统一的选中状态管理、选择变更通知、选中值映射、集合视图同步等核心能力。

`ListBox`、`ComboBox`、`ListView`、`TabControl` 等工业软件常用控件全部派生自该类，是设备选择、配方下拉、报警筛选、参数配置等交互场景的底层支撑。

本文基于 .NET 官方源码，从类定义、属性、事件、方法、底层机制到工业实战案例，完整解析其能力与用法。

------

## 一、官方类定义与核心元数据

### 1.1 完整抽象类签名

csharp:

```c#
namespace System.Windows.Controls.Primitives
{
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public abstract class Selector : ItemsControl
    {
        // 静态依赖属性
        public static readonly DependencyProperty SelectedIndexProperty;
        public static readonly DependencyProperty SelectedItemProperty;
        public static readonly DependencyProperty SelectedValueProperty;
        public static readonly DependencyProperty SelectedValuePathProperty;
        public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty;
        public static readonly DependencyProperty IsSelectedProperty; // 附加属性

        // 构造函数（受保护，抽象类不可直接实例化）
        protected Selector();

        // 公共实例属性
        public int SelectedIndex { get; set; }
        public object SelectedItem { get; set; }
        public object SelectedValue { get; set; }
        public string SelectedValuePath { get; set; }
        public bool? IsSynchronizedWithCurrentItem { get; set; }

        // 核心事件
        public event SelectionChangedEventHandler SelectionChanged;

        // 静态辅助方法（附加属性包装）
        public static bool GetIsSelected(DependencyObject element);
        public static void SetIsSelected(DependencyObject element, bool value);
        public static int GetSelectedIndex(DependencyObject element);
        public static void SetSelectedIndex(DependencyObject element, int value);

        // 受保护扩展虚方法
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e);
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(DependencyObject element, object item);
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
        protected virtual void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs e);
        protected virtual void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e);
        protected virtual void OnSelectedValueChanged(DependencyPropertyChangedEventArgs e);
    }
}
```

### 1.2 核心元数据

| 项           | 官方精确值                                                   | 工业场景说明                                                 |
| :----------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| 命名空间     | `System.Windows.Controls.Primitives`                         | 控件基类所在的 Primitives 命名空间                           |
| 程序集       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| 完整继承链   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector` | 在集合呈现能力之上扩展选择语义                               |
| 类类型       | 抽象类                                                       | 不可直接实例化，仅作为基类被子类继承                         |
| 核心直接子类 | `ListBox`、`ComboBox`、`TabControl`                          | 多选基类 `MultiSelector` 也继承自它（`DataGrid` 等派生自 MultiSelector） |
| 设计定位     | 可选择集合控件的统一抽象                                     | 封装所有选择相关的通用逻辑，子类只需扩展交互与样式           |
| 工业核心场景 | 设备选型、配方选择、报警筛选、参数下拉、列表主从详情         | 所有「列表选择 + 后续操作」的交互底层                        |

------

## 二、核心依赖属性全量解析

所有属性均为依赖属性，完整支持数据绑定、样式、动画。按功能分为三类：

### 2.1 选中核心属性

| 属性字段                    | 包装属性            | 类型     | 默认值         | 官方作用                                   | 工业最佳实践                                                 |
| :-------------------------- | :------------------ | :------- | :------------- | :----------------------------------------- | :----------------------------------------------------------- |
| `SelectedIndexProperty`     | `SelectedIndex`     | `int`    | `-1`           | 当前选中项的索引，-1 表示未选中            | 适合通过索引定位的场景，如自动选中第一条、滚动到指定行       |
| `SelectedItemProperty`      | `SelectedItem`      | `object` | `null`         | 当前选中的数据项对象本身                   | 主从详情场景首选，直接绑定选中的完整业务对象                 |
| `SelectedValueProperty`     | `SelectedValue`     | `object` | `null`         | 选中项按 `SelectedValuePath` 提取的属性值  | 数据提交场景首选，只传 ID / 编码等关键字段，轻量化交互       |
| `SelectedValuePathProperty` | `SelectedValuePath` | `string` | `string.Empty` | 指定从选中项的哪个属性提取 `SelectedValue` | 与 `DisplayMemberPath` 配对使用：显示用 DisplayMemberPath，提交值用 SelectedValuePath |

> 🔑 易混点澄清：
>
> - `DisplayMemberPath`：控制**界面上显示**对象的哪个属性；
> - `SelectedValuePath`：控制 `SelectedValue` 取对象的哪个属性值；
> - 二者完全独立，工业场景通常：显示名称，提交 ID。

### 2.2 同步控制属性

| 属性字段                                | 包装属性                        | 类型    | 默认值         | 官方作用                                                     |
| :-------------------------------------- | :------------------------------ | :------ | :------------- | :----------------------------------------------------------- |
| `IsSynchronizedWithCurrentItemProperty` | `IsSynchronizedWithCurrentItem` | `bool?` | `null`（自动） | 是否与 `ItemsSource` 对应的 `ICollectionView` 集合视图的 `CurrentItem` 保持同步 |

- 取值说明：
  - `true`：强制同步，选中项变化会同步更新集合视图的当前指针，反之亦然；
  - `false`：不同步；
  - `null`（默认）：自动判断，若数据源是集合视图则自动同步。
- 工业价值：主从视图（左侧列表、右侧详情）场景下，开启后多个控件可共享同一个集合视图的当前项，无需手动绑定 `SelectedItem`。

### 2.3 附加属性：IsSelected

csharp:

```c#
public static readonly DependencyProperty IsSelectedProperty;
```

- **作用对象**：附加在每个条目容器（如 `ListBoxItem`、`ComboBoxItem`）上，标记该容器是否被选中。
- **访问方式**：通过 `Selector.GetIsSelected(container)` / `Selector.SetIsSelected(container, value)` 读写。
- **核心价值**：
  1. `Selector` 内部通过设置该属性控制容器的选中状态；
  2. 样式触发器可绑定该属性，实现选中态视觉效果（高亮、变色、边框）；
  3. 所有子类的 `IsSelected` 容器属性，本质都是对该附加属性的包装。

------

## 三、核心事件：SelectionChanged

csharp:

```c#
public event SelectionChangedEventHandler SelectionChanged;
```

### 3.1 触发时机

当选中项发生变化时触发，包括：

- 用户点击选择 / 取消选择；
- 代码修改 `SelectedItem` / `SelectedIndex` / `SelectedValue`；
- 数据源变化导致选中项变更。

### 3.2 事件参数

`SelectionChangedEventArgs` 包含两个核心集合：

| 属性           | 类型    | 说明             |
| :------------- | :------ | :--------------- |
| `AddedItems`   | `IList` | 本次新增选中的项 |
| `RemovedItems` | `IList` | 本次取消选中的项 |

### 3.3 工业场景使用建议

- 轻量逻辑可直接在事件中处理，如更新按钮状态、刷新关联数据；
- **禁止在事件中执行耗时操作**（数据库查询、设备通信、IO 操作），否则会直接阻塞 UI 线程，造成界面卡顿；耗时操作请异步执行。
- MVVM 场景优先通过绑定 `SelectedItem` 实现响应，尽量不用事件回调，保持逻辑在 ViewModel 中。

------

## 四、核心方法解析

### 4.1 静态辅助方法

均为附加属性的强类型包装方法，用于操作条目容器的选中状态：

1. **`GetIsSelected / SetIsSelected`**
   - 读取 / 设置指定容器的选中状态；
   - 自定义控件或视觉树操作时使用，常规 MVVM 场景不需要直接调用。
2. **`GetSelectedIndex / SetSelectedIndex`**
   - 读取 / 设置指定 Selector 的选中索引，一般用实例属性替代。

### 4.2 受保护扩展虚方法

这是自定义选择控件的核心扩展点，`Selector` 重写了 `ItemsControl` 的容器生命周期方法，注入选中逻辑：

#### 1. `PrepareContainerForItemOverride`

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **官方扩展逻辑**：在基类准备容器的基础上，同步该数据项的选中状态到容器的 `IsSelected` 属性。
- **虚拟化适配**：容器复用时，根据数据项的保存状态恢复 `IsSelected`，底层依赖 `IContainItemStorage` 接口持久化状态。

#### 2. `ClearContainerForItemOverride`

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **官方扩展逻辑**：容器回收前，清理容器的选中状态，避免复用时状态错乱。
- 自定义子类如果扩展了选中相关属性，必须在此方法中对应清理，否则虚拟化滚动会出现状态残留。

#### 3. `OnSelectionChanged`

csharp:

```c#
protected virtual void OnSelectionChanged(SelectionChangedEventArgs e);
```

- **作用**：选中变更的核心入口，默认实现触发 `SelectionChanged` 事件。
- **子类扩展**：重写此方法可在选中变化时注入自定义逻辑，如联动其他控件、执行校验，比订阅事件更高效。

#### 4. `OnItemsChanged`

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- **官方逻辑**：集合增删改时，同步维护选中索引与选中项的有效性；例如删除选中项后，自动将选中索引调整为 -1 或相邻项。

------

## 五、核心工作机制

### 5.1 选中状态的两层映射

`Selector` 维护了「数据层 ↔ 容器层」的双向选中映射：

1. **数据层**：`SelectedItem` / `SelectedIndex` / `SelectedValue`，面向业务逻辑，与数据绑定；
2. **容器层**：每个条目容器的 `IsSelected` 附加属性，面向 UI 呈现。
3. **同步机制**：任意一侧变化，都会自动同步到另一侧；虚拟化模式下，不可见的容器不存在，但数据层状态始终有效，容器生成时自动恢复。

### 5.2 选择变更完整流程

以用户点击选中某行为例：

1. 条目容器接收鼠标点击，触发选中交互；
2. 更新 `SelectedItem` / `SelectedIndex` 依赖属性；
3. 触发属性变更回调，更新所有容器的 `IsSelected` 状态（旧项取消、新项选中）；
4. 若开启同步，更新 `ICollectionView` 的 `CurrentItem` 指针；
5. 调用 `OnSelectionChanged`，触发 `SelectionChanged` 事件。

### 5.3 虚拟化下的状态持久化

结合 `ItemsControl` 的 `IContainItemStorage` 接口：

- 容器滚出屏幕被回收时，将该数据项的 `IsSelected` 状态存入存储接口；
- 新数据滚入屏幕、容器复用时，从存储接口读取状态并恢复到容器上；
- **效果**：无论滚动多远，选中状态始终与数据项绑定，不会因为虚拟化回收而丢失或错乱。
- 工业价值：几千上万条的报警列表、生产记录，开启虚拟化后选中功能依然稳定可用。

### 5.4 SelectedValue 映射原理

1. 设置 `SelectedValuePath = "DeviceId"`；
2. 选中某一项时，框架通过反射获取该数据项的 `DeviceId` 属性值；
3. 将值赋值给 `SelectedValue` 属性；
4. 反向：设置 `SelectedValue` 时，遍历集合找到对应属性值匹配的数据项，设为选中项。

------

## 六、核心使用场景与基础用法

| 场景                        | 推荐方案                                                   | 优势                                 |
| :-------------------------- | :--------------------------------------------------------- | :----------------------------------- |
| 主从详情（列表 + 详情面板） | 绑定 `SelectedItem` 到 ViewModel 属性                      | 直接获取完整业务对象，详情页直接绑定 |
| 下拉选择、参数提交          | `DisplayMemberPath` 显示名称 + `SelectedValuePath` 绑定 ID | 提交时只传关键字段，轻量化、解耦     |
| 多控件联动                  | `IsSynchronizedWithCurrentItem="True"` + 共享集合视图      | 多个列表自动同步选中项，无需额外代码 |
| 自定义选中样式              | `ItemContainerStyle` 中绑定 `Selector.IsSelected` 触发器   | 纯样式实现，不侵入数据模型           |

------

## 七、工业场景实战案例

### 案例 1：设备列表主从视图（SelectedItem 绑定）

#### 场景说明

左侧设备列表，选中某台设备后，右侧显示该设备的实时参数、运行状态，是工业监控系统的经典布局。

#### 1. ViewModel

csharp:

```
public class DeviceMonitorViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }

    private DeviceInfo _selectedDevice;
    public DeviceInfo SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            OnPropertyChanged();
            // 选中变化后可加载详情数据
            LoadDeviceDetail(value);
        }
    }

    // 构造函数初始化数据略
}
```

#### 2. XAML 界面

xml:

```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="250"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧设备列表：ListBox 继承自 Selector -->
    <ListBox Grid.Column="0" 
             ItemsSource="{Binding DeviceList}"
             SelectedItem="{Binding SelectedDevice}"
             DisplayMemberPath="DeviceName"/>

    <!-- 右侧设备详情 -->
    <Border Grid.Column="1" Margin="10" Background="#F8F9FA" Padding="15">
        <StackPanel DataContext="{Binding SelectedDevice}">
            <TextBlock FontSize="16" FontWeight="Bold" Text="{Binding DeviceName}"/>
            <TextBlock Margin="0 10" Text="{Binding Temperature, StringFormat=当前温度：{0:F1}℃}"/>
            <TextBlock Text="{Binding Status, StringFormat=运行状态：{0}}"/>
        </StackPanel>
    </Border>
</Grid>
```

#### 核心对应特性

- `SelectedItem` 双向绑定，选中变化自动同步到 ViewModel；
- 完全 MVVM 模式，无后台代码，逻辑可单元测试。

------

### 案例 2：配方下拉选择（SelectedValuePath + SelectedValue）

#### 场景说明

下拉框选择生产配方，界面显示配方名称，后台只需要提交配方 ID，是参数配置、工单下发的常用模式。

#### 1. 数据模型

csharp:

```c#
public class RecipeInfo
{
    public int RecipeId { get; set; }   // 配方ID，提交用
    public string RecipeName { get; set; } // 配方名称，显示用
    public string Description { get; set; }
}
```

#### 2. ViewModel

csharp:

```c#
public class RecipeViewModel : INotifyPropertyChanged
{
    public ObservableCollection<RecipeInfo> RecipeList { get; set; }

    private int _selectedRecipeId;
    public int SelectedRecipeId
    {
        get => _selectedRecipeId;
        set { _selectedRecipeId = value; OnPropertyChanged(); }
    }
}
```

#### 3. XAML 界面

xaml:

```xaml
<ComboBox ItemsSource="{Binding RecipeList}"
          DisplayMemberPath="RecipeName"   <!-- 界面显示配方名 -->
          SelectedValuePath="RecipeId"     <!-- 选中值取RecipeId属性 -->
          SelectedValue="{Binding SelectedRecipeId}" <!-- 绑定到ViewModel的ID -->
          Width="200" HorizontalAlignment="Left"/>
```

#### 核心对应特性

- 显示与提交值分离，符合工业系统「界面友好、数据精简」的交互原则；
- 提交工单时只需传递 `SelectedRecipeId`，无需序列化整个配方对象。

------

### 案例 3：自定义选中行样式（IsSelected 触发器）

#### 场景说明

报警列表中，选中行高亮显示，左侧增加蓝色指示条，替代默认的灰色选中样式，适配工业深色主题。

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmList}"
         DisplayMemberPath="Message">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Foreground" Value="#CCC"/>
            <Setter Property="Padding" Value="8 4"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="ListBoxItem">
                        <DockPanel Background="{TemplateBinding Background}">
                            <!-- 左侧选中指示条 -->
                            <Border DockPanel.Dock="Left" Width="3" x:Name="IndicatorBar" Background="Transparent"/>
                            <ContentPresenter Margin="{TemplateBinding Padding}"/>
                        </DockPanel>
                        
                        <ControlTemplate.Triggers>
                            <!-- 绑定 Selector.IsSelected 附加属性，选中时变化样式 -->
                            <Trigger Property="Selector.IsSelected" Value="True">
                                <Setter Property="Background" Value="#1A3A6B"/>
                                <Setter Property="Foreground" Value="White"/>
                                <Setter TargetName="IndicatorBar" Property="Background" Value="#1677FF"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </ListBox.ItemContainerStyle>
</ListBox>
```

#### 核心对应特性

- 通过 `Selector.IsSelected` 附加属性触发器控制视觉状态；
- 纯样式实现，与业务数据完全解耦，可全局复用。

------

### 案例 4：多选批量操作（ListBox 多选扩展）

#### 场景说明

批量选中多条报警记录，执行确认、导出、删除等批量操作。`ListBox` 继承自 `Selector` 并扩展了多选能力。

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmList}"
         SelectionMode="Extended"  <!-- 按住Ctrl/Shift多选 -->
         x:Name="AlarmListBox">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Message}"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>

<Button Content="批量确认" Click="BatchConfirm_Click" HorizontalAlignment="Right" Margin="0 10"/>
```

csharp:

```c#
private void BatchConfirm_Click(object sender, RoutedEventArgs e)
{
    // AlarmListBox.SelectedItems 为选中项集合
    foreach (var item in AlarmListBox.SelectedItems.Cast<AlarmRecord>())
    {
        item.IsConfirmed = true;
    }
}
```

> 说明：多选能力是 `ListBox` 等子类扩展的，`Selector` 基类本身只定义了单选核心模型；官方多选抽象基类为 `MultiSelector`，`DataGrid` 等控件继承自它。

------

## 八、工业场景最佳实践与避坑指南

### 8.1 最佳实践

1. **数据驱动优先**：始终通过绑定 `SelectedItem` / `SelectedValue` 操作选中状态，不要直接操作 UI 容器；虚拟化下不可见的容器不存在，操作容器会失效。
2. **场景化选择绑定方式**：
   - 主从详情、需要完整对象 → 绑定 `SelectedItem`；
   - 下拉提交、只需要关键字段 → 绑定 `SelectedValue` + `SelectedValuePath`。
3. **选中样式用样式实现**：通过 `ItemContainerStyle` 绑定 `IsSelected` 触发器，不要在数据模型中加 `IsSelected` 属性，保持数据与 UI 分离。
4. **长列表开启虚拟化**：选中状态由 `Selector` 底层持久化存储，不会因为滚动丢失，放心开启 `VirtualizingStackPanel`。
5. **耗时操作异步化**：`SelectionChanged` 事件中不要执行设备通信、数据库查询等耗时操作，避免阻塞 UI。

### 8.2 常见坑点

1. **混淆 DisplayMemberPath 和 SelectedValuePath**
   - 现象：显示正常，但选中值取不到；
   - 解决：`DisplayMemberPath` 管显示，`SelectedValuePath` 管选中值的属性路径，二者要分别设置。
2. **选中项不更新 UI**
   - 现象：代码修改了 `SelectedItem`，界面没变化；
   - 原因：数据项未实现 `INotifyPropertyChanged`，或引用不匹配（不是集合中的同一个对象实例）。
3. **虚拟化下遍历容器找不到选中项**
   - 现象：通过索引找容器，滚出屏幕后返回 null；
   - 解决：永远操作数据层的 `SelectedItem`，不要操作 UI 容器；必须操作容器时，先确保该项在可见区域。
4. **SelectionChanged 多次触发**
   - 现象：初始化、集合变更时意外触发多次；
   - 解决：增加判空逻辑，`SelectedItem` 为 null 时不执行业务逻辑。

------

## 总结

`Selector` 是 WPF 集合控件体系中承上启下的关键基类：向下继承 `ItemsControl` 的集合呈现能力，向上为所有可选择控件提供统一的选中语义与底层机制。理解它的属性、事件、状态同步机制，不仅能正确使用 `ListBox`/`ComboBox` 等常用控件，也是自定义工业专用选择列表的基础。