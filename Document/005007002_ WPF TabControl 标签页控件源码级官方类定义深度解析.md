# 005007002_ WPF `TabControl` 标签页控件源码级官方类定义深度解析

`TabControl` 是 WPF 标准的多页面切换容器控件，直接继承自 `Selector` 单选基类，与 `ListBox`、`ComboBox` 同属一个单选体系。它在 `ItemsControl` 集合呈现 + `Selector` 单选管理的完整能力基础上，通过「标签头 + 内容区」的双层结构，实现了折叠式多页布局：同一时间仅渲染一个选中页面，通过标签头快速切换，在有限界面空间内组织大量分类内容。

它是工业软件中设备详情、系统参数配置、报表中心、多设备并行监控场景的标准布局控件，核心设计思想是**用单选逻辑驱动页面可见性，用单内容渲染控制内存开销**。

------

## 一、官方类定义总览与核心元数据

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls
{
    [System.Windows.StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(System.Windows.Controls.TabItem))]
    public class TabControl : System.Windows.Controls.Primitives.Selector
    {
        // 新增静态依赖属性字段
        public static readonly System.Windows.DependencyProperty TabStripPlacementProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentTemplateProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentTemplateSelectorProperty;
        public static readonly System.Windows.DependencyProperty SelectedContentStringFormatProperty;

        // 构造函数
        public TabControl();

        // 新增公共属性
        public System.Windows.Controls.Dock TabStripPlacement { get; set; }
        public object SelectedContent { get; }
        public System.Windows.DataTemplate SelectedContentTemplate { get; }
        public System.Windows.Controls.DataTemplateSelector SelectedContentTemplateSelector { get; }
        public string SelectedContentStringFormat { get; }

        // 受保护内部属性
        protected internal override bool HandlesScrolling { get; }

        // 受保护重写方法
        protected override System.Windows.DependencyObject GetContainerForItemOverride();
        protected override bool IsItemItsOwnContainerOverride(object item);
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void ClearContainerForItemOverride(System.Windows.DependencyObject element, object item);
        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e);
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e);
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 1.2 核心元数据（官方精确值）

| 项               | 官方精确值                                                   | 工业场景关键说明                                             |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls`                                    | WPF 标准控件命名空间                                         |
| **程序集**       | `PresentationFramework.dll`                                  | WPF 核心框架程序集                                           |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → TabControl` | 完整继承集合呈现、单选管理、容器生命周期全部能力             |
| **默认条目容器** | `TabItem`                                                    | 每个标签页的 UI 容器，继承自 `HeaderedContentControl`，天然具备「标签头 + 页面内容」双部分 |
| **默认布局面板** | `TabPanel`                                                   | 标签条的专属布局面板，支持多行换行、溢出排列                 |
| **核心设计**     | 单选驱动的单内容渲染：同一时间仅渲染选中页，切换时自动卸载旧页面 | 标签越多内存优势越明显                                       |
| **工业核心场景** | 设备详情分页面、系统参数分类配置、多报表切换、多设备并行监控 | 所有需要按类别拆分界面的业务场景                             |

### 1.3 类级特性与模板契约

#### 1. `[StyleTypedProperty]` 样式类型声明

csharp:

```c#
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TabItem))]
```

- 向设计器与 XAML 解析器声明，`ItemContainerStyle` 的目标容器类型为 `TabItem`，提供样式智能提示与编译期类型校验；
- 与 `ListBox`、`ComboBox` 遵循完全一致的 `ItemsControl` 体系约定，保证用法统一。

#### 2. 隐式模板部件契约

`TabControl` 遵循 WPF 复合控件的「逻辑 - 外观分离」设计，自定义控件模板时必须保留以下命名部件，否则对应功能会静默失效：

| 部件约定名称               | 类型               | 官方核心作用                                   | 缺失后果                               |
| :------------------------- | :----------------- | :--------------------------------------------- | :------------------------------------- |
| `PART_SelectedContentHost` | `ContentPresenter` | 承载当前选中标签页的内容，是页面渲染的核心宿主 | 切换标签后内容不显示，整个页面区域空白 |
| 标签条 `ItemsPresenter`    | `ItemsPresenter`   | 承载所有标签头的排列                           | 标签头不显示，无法点击切换             |

> ⚠️ 自定义模板必知：可以修改标签的外观、位置、动画，但**不能删除或重命名核心部件**，否则功能会无报错失效，排查难度极高。

------

## 二、静态依赖属性全量深度解析

`TabControl` 自身新增 5 个核心依赖属性，其余全部继承自 `ItemsControl` 与 `Selector`。

### 2.1 TabControl 新增核心依赖属性

#### 1. TabStripPlacementProperty

csharp:

```c#
public static readonly DependencyProperty TabStripPlacementProperty;
public Dock TabStripPlacement { get; set; }
```

- **类型**：`Dock` 枚举
- **默认值**：`Dock.Top`
- **官方作用**：控制标签条的停靠位置，支持 `Top`（顶部）、`Bottom`（底部）、`Left`（左侧）、`Right`（右侧）四个方向。
- **底层机制**：属性变更时，内部调整 `TabPanel` 的排列方向与布局计算逻辑，自动适配横向 / 纵向标签排列。
- **工业场景价值**：工业宽屏软件常设置为 `Left`，做成左侧垂直标签导航，操作路径更短，符合工控软件左侧菜单的使用习惯。

#### 2. SelectedContentProperty

csharp:

```c#
public static readonly DependencyProperty SelectedContentProperty;
public object SelectedContent { get; }
```

- **类型**：`object`
- **性质**：**只读依赖属性**，对外仅暴露 getter，内部由选中逻辑自动维护
- **官方作用**：获取当前选中标签页的内容对象。
- **底层机制**：选中项变化时，自动从当前 `TabItem` 中提取 `Content` 属性值，同步到该属性。
- **典型用法**：代码中获取当前页面的内容控件、数据上下文，用于动态操作当前页。

#### 3. SelectedContentTemplateProperty

csharp:

```c#
public static readonly DependencyProperty SelectedContentTemplateProperty;
public DataTemplate SelectedContentTemplate { get; }
```

- **类型**：`DataTemplate`
- **性质**：只读
- **官方作用**：获取当前选中内容使用的渲染模板。
- **底层机制**：自动同步当前 `TabItem` 的 `ContentTemplate`，或根据数据类型匹配的资源模板。
- **开发说明**：业务开发很少直接读取，自定义控件模板时可通过绑定该属性渲染内容区。

#### 4. SelectedContentTemplateSelectorProperty

csharp:

```c#
public static readonly DependencyProperty SelectedContentTemplateSelectorProperty;
public DataTemplateSelector SelectedContentTemplateSelector { get; }
```

- **类型**：`DataTemplateSelector`
- **性质**：只读
- **官方作用**：获取当前内容的动态模板选择器。
- **适用场景**：复杂动态页面，不同类型的数据项使用完全不同的页面模板。

#### 5. SelectedContentStringFormatProperty

csharp:

```c#
public static readonly DependencyProperty SelectedContentStringFormatProperty;
public string SelectedContentStringFormat { get; }
```

- **类型**：`string`
- **性质**：只读
- **官方作用**：获取内容文本的格式化字符串。
- **适用场景**：纯文本内容的统一格式化，业务开发使用频率较低。

> 🔑 关键共性：`SelectedContent` 系列属性全部为**只读**，外部不能直接赋值，只能通过切换选中标签间接改变，是「单选驱动内容」设计思想的直接体现。

### 2.2 继承的高频核心属性

全部继承自 `ItemsControl` 与 `Selector`，与 `ListBox`、`ComboBox` 用法完全一致，是日常开发最常用的配置项：

| 分类         | 属性                                                         | 来源         | 核心作用                                         |
| :----------- | :----------------------------------------------------------- | :----------- | :----------------------------------------------- |
| 数据绑定     | `ItemsSource`                                                | ItemsControl | 绑定标签页数据源，MVVM 动态生成标签              |
| 选择控制     | `SelectedIndex` / `SelectedItem` / `SelectedValue` / `SelectedValuePath` | Selector     | 控制当前选中标签，双向绑定联动 ViewModel         |
| 标签头显示   | `DisplayMemberPath`                                          | ItemsControl | 指定标签头显示的字段名，简单绑定场景快速使用     |
| 标签头模板   | `ItemTemplate`                                               | ItemsControl | 完全自定义标签头外观，可加图标、状态灯、关闭按钮 |
| 标签容器样式 | `ItemContainerStyle`                                         | ItemsControl | 自定义 `TabItem` 的高度、背景、选中态、字体等    |
| 整体外观     | `Background` / `BorderBrush` / `Padding`                     | Control      | 控制控件整体背景、边框、内边距                   |

------

## 三、配套核心容器：`TabItem` 类定义深度解析

`TabItem` 是 `TabControl` 的默认条目容器，继承自 `HeaderedContentControl`，天然分为「标签头（Header）」和「页面内容（Content）」两部分，是标签页的基本单元。

### 3.1 官方精简类定义

csharp:

```c#
namespace System.Windows.Controls
{
    public class TabItem : HeaderedContentControl
    {
        public static readonly DependencyProperty IsSelectedProperty;

        public bool IsSelected { get; set; }

        protected virtual void OnSelected(RoutedEventArgs e);
        protected virtual void OnUnselected(RoutedEventArgs e);
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e);
        protected override AutomationPeer OnCreateAutomationPeer();
    }
}
```

### 3.2 核心成员逐行解析

#### 1. `IsSelected` 属性

csharp:

```c#
public static readonly DependencyProperty IsSelectedProperty;
public bool IsSelected { get; set; }
```

- 本质是 `Selector.IsSelected` 附加属性的强类型包装，内部直接操作附加属性；
- 支持双向绑定，可通过 ViewModel 直接控制标签的选中状态；
- 是样式触发器的核心目标属性，用于自定义选中、未选中样式。

#### 2. 双内容模型（继承自 `HeaderedContentControl`）

| 属性              | 作用             | 对应区域                                  |
| :---------------- | :--------------- | :---------------------------------------- |
| `Header`          | 标签头显示的内容 | 顶部 / 侧边标签条上的按钮区域             |
| `Content`         | 标签页的主体内容 | 选中后显示的页面主区域，支持任意 WPF 控件 |
| `HeaderTemplate`  | 标签头的内容模板 | 自定义标签头外观                          |
| `ContentTemplate` | 页面内容的模板   | 自定义页面内容渲染                        |

> 设计价值：头与内容完全分离，标签头只负责切换导航，内容区负责业务呈现，职责清晰、定制灵活。

#### 3. `OnSelected / OnUnselected` 虚方法

csharp:

```c#
protected virtual void OnSelected(RoutedEventArgs e);
protected virtual void OnUnselected(RoutedEventArgs e);
```

- 标签被选中 / 取消选中时触发，是子类扩展的核心入口；
- 官方默认逻辑：触发路由事件，同步视觉状态；
- **工业场景扩展价值**：重写可实现「选中时懒加载数据、取消时释放资源」的优化，大幅提升多标签页面的启动速度。

#### 4. 鼠标交互逻辑

- 重写 `OnMouseLeftButtonDown`，点击标签头时触发选中，自动切换到对应页面；
- 内部调用 `Selector` 的选中逻辑，与整个单选体系保持一致。

------

## 四、核心事件体系

`TabControl` 自身**没有新增公共事件**，所有标签切换逻辑都通过继承的 `SelectionChanged` 事件驱动，与 `ListBox`、`ComboBox` 完全统一。

### 核心事件：`SelectionChanged`

- **触发时机**：选中标签发生变化时触发，无论是鼠标点击、代码赋值还是键盘操作导致的切换都会触发；
- **事件参数**：包含 `AddedItems`（新选中项）和 `RemovedItems`（取消选中项）；
- **工业典型用法**：
  1. 懒加载：切换到对应标签时才加载该页的业务数据，避免启动时一次性加载所有页面拖慢速度；
  2. 状态同步：切换页面时保存当前页的编辑状态、校验数据；
  3. 权限控制：切换前校验用户是否有权限访问该标签页，无权限则取消切换。

------

## 五、核心方法逐行解析

### 5.1 公共方法

`TabControl` 没有新增高频公共方法，通用操作全部通过属性与事件驱动；继承自基类的 `Focus()`、`FindResource()` 等方法照常使用。

### 5.2 受保护重写方法（自定义扩展核心）

这部分是自定义 `TabControl` 子类的全部扩展点，完全遵循 `ItemsControl` 的容器生命周期约定。

#### 1. `GetContainerForItemOverride`

csharp:

```c#
protected override DependencyObject GetContainerForItemOverride();
```

- **官方实现**：返回 `new TabItem()`；
- **设计意义**：将抽象条目容器具体化为 `TabItem`，是标签页的标准 UI 载体。

#### 2. `IsItemItsOwnContainerOverride`

csharp:

```c#
protected override bool IsItemItsOwnContainerOverride(object item);
```

- **官方实现**：判断 `item is TabItem`，是则返回 true；
- **作用**：支持 XAML 中直接添加 `<TabItem>` 静态子元素，无需额外包装。

#### 3. `PrepareContainerForItemOverride`

csharp:

```c#
protected override void PrepareContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行流程**：
  1. 调用基类方法，完成数据上下文、样式、模板的基础准备；
  2. 将数据项同步到 `TabItem` 的 `Header` 与 `Content`；
  3. 同步 `IsSelected` 状态到容器；
  4. 应用 `ItemTemplate` 到标签头。
- **扩展价值**：子类重写可注入自定义标签头、默认样式、权限控制等逻辑。

#### 4. `ClearContainerForItemOverride`

csharp:

```c#
protected override void ClearContainerForItemOverride(DependencyObject element, object item);
```

- **官方执行流程**：
  1. 清除 `TabItem` 的选中状态、数据上下文；
  2. 调用基类方法，完成容器清理；
- **说明**：`TabControl` 默认不开启 UI 虚拟化（标签页数量通常较少），该方法主要用于集合移除项时的资源清理。

#### 5. `OnSelectionChanged`（核心）

csharp:

```c#
protected override void OnSelectionChanged(SelectionChangedEventArgs e);
```

这是标签切换的核心入口，`TabControl` 绝大多数核心逻辑都在这里执行：

1. 调用基类方法，触发 `SelectionChanged` 公共事件；
2. 卸载旧标签页的内容，从视觉树中移除旧页面的 UI 元素；
3. 加载新选中标签的内容，将新页面 UI 加入视觉树；
4. 同步更新 `SelectedContent`、`SelectedContentTemplate` 等只读属性；
5. 更新视觉状态，触发标签头的选中 / 未选中样式切换。

> 🔑 底层本质：标签切换的本质是「替换内容区的 UI 元素」，这也是单内容渲染机制的实现点。

#### 6. `OnItemsChanged`

csharp:

```c#
protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e);
```

- 官方扩展逻辑：标签集合增删改时，同步维护选中状态的有效性，避免选中项被删除后出现异常；
- 集合重置时强制刷新所有标签容器。

------

## 六、官方核心工作机制

### 6.1 单内容渲染机制（最核心设计）

这是 `TabControl` 区别于其他多页面方案的本质特征，也是性能设计的核心。

#### 核心原理

同一时间，视觉树中**只保留当前选中标签页的内容 UI**；切换标签时，旧页面的 UI 从视觉树中卸载销毁，新页面的 UI 创建并加载。

#### 完整切换流程

plaintext:

```tex
用户点击新标签 / 代码修改 SelectedItem
    ↓
触发 OnSelectionChanged
    ↓
1. 旧标签：移除 Content 对应的 UI 元素，释放渲染资源
2. 新标签：根据 Content / ContentTemplate 创建 UI 元素，加入内容区
3. 更新 SelectedContent 系列只读属性
4. 切换标签头的选中视觉状态
    ↓
触发布局更新，界面完成切换
```

#### 优缺点

| 优势                                           | 劣势                                                 |
| :--------------------------------------------- | :--------------------------------------------------- |
| 内存占用低：标签再多也只渲染一个页面的 UI      | 切换会重建页面，输入框文本、滚动位置、展开状态会丢失 |
| 启动速度快：初始只加载默认页，不用渲染所有页面 | 频繁切换时重复创建销毁，有一定性能开销               |
| 隔离性好：页面间 UI 完全独立，互不影响         | 复杂页面切换有明显加载感                             |

#### 工业场景最佳实践

1. **状态持久化到 ViewModel**：所有输入、选中、展开状态全部绑定到 ViewModel，切换后自动恢复，避免状态丢失；
2. **数据懒加载**：在 `SelectionChanged` 中按需加载对应页的数据，不要在窗口初始化时加载所有页面数据；
3. **重内容页缓存**：特别复杂的页面可自定义扩展实现内容缓存，切换时只隐藏不销毁，换取切换速度。

### 6.2 标签条布局机制

- 标签条由 `TabPanel` 专属布局面板负责排列，支持多行换行、标签溢出自动折行；
- 通过 `TabStripPlacement` 切换停靠方向，支持上下左右四种布局；
- 左侧 / 右侧垂直标签时，标签头默认横向排列，可通过自定义 `HeaderTemplate` + 旋转变换实现竖排文字。

### 6.3 选中同步机制

完全复用 `Selector` 单选体系的能力：

- 数据层：`SelectedItem` / `SelectedIndex` 持久保存选中状态，与 UI 无关；
- UI 层：每个 `TabItem` 的 `IsSelected` 属性标记选中状态；
- 双向同步：数据层变化自动同步到 UI，UI 操作自动回写到数据层。

------

## 总结

`TabControl` 是 `Selector` 单选体系在「多页面容器」场景的经典落地：它完整复用了 `ItemsControl` 的条目管理与 `Selector` 的单选能力，通过 `TabItem` 的头 - 内容双模型，实现了紧凑、灵活的多页切换布局。其设计精髓在于**单内容渲染**—— 用页面重建的微小代价换取极低的内存占用，非常适合标签数量多、内容复杂的工业软件场景。