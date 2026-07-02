# 007008002_WPF `ValidationRule` 复杂实战实例

1. 以下实例均面向工业参数配置、PLC 通信、设备调试等真实复杂场景，解决单一场景校验无法覆盖的**可配置复用、格式解析、多字段联动、硬件特性约束**等问题，全部贴合欧姆龙 PLC、工业相机、激光焊接等业务场景，可直接落地到项目中。

   ## 复杂校验规则设计原则

   1. **高复用性**：通过属性 / 参数配置规则，一个类覆盖一类场景，避免重复造轮子；
   2. **分层校验**：格式校验前置，范围 / 步长校验后置，错误提示精准定位问题；
   3. **健壮性优先**：兼容空值、异常格式、边界值，校验失败不抛出异常，统一返回结构化错误信息；
   4. **场景贴合**：匹配工业硬件的真实约束（寄存器地址规则、参数步长、单位制式）。

   ------

   ## 实例 1：通用可配置正则校验规则

   ### 应用场景

   工业软件中存在大量格式类校验：IP 地址、端口号、设备条码、PLC 地址前缀、序列号等。如果每个格式写一个校验类，会产生大量重复代码。

   本规则通过**配置正则表达式 + 错误提示**，用一个类覆盖所有格式校验场景，是项目级通用基础设施。

   ### 代码实现

   csharp:

   ```
   using System.Globalization;
   using System.Text.RegularExpressions;
   using System.Windows.Controls;
   
   /// <summary>
   /// 通用正则校验规则：通过Pattern属性配置正则，适配所有格式校验场景
   /// </summary>
   public class RegexValidationRule : ValidationRule
   {
       /// <summary>
       /// 正则表达式
       /// </summary>
       public string Pattern { get; set; }
   
       /// <summary>
       /// 自定义错误提示
       /// </summary>
       public string ErrorMessage { get; set; } = "输入格式不正确";
   
       /// <summary>
       /// 是否允许空值
       /// </summary>
       public bool AllowEmpty { get; set; } = false;
   
       public override ValidationResult Validate(object value, CultureInfo cultureInfo)
       {
           string input = value?.ToString()?.Trim();
   
           // 空值处理
           if (string.IsNullOrWhiteSpace(input))
               return AllowEmpty 
                   ? ValidationResult.ValidResult 
                   : new ValidationResult(false, "输入内容不能为空");
   
           // 正则匹配
           try
           {
               bool isMatch = Regex.IsMatch(input, Pattern, RegexOptions.IgnoreCase);
               return isMatch 
                   ? ValidationResult.ValidResult 
                   : new ValidationResult(false, ErrorMessage);
           }
           catch
           {
               return new ValidationResult(false, "校验规则配置错误");
           }
       }
   }
   ```

   ### XAML 使用示例

   xaml:

   ```xaml
   <StackPanel Spacing="10" Margin="30">
       <!-- 1. PLC IP地址校验 -->
       <TextBlock Text="PLC IP地址："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="PlcIpAddress" Mode="TwoWay" UpdateSourceTrigger="LostFocus">
                   <Binding.ValidationRules>
                       <local:RegexValidationRule 
                           Pattern="^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$"
                           ErrorMessage="请输入合法的IPv4地址，如 192.168.1.100"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   
       <!-- 2. 通信端口校验 -->
       <TextBlock Text="通信端口："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="PlcPort" Mode="TwoWay" UpdateSourceTrigger="LostFocus">
                   <Binding.ValidationRules>
                       <local:RegexValidationRule 
                           Pattern="^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$"
                           ErrorMessage="端口号范围：1 ~ 65535"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   
       <!-- 3. 产品条码格式校验 -->
       <TextBlock Text="产品条码："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="ProductBarcode" Mode="TwoWay" UpdateSourceTrigger="PropertyChanged">
                   <Binding.ValidationRules>
                       <local:RegexValidationRule 
                           Pattern="^[A-Z]{2}\d{8}$"
                           ErrorMessage="条码格式为2位大写字母+8位数字"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   </StackPanel>
   ```

   ### 核心亮点

   1. **极致复用**：一个类覆盖所有格式校验场景，新增格式无需编码，只需配置正则；
   2. **灵活配置**：支持空值开关、自定义错误信息，适配不同业务要求；
   3. **异常兼容**：正则表达式配置错误时不会导致程序崩溃，友好提示。

   ------

   ## 实例 2：PLC 寄存器地址合法性校验

   ### 应用场景

   PLC 参数配置页面，用户手动输入寄存器地址（如欧姆龙的 D 区、W 区、H 区、A 区），需要校验：

   1. 区域名称是否合法；

   2. 地址数值是否在对应区域的有效范围内；

   3. 支持十六进制 / 十进制地址格式。

      

      避免非法地址导致通信报错。

   ### 代码实现

   csharp:

   ```c#
   using System;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Linq;
   using System.Windows.Controls;
   
   /// <summary>
   /// 欧姆龙PLC寄存器地址校验规则
   /// 支持D、W、H、A、C、T等常用区域，分区域校验地址范围
   /// </summary>
   public class PlcAddressValidationRule : ValidationRule
   {
       // 各区域的地址最大值（十进制）
       private static readonly Dictionary<string, int> _areaMaxAddress = new()
       {
           { "D", 65535 },   // 数据寄存器
           { "W", 9999 },    // 工作寄存器
           { "H", 511 },     // 保持继电器
           { "A", 9999 },    // 特殊辅助继电器
           { "C", 4095 },    // 计数器
           { "T", 4095 }     // 定时器
       };
   
       /// <summary>
       /// 允许的区域列表，逗号分隔，为空则允许所有区域
       /// </summary>
       public string AllowAreas { get; set; }
   
       /// <summary>
       /// 是否支持十六进制地址（带H后缀）
       /// </summary>
       public bool SupportHex { get; set; } = false;
   
       public override ValidationResult Validate(object value, CultureInfo cultureInfo)
       {
           string address = value?.ToString()?.Trim().ToUpper();
           if (string.IsNullOrWhiteSpace(address))
               return new ValidationResult(false, "寄存器地址不能为空");
   
           // 1. 拆分区域前缀和地址数值
           int splitIndex = 0;
           while (splitIndex < address.Length && char.IsLetter(address[splitIndex]))
               splitIndex++;
   
           if (splitIndex == 0 || splitIndex >= address.Length)
               return new ValidationResult(false, "地址格式错误，示例：D100、W200");
   
           string area = address.Substring(0, splitIndex);
           string numStr = address.Substring(splitIndex);
   
           // 2. 校验区域是否允许
           if (!_areaMaxAddress.ContainsKey(area))
               return new ValidationResult(false, $"不支持的寄存器区域：{area}");
   
           if (!string.IsNullOrWhiteSpace(AllowAreas))
           {
               var allowList = AllowAreas.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(a => a.Trim().ToUpper());
               if (!allowList.Contains(area))
                   return new ValidationResult(false, $"该场景仅允许使用 {AllowAreas} 区域地址");
           }
   
           // 3. 解析地址数值
           int addressNum;
           if (SupportHex && numStr.EndsWith("H"))
           {
               string hexNum = numStr.TrimEnd('H');
               if (!int.TryParse(hexNum, NumberStyles.HexNumber, cultureInfo, out addressNum))
                   return new ValidationResult(false, "十六进制地址格式错误");
           }
           else
           {
               if (!int.TryParse(numStr, out addressNum) || addressNum < 0)
                   return new ValidationResult(false, "地址编号必须为正整数");
           }
   
           // 4. 校验地址范围
           int max = _areaMaxAddress[area];
           if (addressNum > max)
               return new ValidationResult(false, $"{area} 区地址最大为 {max}，超出有效范围");
   
           return ValidationResult.ValidResult;
       }
   }
   ```

   ### XAML 使用示例

   xaml:

   ```xaml
   <StackPanel Spacing="10" Margin="30">
       <TextBlock Text="心跳寄存器地址："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="HeartbeatAddress" Mode="TwoWay" UpdateSourceTrigger="LostFocus">
                   <Binding.ValidationRules>
                       <!-- 仅允许D区和W区 -->
                       <local:PlcAddressValidationRule AllowAreas="D,W"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   
       <TextBlock Text="触发输出地址："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="TriggerAddress" Mode="TwoWay" UpdateSourceTrigger="LostFocus">
                   <Binding.ValidationRules>
                       <local:PlcAddressValidationRule AllowAreas="H" SupportHex="True"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   </StackPanel>
   ```

   ### 核心亮点

   1. **贴合工业场景**：完全匹配 PLC 编程的地址规则，内置各区域的硬件范围约束；
   2. **灵活配置**：可限制允许的区域、是否支持十六进制，适配不同通信场景；
   3. **错误精准**：区分 “格式错、区域不支持、超出范围” 等多种错误，引导用户快速修正。

   ------

   ## 实例 3：带单位智能数值解析校验

   ### 应用场景

   工业参数输入支持带单位输入，比如曝光时间可以输入「1500」或「1500μs」、厚度可以输入「2.5」或「2.5mm」，自动识别单位并转换为标准数值，同时校验范围。

   提升操作体验，符合工程师的输入习惯，同时保证数据格式统一。

   ### 代码实现

   csharp:

   ```c#
   using System;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Windows.Controls;
   
   /// <summary>
   /// 带单位数值校验：自动识别单位后缀，转换为标准单位后校验范围
   /// 支持多单位自动换算，比如时间单位：s、ms、μs；长度单位：m、mm、μm
   /// </summary>
   public class UnitNumberValidationRule : ValidationRule
   {
       /// <summary>
       /// 单位类型：Time(时间)、Length(长度)、Pressure(压力)
       /// </summary>
       public string UnitType { get; set; } = "Time";
   
       /// <summary>
       /// 最小值（标准单位）
       /// </summary>
       public double Min { get; set; } = 0;
   
       /// <summary>
       /// 最大值（标准单位）
       /// </summary>
       public double Max { get; set; } = double.MaxValue;
   
       // 各单位相对于标准单位的换算系数
       private static readonly Dictionary<string, double> _timeUnits = new()
       {
           { "S", 1000000 },    // 秒 → 微秒（标准单位μs）
           { "MS", 1000 },      // 毫秒 → 微秒
           { "US", 1 },         // 微秒（标准单位）
           { "μS", 1 }
       };
   
       private static readonly Dictionary<string, double> _lengthUnits = new()
       {
           { "M", 1000 },       // 米 → 毫米（标准单位mm）
           { "CM", 10 },        // 厘米 → 毫米
           { "MM", 1 },         // 毫米（标准单位）
           { "μm", 0.001 }      // 微米 → 毫米
       };
   
       public override ValidationResult Validate(object value, CultureInfo cultureInfo)
       {
           string input = value?.ToString()?.Trim();
           if (string.IsNullOrWhiteSpace(input))
               return new ValidationResult(false, "数值不能为空");
   
           // 1. 拆分数值和单位
           int numEndIndex = 0;
           while (numEndIndex < input.Length 
                  && (char.IsDigit(input[numEndIndex]) || input[numEndIndex] == '.' || input[numEndIndex] == '-'))
               numEndIndex++;
   
           string numStr = input.Substring(0, numEndIndex);
           string unit = input.Substring(numEndIndex).Trim().ToUpper();
   
           // 2. 解析数值
           if (!double.TryParse(numStr, NumberStyles.Float, cultureInfo, out double numValue))
               return new ValidationResult(false, "请输入有效数字");
   
           // 3. 无单位则直接校验范围
           if (string.IsNullOrEmpty(unit))
           {
               if (numValue < Min || numValue > Max)
                   return new ValidationResult(false, $"取值范围：{Min} ~ {Max}");
               return ValidationResult.ValidResult;
           }
   
           // 4. 获取单位换算系数
           Dictionary<string, double> unitDict = UnitType switch
           {
               "Time" => _timeUnits,
               "Length" => _lengthUnits,
               _ => new Dictionary<string, double>()
           };
   
           if (!unitDict.TryGetValue(unit, out double scale))
               return new ValidationResult(false, $"不支持的单位：{unit}");
   
           // 5. 转换为标准单位后校验范围
           double standardValue = numValue * scale;
           if (standardValue < Min || standardValue > Max)
               return new ValidationResult(false, $"有效范围：{Min} ~ {Max}（标准单位）");
   
           return ValidationResult.ValidResult;
       }
   }
   ```

   ### XAML 使用示例

   xaml:

   ```xaml
   <StackPanel Spacing="10" Margin="30">
       <TextBlock Text="相机曝光时间："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="ExposureTime" Mode="TwoWay" UpdateSourceTrigger="LostFocus"
                        Converter="{StaticResource ExposureUnitConverter}">
                   <Binding.ValidationRules>
                       <!-- 时间类型，范围100~10000μs -->
                       <local:UnitNumberValidationRule 
                           UnitType="Time" Min="100" Max="10000"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   
       <TextBlock Text="焊接间隙："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="WeldGap" Mode="TwoWay" UpdateSourceTrigger="LostFocus"
                        Converter="{StaticResource LengthUnitConverter}">
                   <Binding.ValidationRules>
                       <!-- 长度类型，范围0.1~5mm -->
                       <local:UnitNumberValidationRule 
                           UnitType="Length" Min="0.1" Max="5"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   </StackPanel>
   ```

   ### 核心亮点

   1. **用户友好**：支持带单位输入，符合工业现场操作习惯，无需手动删除单位；
   2. **自动换算**：自动转换为标准单位，保证数据源数值统一；
   3. **可扩展**：新增单位类型只需添加对应字典，无需修改核心校验逻辑。

   ------

   ## 实例 4：双值联动校验（上下限约束）

   ### 应用场景

   参数配置中的上下限输入（如温度上下限、压力上下限、阈值范围），要求**最大值必须大于等于最小值**，修改任意一端都要重新校验。

   难点：`ValidationRule` 默认只能获取当前控件的值，无法直接绑定另一个属性的值。本实例通过**继承 DependencyObject 实现依赖属性绑定**，突破这个限制，实现多字段联动校验。

   ### 代码实现

   csharp:

   ```c#
   using System.Globalization;
   using System.Windows;
   using System.Windows.Controls;
   
   /// <summary>
   /// 大于等于校验：当前值必须大于等于指定的绑定值
   /// 用于上下限联动校验，支持绑定另一个属性
   /// </summary>
   public class GreaterThanOrEqualRule : ValidationRule
   {
       // 注册依赖属性，支持绑定另一个比较值
       public static readonly DependencyProperty CompareValueProperty =
           DependencyProperty.Register(
               nameof(CompareValue),
               typeof(double),
               typeof(GreaterThanOrEqualRule),
               new PropertyMetadata(0d));
   
       /// <summary>
       /// 比较基准值（支持绑定）
       /// </summary>
       public double CompareValue
       {
           get => (double)GetValue(CompareValueProperty);
           set => SetValue(CompareValueProperty, value);
       }
   
       /// <summary>
       /// 错误提示前缀
       /// </summary>
       public string ErrorPrefix { get; set; } = "上限值";
   
       public override ValidationResult Validate(object value, CultureInfo cultureInfo)
       {
           if (!double.TryParse(value?.ToString(), out double currentValue))
               return new ValidationResult(false, "请输入有效数字");
   
           if (currentValue < CompareValue)
               return new ValidationResult(false, $"{ErrorPrefix}不能小于 {CompareValue}");
   
           return ValidationResult.ValidResult;
       }
   }
   ```

   ### XAML 使用示例

   > 注意：ValidationRule 不在可视化树中，绑定需要通过 `ElementName` 引用控件。

   xaml:

   ```xaml
   <StackPanel Spacing="10" Margin="30">
       <StackPanel Orientation="Horizontal" Spacing="10">
           <TextBlock Width="80" VerticalAlignment="Center">温度下限：</TextBlock>
           <TextBox x:Name="TxtMinTemp" Width="150">
               <TextBox.Text>
                   <Binding Path="MinTemperature" Mode="TwoWay" UpdateSourceTrigger="PropertyChanged"/>
               </TextBox.Text>
           </TextBox>
           <TextBlock VerticalAlignment="Center">℃</TextBlock>
       </StackPanel>
   
       <StackPanel Orientation="Horizontal" Spacing="10">
           <TextBlock Width="80" VerticalAlignment="Center">温度上限：</TextBlock>
           <TextBox Width="150">
               <TextBox.Text>
                   <Binding Path="MaxTemperature" Mode="TwoWay" UpdateSourceTrigger="PropertyChanged">
                       <Binding.ValidationRules>
                           <!-- 绑定下限输入框的值作为比较基准 -->
                           <local:GreaterThanOrEqualRule 
                               ErrorPrefix="温度上限"
                               CompareValue="{Binding Text, ElementName=TxtMinTemp, Mode=OneWay}"/>
                       </Binding.ValidationRules>
                   </Binding>
               </TextBox.Text>
           </TextBox>
           <TextBlock VerticalAlignment="Center">℃</TextBlock>
       </StackPanel>
   </StackPanel>
   ```

   ### 核心亮点

   1. **突破限制**：通过依赖属性让 ValidationRule 支持绑定，实现跨控件联动校验；
   2. **实时联动**：修改下限值时，上限的校验会自动重新触发，实时更新错误状态；
   3. **通用复用**：不仅限于温度上下限，所有范围类输入都可直接使用。

   ### 注意事项

   - 依赖属性的绑定源推荐用 `ElementName`，无法直接继承 DataContext；
   - 如果需要绑定 ViewModel 属性，可通过父控件的 Tag 属性做中转。

   ------

   ## 实例 5：数值步长校验

   ### 应用场景

   PLC、运动控制器、相机的很多参数有固定步长要求，比如：

   - 曝光时间必须是 10μs 的整数倍；

   - 电机速度必须是 50 的整数倍；

   - 激光功率步进为 1%。

     

     不符合步长的参数下发到设备会被截断或不响应，必须在校验层拦截。

   ### 代码实现

   csharp:

   ```c#
   using System;
   using System.Globalization;
   using System.Windows.Controls;
   
   /// <summary>
   /// 数值步长校验：数值必须是指定步长的整数倍
   /// </summary>
   public class StepValidationRule : ValidationRule
   {
       /// <summary>
       /// 步长值
       /// </summary>
       public double Step { get; set; } = 1;
   
       /// <summary>
       /// 计算精度（处理浮点数误差）
       /// </summary>
       public double Tolerance { get; set; } = 1e-6;
   
       public override ValidationResult Validate(object value, CultureInfo cultureInfo)
       {
           if (!double.TryParse(value?.ToString(), NumberStyles.Float, cultureInfo, out double num))
               return new ValidationResult(false, "请输入有效数字");
   
           // 计算余数，处理浮点数精度误差
           double remainder = num % Step;
           bool isValid = remainder < Tolerance || Math.Abs(remainder - Step) < Tolerance;
   
           if (!isValid)
               return new ValidationResult(false, $"数值必须是 {Step} 的整数倍");
   
           return ValidationResult.ValidResult;
       }
   }
   ```

   ### XAML 使用示例

   xaml:

   ```xaml
   <StackPanel Spacing="10" Margin="30">
       <TextBlock Text="相机曝光时间（步长10μs）："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="ExposureTime" Mode="TwoWay" UpdateSourceTrigger="LostFocus">
                   <Binding.ValidationRules>
                       <!-- 分层校验：先范围，后步长 -->
                       <local:DoubleRangeValidationRule Min="100" Max="10000"/>
                       <local:StepValidationRule Step="10"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   
       <TextBlock Text="传送带速度（步长50）："/>
       <TextBox Width="250">
           <TextBox.Text>
               <Binding Path="ConveyorSpeed" Mode="TwoWay" UpdateSourceTrigger="LostFocus">
                   <Binding.ValidationRules>
                       <local:DoubleRangeValidationRule Min="0" Max="3000"/>
                       <local:StepValidationRule Step="50"/>
                   </Binding.ValidationRules>
               </Binding>
           </TextBox.Text>
       </TextBox>
   </StackPanel>
   ```

   ### 核心亮点x

   1. **贴合硬件特性**：匹配工业设备参数的步进约束，从源头避免非法参数下发；
   2. **精度容错**：处理浮点数运算误差，避免因精度问题导致的误判；
   3. **组合使用**：可与范围校验规则叠加，实现「格式→范围→步长」的分层校验。

   ------

   ## 高级注意事项

   ### 1. 联动校验的绑定限制

   `ValidationRule` 本身不继承 `FrameworkElement`，没有 `DataContext`，无法直接继承数据源。绑定 ViewModel 属性时，推荐两种方案：

   - 简单场景：通过 `ElementName` 绑定另一个控件的值；
   - 复杂场景：改用 `IDataErrorInfo` 在 ViewModel 层做业务联动校验。

   ### 2. 校验阶段的选择

   - `RawProposedValue`：原始字符串阶段，适合格式、正则、空值校验；
   - `ConvertedProposedValue`：转换后强类型阶段，适合范围、步长、联动校验；
   - 分层校验可以让错误提示更精准，同时避免格式错误导致后续校验异常。

   ### 3. 性能优化

   - 高频输入（`UpdateSourceTrigger=PropertyChanged`）的场景，避免在校验中做复杂运算、正则回溯、IO 操作；
   - 正则表达式尽量优化，避免灾难性回溯；
   - 静态缓存字典、常量配置，避免每次校验都重新创建对象。

   ### 4. 与业务校验的边界

   - `ValidationRule` 只负责**UI 输入层的格式、范围、步长等基础校验**；
   - 涉及业务规则（如 “设备运行中不允许修改该参数”、“权限不足无法修改”）的校验，应放在 ViewModel 层通过 `IDataErrorInfo` 实现，职责分离。

   ------

   ## 选型总结

   | 场景                           | 推荐方案                      |
   | :----------------------------- | :---------------------------- |
   | 通用格式校验（IP、条码、地址） | 可配置正则校验规则            |
   | PLC 寄存器地址校验             | 专用分区域地址校验            |
   | 带单位参数输入                 | 智能单位解析校验 + 值转换器   |
   | 上下限等联动校验               | 依赖属性绑定的联动校验规则    |
   | 硬件步进参数校验               | 步长校验规则                  |
   | 复杂业务规则、多字段强联动     | ViewModel 层 `IDataErrorInfo` |