# 007008001_WPF `ValidationRule` 验证规则完整深度解析

`ValidationRule` 是 WPF 数据绑定体系中**UI 输入侧的第一道校验防线**，位于 `System.Windows.Controls` 命名空间。它在双向绑定的「UI 值回写数据源」之前执行，对用户输入做合法性检查，校验失败则阻止值回写数据源，并在界面标记错误状态，从源头避免非法数据进入业务层，是工业参数配置、设备参数下发场景的必备特性。

------

## 一、核心定义与底层原理

### 1. 本质与定位

`ValidationRule` 是一个抽象基类，通过自定义子类实现具体校验逻辑，附加在数据绑定上。它属于**视图层校验**，只关心输入格式、范围、合法性，不包含业务规则，作用是：

- 拦截非法输入，避免污染数据源；
- 实时给出错误提示，引导用户正确输入；
- 工业场景核心价值：防止越界参数、非法格式下发到 PLC、相机等硬件，避免设备异常。

### 2. 在绑定链路中的位置

双向绑定（`TwoWay`）回写数据源的完整链路：

plaintext:

```tex
用户修改UI控件值
    ↓
触发依赖属性变更
    ↓
按顺序执行绑定的 ValidationRule 校验
    ↓ 校验通过
执行 ConvertBack（值转换器反向转换）
    ↓
回写到数据源属性
```

只要任意一条校验规则失败，链路就会终止回写，标记绑定为错误状态，触发错误样式显示，**数据源的值保持不变**，从根本上保证数据安全。

### 3. 校验执行阶段：`ValidationStep`

`ValidationRule` 有一个 `ValidationStep` 属性，控制校验在链路的哪个阶段执行，共 4 个阶段：

| 阶段                         | 说明                                 | 典型用途                                        |
| :--------------------------- | :----------------------------------- | :---------------------------------------------- |
| `RawProposedValue`（默认值） | 转换之前，使用原始输入字符串校验     | 校验格式：是不是数字、是不是合法 IP、是不是空值 |
| `ConvertedProposedValue`     | 经过转换器转换后，使用目标类型值校验 | 校验范围：数值是否在 100~10000 之间             |
| `UpdatedValue`               | 数据源更新之后校验                   | 极少用，用于更新后二次校验                      |
| `CommittedValue`             | 提交确认后校验                       | 极少用，用于最终提交校验                        |

工业场景最佳实践：**分层校验**—— 先用 `RawProposedValue` 校验格式，再用 `ConvertedProposedValue` 校验数值范围，错误提示更精准。

### 4. 与其他校验方式的区别

WPF 有三层校验体系，`ValidationRule` 是最外层的 UI 层校验：

| 校验方式               | 层级         | 定义位置             | 适用场景                                     |
| :--------------------- | :----------- | :------------------- | :------------------------------------------- |
| `ValidationRule`       | UI 绑定层    | XAML 绑定中          | 输入格式、数值范围、长度限制等 UI 侧基础校验 |
| `IDataErrorInfo`       | 数据源业务层 | ViewModel / 实体类中 | 业务规则校验、多字段关联校验                 |
| `INotifyDataErrorInfo` | 数据源业务层 | ViewModel / 实体类中 | 异步校验、多错误、动态校验规则               |

工业项目推荐分层：UI 层用 `ValidationRule` 做格式 / 范围硬校验，业务层用 `IDataErrorInfo` 做业务逻辑校验，各司其职。

------

## 二、标准使用三步法

### 第一步：自定义校验规则类

继承 `ValidationRule` 抽象类，重写 `Validate` 方法：

- 输入：待校验的值、区域文化信息；
- 返回：`ValidationResult` 对象，成功返回 `ValidationResult.ValidResult`，失败返回带错误信息的结果。

### 第二步：绑定中附加校验规则

校验规则是绑定的集合属性，必须使用**属性元素语法**添加，不能写在 `{Binding}` 简写中。

### 第三步：配置错误显示

WPF 默认错误模板是控件周围的红色边框，工业软件通常会自定义错误模板，增加图标、Tooltip 错误详情，提升交互体验。

------

## 三、核心注意事项与避坑指南

### 1. 仅双向绑定生效

校验只在**值从 UI 回写数据源**时触发，单向绑定（`OneWay`/`OneTime`）不会执行校验，因为不存在回写动作。

- 可编辑控件（`TextBox`、`Slider`、`DatePicker`）默认双向绑定，可正常触发；
- 纯展示控件无法触发校验。

### 2. 校验失败不阻止输入，只阻止回写

校验失败不会清空用户输入，也不会禁止继续编辑，只是：

- 控件显示错误样式；
- 值不会回写到数据源，数据源保持旧值不变。

> 工业场景注意：参数下发前必须主动校验所有输入合法性，不能仅凭样式判断，更不能直接从控件读值下发。

### 3. 默认错误模板的布局坑

默认错误样式通过 `AdornerLayer`（装饰层）绘制，位于控件上方，不占用布局空间；如果控件外容器设置了 `ClipToBounds="True"`，红色边框会被裁剪掉。

- 解决方案：自定义错误模板时控制外边距，或关闭容器的裁剪。

### 4. 执行时机与性能

校验触发频率由 `UpdateSourceTrigger` 决定：

- `PropertyChanged`：每次输入都校验，实时性好，但高频输入会重复执行校验；
- `LostFocus`：控件失焦才校验，性能更好，适合长文本、复杂校验。

> 最佳实践：简单范围校验用 `PropertyChanged` 实时提示，复杂格式校验用 `LostFocus` 减少性能开销。

### 5. 多规则执行顺序

多个校验规则按 XAML 中添加的顺序从上到下执行，默认所有规则都会执行，错误信息会收集到 `Validation.Errors` 集合中。

- 建议顺序：先格式校验，后范围校验，最后业务校验，前置规则失败可快速定位问题。

### 6. 纯函数设计，无状态无副作用

校验规则应是纯函数：相同输入永远返回相同结果，禁止在校验中做 IO 操作、数据库查询、设备通信、修改外部状态。

- 原因：校验触发频率高，耗时操作会直接导致输入卡顿；且校验不应该产生副作用。

### 7. 错误信息要明确

失败时返回的错误信息要具体，不要只写 “输入错误”，要说明 “曝光时间范围 100~10000μs”、“请输入合法的 IPv4 地址”，引导用户快速修正。

------

## 四、基础实战实例（工业场景版）

所有实例围绕工业上位机常见的参数配置场景设计，可直接复制运行。

### 实例 1：基础数值范围校验（相机曝光时间）

**场景**：相机曝光时间输入框，限制输入值必须在 100~10000μs 之间，超出范围提示错误，阻止非法值写入数据源。

#### 1. 自定义校验规则

csharp:

```c#
using System.Globalization;
using System.Windows.Controls;

/// <summary>
/// 数值范围校验规则
/// </summary>
public class DoubleRangeValidationRule : ValidationRule
{
    /// <summary> 最小值 </summary>
    public double Min { get; set; } = 0;
    /// <summary> 最大值 </summary>
    public double Max { get; set; } = double.MaxValue;

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        // 1. 先校验能不能转成数字
        if (!double.TryParse(value?.ToString(), out double num))
            return new ValidationResult(false, "请输入有效数字");

        // 2. 校验范围
        if (num < Min || num > Max)
            return new ValidationResult(false, $"取值范围：{Min} ~ {Max} μs");

        // 3. 校验通过
        return ValidationResult.ValidResult;
    }
}
```

#### 2. XAML 绑定中使用

xaml:

```xaml
<StackPanel Margin="30" Spacing="10">
    <TextBlock Text="相机曝光时间（μs）："/>
    <TextBox Width="250">
        <TextBox.Text>
            <Binding Path="ExposureTime" 
                     Mode="TwoWay" 
                     UpdateSourceTrigger="PropertyChanged">
                <Binding.ValidationRules>
                    <!-- 添加校验规则，配置阈值 -->
                    <local:DoubleRangeValidationRule Min="100" Max="10000"/>
                </Binding.ValidationRules>
            </Binding>
        </TextBox.Text>
    </TextBox>
</StackPanel>
```

**效果**：输入非数字、小于 100、大于 10000 时，输入框自动显示红色边框，值不会回写到 ViewModel 的 `ExposureTime` 属性。

------

### 实例 2：字符串格式校验（PLC IP 地址）

**场景**：PLC 通信 IP 地址输入框，校验输入是否符合 IPv4 格式，避免非法地址导致连接失败。

#### 1. 自定义校验规则

csharp:

```c#
using System.Globalization;
using System.Net;
using System.Windows.Controls;

/// <summary>
/// IPv4 地址格式校验
/// </summary>
public class IpAddressValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string input = value?.ToString()?.Trim();
        
        if (string.IsNullOrWhiteSpace(input))
            return new ValidationResult(false, "IP 地址不能为空");

        // 用系统内置类校验IP格式
        bool isValid = IPAddress.TryParse(input, out IPAddress ip) 
                       && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;

        if (!isValid)
            return new ValidationResult(false, "请输入合法的 IPv4 地址，如 192.168.1.1");

        return ValidationResult.ValidResult;
    }
}
```

#### 2. XAML 使用

xaml:

```xaml
<TextBox Width="250">
    <TextBox.Text>
        <Binding Path="PlcIpAddress" Mode="TwoWay" UpdateSourceTrigger="LostFocus">
            <Binding.ValidationRules>
                <local:IpAddressValidationRule/>
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>
```

------

### 实例 3：自定义错误提示模板（带 Tooltip 详情）

默认红色边框没有错误详情，工业软件通常自定义模板，鼠标悬停显示完整错误信息，体验更好。

#### 1. 定义全局错误样式

放在 `Window.Resources` 或全局资源字典中：

xaml:

```xaml
<Window.Resources>
    <!-- 自定义错误模板 -->
    <ControlTemplate x:Key="ValidationErrorTemplate">
        <DockPanel>
            <!-- 右侧红色感叹号图标 -->
            <TextBlock DockPanel.Dock="Right" 
                       Foreground="Red" 
                       FontSize="16" 
                       FontWeight="Bold"
                       Text="!"
                       Margin="2 0 0 0"
                       ToolTip="{Binding ElementName=AdornerPlaceholder, Path=AdornedElement.(Validation.Errors)[0].ErrorContent}"/>
            <!-- 占位符，承载原控件 -->
            <AdornedElementPlaceholder x:Name="AdornerPlaceholder"/>
        </DockPanel>
    </ControlTemplate>

    <!-- 文本框默认应用错误模板 + 悬停提示 -->
    <Style TargetType="TextBox">
        <Setter Property="Validation.ErrorTemplate" Value="{StaticResource ValidationErrorTemplate}"/>
        <Style.Triggers>
            <Trigger Property="Validation.HasError" Value="True">
                <Setter Property="ToolTip" 
                        Value="{Binding RelativeSource={RelativeSource Self}, 
                                Path=(Validation.Errors)[0].ErrorContent}"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</Window.Resources>
```

**效果**：输入错误时，输入框右侧显示红色感叹号，鼠标悬停弹出完整错误信息，比默认样式更直观，且不占用额外布局空间。

------

### 实例 4：分层校验：格式 + 范围（Raw + Converted）

**场景**：分两层校验：

1. 原始输入阶段（Raw）：校验是不是有效数字；

2. 转换后阶段（Converted）：校验数值是否在合法范围。

   

   分层后错误提示更精准，用户能立刻知道是格式错了还是范围错了。

#### 1. 两个校验规则

csharp:

```c#
/// <summary>
/// 第一层：原始字符串格式校验（Raw）
/// </summary>
public class DoubleFormatValidationRule : ValidationRule
{
    public DoubleFormatValidationRule()
    {
        // 指定为转换前校验
        ValidationStep = ValidationStep.RawProposedValue;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (!double.TryParse(value?.ToString(), out _))
            return new ValidationResult(false, "输入格式错误，请输入数字");
        
        return ValidationResult.ValidResult;
    }
}

/// <summary>
/// 第二层：转换后数值范围校验（Converted）
/// </summary>
public class DoubleRangeConvertedRule : ValidationRule
{
    public double Min { get; set; }
    public double Max { get; set; }

    public DoubleRangeConvertedRule()
    {
        // 指定为转换后校验
        ValidationStep = ValidationStep.ConvertedProposedValue;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not double num)
            return new ValidationResult(false, "数值转换失败");

        if (num < Min || num > Max)
            return new ValidationResult(false, $"数值需在 {Min} ~ {Max} 之间");

        return ValidationResult.ValidResult;
    }
}
```

#### 2. XAML 组合使用

xaml:

```xaml
<TextBox Width="250">
    <TextBox.Text>
        <Binding Path="ExposureTime" Mode="TwoWay" UpdateSourceTrigger="PropertyChanged">
            <Binding.ValidationRules>
                <!-- 第一层：格式校验 -->
                <local:DoubleFormatValidationRule/>
                <!-- 第二层：范围校验 -->
                <local:DoubleRangeConvertedRule Min="100" Max="10000"/>
            </Binding.ValidationRules>
        </Binding>
    </TextBox.Text>
</TextBox>
```

**效果**：输入字母提示 “输入格式错误”，输入数字但超出范围提示 “数值需在 100 ~ 10000 之间”，错误提示精准分层。

------

### 补充：提交前统一校验

参数下发前，需要确认所有输入都合法，可通过 `Validation.GetHasError()` 方法判断：

csharp:

```c#
private void BtnApply_Click(object sender, RoutedEventArgs e)
{
    // 检查输入框是否有校验错误
    bool hasError = Validation.GetHasError(ExposureTextBox);
    if (hasError)
    {
        MessageBox.Show("参数输入不合法，请修正后再提交");
        return;
    }

    // 校验通过，下发参数到设备
    Vm.ApplyParams();
}
```

------

## 五、选型总结

1. **UI 层基础校验**（格式、范围、长度、非空）：优先用 `ValidationRule`，轻量、声明式、不侵入业务代码；
2. **业务规则校验**（多字段关联、权限、业务逻辑）：用 `IDataErrorInfo` / `INotifyDataErrorInfo`，放在 ViewModel 中；
3. **工业场景推荐**：参数配置输入全部加范围校验，硬件通信地址加格式校验，从 UI 层拦截非法输入，避免设备异常；
4. **体验优化**：自定义错误模板，搭配 Tooltip 显示详细错误信息，兼顾视觉和实用性。