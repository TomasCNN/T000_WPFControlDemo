# 008005002_WPF `MultiTrigger` 多条件触发器简单实例合集

以下实例均基于**控件自身依赖属性**的多条件组合（纯 UI 交互状态），无需额外业务代码，纯 XAML 即可运行，全部贴合工业上位机的按钮、参数输入框、数据列表等常用场景。

------

## 实例 1：按钮「启用 + 鼠标悬停」复合高亮

### 应用场景

工业操作按钮，**禁用状态下悬停无任何高亮反馈**，只有「控件启用 + 鼠标悬停」同时满足时，才显示背景变浅 + 外发光效果，避免操作人员误判禁用按钮可点击。

### 完整代码

xaml:

```xaml
<Window x:Class="MultiTriggerDemo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MultiTrigger 按钮复合状态" Height="250" Width="450">

    <Window.Resources>
        <Style x:Key="OperateButtonStyle" TargetType="Button">
            <!-- 默认状态 -->
            <Setter Property="Width" Value="120"/>
            <Setter Property="Height" Value="36"/>
            <Setter Property="Background" Value="#2E7DFF"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Margin" Value="0 0 15 0"/>

            <Style.Triggers>
                <!-- 单条件：禁用状态 -->
                <Trigger Property="IsEnabled" Value="False">
                    <Setter Property="Background" Value="#CCCCCC"/>
                    <Setter Property="Foreground" Value="#999999"/>
                    <Setter Property="Cursor" Value="No"/>
                </Trigger>

                <!-- 多条件：启用 + 鼠标悬停，同时满足才高亮发光 -->
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property="IsEnabled" Value="True"/>
                        <Condition Property="IsMouseOver" Value="True"/>
                    </MultiTrigger.Conditions>
                    <MultiTrigger.Setters>
                        <Setter Property="Background" Value="#5597FF"/>
                        <Setter Property="Effect">
                            <Setter.Value>
                                <DropShadowEffect 
                                    Color="#2E7DFF" 
                                    Opacity="0.4" 
                                    BlurRadius="8" 
                                    ShadowDepth="0"/>
                            </Setter.Value>
                        </Setter>
                    </MultiTrigger.Setters>
                </MultiTrigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <StackPanel Orientation="Horizontal" Margin="50" VerticalAlignment="Center">
        <Button Content="启动设备" Style="{StaticResource OperateButtonStyle}"/>
        <Button Content="禁用按钮" Style="{StaticResource OperateButtonStyle}" IsEnabled="False"/>
    </StackPanel>
</Window>
```

### 效果说明

- 正常启用按钮：默认蓝色，鼠标悬停时背景变浅并出现蓝色外发光；
- 禁用按钮：灰色显示，鼠标悬停无任何变化，不会误导操作人员。

### 核心要点

多条件实现了「状态过滤」：只有符合前置条件（启用）的交互，才会触发视觉反馈，交互逻辑更严谨。

------

## 实例 2：文本框「聚焦 + 只读」状态强化

### 应用场景

设备参数只读展示框（如设备序列号、固件版本），用户可以选中复制但不能修改；**当输入框获得焦点且为只读时**，加深边框并高亮背景，清晰标识当前选中的参数项，方便用户定位复制。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style x:Key="ReadOnlyParamStyle" TargetType="TextBox">
        <!-- 默认只读样式 -->
        <Setter Property="Width" Value="280"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Padding" Value="6 4"/>
        <Setter Property="IsReadOnly" Value="True"/>
        <Setter Property="Background" Value="#F5F6FA"/>
        <Setter Property="BorderBrush" Value="#E4E7ED"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Foreground" Value="#333"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>

        <Style.Triggers>
            <!-- 多条件：获得焦点 + 只读状态 -->
            <MultiTrigger>
                <MultiTrigger.Conditions>
                    <Condition Property="IsKeyboardFocused" Value="True"/>
                    <Condition Property="IsReadOnly" Value="True"/>
                </MultiTrigger.Conditions>
                <MultiTrigger.Setters>
                    <Setter Property="BorderBrush" Value="#2E7DFF"/>
                    <Setter Property="BorderThickness" Value="2"/>
                    <Setter Property="Background" Value="#F0F7FF"/>
                </Setter>
            </MultiTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="40" Spacing="15">
    <TextBlock Text="设备基础参数" FontSize="16" FontWeight="Bold"/>
    <TextBox Text="设备序列号：SN-2024-00156" Style="{StaticResource ReadOnlyParamStyle}"/>
    <TextBox Text="固件版本：V2.3.1" Style="{StaticResource ReadOnlyParamStyle}"/>
    <TextBox Text="出厂日期：2024-05-18" Style="{StaticResource ReadOnlyParamStyle}"/>
</StackPanel>
```

### 效果说明

- 默认状态：浅灰背景、细边框，和普通可编辑输入框形成视觉区分；
- 点击选中（获得焦点）时：边框变蓝加粗、背景变浅蓝，清晰标识当前操作项。

------

## 实例 3：列表项「选中 + 拥有焦点」状态区分

### 应用场景

缺陷记录、工位列表等多列表界面，区分 **「选中但无焦点」**和**「选中且有焦点」** 两种状态，避免多个列表切换时，用户分不清当前操作焦点在哪。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style x:Key="DefectListItemStyle" TargetType="ListBoxItem">
        <Setter Property="Padding" Value="10 6"/>
        <Setter Property="Foreground" Value="#333"/>
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="BorderThickness" Value="0 0 1 0"/>
        <Setter Property="BorderBrush" Value="#EEE"/>

        <Style.Triggers>
            <!-- 单条件：鼠标悬停 -->
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#F5F6FA"/>
            </Trigger>

            <!-- 单条件：选中但无焦点（浅灰色） -->
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="#E8E8E8"/>
                <Setter Property="Foreground" Value="#333"/>
            </Trigger>

            <!-- 多条件：选中 + 拥有键盘焦点（深蓝色，优先级更高） -->
            <MultiTrigger>
                <MultiTrigger.Conditions>
                    <Condition Property="IsSelected" Value="True"/>
                    <Condition Property="IsKeyboardFocusWithin" Value="True"/>
                </MultiTrigger.Conditions>
                <MultiTrigger.Setters>
                    <Setter Property="Background" Value="#2E7DFF"/>
                    <Setter Property="Foreground" Value="White"/>
                </Setter>
            </MultiTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<Grid Margin="40">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="20"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>

    <!-- 左侧缺陷列表 -->
    <ListBox Grid.Column="0" 
             ItemContainerStyle="{StaticResource DefectListItemStyle}">
        <ListBoxItem>缺陷记录_001 焊点偏移</ListBoxItem>
        <ListBoxItem>缺陷记录_002 焊盘漏焊</ListBoxItem>
        <ListBoxItem>缺陷记录_003 锡膏不足</ListBoxItem>
    </ListBox>

    <!-- 右侧工位列表 -->
    <ListBox Grid.Column="2"
             ItemContainerStyle="{StaticResource DefectListItemStyle}">
        <ListBoxItem>上料工位</ListBoxItem>
        <ListBoxItem>焊接工位</ListBoxItem>
        <ListBoxItem>检测工位</ListBoxItem>
    </ListBox>
</Grid>
```

### 效果说明

- 选中但焦点不在当前列表：浅灰色背景，标识已选中但非当前操作区；
- 选中且焦点在当前列表：深蓝色背景 + 白色文字，清晰标识当前操作焦点。

### 核心要点

通过多条件拆分状态粒度，让高密度数据界面的交互层级更清晰，工业场景下可有效降低操作失误率。

------

## 实例 4：输入框「聚焦 + 校验错误」强化提示

### 应用场景

参数输入校验场景，**用户正在编辑（获得焦点）且输入非法时**，加粗红色边框并显示错误提示，强化提醒；非聚焦状态下仅显示细红框，界面更柔和，避免满屏红色造成视觉干扰。

### 完整代码

xaml:

```xaml
<Window.Resources>
    <Style TargetType="TextBox">
        <Setter Property="Width" Value="280"/>
        <Setter Property="Height" Value="32"/>
        <Setter Property="Padding" Value="6 4"/>
        <Setter Property="BorderBrush" Value="#CCC"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>

        <Style.Triggers>
            <!-- 单条件：获得焦点 -->
            <Trigger Property="IsFocused" Value="True">
                <Setter Property="BorderBrush" Value="#2E7DFF"/>
            </Trigger>

            <!-- 单条件：校验错误（非聚焦时，细红框） -->
            <Trigger Property="Validation.HasError" Value="True">
                <Setter Property="BorderBrush" Value="Red"/>
            </Trigger>

            <!-- 多条件：获得焦点 + 校验错误（加粗红框 + 提示） -->
            <MultiTrigger>
                <MultiTrigger.Conditions>
                    <Condition Property="IsFocused" Value="True"/>
                    <Condition Property="Validation.HasError" Value="True"/>
                </MultiTrigger.Conditions>
                <MultiTrigger.Setters>
                    <Setter Property="BorderBrush" Value="Red"/>
                    <Setter Property="BorderThickness" Value="2"/>
                    <Setter Property="ToolTip"
                            Value="{Binding RelativeSource={RelativeSource Self},
                                    Path=(Validation.Errors)[0].ErrorContent}"/>
                </Setter>
            </MultiTrigger>
        </Style.Triggers>
    </Style>
</Window.Resources>

<StackPanel Margin="40" Spacing="15">
    <TextBlock Text="曝光时间（100~10000μs）："/>
    <!-- 配合数值范围校验规则使用 -->
    <TextBox>
        <TextBox.Text>
            <Binding Path="ExposureTime" Mode="TwoWay" UpdateSourceTrigger="PropertyChanged">
                <Binding.ValidationRules>
                    <local:DoubleRangeValidationRule Min="100" Max="10000"/>
                </Binding.ValidationRules>
            </Binding>
        </TextBox.Text>
    </TextBox>
</StackPanel>
```

### 效果说明

- 输入错误且未聚焦：细红色边框，低调提示；
- 输入错误且正在编辑（聚焦）：加粗红色边框，鼠标悬停显示错误详情，强提醒用户修正。

------

## 使用总结与选型

### 核心特性回顾

1. **仅支持与逻辑**：所有条件必须同时满足，无法直接实现「或逻辑」；需要或逻辑时写多个独立触发器即可。
2. **仅监听控件自身属性**：所有条件都必须是目标控件的依赖属性（含附加属性）；如果需要绑定业务数据，请使用 `MultiDataTrigger`。
3. **自动恢复**：任意条件不满足时，自动撤销属性设置，无需编写反向触发器。

### 快速选型

| 场景                           | 推荐触发器         |
| :----------------------------- | :----------------- |
| 单个控件属性控制状态           | `Trigger`          |
| 多个控件 UI 属性同时满足才触发 | `MultiTrigger`     |
| 单个业务数据控制状态           | `DataTrigger`      |
| 多个业务数据同时满足才触发     | `MultiDataTrigger` |