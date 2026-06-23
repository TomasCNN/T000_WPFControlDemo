# 005006006_ 除了基础绑定，WPF ListBox 常用的功能和特性

`ListBox` 是 WPF 最基础的单选 / 多选列表控件，继承自 `Selector` → `ItemsControl` 体系，除了基础数据绑定，核心能力围绕**选择交互、外观定制、性能优化、数据组织、高级交互**五大维度展开，轻量且灵活，是工业软件中设备列表、报警列表、菜单导航的核心控件。以下是最常用的功能与特性，附核心用法与适用场景。

------

## 一、丰富的选择模式（核心交互能力）

选择是 `ListBox` 最核心的能力，支持三种选择模式，覆盖单选、简单多选、扩展多选全部场景。

### 1. 三种选择模式

通过 `SelectionMode` 属性控制：

| 枚举值           | 行为说明                                                     | 典型场景                                 |
| :--------------- | :----------------------------------------------------------- | :--------------------------------------- |
| `Single`（默认） | 同一时间只能选中一项                                         | 设备详情联动、菜单导航、单选筛选         |
| `Multiple`       | 单击切换选中状态，可同时选中多项，无需按住 Ctrl              | 简单批量勾选、标签多选                   |
| `Extended`       | 标准多选：Ctrl 点选、Shift 连选，和 Windows 资源管理器行为一致 | 报警批量确认、记录批量导出等工业批量操作 |

### 2. 选中相关属性

| 属性                                  | 作用               | 说明                                                 |
| :------------------------------------ | :----------------- | :--------------------------------------------------- |
| `SelectedItem`                        | 当前选中的数据对象 | 单选场景核心，双向绑定联动 ViewModel                 |
| `SelectedIndex`                       | 当前选中项的索引   | 从 0 开始，-1 表示未选中                             |
| `SelectedValue` + `SelectedValuePath` | 选中项的指定字段值 | 适合只需要 ID / 编码的场景，比如选中设备只取设备编号 |
| `SelectedItems`                       | 所有选中项的集合   | 多选场景核心，只读集合，批量操作时遍历获取数据       |

### 3. 内置批量操作方法

csharp:

```c#
listBox.SelectAll();   // 全选
listBox.UnselectAll(); // 取消全选
```

> 这两个方法是框架内部优化实现，批量选中性能远高于循环逐条设置，工业批量操作优先使用。

### 4. 核心事件

`SelectionChanged`：选中项变化时触发，是主从联动、级联筛选的核心入口。

------

## 二、高度灵活的项呈现定制（外观核心）

作为 `ItemsControl` 体系的控件，`ListBox` 支持从「显示字段」到「自定义模板」再到「容器样式」的全层级外观定制，完全不受默认样式限制。

### 1. 最简显示：DisplayMemberPath

指定列表项显示的属性名，适合纯文本列表。

xaml:

```xaml
<ListBox ItemsSource="{Binding DeviceList}" DisplayMemberPath="DeviceName"/>
```

### 2. 自定义条目内容：ItemTemplate

用 `DataTemplate` 完全自定义每个条目的布局和内容，比如加图标、状态灯、多字段排版。

xaml:

```xaml
<ListBox ItemsSource="{Binding DeviceList}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <DockPanel Height="36">
                <!-- 状态指示灯 -->
                <Ellipse DockPanel.Dock="Left" Width="8" Height="8" Fill="{Binding IsRunning, Converter={StaticResource BoolToColorConverter}}"/>
                <StackPanel Margin="8 0 0 0">
                    <TextBlock Text="{Binding DeviceName}" FontWeight="SemiBold"/>
                    <TextBlock Text="{Binding DeviceCode}" FontSize="11" Foreground="#666"/>
                </StackPanel>
            </DockPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

> 适用场景：设备状态列表、报警条目、带图标的菜单等所有需要丰富展示的列表。

### 3. 自定义容器样式：ItemContainerStyle

控制每个条目容器（`ListBoxItem`）的整体样式，比如高度、背景、选中态、悬浮态、边距等。

xaml:

```xaml
<ListBox.ItemContainerStyle>
    <Style TargetType="ListBoxItem">
        <Setter Property="Height" Value="36"/>
        <Setter Property="Padding" Value="10 0"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#F0F7FF"/>
            </Trigger>
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="#E6F4FF"/>
                <Setter Property="Foreground" Value="#0958D9"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</ListBox.ItemContainerStyle>
```

### 4. 动态选择模板 / 样式

- `ItemTemplateSelector`：根据数据动态选择不同的条目模板
- `ItemContainerStyleSelector`：根据数据动态选择不同的容器样式

> 适用场景：不同等级的报警用不同的模板 / 颜色，不同类型的设备用不同的布局。

------

## 三、UI 虚拟化（大数据性能核心）

这是工业长列表最关键的特性，专门解决「上千 / 上万条数据卡顿、内存暴涨」的问题。

### 核心原理

只生成屏幕可见区域的条目容器，滚出屏幕的条目会被销毁或回收复用。万级数据下，内存中始终只有几十个 UI 对象，内存占用降低 90% 以上，滚动流畅。

### 开启方式

xaml:

```xaml
<ListBox ItemsSource="{Binding HistoryAlarmList}">
    <!-- 替换布局面板为虚拟化面板 -->
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <!-- Recycling：容器回收复用，性能最优；Standard：滚出销毁，滚入重建 -->
            <VirtualizingStackPanel VirtualizationMode="Recycling"/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

### 配套优化

- `ScrollViewer.IsDeferredScrollingEnabled="True"`：延迟滚动，拖动滚动条时只显示提示，松开后再渲染，大数据量下拖动更流畅。
- 固定行高：行高固定时虚拟化性能最优，避免逐行计算高度。

### 常见坑点

- ❌ 外层包裹 `ScrollViewer` 会导致虚拟化完全失效；
- ❌ 用普通 `StackPanel` 作为 `ItemsPanel` 会关闭虚拟化；
- ❌ 行高完全动态（`Height="Auto"`）会大幅降低虚拟化性能。

------

## 四、数据视图：分组、排序、过滤

基于 `ICollectionView` 视图层实现，**不需要修改原始数据源**，就能对列表数据进行分组、排序、筛选，是 WPF 列表体系的经典设计。

### 1. 分组

按指定字段对数据分组，每组显示分组标题。

xaml:

```xaml
<ListBox ItemsSource="{Binding AlarmGroupView}">
    <!-- 分组头样式 -->
    <ListBox.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <Border Background="#E6F4FF" Padding="8 4">
                        <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                    </Border>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
        </GroupStyle>
    </ListBox.GroupStyle>
</ListBox>
```

> ViewModel 中通过 `CollectionViewSource` 配置分组：
>
> csharp:
>
> ```c#
> var cvs = new CollectionViewSource { Source = AlarmList };
> cvs.GroupDescriptions.Add(new PropertyGroupDescription("DeviceName"));
> AlarmGroupView = cvs.View;
> ```

### 2. 排序

按指定字段升序 / 降序排列，同样通过 `CollectionViewSource.SortDescriptions` 配置。

### 3. 过滤

通过 `Filter` 委托按条件筛选数据，比如只显示未确认的报警。

csharp:

```c#
cvs.View.Filter = item => (item as AlarmRecord).IsConfirmed == false;
```

------

## 五、拖拽交互能力

`ListBox` 原生支持 WPF 拖放框架，可实现**同列表内拖拽排序、跨控件拖拽数据**，是工艺配置、物料分配等场景的常用交互。

### 核心配置

1. 开启 `AllowDrop="True"`；
2. 监听 `MouseMove` / `Drop` 等事件，配合 `DragDrop.DoDragDrop` 实现拖拽逻辑；
3. 拖拽到列表边缘时会自动滚动，适合长列表拖拽排序。

> 典型场景：生产工序拖拽调整顺序、物料拖拽分配到设备、标签拖拽分组。

------

## 六、键盘导航与文本搜索（工控友好）

非常适合工控机无鼠标、纯键盘操作的场景。

### 1. 标准键盘导航

- 方向键 ↑↓：切换选中项；
- Home / End：跳到首项 / 末项；
- PageUp / PageDown：翻页；
- 空格：切换选中状态。

### 2. 文本快速搜索

通过 `IsTextSearchEnabled` 开启（默认开启），键盘输入字符会自动跳转到第一个匹配的条目。

xaml:

```xaml
<ListBox ItemsSource="{Binding DeviceList}"
         IsTextSearchEnabled="True"
         TextSearch.TextPath="DeviceCode"/>
```

- `TextSearch.TextPath`：指定用于匹配搜索的字段，比如按设备编号搜索；
- 适用场景：长列表快速定位，纯键盘操作效率高。

------

## 七、交替行与视觉状态精细化

### 1. 奇偶行交替背景

通过 `AlternationCount` 设置交替周期，配合触发器实现奇偶行不同背景，提升长列表可读性。

xaml:

```xaml
<ListBox AlternationCount="2">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Style.Triggers>
                <Trigger Property="ItemsControl.AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#F8F9FA"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ListBox.ItemContainerStyle>
</ListBox>
```

### 2. 精细化选中状态

通过 `Selector.IsSelectionActive` 区分「控件有焦点时的选中」和「控件失焦时的选中」，避免失焦后选中态看不清。

xaml:

```xaml
<MultiTrigger>
    <MultiTrigger.Conditions>
        <Condition Property="IsSelected" Value="True"/>
        <Condition Property="Selector.IsSelectionActive" Value="False"/>
    </MultiTrigger.Conditions>
    <Setter Property="Background" Value="#E0E0E0"/>
</MultiTrigger>
```

------

## 八、内置滚动与定位能力

1. **自带滚动条**：`ListBox` 内置 `ScrollViewer`，不需要外层额外包裹；
2. **滚动定位**：`ScrollIntoView(object item)` 方法，可将指定数据项自动滚动到可视区域，常用于搜索定位、新增后跳转；
3. **滚动控制**：通过 `ScrollViewer.HorizontalScrollBarVisibility` / `VerticalScrollBarVisibility` 控制滚动条显示策略。

------

## 九、其他实用特性

1. **`IsSynchronizedWithCurrentItem`**：和 `CollectionView` 的当前项保持同步，适合主从视图联动；
2. **虚拟化下状态持久化**：选中状态保存在数据层，不依赖 UI 容器，滚动时选中状态不会丢失；
3. **支持内容对齐、边距、边框等基础样式定制**，适配工业深色 / 浅色主题；
4. **支持无障碍与自动化测试**：继承 `ItemsControl` 完整的 UI Automation 支持。

------

## 选型总结

- 单列选择、菜单导航、简单列表 → 优先用 `ListBox`，轻量高性能；
- 多列只读结构化数据 → 用 `ListView + GridView`；
- 需要单元格编辑、复杂表格 → 用 `DataGrid`。