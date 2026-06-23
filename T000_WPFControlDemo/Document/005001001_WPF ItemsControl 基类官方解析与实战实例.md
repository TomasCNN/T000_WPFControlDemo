# 005001001_WPF `ItemsControl` 基类官方解析与实战实例

`ItemsControl` 是 WPF 所有集合类条目控件的**核心基类**，定义于 `System.Windows.Controls` 命名空间，程序集 `PresentationFramework.dll`。它的核心职责是实现「数据集合 → UI 条目列表」的自动映射，支持数据绑定、自定义条目外观、自定义布局面板、分组展示等能力。我们常用的 `ListBox`、`ComboBox`、`ListView`、`TabControl`、`Menu` 等控件，全部继承自 `ItemsControl` 并在此基础上扩展交互能力。

------

## 一、官方类定义深度解析

### 1.1 完整继承体系

plaintext：

```tex
System.Windows.Threading.DispatcherObject
  → System.Windows.DependencyObject
    → System.Windows.Media.Visual
      → System.Windows.UIElement
        → System.Windows.FrameworkElement
          → System.Windows.Controls.Control
            → System.Windows.Controls.ItemsControl
```

- 继承自 `Control`：拥有背景、边框、字体、控件模板等通用控件能力
- 核心定位：**纯展示型集合控件基类**，自身不提供选中、编辑、滚动条等交互能力，这些能力由子类（如 `ListBox`、`Selector`）扩展实现
- 非抽象类：可直接实例化使用，适合纯展示、无需选中交互的列表场景
- 设计思想：每个条目本质是内容控件的延伸，通过 `ContentPresenter` 承载单条数据的渲染，完美复用内容控件体系的能力

### 1.2 核心依赖属性

所有 `static readonly DependencyProperty` 均为 WPF 属性系统的唯一标识符，支持数据绑定、样式、动画。按功能维度分类如下：

#### 🔹 数据源类

| 依赖属性标识符              | 对应 CLR 属性       | 类型          | 默认值         | 核心说明                                                     |
| :-------------------------- | :------------------ | :------------ | :------------- | :----------------------------------------------------------- |
| `ItemsSourceProperty`       | `ItemsSource`       | `IEnumerable` | `null`         | **核心绑定属性**，指定条目数据源集合。MVVM 场景下绑定 `ObservableCollection<T>` 可实现集合变更自动同步 UI。⚠️ 与 `Items` 互斥，二者不能同时赋值。 |
| `DisplayMemberPathProperty` | `DisplayMemberPath` | `string`      | `string.Empty` | 快速显示路径：指定数据对象的某个属性名作为显示文本，无需编写 `DataTemplate`，适合简单文本列表。 |
| `ItemStringFormatProperty`  | `ItemStringFormat`  | `string`      | `null`         | 条目的字符串格式化格式，例如 `{0:yyyy-MM-dd}` 格式化日期显示。 |

#### 🔹 条目外观类

表格

| 依赖属性标识符                 | 对应 CLR 属性          | 类型                   | 默认值 | 核心说明                                                     |
| :----------------------------- | :--------------------- | :--------------------- | :----- | :----------------------------------------------------------- |
| `ItemTemplateProperty`         | `ItemTemplate`         | `DataTemplate`         | `null` | **条目外观核心**，定义每个数据项如何渲染为 UI。MVVM 场景下通过数据模板实现「数据驱动 UI」。 |
| `ItemTemplateSelectorProperty` | `ItemTemplateSelector` | `DataTemplateSelector` | `null` | 动态模板选择器：根据数据项的状态，自动选择不同的 `DataTemplate`，例如报警等级不同显示不同样式。 |

#### 🔹 布局面板类

| 依赖属性标识符       | 对应 CLR 属性 | 类型                 | 默认值            | 核心说明                                                     |
| :------------------- | :------------ | :------------------- | :---------------- | :----------------------------------------------------------- |
| `ItemsPanelProperty` | `ItemsPanel`  | `ItemsPanelTemplate` | 垂直 `StackPanel` | 定义所有条目的布局容器。默认是垂直排列的 `StackPanel`，可替换为 `WrapPanel`、`UniformGrid`、`VirtualizingStackPanel` 等，实现不同的排列效果。 |

#### 🔹 条目容器类

| 依赖属性标识符                       | 对应 CLR 属性                | 类型            | 默认值 | 核心说明                                                     |
| :----------------------------------- | :--------------------------- | :-------------- | :----- | :----------------------------------------------------------- |
| `ItemContainerStyleProperty`         | `ItemContainerStyle`         | `Style`         | `null` | 定义每个条目容器（默认 `ContentPresenter`）的样式，可设置边距、背景、触发器等。 |
| `ItemContainerStyleSelectorProperty` | `ItemContainerStyleSelector` | `StyleSelector` | `null` | 动态容器样式选择器，根据数据项状态选择不同的容器样式。       |

#### 🔹 分组展示类

| 依赖属性标识符       | 对应 CLR 属性 | 类型                               | 默认值  | 核心说明                                                     |
| :------------------- | :------------ | :--------------------------------- | :------ | :----------------------------------------------------------- |
| `GroupStyleProperty` | `GroupStyle`  | `ObservableCollection<GroupStyle>` | 空集合  | 分组样式定义，配合 `CollectionViewSource` 实现按字段分组展示，例如按报警等级、设备类型分组。 |
| `IsGroupingProperty` | `IsGrouping`  | `bool`                             | `false` | 只读属性，标识当前是否启用了分组。                           |

### 1.3 核心公共属性

- `Items`：`ItemCollection` 类型，逻辑条目集合，静态添加子元素时使用；设置 `ItemsSource` 后该集合变为只读。
- `ItemContainerGenerator`：`ItemContainerGenerator` 类型，条目容器生成器，是 ItemsControl 的核心引擎，负责将数据项转换为 UI 容器元素。
- `HasItems`：`bool` 类型，只读，标识条目集合是否有内容。
- `AlternationCount`：`int` 类型，交替行计数周期，用于实现交替行背景色。

### 1.4 核心方法

#### 公共方法

- `static ItemsControl GetItemsOwner(DependencyObject element)`：静态工具方法，获取某个容器所属的 ItemsControl 实例。

#### 可重写方法（自定义控件用）

| 方法                                                         | 作用                                                         |
| :----------------------------------------------------------- | :----------------------------------------------------------- |
| `GetContainerForItemOverride()`                              | 创建条目容器实例，默认返回 `ContentPresenter`；子类重写可返回自定义容器（如 ListBox 返回 ListBoxItem）。 |
| `IsItemItsOwnContainerOverride(object item)`                 | 判断数据项本身是否就是容器元素，默认返回 false。             |
| `PrepareContainerForItemOverride(DependencyObject element, object item)` | 容器生成后，将数据绑定到容器上，是条目渲染的关键环节。       |
| `ClearContainerForItemOverride(DependencyObject element, object item)` | 容器回收时清理数据，用于虚拟化场景下的容器复用。             |
| `OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)` | 数据源变更时的回调，可重写自定义集合变更逻辑。               |

### 1.5 核心事件

- `ItemContainerGenerator.StatusChanged`：条目生成器状态变化时触发，例如条目开始生成、生成完成。
- 继承自 `Control` / `FrameworkElement` 的通用事件：`Loaded`、`Unloaded`、`SizeChanged` 等。

> 注意：ItemsControl 自身没有 `SelectionChanged` 之类的选择事件，选择能力由子类 `Selector` 及其派生类（ListBox、ComboBox 等）提供。

------

## 二、核心功能与定位

### 2.1 核心能力

1. **集合 - UI 自动映射**：只需提供数据源集合，自动为每个数据项生成对应的 UI 元素，无需手动循环创建控件。
2. **灵活的外观定制**：通过 `ItemTemplate` 完全自定义每个条目的视觉效果，支持任意 UI 元素组合。
3. **可替换的布局面板**：通过 `ItemsPanel` 切换布局方式，支持垂直列表、水平列表、瀑布流、网格等多种排列。
4. **MVVM 友好**：完美支持数据绑定，配合 `ObservableCollection<T>` 实现集合增删改自动同步 UI，符合数据驱动开发思想。
5. **分组与模板选择**：支持按字段分组展示、动态选择模板 / 样式，适配复杂业务场景。
6. **可扩展的基类设计**：通过重写容器生成方法，可自定义条目容器与交互逻辑，是所有集合控件的基础。

### 2.2 与常见派生控件的关系

| 控件           | 继承关系                             | 扩展能力                      | 适用场景                     |
| :------------- | :----------------------------------- | :---------------------------- | :--------------------------- |
| `ItemsControl` | 基类                                 | 纯展示，无选中、无滚动        | 静态展示列表、自定义布局卡片 |
| `ListBox`      | ItemsControl → Selector → ListBox    | 单选 / 多选、滚动条、键盘导航 | 可选择的列表、数据项选择     |
| `ComboBox`     | ItemsControl → Selector → ComboBox   | 下拉选择、文本输入            | 下拉选项选择                 |
| `ListView`     | ItemsControl → Selector → ListView   | 多列视图、GridView 表格       | 多列数据表格展示             |
| `TabControl`   | ItemsControl → Selector → TabControl | 选项卡切换                    | 多标签页                     |
| `Menu`         | ItemsControl → MenuBase → Menu       | 层级菜单、命令绑定            | 顶部菜单栏、右键菜单         |

### 2.3 工业软件典型场景

1. **报警记录列表**：实时滚动展示设备报警信息，按等级区分颜色。
2. **设备状态卡片**：网格 / 瀑布流布局展示多台设备的运行状态。
3. **产能时段列表**：按小时 / 班次展示产能数据条目。
4. **参数配置列表**：动态生成配置项条目，支持增删。
5. **日志输出面板**：实时追加运行日志条目。

------

## 三、基础使用方法

### 3.1 静态条目添加

最简单的用法，直接在 XAML 中添加子元素，ItemsControl 自动将子元素加入 `Items` 集合。

xaml:

```xaml
<ItemsControl>
    <Button Content="按钮1"/>
    <TextBox Text="输入框" Margin="0,5"/>
    <TextBlock Text="文本块"/>
</ItemsControl>
```

> 特点：简单直接，适合固定内容的列表；所有子元素直接作为 UI 元素存在。

### 3.2 数据绑定（ItemsSource）

MVVM 标准用法，绑定一个集合类型的数据源。

1. ViewModel 中定义集合：

csharp:

```c#
public ObservableCollection<DeviceInfo> DeviceList { get; set; } = new ObservableCollection<DeviceInfo>();
```

1. XAML 绑定：

xaml:

```xaml
<ItemsControl ItemsSource="{Binding DeviceList}"
              DisplayMemberPath="DeviceName"/>
```

> 特点：数据驱动，集合增删自动同步 UI；`DisplayMemberPath` 指定显示的属性名，快速实现文本列表。

### 3.3 自定义条目外观（ItemTemplate）

当条目需要复杂布局（图标 + 文字 + 状态）时，通过 `DataTemplate` 定义每个条目的外观。

xaml:

```xaml
<ItemsControl ItemsSource="{Binding DeviceList}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <Ellipse Width="8" Height="8" Fill="{Binding StatusColor}" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding DeviceName}" Width="80"/>
                <TextBlock Text="{Binding CurrentYield}" Foreground="Gray"/>
            </StackPanel>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### 3.4 替换布局面板（ItemsPanel）

默认是垂直 StackPanel，通过 `ItemsPanel` 替换为其他布局面板，实现不同排列效果。

例如改为水平排列：

xaml:

```xaml
<ItemsControl ItemsSource="{Binding DeviceList}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel Orientation="Horizontal" Spacing="10"/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

常用替换面板：`WrapPanel`（自动换行卡片）、`UniformGrid`（均匀网格）、`VirtualizingStackPanel`（大数据量虚拟化）。

### 3.5 条目容器样式（ItemContainerStyle）

设置每个条目容器的样式，比如边距、背景、鼠标悬停效果。

xaml:

```xaml
<ItemsControl ItemsSource="{Binding AlarmList}">
    <ItemsControl.ItemContainerStyle>
        <Style TargetType="ContentPresenter">
            <Setter Property="Margin" Value="0,2"/>
            <Setter Property="Padding" Value="5"/>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#F0F8FF"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ItemsControl.ItemContainerStyle>
</ItemsControl>
```

> 注意：ItemsControl 的默认条目容器是 `ContentPresenter`，所以样式目标类型是 `ContentPresenter`；ListBox 的容器是 `ListBoxItem`，目标类型对应修改。

------

## 四、实战实例（工业场景适配）

### 实例 1：实时报警列表（ObservableCollection + 数据模板）

**场景**：实时展示设备报警记录，按等级显示不同颜色，新增报警自动追加到列表顶部。

#### ViewModel 代码

csharp:

```c#
public class AlarmViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// 报警集合，ObservableCollection 实现增删自动同步UI
    /// </summary>
    public ObservableCollection<AlarmInfo> AlarmList { get; set; } = new ObservableCollection<AlarmInfo>();

    public AlarmViewModel()
    {
        // 模拟初始数据
        AlarmList.Add(new AlarmInfo { Level = "一级", Content = "温度超限", Time = DateTime.Now.AddMinutes(-5), Color = "Red" });
        AlarmList.Add(new AlarmInfo { Level = "二级", Content = "节拍偏慢", Time = DateTime.Now.AddMinutes(-15), Color = "Orange" });
        AlarmList.Add(new AlarmInfo { Level = "提示", Content = "滤芯即将到期", Time = DateTime.Now.AddHours(-1), Color = "Green" });
    }

    // 模拟新增报警（实际由PLC报警事件触发）
    public void AddNewAlarm(string level, string content)
    {
        // 直接添加即可自动刷新界面（需在UI线程执行）
        AlarmList.Insert(0, new AlarmInfo
        {
            Level = level,
            Content = content,
            Time = DateTime.Now,
            Color = level == "一级" ? "Red" : "Orange"
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public class AlarmInfo
{
    public string Level { get; set; }
    public string Content { get; set; }
    public DateTime Time { get; set; }
    public string Color { get; set; }
}
```

#### XAML 布局

xaml:

```xaml
<Border BorderBrush="#DDD" BorderThickness="1" Width="350" Margin="20">
    <DockPanel>
        <TextBlock DockPanel.Dock="Top" Text="实时报警" FontWeight="Bold" Padding="8" Background="#F5F5F5"/>
        
        <!-- 报警列表，外层包 ScrollViewer 实现滚动 -->
        <ScrollViewer MaxHeight="300" VerticalScrollBarVisibility="Auto">
            <ItemsControl ItemsSource="{Binding AlarmList}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="5">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <!-- 报警等级色条 -->
                            <Rectangle Grid.Column="0" Width="4" Fill="{Binding Color}" Margin="0,2,8,2" RadiusX="2" RadiusY="2"/>
                            
                            <StackPanel Grid.Column="1">
                                <TextBlock Text="{Binding Content}" FontWeight="SemiBold"/>
                                <TextBlock Text="{Binding Time, StringFormat='{}{0:MM-dd HH:mm:ss}'}" FontSize="11" Foreground="Gray"/>
                            </StackPanel>
                            
                            <TextBlock Grid.Column="2" Text="{Binding Level}" Foreground="{Binding Color}" FontSize="11" VerticalAlignment="Center"/>
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>
    </DockPanel>
</Border>
```

**关键点**：

- `ObservableCollection<T>` 实现集合变更通知，新增报警自动刷新界面，无需手动操作控件
- ItemsControl 自身无滚动条，外层包裹 `ScrollViewer` 实现滚动
- 数据模板完全自定义条目外观，颜色绑定实现等级区分

### 实例 2：卡片式设备总览（WrapPanel 布局）

**场景**：车间设备状态总览，卡片式布局自动换行，适配不同分辨率大屏。

xaml:

```xaml
<ScrollViewer Margin="20">
    <ItemsControl ItemsSource="{Binding DeviceList}">
        <!-- 替换布局面板为自动换行的 WrapPanel -->
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <WrapPanel Orientation="Horizontal" ItemWidth="180" ItemHeight="100" Margin="-5"/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>

        <!-- 卡片数据模板 -->
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Border BorderBrush="#DDD" BorderThickness="1" CornerRadius="4" Padding="10" Margin="5">
                    <StackPanel>
                        <StackPanel Orientation="Horizontal" Spacing="6">
                            <Ellipse Width="10" Height="10" Fill="{Binding StatusColor}" VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding DeviceId}" FontWeight="Bold" FontSize="14"/>
                        </StackPanel>
                        <TextBlock Text="{Binding StatusText}" Margin="0,5" Foreground="Gray"/>
                        <TextBlock Text="{Binding CurrentYield, StringFormat='产量：{0} 片'}" FontSize="12"/>
                    </StackPanel>
                </Border>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

### 实例 3：时段产能列表（交替行样式）

**场景**：按小时展示时段产能，交替行背景色提升长列表可读性。

xaml:

```xaml
<ItemsControl ItemsSource="{Binding HourYieldList}" Width="300" Margin="20">
    <!-- 开启交替计数，每2行循环一次 -->
    <ItemsControl.AlternationCount>2</ItemsControl.AlternationCount>

    <!-- 条目容器样式：交替行背景 + 内边距 -->
    <ItemsControl.ItemContainerStyle>
        <Style TargetType="ContentPresenter">
            <Setter Property="Padding" Value="8,5"/>
            <Style.Triggers>
                <!-- 偶数行（索引为1）设置浅灰背景 -->
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#F8F9FA"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ItemsControl.ItemContainerStyle>

    <!-- 条目内容模板 -->
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <TextBlock Text="{Binding Hour, StringFormat='{}{0}:00'}"/>
                <TextBlock Grid.Column="1" Text="{Binding Yield}" HorizontalAlignment="Center"/>
                <TextBlock Grid.Column="2" Text="{Binding Rate, StringFormat='{}{0}%'}" HorizontalAlignment="Right" Foreground="Green"/>
            </Grid>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### 实例 4：按等级分组的报警列表（GroupStyle 分组）

**场景**：报警记录按等级分组展示（一级报警、二级报警、提示），结构更清晰。

首先在资源中定义分组视图：

xaml:

```xaml
<Window.Resources>
    <CollectionViewSource x:Key="GroupedAlarms" Source="{Binding AlarmList}">
        <CollectionViewSource.GroupDescriptions>
            <PropertyGroupDescription PropertyName="Level"/>
        </CollectionViewSource.GroupDescriptions>
    </CollectionViewSource>
</Window.Resources>
```

然后 ItemsControl 绑定分组视图，定义分组标题样式：

xaml:

```xaml
<ItemsControl ItemsSource="{Binding Source={StaticResource GroupedAlarms}}"
              Width="350" Margin="20">
    <!-- 分组标题样式 -->
    <ItemsControl.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <Border Background="#E6F2FF" Padding="6,4" Margin="0,5,0,2">
                        <TextBlock Text="{Binding Name}" FontWeight="Bold" Foreground="#007ACC"/>
                    </Border>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ItemsControl.GroupStyle>

    <!-- 条目内容模板 -->
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Content}" Padding="10,3"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

------

## 五、常见问题与最佳实践

1. **Items 与 ItemsSource 互斥**

   不能同时使用 `Items` 静态添加和 `ItemsSource` 数据绑定，否则会抛出异常。MVVM 场景统一使用 `ItemsSource` 绑定 `ObservableCollection<T>`。

2. **滚动条支持**

   ItemsControl 自身不带滚动条，需要滚动功能时外层包裹 `ScrollViewer`。大数据量建议直接使用 `ListBox` / `ListView`，内置滚动与虚拟化支持，性能更好。

3. **条目容器类型匹配**

   ItemsControl 默认容器是 `ContentPresenter`，写 `ItemContainerStyle` 时 TargetType 必须对应；ListBox 容器为 `ListBoxItem`，注意区分。

4. **大数据量性能优化**

   条目数量超过 100 条时，建议替换 `VirtualizingStackPanel` 开启 UI 虚拟化，只生成可视区域的容器，大幅提升性能。

   xaml:

   ```xaml
   <ItemsControl.ItemsPanel>
       <ItemsPanelTemplate>
           <VirtualizingStackPanel/>
       </ItemsPanelTemplate>
   </ItemsControl.ItemsPanel>
   ```

5. **交互场景选型**

   需要单选、多选、键盘导航功能时，优先使用 `ListBox` / `ListView`，不要基于 ItemsControl 自行实现选择逻辑。ItemsControl 的定位是纯展示容器。

6. **线程安全**

   `ObservableCollection<T>` 的增删操作必须在 UI 线程执行；后台 PLC 线程新增报警时，需通过 `Dispatcher` 调度到 UI 线程操作集合。

