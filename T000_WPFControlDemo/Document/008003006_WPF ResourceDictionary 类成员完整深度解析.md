# 008003006_WPF `ResourceDictionary` 类成员完整深度解析

**源码：**

```c#
public class ResourceDictionary : IDictionary, ICollection, IEnumerable, ISupportInitialize, IUriContext, INameScope
{
    public ResourceDictionary();
 
    public object this[object key] { get; set; }
 
    public ICollection Keys { get; }
    public DeferrableContent DeferrableContent { get; set; }
    public bool InvalidatesImplicitDataTemplateResources { get; set; }
    public bool IsReadOnly { get; }
    public bool IsFixedSize { get; }
    public Uri Source { get; set; }
    public Collection<ResourceDictionary> MergedDictionaries { get; }
    public ICollection Values { get; }
    public int Count { get; }
 
    public void Add(object key, object value);
    public void BeginInit();
    public void Clear();
    public bool Contains(object key);
    public void CopyTo(DictionaryEntry[] array, int arrayIndex);
    public void EndInit();
    public object FindName(string name);
    public IDictionaryEnumerator GetEnumerator();
    public void RegisterName(string name, object scopedElement);
    public void Remove(object key);
    public void UnregisterName(string name);
    protected virtual void OnGettingValue(object key, ref object value, out bool canCache);
 
}
```

面按「构造函数 → 属性 → 公开方法 → 受保护回调方法」的顺序，逐成员解析官方定义、设计意图、资源体系作用与开发注意事项，覆盖所有易踩坑的细节。

------

## 一、构造函数

csharp:

```c#
public ResourceDictionary();
```

- **官方定义**：初始化一个空的 `ResourceDictionary` 实例。
- **内部行为**：初始化底层哈希表存储结构、创建空的 `MergedDictionaries` 集合、初始化名称范围。
- **使用场景**：
  1. 动态加载主题 / 外部字典时手动创建实例：`new ResourceDictionary { Source = new Uri("...") }`；
  2. 自定义扩展资源字典时调用基类构造。
- **注意**：构造函数仅创建空容器，不会自动加载任何资源；加载外部文件通过 `Source` 属性完成。

------

## 二、核心属性详解

### 1. 索引器：资源读写核心入口

csharp:

```c#
public object this[object key] { get; set; }
```

- **类型**：`object` 键 → `object` 值，支持读写
- **官方作用**：通过键读取或设置资源值，是字典最核心的访问入口。
- **读取行为（重点，极易踩坑）**：
  1. 先查找**当前字典的本地资源**；
  2. 本地找不到时，**逆序遍历 `MergedDictionaries` 合并字典**（从最后一个合并的字典往前找），因为后合并的字典优先级更高；
  3. 全部找不到则返回 `null`。
- **写入行为**：仅写入**当前字典的本地资源**，不会修改任何合并字典；如果键已存在则直接覆盖旧值。
- **键类型**：键为 `object` 类型，不仅支持字符串（`x:Key="xxx"`），还支持 `Type`（隐式样式）、`ComponentResourceKey`（跨程序集资源）等多种合法键类型。

> ⚠️ 重要不一致：**读取会遍历合并字典，写入 / 删除 / 查询只操作本地**。`Contains`、`Keys`、`Count` 等成员都只统计本地资源，和索引器的读取范围不一致，是新手最高频的踩坑点。

### 2. Keys / Values 键值集合

csharp:

```c#
public ICollection Keys { get; }
public ICollection Values { get; }
```

- **官方作用**：分别获取当前字典**本地资源**的所有键、所有值的集合。
- **注意**：仅包含本地资源，**不包含合并字典中的资源**。如果需要获取所有资源的键，需要自行递归遍历 `MergedDictionaries`。

### 3. Count 资源计数

csharp:

```c#
public int Count { get; }
```

- **官方作用**：返回当前字典**本地资源**的条目总数。
- **注意**：同样只统计本地资源，不包含合并字典；不要用 `Count` 判断合并字典的资源总数。

### 4. IsReadOnly / IsFixedSize 只读与固定大小

csharp:

```c#
public bool IsReadOnly { get; }
public bool IsFixedSize { get; }
```

- `IsReadOnly`：指示字典是否只读。
  - 用户自定义字典默认 `false`，支持增删改；
  - WPF 系统主题资源字典为 `true`，强行修改会抛出异常。
- `IsFixedSize`：来自 `IDictionary` 接口，指示字典大小是否固定。
  - 普通字典默认 `false`，支持动态增删；
  - 系统只读字典返回 `true`。

### 5. Source 外部字典路径

csharp:

```c#
public Uri Source { get; set; }
```

- **官方作用**：获取或设置外部资源字典文件的 URI 路径。设置后 WPF 会自动加载并解析指定的 `.xaml` 文件，填充当前字典的内容。
- **支持的路径格式**：
  1. 项目内相对路径：`/Styles/BaseStyles.xaml`
  2. 跨程序集 Pack URI：`pack://application:,,,/MyAssembly;component/Styles/Common.xaml`
- **注意**：
  - 设置 `Source` 会**清空当前字典的本地资源**，替换为加载的文件内容；
  - 相对路径基于 `IUriContext` 上下文基准解析，嵌套合并时路径基准会自动适配。

### 6. MergedDictionaries 合并字典集合

csharp:

```c#
public Collection<ResourceDictionary> MergedDictionaries { get; }
```

- **官方作用**：合并资源字典集合，是 WPF 模块化资源、主题切换的核心基础设施。

- **集合性质**：属性本身只读（不能重新赋值集合对象），但可以向集合内添加、移除字典实例。

- **优先级规则（官方明确规定）**：

  

  本地资源 > 后添加的合并字典 > 先添加的合并字典

  

  查找资源时，本地找不到的话，从最后一个合并的字典往前逆序查找。

- **嵌套支持**：合并的字典内部还可以再合并其他字典，支持无限嵌套；但嵌套过深（超过 3 层）会降低查找性能，建议控制层级。

- **常见坑**：每个窗口都重复合并同一个字典文件，会创建多份独立实例，造成内存浪费；全局通用字典建议只在 `App.xaml` 合并一次。

### 7. DeferrableContent 延迟加载内容

csharp:

```c#
public DeferrableContent DeferrableContent { get; set; }
```

- **官方作用**：存储可延迟解析的 XAML 原始内容，由 XAML 编译器自动生成和设置。
- **设计意图**：大型资源字典的性能优化。字典加载时不立即解析所有资源的 XAML，只保存原始二进制内容；**首次访问对应资源时才完成解析与实例化**，大幅提升程序冷启动速度。
- **开发注意**：几乎不需要手动操作该属性，完全由框架自动处理；手动赋值会覆盖原有内容，极易导致资源解析异常。

### 8. InvalidatesImplicitDataTemplateResources 隐式模板失效开关

csharp:

```c#
public bool InvalidatesImplicitDataTemplateResources { get; set; }
```

- **官方定义**：获取或设置一个值，指示当隐式数据模板资源变更时，是否强制重新计算所有匹配的元素。WPF 4.0 新增属性。
- **解决的问题**：默认情况下，动态替换包含隐式 `DataTemplate`（通过 `DataType` 匹配）的合并字典后，已经渲染的元素不会自动重新匹配模板，出现「切换主题后数据模板不更新」的 bug。
- **使用建议**：
  - 动态切换主题、且主题中包含隐式数据模板时，将此属性设为 `true`，字典变更时会自动触发隐式模板失效，强制所有元素重新选择模板；
  - 普通无模板的纯样式字典保持默认 `false` 即可，避免不必要的性能开销。

------

## 三、公开方法详解

### 1. 资源增删改查基础方法

csharp:

```c#
public void Add(object key, object value);
public void Remove(object key);
public void Clear();
public bool Contains(object key);
```

| 方法              | 官方作用                       | 注意事项                                                     |
| :---------------- | :----------------------------- | :----------------------------------------------------------- |
| `Add(key, value)` | 向**本地字典**添加一条资源     | 键已存在会抛出 `ArgumentException`，添加前建议用 `Contains` 校验 |
| `Remove(key)`     | 从**本地字典**移除指定键的资源 | 键不存在时静默处理，不会抛异常                               |
| `Clear()`         | 清空**本地字典**的所有资源     | 不会影响 `MergedDictionaries` 合并字典集合                   |
| `Contains(key)`   | 判断**本地字典**是否存在指定键 | 仅查本地，不会遍历合并字典；和索引器读取范围不一致           |

> 核心原则：所有增删查方法都只操作**本地资源**，不会修改或遍历合并字典。

### 2. CopyTo 批量复制

csharp:

```c#
public void CopyTo(DictionaryEntry[] array, int arrayIndex);
```

- 来自 `ICollection` 接口，将本地字典的所有键值对（`DictionaryEntry` 结构）复制到指定数组，从指定索引开始写入。
- 业务开发极少直接使用，一般用于批量导出、备份资源。

### 3. GetEnumerator 枚举器

csharp:

```c#
public IDictionaryEnumerator GetEnumerator();
```

- 返回字典枚举器，用于遍历本地字典的所有键值对；支持 `foreach` 语法（由 `IEnumerable` 接口提供）。
- 注意：遍历的仅为本地资源，不包含合并字典。

### 4. BeginInit / EndInit 批量初始化

csharp:

```c#
public void BeginInit();
public void EndInit();
```

- 来自 `ISupportInitialize` 接口，标记批量初始化的开始与结束。
- **工作原理**：
  1. 调用 `BeginInit()` 后，添加资源不会触发状态变更通知；
  2. 调用 `EndInit()` 后，所有添加的资源统一生效，并触发一次全局通知。
- **设计意图**：批量添加大量资源时，避免逐个添加触发多次通知，显著提升加载性能。
- **使用场景**：
  - XAML 解析器加载字典时会自动成对调用，框架内部使用；
  - 手动批量添加上百条资源时，可手动成对调用优化性能。

### 5. 名称范围管理方法（INameScope 接口）

csharp:

```c#
public object FindName(string name);
public void RegisterName(string name, object scopedElement);
public void UnregisterName(string name);
```

- 来自 `INameScope` 接口，负责 XAML 名称范围的管理，实现模板内名称隔离。

  | 方法                        | 作用                                                 |      |
  | --------------------------- | ---------------------------------------------------- | ---- |
  | FindName(name)              | 在当前名称范围内查找指定名称的对象                   |      |
  | RegisterName(name, element) | 注册名称与对象的映射，XAML 中 x:Name会自动调用此方法 |      |
  | UnregisterName(name)        | 注销名称映射                                         |      |

  

- **开发定位**：框架内部使用，用于控件模板、数据模板内部的元素名称解析，保证模板内的名称不会和外部作用域冲突。普通业务开发几乎不会直接调用。

------

## 四、受保护虚方法：资源共享机制的核心

csharp：

```c#
protected virtual void OnGettingValue(object key, ref object value, out bool canCache);
```

这是 `ResourceDictionary` 最核心的扩展点，也是 `x:Shared` 特性的底层实现，绝大多数开发者很少接触到。

### 官方定义

当从字典中获取资源值时调用，允许子类重写此方法，修改返回的资源值，并指示该值是否可以被缓存复用。

### 参数详解

| 参数       | 类型         | 作用                                                     |
| :--------- | :----------- | :------------------------------------------------------- |
| `key`      | `object`     | 当前正在查找的资源键                                     |
| `value`    | `ref object` | 引用传递，当前找到的资源值；可在重写中修改，返回给调用方 |
| `canCache` | `out bool`   | 输出参数，指示该资源值是否可以被缓存复用                 |

### 设计意图

1. **`x:Shared` 特性的底层实现**：

   - 默认返回 `canCache = true`，资源会被缓存为单例，所有引用共享同一个实例，对应 `x:Shared="True"`（默认值）；
   - 如果资源标记了 `x:Shared="False"`，框架内部会通过此回调返回 `canCache = false`，每次获取资源都创建全新实例。

2. **自定义资源扩展**：

   

   子类可重写此方法，实现动态生成资源、按需解析资源、权限控制资源返回等高级扩展。

### 开发注意

- 普通业务开发不需要重写；
- 自定义资源字典类时，重写后建议调用基类方法，保留默认的缓存逻辑。

------

## 五、整体设计总结

`ResourceDictionary` 的成员设计可以分为四层，共同支撑起 WPF 完整的资源体系：

1. **存储层**：索引器、增删改查方法、Keys/Values/Count → 核心键值存储与访问能力
2. **模块化层**：`MergedDictionaries`、`Source` → 资源拆分、合并、外部文件加载能力
3. **性能优化层**：`BeginInit/EndInit`、`DeferrableContent`、`OnGettingValue` → 批量加载优化、延迟解析、单例缓存
4. **名称隔离层**：`INameScope` 相关方法 → 模板内名称范围管理，避免命名冲突
5. **动态适配层**：`InvalidatesImplicitDataTemplateResources` → 动态切换资源时的 UI 刷新控制

### 核心设计原则

- **本地优先，后合并优先**：保证就近覆盖、主题替换的语义正确；
- **性能优先**：通过延迟加载、单例缓存、批量初始化多重优化，兼顾高复用与高性能；
- **可扩展**：通过虚方法、多类型键、合并机制，支持自定义扩展与复杂架构。