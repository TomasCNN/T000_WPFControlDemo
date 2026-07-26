# 005004004_WPF `GridView` 视图源码级官方类定义深度解析

`GridView` 是 WPF 官方唯一内置的 `ViewBase` 视图实现，专门为 `ListView` 提供**多列表格化呈现**能力。它本身不是独立控件，而是 `ListView` 的「视图插件」：将原本单列的列表，转化为带列头、支持列宽拖动、可自定义单元格的轻量表格，是工业软件中设备台账、生产记录、报警明细等**只读结构化数据**场景的首选方案，性能远优于重型 `DataGrid`。

本文基于 .NET 官方源码，从类定义、依赖属性、资源键、核心方法到底层机制逐行深度解析，完整还原其设计逻辑与工业场景价值。

------

## 一、类定义总览与核心定位

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    public class GridView : ViewBase, IAddChild
    {
        // 静态依赖属性
        public static readonly DependencyProperty ColumnCollectionProperty;
        public static readonly DependencyProperty ColumnHeaderContainerStyleProperty;
        public static readonly DependencyProperty ColumnHeaderTemplateProperty;
        public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty;
        public static readonly DependencyProperty ColumnHeaderStringFormatProperty;
        public static readonly DependencyProperty AllowsColumnReorderProperty;
        public static readonly DependencyProperty ColumnHeaderContextMenuProperty;
        public static readonly DependencyProperty ColumnHeaderToolTipProperty;

        // 构造函数
        public GridView();

        // 静态资源键（样式定制核心）
        public static ResourceKey GridViewItemContainerStyleKey { get; }
        public static ResourceKey GridViewStyleKey { get; }
        public static ResourceKey GridViewScrollViewerStyleKey { get; }

        // 公共实例属性
        public string ColumnHeaderStringFormat { get; set; }
        public DataTemplateSelector ColumnHeaderTemplateSelector { get; set; }
        public DataTemplate ColumnHeaderTemplate { get; set; }
        public Style ColumnHeaderContainerStyle { get; set; }
        public GridViewColumnCollection Columns { get; }
        public object ColumnHeaderToolTip { get; set; }
        public bool AllowsColumnReorder { get; set; }
        public ContextMenu ColumnHeaderContextMenu { get; set; }

        // 重写属性（ViewBase 契约）
        protected internal override object ItemContainerDefaultStyleKey { get; }
        protected internal override object DefaultStyleKey { get; }

        // 公共静态方法
        public static GridViewColumnCollection GetColumnCollection(DependencyObject element);
        public static void SetColumnCollection(DependencyObject element, GridViewColumnCollection collection);
        public static bool ShouldSerializeColumnCollection(DependencyObject obj);

        // 公共方法
        public override string ToString();

        // IAddChild 接口实现
        protected virtual void AddChild(object column);
        protected virtual void AddText(string text);

        // ViewBase 核心契约方法
        protected internal override void ClearItem(ListViewItem item);
        protected internal override IViewAutomationPeer GetAutomationPeer(ListView parent);
        protected internal override void PrepareItem(ListViewItem item);
    }
}
```

### 1.2 核心元数据

| 项             | 官方精确值                                               | 工业场景说明                                                 |
| :------------- | :------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**   | `System.Windows.Controls`                                | WPF 标准控件命名空间                                         |
| **程序集**     | `PresentationFramework.dll`                              | WPF 核心框架程序集                                           |
| **完整继承链** | `Object → DependencyObject → ViewBase → GridView`        | 视图抽象基类的标准表格实现                                   |
| **实现接口**   | `IAddChild`                                              | 支持 XAML 直接声明列，无需手动加集合                         |
| **宿主控件**   | `ListView`                                               | 不能独立使用，必须挂载到 `ListView.View` 属性上              |
| **核心职责**   | 为列表提供多列表格呈现、列头管理、列宽控制、单元格模板化 | 将单列列表转化为结构化表格视图                               |
| **工业定位**   | 只读多列数据的轻量方案                                   | 替代重型 `DataGrid`，内存占用低、滚动性能好，适合报警、台账、记录类场景 |

### 1.3 接口与契约说明

1. **继承 `ViewBase`**

   遵守视图基类的生命周期契约：必须实现 `PrepareItem` / `ClearItem`，和 `ListView` 的容器生命周期对接；必须提供 `DefaultStyleKey` 定义默认视觉样式。

2. **实现 `IAddChild`**

   XAML 解析器约定接口，支持直接在 `<GridView>` 标签内写 `<GridViewColumn>` 子元素，自动添加到 `Columns` 集合中，这是我们常用的 XAML 声明式写法的底层支撑。

------

## 二、静态依赖属性全量解析

按功能分为**列集合、列头呈现、交互控制**三大类，全部为依赖属性，支持数据绑定、样式、动画。

### 2.1 列集合类（核心）

| 属性字段                   | 对应成员           | 类型                       | 官方作用             | 底层说明                                                     |
| :------------------------- | :----------------- | :------------------------- | :------------------- | :----------------------------------------------------------- |
| `ColumnCollectionProperty` | `Columns` 实例属性 | `GridViewColumnCollection` | 存储所有列定义的集合 | 以附加属性形式存储，支持在视觉树中共享列集合；是 `GridView` 的核心数据结构 |

- `Columns` 是 `GridView` 最核心的属性，集合中每个 `GridViewColumn` 定义一列的表头、绑定、模板、宽度。
- 集合实现了 `INotifyCollectionChanged`，增删列时自动刷新表格布局。

### 2.2 列头呈现类

控制列头区域的外观与内容，工业场景常用于自定义深色主题、列头图标、排序指示等。

表格

| 属性字段                               | 包装属性                       | 类型                   | 默认值 | 官方作用                     | 工业场景价值                                       |
| :------------------------------------- | :----------------------------- | :--------------------- | :----- | :--------------------------- | :------------------------------------------------- |
| `ColumnHeaderContainerStyleProperty`   | `ColumnHeaderContainerStyle`   | `Style`                | `null` | 所有列头容器的统一样式       | 自定义列头背景、高度、边框、字体，适配工业深色主题 |
| `ColumnHeaderTemplateProperty`         | `ColumnHeaderTemplate`         | `DataTemplate`         | `null` | 所有列头的默认内容模板       | 统一自定义列头外观，比如带图标、排序箭头的列头     |
| `ColumnHeaderTemplateSelectorProperty` | `ColumnHeaderTemplateSelector` | `DataTemplateSelector` | `null` | 根据列动态选择不同的列头模板 | 不同类型的列（数值 / 文本 / 状态）使用不同列头样式 |
| `ColumnHeaderStringFormatProperty`     | `ColumnHeaderStringFormat`     | `string`               | `null` | 列头文本的统一格式化字符串   | 批量格式化列头文本，如添加单位后缀                 |

> 🔑 优先级规则：单列设置的 `HeaderTemplate` 优先级 > `GridView` 全局的 `ColumnHeaderTemplate`。

### 2.3 交互控制类

控制表格的交互行为，工业场景常根据操作规范开关对应功能。

| 属性字段                          | 包装属性                  | 类型          | 默认值 | 官方作用                     | 工业场景最佳实践                                             |
| :-------------------------------- | :------------------------ | :------------ | :----- | :--------------------------- | :----------------------------------------------------------- |
| `AllowsColumnReorderProperty`     | `AllowsColumnReorder`     | `bool`        | `true` | 是否允许拖动列头调整列的顺序 | 关键数据表格建议设为 `false`，防止操作人员误拖拽打乱固定列序 |
| `ColumnHeaderContextMenuProperty` | `ColumnHeaderContextMenu` | `ContextMenu` | `null` | 所有列头共享的右键菜单       | 实现「显示 / 隐藏列、按列排序、导出该列」等工业常用右键功能  |
| `ColumnHeaderToolTipProperty`     | `ColumnHeaderToolTip`     | `object`      | `null` | 列头的统一提示信息           | 给缩写列名添加完整说明，提升操作易用性                       |

------

## 三、静态资源键：样式定制的核心入口

这三个静态只读属性是 `GridView` 样式体系的关键，很多人自定义表格样式找不到入口，本质就是没用到这几个资源键。

### 1. `GridViewStyleKey`

csharp:

```c#
public static ResourceKey GridViewStyleKey { get; }
```

- **作用**：`ListView` 挂载 `GridView` 后，整体控件的默认样式键。

- **对应目标**：`ListView` 控件本身的控件模板，包含列头区域、滚动区域、边框等整体结构。

- **典型用法**：基于该键重写样式，自定义表格整体边框、列头背景、网格线、滚动条样式等。

- 示例：

  xaml:

  ```xaml
  <Style x:Key="{x:Static GridView.GridViewStyleKey}" TargetType="ListView">
      <Setter Property="BorderBrush" Value="#333"/>
      <Setter Property="BorderThickness" Value="1"/>
  </Style>
  ```

### 2. `GridViewItemContainerStyleKey`

csharp:

```c#
public static ResourceKey GridViewItemContainerStyleKey { get; }
```

- **作用**：每个数据行容器（`ListViewItem`）的默认样式键。
- **对应目标**：行的高度、背景、选中态、分割线等行级样式。
- **说明**：通常我们直接设置 `ListView.ItemContainerStyle` 即可，该键是系统默认样式的标识。

### 3. `GridViewScrollViewerStyleKey`

csharp:

```c#
public static ResourceKey GridViewScrollViewerStyleKey { get; }
```

- **作用**：`GridView` 内置滚动查看器的默认样式键。
- **对应目标**：表格内部的 `ScrollViewer`，控制滚动条外观、滚动行为。
- **工业场景**：自定义宽滚动条、深色滚动条样式，适配工控触摸屏操作。

------

## 四、重写属性（ViewBase 契约）

### 1. `DefaultStyleKey`

csharp:

```c#
protected internal override object DefaultStyleKey { get; }
```

- 返回值就是 `GridViewStyleKey`；
- 告诉框架：当 `ListView` 使用该视图时，整体控件套用 `GridViewStyleKey` 对应的默认样式；
- 是视图切换时样式自动切换的核心机制。

### 2. `ItemContainerDefaultStyleKey`

csharp:

```c#
protected internal override object ItemContainerDefaultStyleKey { get; }
```

- 返回值就是 `GridViewItemContainerStyleKey`；
- 告诉框架：每个条目容器套用 `GridView` 专属的行容器默认样式。

------

## 五、核心方法逐行解析

### 5.1 公共静态方法

#### 1. `GetColumnCollection / SetColumnCollection`

csharp:

```c#
public static GridViewColumnCollection GetColumnCollection(DependencyObject element);
public static void SetColumnCollection(DependencyObject element, GridViewColumnCollection collection);
```

- `ColumnCollectionProperty` 附加属性的强类型读写器；
- 内部用于在视觉树的不同元素间共享列集合，保证列头和数据行的列宽、列序完全同步；
- 业务开发很少直接调用，属于框架内部机制。

#### 2. `ShouldSerializeColumnCollection`

csharp:

```c#
public static bool ShouldSerializeColumnCollection(DependencyObject obj);
```

- XAML 设计器序列化约定方法，判断是否需要序列化列集合；
- 设计器支持相关，业务开发无需关注。

### 5.2 IAddChild 接口实现

#### `AddChild / AddText`

csharp:

```c#
protected virtual void AddChild(object column);
protected virtual void AddText(string text);
```

- `IAddChild` 接口的核心实现；

- XAML 解析时，遇到 `<GridViewColumn>` 子元素，自动调用 `AddChild` 将其添加到 `Columns` 集合中；

- 这就是我们可以直接在 XAML 里声明列的底层原理：

  xaml:

  ```xaml
  <GridView>
      <!-- 这些列会通过 AddChild 自动加入 Columns 集合 -->
      <GridViewColumn Header="编号" DisplayMemberBinding="{Binding Id}"/>
      <GridViewColumn Header="名称" DisplayMemberBinding="{Binding Name}"/>
  </GridView>
  ```

### 5.3 ViewBase 核心生命周期方法

这两个方法是 `ViewBase` 抽象契约的核心实现，也是 `GridView` 和 `ListView` 容器生命周期的对接点。

#### 1. `PrepareItem(ListViewItem item)`

csharp:

```c#
protected internal override void PrepareItem(ListViewItem item);
```

- **触发时机**：`ListView` 生成 / 复用行容器时，在 `PrepareContainerForItemOverride` 中调用。
- **官方核心执行逻辑**：
  1. 给 `ListViewItem` 应用 `GridView` 专属的行容器默认样式；
  2. 将行的内容模板替换为 `GridViewRowPresenter`（表格行呈现器）；
  3. 绑定列集合、列宽信息，让该行按列拆分单元格；
  4. 每个单元格按列定义应用 `DisplayMemberBinding` 或 `CellTemplate`；
  5. 同步交替行、选中态等基础状态。
- **虚拟化适配**：容器从回收池取出复用时，会重新执行此方法，绑定对应行的数据，保证滚动时单元格内容正确。

#### 2. `ClearItem(ListViewItem item)`

csharp:

```c#
protected internal override void ClearItem(ListViewItem item);
```

- **触发时机**：行容器滚出屏幕进入回收池时，在 `ClearContainerForItemOverride` 中调用。
- **官方核心执行逻辑**：
  1. 清除行上的单元格绑定、数据模板；
  2. 解除列集合的关联；
  3. 还原容器为通用状态，等待复用。
- ⚠️ **性能关键**：正是因为有完整的清理逻辑，`GridView` 才能完美适配 UI 虚拟化，万级数据滚动不会出现样式残留、数据错乱。

### 5.4 自动化支持

#### `GetAutomationPeer(ListView parent)`

csharp:

```c#
protected internal override IViewAutomationPeer GetAutomationPeer(ListView parent);
```

- 返回 `GridViewAutomationPeer`，为 UI 自动化框架提供表格视图的无障碍支持；
- 支持自动化测试工具识别表格行列、单元格内容。

------

## 六、核心底层工作机制

### 6.1 双层呈现架构

`GridView` 的表格效果由两个呈现器配合完成，共享同一套列集合：

1. **列头层**：`GridViewHeaderRowPresenter`
   - 位于控件顶部，渲染所有列的表头；
   - 负责处理列头点击、拖拽排序、拖动调整列宽、拖拽重排等交互；
   - 列宽变化时，实时同步给数据行。
2. **数据行层**：`GridViewRowPresenter`
   - 每个 `ListViewItem` 内部都有一个，按列顺序渲染单元格；
   - 所有行共享列宽信息，保证所有行的列对齐。

> 对齐原理：所有行都绑定同一个列集合的宽度，拖动列头时修改列宽属性，所有行自动同步更新，因此永远不会出现列头和内容错位。

### 6.2 列宽拖动与重排序

- **列宽拖动**：列头边缘拖拽时，修改对应 `GridViewColumn.Width` 属性，所有行同步更新；支持固定宽度、自动宽度、比例宽度。
- **列重排序**：拖动列头移动位置时，调整 `Columns` 集合中元素的顺序，所有行同步更新列顺序；由 `AllowsColumnReorder` 属性控制开关。

### 6.3 单元格呈现优先级

每一列的内容渲染遵循明确的优先级：

> **`CellTemplate`（自定义模板） > `DisplayMemberBinding`（文本绑定） > 默认 `ToString()`**

- 简单文本用 `DisplayMemberBinding`，性能最优；
- 状态灯、按钮、进度条等复杂内容用 `CellTemplate`。

### 6.4 UI 虚拟化兼容原理

`GridView` 能完美支持 UI 虚拟化，核心有三点：

1. **列信息共享**：列定义、列宽是全局共享的，不需要每行存一份，内存开销极低；
2. **行级回收**：`PrepareItem` / `ClearItem` 完整成对实现，行容器可以安全回收复用；
3. **只渲染可见行**：`VirtualizingStackPanel` 只生成可见区域的行，不可见行只有数据，没有 UI 对象。

- 实际效果：10 万行数据 + 10 列，开启虚拟化后内存占用仅为全量渲染的 5%~10%，滚动流畅。

------

## 七、工业场景最佳实践与常见坑点

### 最佳实践

1. **只读多列优先用 ListView+GridView**

   相比 `DataGrid` 更轻量、性能更好、样式更灵活，80% 以上的只读数据展示场景都能覆盖。

2. **大数据量避免大量 Auto 列**

   - `Width="Auto"` 会让每一行都参与宽度计算，数据量大时性能急剧下降；
   - 优化：固定列宽或星号比例列宽，只给少数列开自动宽度。

3. **关键表格关闭列重排序**

   工业生产数据表格通常有固定列序要求，设置 `AllowsColumnReorder="False"`，防止误操作打乱布局。

4. **复杂单元格用轻量模板**

   单元格内尽量减少视觉树层级，避免嵌套大量布局控件，否则滚动时会出现卡顿。

5. **列头右键菜单扩展实用功能**

   利用 `ColumnHeaderContextMenu` 实现「显示 / 隐藏列、按本列排序、导出列数据」等功能，大幅提升操作效率。

### 常见坑点

1. **修改列属性界面不更新**
   - 现象：代码修改 `GridViewColumn.Width` 不生效；
   - 原因：直接修改集合内对象的属性，集合变更事件不会触发；
   - 解决：强制刷新布局，或通过绑定实现属性变更通知。
2. **虚拟化下单元格内容错乱**
   - 现象：滚动后单元格内容不对、样式残留；
   - 原因：自定义 `CellTemplate` 中有动画、事件绑定，回收时未清理；
   - 解决：使用数据触发器替代状态变更，避免在单元格模板中写后台事件。
3. **列头与内容不对齐**
   - 通常是自定义单元格模板时加了额外的 Margin/Padding，和列头的内边距不一致，统一边距即可解决。

------

## 总结

`GridView` 是 WPF 视图架构的经典实现：它本身不处理数据、不管理选择，只专注于「把列表行渲染成多列表格」这一件事，通过 `ViewBase` 契约和 `ListView` 无缝对接。这种职责分离的设计，既保留了列表控件的高性能与高一致性，又提供了结构化表格的呈现能力，是工业软件中只读多列数据展示的最优解。