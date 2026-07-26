# 005004003_WPF ListView 源码级类定义逐行深度解析

**源码：**

```c#
public class ListView : ListBox
{
    public static readonly DependencyProperty ViewProperty;
 
    public ListView();
 
    public ViewBase View { get; set; }
 
    protected override void ClearContainerForItemOverride(DependencyObject element, object item);
    protected override DependencyObject GetContainerForItemOverride();
    protected override bool IsItemItsOwnContainerOverride(object item);
    protected override AutomationPeer OnCreateAutomationPeer();
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
}
```

你给出的是 .NET 运行时源码中 `ListView` 的核心成员精简定义，完整保留了其最核心的 **「可插拔视图扩展 + 容器生命周期适配」** 能力。`ListView` 本身几乎没有新增业务逻辑，所有扩展都围绕「视图解耦」展开：在完整复用 `ListBox` 全部能力的基础上，仅通过少量重写方法接入 `ViewBase` 视图对象，就实现了多呈现模式的支持，是 WPF「职责分离」设计思想的典型体现。

------

## 一、类定义总览与核心定位

### 完整继承链

plaintext：

```tex
Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement
    → Control → ItemsControl → Selector → ListBox → ListView
```

- **核心设计思想**：策略模式 + 单一职责。列表通用逻辑（选择、滚动、容器管理、虚拟化）完全复用 `ListBox`，条目呈现方式完全抽离到独立的 `ViewBase` 视图对象中，二者通过固定契约对接。
- **默认条目容器**：`ListViewItem`（继承自 `ListBoxItem`，仅做类型区分，无额外业务逻辑）
- **官方内置视图**：`GridView`（多列表格视图，工业场景 90% 以上的 ListView 场景使用该视图）
- **工业定位**：结构化只读多列数据的首选控件，性能优于重型 `DataGrid`，样式灵活性高于 `ListBox`。

------

## 二、静态依赖属性：`ViewProperty`

csharp:

```c#
public static readonly DependencyProperty ViewProperty;
```

这是 `ListView` 唯一新增的依赖属性，也是整个控件的灵魂。

- **对应包装属性**：`public ViewBase View { get; set; }`
- **属性类型**：`ViewBase`（抽象基类）
- **默认值**：`null`，未设置视图时，`ListView` 的外观、行为与普通 `ListBox` 完全一致。
- **内部变更回调**：属性值变化时，框架内部会执行视图切换完整流程：卸载旧视图、更新控件默认样式、刷新所有可见容器的呈现。
- **设计价值**：将「数据列表逻辑」与「条目视觉呈现」彻底解耦，列表本身不关心条目是单列、多列还是卡片式，全部交给视图对象实现，扩展性极强。

------

## 三、构造函数

csharp:

```c#
public ListView();
```

### 官方内部执行逻辑

1. 调用基类 `ListBox` 构造函数，完成列表基础框架初始化；
2. 重写默认样式键 `DefaultStyleKey` 为 `typeof(ListView)`，加载 `ListView` 专属的默认控件模板；
3. 初始化内部状态，`View` 属性默认为 `null`。

### 关键说明

未设置 `View` 时，`ListView` 等价于一个 `ListBox`，所有选择、滚动、虚拟化能力完全一致，仅默认样式有细微差别。

------

## 四、公共实例属性：`View`

csharp:

```c#
public ViewBase View { get; set; }
```

- **类型约束**：必须是 `ViewBase` 的子类，WPF 官方仅内置 `GridView` 一种实现，开发者可自定义图标视图、卡片视图、磁贴视图等。
- **官方核心作用**：指定列表的呈现视图模式，是 `ListView` 区别于 `ListBox` 的唯一标志。
- **工业场景核心价值**
  1. 设置为 `GridView` 可实现轻量级多列表格，替代重型 `DataGrid`，内存占用更低、滚动更流畅；
  2. 支持运行时动态切换视图（表格 / 列表 / 大图标），满足不同数据查看需求；
  3. 视图与数据完全解耦，同一套数据源可适配多种展示形式，架构更灵活。
- **使用注意**：更换 `View` 会触发全量容器刷新，频繁切换会有性能开销，大数据量场景不建议高频切换。

------

## 五、受保护重写方法逐行解析（核心）

这 6 个重写方法是 `ListView` 的全部核心扩展点，**所有逻辑都围绕「在容器生命周期中接入视图处理」展开**，没有任何多余功能。

### 1. `GetContainerForItemOverride`

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- **官方实现**：直接返回 `new ListViewItem()`。
- **设计作用**：将基类的默认容器 `ListBoxItem` 替换为 `ListViewItem`，作为视图样式与模板的目标类型锚点。
- **底层意义**：`GridView` 等视图的单元格模板，都是针对 `ListViewItem` 类型编写的，容器类型统一才能保证视图样式正确应用。

### 2. `IsItemItsOwnContainerOverride`

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- **官方实现**：`return item is ListViewItem;`
- **作用**：判断传入对象本身是否已经是 `ListViewItem` 容器，是则直接使用，不再额外包装一层。
- **典型场景**：XAML 中直接添加 `<ListViewItem>` 静态子元素时，可直接作为容器使用，符合直觉。

### 3. `PrepareContainerForItemOverride`

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

这是容器初始化的核心入口，`ListView` 在此处接入视图的准备逻辑。

#### 官方执行流程

1. **先调用基类方法**：完成数据上下文绑定、选中状态同步、`ItemContainerStyle` 应用、交替索引设置等基础准备工作；
2. **关键扩展**：判断 `View != null` 时，调用 `View.PrepareItem((ListViewItem)element)`，由视图对象向容器应用专属的呈现逻辑：
   - 若为 `GridView`：给容器应用多行单元格布局模板、绑定列宽、设置单元格内容绑定；
   - 若为自定义视图：应用自定义的条目样式与模板。

#### 虚拟化适配

容器从回收池取出复用时，同样会执行此方法，视图负责恢复对应数据项的呈现状态，是虚拟化模式下视图不出现错乱的核心保障。

### 4. `ClearContainerForItemOverride`

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

容器回收前的清理入口，与 `Prepare` 成对出现，是 UI 虚拟化的生命线。

#### 官方执行流程

1. **关键扩展**：判断 `View != null` 时，调用 `View.ClearItem((ListViewItem)element)`，由视图清理自己附加在容器上的所有内容：
   - 清除单元格模板、解除列宽绑定、移除视图专属样式；
   - 避免容器复用时出现样式残留、数据错乱。
2. **再调用基类方法**：清理选中状态、数据上下文、基础样式，容器进入回收池等待复用。

#### ⚠️ 工业场景必知

如果自定义 `ViewBase` 子类，**必须成对实现 `PrepareItem` 和 `ClearItem`**，只写准备不写清理，虚拟化滚动时必然出现样式残留、数据错乱、事件内存泄漏。

### 5. `OnItemsChanged`

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

集合数据变更时的回调，`ListView` 在基类逻辑基础上补充了视图侧的同步。

#### 官方扩展逻辑

1. 先调用基类方法，完成容器的增量增删、选中状态有效性维护；
2. 通知当前视图对象集合发生了变化，触发视图层面的布局刷新：
   - 例如 `GridView` 会重新计算列宽、更新行的单元格布局；
   - 集合重置（Clear）时，强制刷新所有可见容器的视图呈现，保证数据与视图完全一致。

#### 触发时机

- 绑定的 `ObservableCollection<T>` 发生增、删、改、重置时；
- 手动操作 `Items` 集合时。

### 6. `OnCreateAutomationPeer`

csharp:

```c#
protected override AutomationPeer OnCreateAutomationPeer();
```

- **官方返回值**：`ListViewAutomationPeer`
- **作用**：提供 UI 自动化对等类，支持 Windows 无障碍访问、UI 自动化测试框架。
- **工业场景价值**：支持产线软件的自动化回归测试、无人值守场景的自动化操作验证。

------

## 六、核心底层工作机制

### 6.1 视图挂载完整流程

plaintext:

```tex
设置 View 属性
    ↓
触发 ViewProperty 依赖属性变更回调
    ↓
1. 卸载旧视图：遍历所有可见容器，调用 oldView.ClearItem 清理
2. 更新控件默认样式，应用新视图的 DefaultStyleKey
3. 替换 ItemsPresenter 与布局面板的视图逻辑
4. 遍历所有可见容器，调用 newView.PrepareItem 应用新呈现
    ↓
触发整体布局更新，界面切换为新视图样式
```

### 6.2 UI 虚拟化兼容原理

`ListView` 能完美支持虚拟化，依赖三层保障：

1. **基层**：`ListBox` / `ItemsControl` 原生支持 `VirtualizingStackPanel` 虚拟化，负责容器的创建、回收、复用；
2. **契约层**：`ViewBase` 定义了 `PrepareItem` / `ClearItem` 生命周期，强制视图必须支持容器回收时的状态清理；
3. **数据层**：选中状态、数据上下文由基类持久化存储，视图只负责呈现，不持有业务状态。

- **实际效果**：万级数据 + `GridView` 场景下，开启虚拟化后内存占用仅为全量生成的 5%~10%，滚动流畅无卡顿。

### 6.3 与 ListBox 的本质区别

| 维度     | ListBox                | ListView                                         |
| :------- | :--------------------- | :----------------------------------------------- |
| 呈现逻辑 | 内置固定的单列内容呈现 | 呈现逻辑完全委托给 View 对象，本身不定义呈现方式 |
| 容器类型 | ListBoxItem            | ListViewItem                                     |
| 扩展方向 | 扩展选择、交互逻辑     | 扩展视图呈现模式                                 |
| 典型场景 | 单列选择列表           | 多列结构化表格、多视图切换                       |

------

## 七、工业场景选型提示

1. **单列选择、菜单导航**：优先用 `ListBox`，更轻量、开销更低；
2. **只读结构化多列数据**（设备台账、生产记录、报警明细）：优先用 `ListView + GridView`，性能优、样式灵活；
3. **需要单元格编辑、行验证、复杂表格交互**：再使用 `DataGrid`。