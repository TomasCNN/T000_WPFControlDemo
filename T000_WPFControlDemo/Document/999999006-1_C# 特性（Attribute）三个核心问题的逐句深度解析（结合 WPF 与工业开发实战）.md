# 999999006-1_C# 特性（Attribute）三个核心问题的逐句深度解析（结合 WPF 与工业开发实战）

## 问题 1：特性和普通的代码有什么区别？

### 核心本质区别

**普通代码是 "执行逻辑"，特性是 "描述逻辑的元数据"**。

- 普通代码：告诉计算机 "要做什么"，运行时直接执行
- 特性：告诉计算机 "这个东西是什么 / 有什么属性"，本身不执行任何操作，只是给代码打标签，需要其他代码（通常是框架）通过反射读取后才能发挥作用

### 多维度详细对比

| 对比维度     | 普通代码                       | 特性（Attribute）                       |
| :----------- | :----------------------------- | :-------------------------------------- |
| **核心作用** | 执行业务逻辑                   | 存储元数据（描述数据的数据）            |
| **执行时机** | 运行时主动执行                 | 编译时嵌入程序集，运行时被反射读取      |
| **工作原理** | CPU 直接执行指令               | 作为元数据存在于程序集的元数据表中      |
| **设计思想** | 命令式编程（告诉计算机怎么做） | 声明式编程（告诉计算机是什么）          |
| **修改方式** | 修改代码需要重新编译           | 可以在不修改业务逻辑的情况下添加 / 移除 |
| **作用对象** | 直接操作数据和状态             | 修饰类、方法、属性、程序集等代码元素    |
| **返回值**   | 可以有返回值                   | 本身没有返回值，只是存储数据            |

### 通俗类比 + 项目实例

把代码比作**快递包裹**：

- 普通代码：包裹里的商品（真正有价值的东西，你买它就是为了用它）
- 特性：包裹上的快递单（描述包裹的信息：收件人、地址、电话、重量）
- 反射：快递员（读取快递单上的信息，然后进行分拣、派送）

**我们项目中的实例对比**：

csharp:

```c#
// 普通代码：主动初始化Log4Net（命令式）
var config = new FileInfo("log4net.config");
log4net.Config.XmlConfigurator.Configure(config);

// 特性：声明式配置Log4Net（只打标签，不执行代码）
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
```

- 普通代码：你主动调用方法去初始化
- 特性：你只是打了个标签，Log4Net 框架在启动时会自动扫描所有程序集，找到这个标签，然后帮你完成初始化

### 关键误区澄清

**特性本身不执行任何代码！** 很多初学者以为特性会自动执行，这是最大的误区。

- 如果你打了标签，但没有任何人去读这个标签，那这个标签就毫无意义
- 真正起作用的是**读取特性的反射代码**，而不是特性本身

------

## 问题 2：特性在 WPF 中有哪些应用场景？

WPF 是**特性驱动的框架**，几乎所有核心功能都基于特性实现。你之前深入学习过的 TextBlock、依赖属性、数据绑定，背后都是特性在支撑。下面是工业开发中最常用的 8 个场景，每个都有你熟悉的实例。

### 1. XAML 解析核心特性

这是 WPF 最基础也是最重要的特性，让 XAML 能够和 C# 代码无缝映射。

#### ① `[ContentProperty]` 内容特性

**作用**：指定类的默认内容属性，XAML 标签中间的内容会自动赋值给这个属性。

**你最熟悉的实例**：

csharp:

```c#
// TextBlock源码中的特性
[ContentProperty(nameof(Inlines))]
public class TextBlock : FrameworkElement { }
```

**为什么你可以这样写？**

xaml:

```xaml
<TextBlock>Hello World</TextBlock>
<!-- 等价于 -->
<TextBlock Inlines="Hello World"/>
```

**工业意义**：让 XAML 代码更简洁，所有容器控件（Grid、StackPanel、Border）都用了这个特性。

#### ② `[XmlnsDefinition]` 命名空间映射特性

**作用**：将 CLR 命名空间映射到 XML 命名空间，让你可以在 XAML 中引用自定义控件。

**实例**：

csharp:

```c#
// 在AssemblyInfo.cs中
[assembly: XmlnsDefinition("http://schemas.yourcompany.com/industrial", "Industrial.Controls")]
```

**然后你就可以在 XAML 中这样引用**：

xaml:

```xaml
<Window xmlns:controls="http://schemas.yourcompany.com/industrial">
    <controls:SimpleTextBlockLogger />
</Window>
```

### 2. 依赖属性系统特性

WPF 的依赖属性系统完全基于特性构建。

#### ① `[DependencyProperty]` 依赖属性特性

**作用**：标记一个静态字段为依赖属性，WPF 会自动为其提供数据绑定、样式、动画等功能。

**实例**：

csharp:

```c#
public static readonly DependencyProperty TemperatureProperty = DependencyProperty.Register(
    nameof(Temperature),
    typeof(double),
    typeof(DeviceControl),
    new PropertyMetadata(0.0));
```

#### ② `[AttachedProperty]` 附加属性特性

**作用**：标记一个静态方法为附加属性，允许你在子元素上设置父容器的属性。

**你每天都在用的实例**：

xaml:

```xaml
<Grid>
    <!-- Grid.Row和Grid.Column就是附加属性 -->
    <TextBlock Grid.Row="0" Grid.Column="0" Text="温度："/>
</Grid>
```

### 3. 数据验证特性

工业上位机中参数配置界面的必备功能，用特性声明式实现数据验证。

**实例**：

csharp:

```c#
public class DeviceParameter
{
    [Required(ErrorMessage = "设备名称不能为空")]
    [StringLength(50, ErrorMessage = "设备名称不能超过50个字符")]
    public string Name { get; set; }

    [Range(-40, 100, ErrorMessage = "温度必须在-40℃到100℃之间")]
    public double Temperature { get; set; }

    [Range(0, 10, ErrorMessage = "压力必须在0MPa到10MPa之间")]
    public double Pressure { get; set; }
}
```

**WPF 会自动读取这些特性，在用户输入时进行验证，并显示错误提示**。

### 4. 命令特性

WPF 的 MVVM 模式核心，用特性将按钮点击事件绑定到 ViewModel 的命令。

**实例**：

csharp:

```c#
public class MainViewModel
{
    // 标记为命令
    public ICommand StartCommand { get; }
    
    public MainViewModel()
    {
        StartCommand = new RelayCommand(StartDevice);
    }
    
    private void StartDevice()
    {
        // 启动设备逻辑
    }
}
```

xaml:

```xaml
<Button Content="启动设备" Command="{Binding StartCommand}"/>
```

### 5. 样式与模板特性

用于自定义控件的样式和模板。

#### ① `[StyleTypedProperty]` 样式类型特性

**作用**：指定控件的样式属性的类型，让设计器能够正确识别。

**实例**：

csharp:

```c#
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ListBoxItem))]
public class ListBox : Selector { }
```

#### ② `[TemplatePart]` 模板部件特性

**作用**：指定控件模板中必须包含的部件，让自定义控件的开发更规范。

**实例**：

csharp:

```c#
[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Button", Type = typeof(Button))]
public class NumericUpDown : Control { }
```

### 6. 设计器支持特性

让你的自定义控件在 Visual Studio 设计器中能够正常显示和编辑。

**常用特性**：

- `[DesignerCategory]`：指定控件的设计器类别
- `[ToolboxItem]`：指定控件是否显示在工具箱中
- `[Browsable]`：指定属性是否显示在属性窗口中
- `[Category]`：指定属性在属性窗口中的分类

### 7. 序列化特性

用于将 WPF 对象序列化为 XAML 或其他格式。

**实例**：

csharp:

```c#
[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
public InlineCollection Inlines { get; }
```

**作用**：告诉 XAML 设计器，应该将 Inlines 集合的内容序列化，而不是集合本身。

### 8. 工业场景专用特性

我们之前实现的 PLC 寄存器地址映射特性，就是典型的工业场景自定义特性：

csharp:

```c#
public class DeviceData
{
    [PlcAddress("DB1.DBD0", typeof(double), Scale = 0.1)]
    public double Temperature { get; set; }
}
```

------

## 问题 3：如何定义和使用自定义特性？

### 完整三步法

1. **定义特性类**：继承自`System.Attribute`
2. **应用特性**：将特性打在目标代码元素上
3. **反射读取特性**：运行时通过反射读取特性的值，执行相应逻辑

### 步骤 1：定义自定义特性

csharp:

```c#
using System;

// 第一步：定义特性类，必须继承自Attribute
// [AttributeUsage]：限制特性的使用范围，这是自定义特性的关键
[AttributeUsage(
    AttributeTargets.Property, // 只能应用在属性上
    AllowMultiple = false,     // 一个属性只能应用一次
    Inherited = true)]         // 子类可以继承父类的特性
public class PlcAddressAttribute : Attribute
{
    // 特性的构造函数参数（必选参数）
    public PlcAddressAttribute(string address, Type dataType)
    {
        Address = address;
        DataType = dataType;
    }

    // 特性的属性（可选参数）
    public string Address { get; }
    public Type DataType { get; }
    public double Scale { get; set; } = 1.0; // 默认值1.0
    public string Description { get; set; }
}
```

#### `[AttributeUsage]` 三个核心参数详解

| 参数               | 含义                                 | 工业最佳实践                                 |
| :----------------- | :----------------------------------- | :------------------------------------------- |
| `AttributeTargets` | 特性可以应用的目标                   | 尽可能精确，比如只允许应用在属性上，避免滥用 |
| `AllowMultiple`    | 同一个目标是否可以应用多个相同的特性 | 大部分情况设为 false，只有特殊场景设为 true  |
| `Inherited`        | 子类是否可以继承父类的特性           | 一般设为 true，符合面向对象的继承思想        |

### 步骤 2：应用特性

将特性打在你想要修饰的代码元素上：

csharp:

```c#
/// <summary>
/// 设备数据实体类
/// </summary>
public class DeviceData
{
    // 应用特性，传入必选参数
    [PlcAddress("DB1.DBD0", typeof(double), Scale = 0.1, Description = "设备温度")]
    public double Temperature { get; set; }

    [PlcAddress("DB1.DBD4", typeof(double), Scale = 0.1, Description = "设备压力")]
    public double Pressure { get; set; }

    [PlcAddress("DB1.DBW8", typeof(int), Description = "电机转速")]
    public int Speed { get; set; }

    [PlcAddress("DB1.DBX10.0", typeof(bool), Description = "运行状态")]
    public bool IsRunning { get; set; }
}
```

### 步骤 3：反射读取特性

这是最关键的一步，特性的价值只有在被读取时才能体现出来：

csharp:

```c#
using System.Reflection;

/// <summary>
/// PLC数据映射器
/// 通过反射读取PlcAddress特性，自动实现数据读写
/// </summary>
public class PlcDataMapper
{
    private readonly PlcCommunicator _plc;

    public PlcDataMapper(PlcCommunicator plc)
    {
        _plc = plc;
    }

    /// <summary>
    /// 从PLC读取数据，填充到实体对象
    /// </summary>
    public T ReadFromPlc<T>() where T : new()
    {
        T obj = new T();
        
        // 获取类型的所有公共属性
        PropertyInfo[] properties = typeof(T).GetProperties(
            BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            // 读取属性上的PlcAddress特性
            var attribute = prop.GetCustomAttribute<PlcAddressAttribute>();
            
            // 如果没有这个特性，跳过
            if (attribute == null)
                continue;

            // 根据特性中的地址和类型，从PLC读取数据
            object rawValue = _plc.ReadData(attribute.Address, attribute.DataType);
            
            // 应用缩放系数
            if (attribute.Scale != 1.0 && rawValue is double d)
            {
                rawValue = d * attribute.Scale;
            }

            // 将值设置到对象的属性
            prop.SetValue(obj, rawValue);
        }

        return obj;
    }
}
```

### 步骤 4：使用示例

csharp:

```c#
class Program
{
    static void Main()
    {
        // 连接PLC
        var plc = new PlcCommunicator("192.168.1.100");
        var mapper = new PlcDataMapper(plc);

        // 一行代码读取所有参数，自动映射
        DeviceData data = mapper.ReadFromPlc<DeviceData>();
        
        Console.WriteLine($"温度：{data.Temperature}℃");
        Console.WriteLine($"压力：{data.Pressure}MPa");
    }
}
```

### 自定义特性最佳实践

1. **特性类命名规范**：以`Attribute`结尾，这是 C# 的约定
2. **只存储元数据**：不要在特性中写复杂的业务逻辑，特性应该是轻量级的
3. **使用不可变属性**：特性的属性应该只有 getter，没有 setter，在构造函数中初始化
4. **合理使用可选参数**：对于不常用的参数，提供默认值
5. **缓存反射结果**：反射有性能开销，对于频繁使用的特性，应该缓存读取结果
6. **工业开发优先使用**：对于配置类、映射类、验证类的场景，特性 + 反射是最佳实践

------

## 总结

特性是 C# 中**声明式编程**的核心，它让我们可以用 "打标签" 的方式来描述代码，然后由框架或工具通过反射读取这些标签，自动完成相应的工作。

在 WPF 工业开发中，特性无处不在：

- XAML 解析依赖特性
- 依赖属性系统依赖特性
- 数据绑定和验证依赖特性
- 我们自己实现的 PLC 地址映射也依赖特性

掌握特性的原理和用法，你就能从 "使用框架" 升级到 "设计框架"，写出更简洁、更灵活、更易维护的工业级代码。