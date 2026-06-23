# 005005003_WPF `MultiSelector` 多选基类源码级官方定义深度解析

**源码:**

```c#
public abstract class MultiSelector : Selector
{
    protected MultiSelector();
 
    public IList SelectedItems { get; }
    protected bool CanSelectMultipleItems { get; set; }
    protected bool IsUpdatingSelectedItems { get; }
 
    public void SelectAll();
    public void UnselectAll();
    protected void BeginUpdateSelectedItems();
    protected void EndUpdateSelectedItems();
}
```

`MultiSelector` 是 WPF 所有**支持批量多选的集合控件**的抽象基类，直接继承自 `Selector` 单选基类，在完整继承单选项管理、状态同步、容器生命周期等能力的基础上，新增了**标准化多选集合、批量更新机制、全选 / 全不选通用逻辑**，是 `DataGrid` 等复杂可编辑表格控件的多选能力底层支撑。

相比 `ListBox` 自行实现的多选逻辑，`MultiSelector` 提供了更规范的多选架构，尤其是「批量更新挂起通知」机制，大幅提升了大数据量下批量选中的性能，是工业软件中批量确认、批量导出、批量归档等操作的底层性能保障。

------

## 一、官方类定义总览与核心定位

### 1.1 完整类签名（官方原生定义）

csharp:

```c#
namespace System.Windows.Controls.Primitives
{
    public abstract class MultiSelector : Selector
    {
        // 构造函数
        protected MultiSelector();

        // 公共属性
        public IList SelectedItems { get; }

        // 受保护属性
        protected bool CanSelectMultipleItems { get; set; }
        protected bool IsUpdatingSelectedItems { get; }

        // 公共方法
        public void SelectAll();
        public void UnselectAll();

        // 受保护批量更新方法
        protected void BeginUpdateSelectedItems();
        protected void EndUpdateSelectedItems();
    }
}
```

### 1.2 核心元数据

| 项               | 官方精确值                                                   | 说明                                                         |
| :--------------- | :----------------------------------------------------------- | :----------------------------------------------------------- |
| **命名空间**     | `System.Windows.Controls.Primitives`                         | WPF 底层基类专属命名空间，表明这是抽象基类而非直接使用的控件 |
| **完整继承链**   | `Object → DispatcherObject → DependencyObject → Visual → UIElement → FrameworkElement → Control → ItemsControl → Selector → MultiSelector` | 在单选能力基础上扩展标准化多选架构                           |
| **类类型**       | 抽象类                                                       | 不可直接实例化，仅作为基类被子类继承                         |
| **核心直接子类** | `DataGrid`                                                   | WPF 官方唯一天生继承该类的标准控件                           |
| **设计定位**     | 多选集合控件的通用抽象                                       | 封装多选通用逻辑与批量更新机制，子类只需实现具体交互方式     |
| **核心价值**     | 标准化多选集合 + 批量更新性能优化                            | 解决大数据量下批量选中反复触发事件的性能问题                 |

> 💡 补充说明：`ListBox` 并未继承 `MultiSelector`，它是直接继承 `Selector` 后自行实现了多选逻辑；`MultiSelector` 是 WPF 后续推出的更规范的多选抽象，主要供 `DataGrid` 等复杂控件使用，核心差异在于提供了**批量更新挂起通知**的标准机制。

------

## 二、构造函数

csharp:

```c#
protected MultiSelector();
```

- 受保护构造函数，仅子类可调用；
- 内部初始化 `SelectedItems` 空集合实例，初始化多选状态标记；
- 继承基类 `Selector` 的所有基础能力，包括选中属性、事件、容器生命周期等。

------

## 三、核心属性逐行解析

### 3.1 公共属性：SelectedItems

csharp:

```c#
public IList SelectedItems { get; }
```

- **类型**：`IList`（非泛型集合接口）
- **性质**：**只读依赖属性**，对外仅暴露 getter，内部可维护集合内容
- **官方作用**：存储当前所有选中的数据项，是多选操作的核心数据入口
- **底层机制**：
  1. 集合实例由框架内部创建，外部不能直接替换整个集合，只能增删元素；
  2. 单选模式下集合元素数为 0 或 1，多选模式下可包含多个元素；
  3. 选中项变化时，集合内容同步更新，同时触发 `SelectionChanged` 事件。
- **工业场景用法**：批量确认、批量导出、批量删除时，直接遍历该集合获取所有选中项执行业务逻辑。

> ⚠️ 常见误区：`SelectedItems` 不能直接做双向绑定，MVVM 多选场景需通过附加属性、行为或事件同步到 ViewModel，该特性与 `ListBox` 一致。

### 3.2 受保护属性：CanSelectMultipleItems

csharp:

```c#
protected bool CanSelectMultipleItems { get; set; }
```

- **类型**：`bool`
- **访问级别**：受保护，仅子类内部可访问
- **官方作用**：**多选总开关**，控制当前控件是否允许多选
- **底层联动**：
  - 设为 `false` 时，选中新项会自动取消之前所有选中项，退化为单选行为；
  - 设为 `true` 时，允许多个项同时处于选中状态；
  - 子类通常将其与对外的 `SelectionMode` 属性绑定，比如 `DataGrid.SelectionMode` 切换为 `Single` 时，内部将该值设为 `false`。
- **设计意义**：将多选开关抽离为基类统一开关，子类不需要重复实现单选 / 多选的切换逻辑。

### 3.3 受保护属性：IsUpdatingSelectedItems

csharp:

```c#
protected bool IsUpdatingSelectedItems { get; }
```

- **类型**：`bool`，只读
- **访问级别**：受保护
- **官方作用**：标记当前是否处于**批量选中更新区间**内
- **核心机制**：
  1. 调用 `BeginUpdateSelectedItems()` 后，该值变为 `true`；
  2. 处于批量更新状态时，修改选中项**不会触发 `SelectionChanged` 事件，也不会同步更新 UI**；
  3. 调用 `EndUpdateSelectedItems()` 后，该值变回 `false`，统一触发一次变更事件、批量刷新 UI。
- **性能价值**：批量选中上千条数据时，避免逐条触发事件与 UI 刷新，性能提升一个数量级，是工业大数据批量操作的核心优化点。

------

## 四、核心方法逐行解析

### 4.1 公共方法：SelectAll / UnselectAll

csharp:

```c#
public void SelectAll();
public void UnselectAll();
```

#### SelectAll

- **官方作用**：选中列表中所有数据项
- **前置条件**：`CanSelectMultipleItems = true` 时才有效；单选模式下调用无意义
- **底层实现**：内部自动使用「批量更新」机制，一次性全选后只触发一次 `SelectionChanged` 事件，性能远高于循环逐条选中
- **工业场景**：批量全选报警进行确认、全选生产记录进行导出

#### UnselectAll

- **官方作用**：取消所有选中，清空选中状态
- **适用所有选择模式**：单选模式下等价于将 `SelectedIndex` 设为 -1
- **底层同样使用批量更新**，一次性清空后只触发一次变更事件
- **工业场景**：筛选条件变更后重置选中状态、批量操作完成后清空选择

### 4.2 受保护核心方法：批量更新配对

这是 `MultiSelector` 最核心的价值所在，也是它区别于普通自行实现多选的关键。

#### BeginUpdateSelectedItems

csharp:

```c#
protected void BeginUpdateSelectedItems();
```

- **官方作用**：开启批量更新模式，挂起选中变更通知
- **执行效果**：
  1. 将 `IsUpdatingSelectedItems` 标记设为 `true`；
  2. 后续所有对选中项的修改，都只更新内部数据集合，**不触发 `SelectionChanged` 事件、不刷新 UI**；
  3. 避免批量修改时反复触发事件、反复重绘 UI 导致的性能卡顿。

#### EndUpdateSelectedItems

csharp:

```c#
protected void EndUpdateSelectedItems();
```

- **官方作用**：结束批量更新模式，统一提交变更并触发通知
- **执行效果**：
  1. 将 `IsUpdatingSelectedItems` 标记设为 `false`；
  2. 计算本次批量更新的新增选中项与取消选中项；
  3. **统一触发一次 `SelectionChanged` 事件**，批量刷新 UI 视觉状态；
  4. 同步更新集合视图、焦点等关联状态。

> 🔑 设计思想：**批量操作 + 单次通知**，类似数据库事务的「批量提交」思想。多次修改合并为一次通知，大幅降低事件与 UI 渲染开销，是大数据量多选场景的性能基石。

------

## 五、核心工作机制

### 5.1 多选状态同步机制

`MultiSelector` 的多选状态完全复用 `Selector` 基类的「数据层 - UI 层双层映射」模型，仅扩展了集合维度：

1. **数据层**：`SelectedItems` 集合持久保存所有选中数据项，与虚拟化无关，始终有效；
2. **UI 层**：每个可见容器的 `IsSelected` 附加属性标记单条选中状态；
3. **同步规则**：数据层变化批量同步到可见容器，容器生成时从数据层恢复选中状态；
4. **虚拟化兼容**：选中状态绑定在数据项上，容器滚出屏幕回收时状态不丢失，滚入时自动恢复。

### 5.2 批量更新完整流程

plaintext:

```tex
调用 BeginUpdateSelectedItems()
    ↓
IsUpdatingSelectedItems = true
    ↓
循环修改选中状态（添加/移除）
    → 仅更新内部 SelectedItems 集合
    → 不触发 SelectionChanged 事件
    → 不逐行刷新 UI
    ↓
调用 EndUpdateSelectedItems()
    ↓
IsUpdatingSelectedItems = false
    ↓
计算本次变更的 AddedItems / RemovedItems
    ↓
统一触发一次 SelectionChanged 事件
    ↓
批量更新所有可见容器的 IsSelected 状态
    ↓
一次性重绘 UI
```

### 5.3 与 ListBox 多选的本质区别

| 维度         | ListBox 多选                     | MultiSelector 多选                |
| :----------- | :------------------------------- | :-------------------------------- |
| 实现方式     | 继承 Selector 自行实现           | 基类封装标准多选架构              |
| 批量更新机制 | 无标准批量接口，逐条触发事件     | 内置 Begin/End 批量更新，单次通知 |
| 大数据性能   | 批量选中时事件频繁触发，性能较差 | 批量操作性能优异，适合大数据量    |
| 典型控件     | ListBox、ListView                | DataGrid                          |

------

## 六、子类扩展与工业场景应用

### 6.1 子类典型扩展方式

自定义多选控件时，继承 `MultiSelector` 后只需关注两点：

1. **交互逻辑**：实现鼠标点击、Ctrl/Shift 组合键、拖拽框选等交互，调用基类方法修改选中状态；
2. **批量优化**：批量选中场景下，主动调用 `BeginUpdateSelectedItems` / `EndUpdateSelectedItems` 包裹操作，获得性能提升。

### 6.2 工业场景价值

工业软件中常见的「报警批量确认」「生产记录批量导出」「配方批量下发」等场景，往往需要一次选中上百上千条数据：

- 若没有批量更新机制，每选中一条就触发一次事件、刷新一次 UI，会出现明显卡顿；
- `MultiSelector` 的批量更新机制将 N 次通知合并为 1 次，大幅提升操作流畅度，是 `DataGrid` 能支撑工业大数据批量操作的底层保障。

------

## 总结

`MultiSelector` 是 `Selector` 单选体系向多选的标准化延伸，它的核心价值不在于「实现多选」，而在于**提供了工业级的批量更新性能优化机制**。通过「开始 - 结束」的批量区间模式，将多次选中变更合并为一次通知，完美解决了大数据量下批量选中的性能问题，是 `DataGrid` 等复杂表格控件多选能力的底层基石。