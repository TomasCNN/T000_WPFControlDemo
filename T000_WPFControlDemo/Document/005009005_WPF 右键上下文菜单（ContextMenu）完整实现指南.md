# 005009005_WPF 右键上下文菜单（ContextMenu）完整实现指南

在 WPF 中，右键上下文菜单通过 `ContextMenu` 控件实现，它与 `Menu` 同继承自 `MenuBase` 抽象基类，菜单项完全复用 `MenuItem`，核心逻辑与主菜单一致，唯一区别是**触发方式与承载形式**：它通过 `Popup` 悬浮弹出，右键点击目标控件时在鼠标位置显示，是工业软件中设备快捷操作、数据批量处理的核心交互方式。

> 核心注意点：`ContextMenu` 位于独立的 `Popup` 窗口中，**不在主界面的视觉树内**，因此无法直接继承父元素的 `DataContext`，这是 MVVM 绑定时最容易踩坑的根源。

------

## 一、基础实现：静态右键菜单

最简单的实现方式：直接在目标控件的 `ContextMenu` 属性中定义静态菜单项，适合菜单固定、逻辑简单的场景。

### 1.1 给单个控件添加右键菜单

通过 `FrameworkElement.ContextMenu` 属性关联，所有继承自 `FrameworkElement` 的控件（Grid、Border、Image、TextBox 等）都支持。

xaml:

```xaml
<Window x:Class="ContextMenuDemo.MainWindow"
        Title="右键菜单示例" Height="350" Width="500">
    <Grid Background="Transparent">
        <!-- 给整个Grid区域添加右键菜单 -->
        <Grid.ContextMenu>
            <ContextMenu>
                <MenuItem Header="刷新数据" InputGestureText="F5"/>
                <MenuItem Header="导出报表"/>
                <Separator/>
                <MenuItem Header="全屏显示" IsCheckable="True"/>
            </ContextMenu>
        </Grid.ContextMenu>

        <TextBlock Text="右键点击空白区域查看菜单" 
                   HorizontalAlignment="Center" VerticalAlignment="Center"
                   Foreground="#999"/>
    </Grid>
</Window>
```

#### 要点说明

- 容器控件必须有背景才能接收鼠标右键事件，`Background="Transparent"` 是常用写法；如果背景为 `null`，右键不会触发菜单。
- 菜单项用法与主菜单完全一致，支持 `Icon`、`IsCheckable`、`Separator`、`InputGestureText` 等所有 `MenuItem` 特性。

### 1.2 给列表项添加右键菜单（工业最常用场景）

给 `ListBox`/`DataGrid` 的每一行单独绑定右键菜单，实现「选中设备→右键操作」的标准工控交互。

xaml:

```xaml
<ListBox ItemsSource="{Binding DeviceList}"
         DisplayMemberPath="DeviceName"
         BorderBrush="#DDD" BorderThickness="1"
         Width="260" Margin="10">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Padding" Value="8 4"/>
            <!-- 为每个列表项单独设置右键菜单 -->
            <Setter Property="ContextMenu">
                <Setter.Value>
                    <ContextMenu>
                        <MenuItem Header="启动设备"/>
                        <MenuItem Header="停止设备"/>
                        <Separator/>
                        <MenuItem Header="查看详情"/>
                        <MenuItem Header="导出运行记录"/>
                    </ContextMenu>
                </Setter.Value>
            </Setter>
        </Style>
    </ListBox.ItemContainerStyle>
</ListBox>
```

------

## 二、进阶实现：MVVM 命令绑定（核心重点）

工业项目推荐使用 MVVM 架构，将菜单操作绑定到 `ICommand` 命令。**这是右键菜单的最大难点**，核心要解决「数据上下文传递」的问题。

### 2.1 问题根源

`ContextMenu` 寄宿在独立的 `Popup` 窗口中，不属于主界面的视觉树，因此**无法直接继承父元素的 `DataContext`**。直接写 `{Binding StartCommand}` 会静默失效，因为找不到对应数据源。

### 2.2 标准解决方案：通过 PlacementTarget 中转

`ContextMenu` 有一个专属属性 `PlacementTarget`，指向触发右键菜单的目标元素（也就是你右键点击的那个控件）。我们可以通过它间接获取主视觉树中的数据上下文。

#### 完整绑定写法

xaml:

```xaml
<ListBox ItemsSource="{Binding DeviceList}"
         BorderBrush="#DDD" BorderThickness="1"
         Width="260" Margin="10">
    <ListBox.ItemContainerStyle>
        <Style TargetType="ListBoxItem">
            <Setter Property="Padding" Value="8 4"/>
            <Setter Property="ContextMenu">
                <Setter.Value>
                    <ContextMenu>
                        <!-- 
                            绑定逻辑：
                            1. RelativeSource 找到当前 ContextMenu 自身
                            2. 通过 PlacementTarget 获取右键点击的 ListBoxItem
                            3. 再通过 DataContext 拿到列表项对应的设备数据
                        -->
                        <MenuItem Header="启动设备"
                                  Command="{Binding PlacementTarget.DataContext.StartDeviceCommand, 
                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                  CommandParameter="{Binding}"/>
                        
                        <MenuItem Header="停止设备"
                                  Command="{Binding PlacementTarget.DataContext.StopDeviceCommand, 
                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                  CommandParameter="{Binding}"/>
                        
                        <Separator/>
                        
                        <MenuItem Header="查看详情"
                                  Command="{Binding PlacementTarget.DataContext.ViewDetailCommand, 
                                            RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                  CommandParameter="{Binding}"/>
                    </ContextMenu>
                </Setter.Value>
            </Setter>
        </Style>
    </ListBox.ItemContainerStyle>
</ListBox>
```

### 2.3 绑定语法拆解

| 绑定部分                                                   | 作用                                                         |
| :--------------------------------------------------------- | :----------------------------------------------------------- |
| `RelativeSource={RelativeSource AncestorType=ContextMenu}` | 向上查找，找到当前菜单项所属的 `ContextMenu` 控件            |
| `PlacementTarget.DataContext`                              | 获取右键点击的目标元素（ListBoxItem）的数据上下文，也就是单条设备数据 |
| `CommandParameter="{Binding}"`                             | 将当前设备对象作为参数传递给命令，命令中可直接拿到完整的设备信息 |

> 如果命令定义在**窗口级 ViewModel** 中（不是单条数据里），可以将 `PlacementTarget` 指向 ListBox 本身，再取 ListBox 的 DataContext：
>
> xaml:
>
> ```xaml
> Command="{Binding PlacementTarget.DataContext.StartDeviceCommand, 
>           RelativeSource={RelativeSource AncestorType=ContextMenu}}"
> ```
>
> 此时需要保证 ListBox 的 DataContext 是窗口级 ViewModel。

------

## 三、动态右键菜单：根据状态显隐菜单项

工业场景中，菜单项通常需要根据设备状态动态变化：比如设备运行时显示「停止」，停机时显示「启动」。有两种常用实现方式。

### 3.1 方式一：绑定 Visibility（简单场景）

通过数据触发器绑定菜单项的 `Visibility`，根据数据状态显示或隐藏。

xaml:

```xaml
<ContextMenu>
    <MenuItem Header="启动设备"
              Command="{Binding PlacementTarget.DataContext.StartDeviceCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}}"
              CommandParameter="{Binding}">
        <MenuItem.Style>
            <Style TargetType="MenuItem">
                <Setter Property="Visibility" Value="Visible"/>
                <Style.Triggers>
                    <!-- 设备运行中时，隐藏启动按钮 -->
                    <DataTrigger Binding="{Binding IsRunning}" Value="True">
                        <Setter Property="Visibility" Value="Collapsed"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </MenuItem.Style>
    </MenuItem>

    <MenuItem Header="停止设备"
              Command="{Binding PlacementTarget.DataContext.StopDeviceCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}}"
              CommandParameter="{Binding}">
        <MenuItem.Style>
            <Style TargetType="MenuItem">
                <Setter Property="Visibility" Value="Collapsed"/>
                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsRunning}" Value="True">
                        <Setter Property="Visibility" Value="Visible"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </MenuItem.Style>
    </MenuItem>
</ContextMenu>
```

### 3.2 方式二：数据驱动动态生成（复杂场景）

菜单项数量、类型完全由数据决定时，通过 `ItemsSource` 绑定菜单集合，配合 `HierarchicalDataTemplate` 生成多级菜单，适合权限动态菜单、配置化菜单。

xaml:

```xaml
<ContextMenu ItemsSource="{Binding PlacementTarget.DataContext.ContextMenuItems, 
                          RelativeSource={RelativeSource AncestorType=ContextMenu}}">
    <ContextMenu.ItemTemplate>
        <HierarchicalDataTemplate DataType="{x:Type local:MenuCommandItem}"
                                  ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding MenuText}"/>
        </HierarchicalDataTemplate>
    </ContextMenu.ItemTemplate>
</ContextMenu>
```

------

## 四、常用配置与进阶属性

### 4.1 弹出位置控制

通过 `Placement` 属性设置弹出对齐方式，常用值：

- `MousePoint`（默认）：鼠标点击位置弹出
- `Bottom`：目标元素底部弹出
- `Right`：目标元素右侧弹出
- `Center`：目标元素中心弹出

xaml:

```xaml
<ContextMenu Placement="Bottom" PlacementTarget="{Binding ElementName=TargetBtn}">
```

### 4.2 手动控制打开 / 关闭

通过 `IsOpen` 属性可在代码中手动控制右键菜单的显示与隐藏：

csharp:

```c#
// 手动打开右键菜单
MyContextMenu.PlacementTarget = TargetElement;
MyContextMenu.IsOpen = true;
```

### 4.3 点击后保持打开

默认点击菜单项后菜单会自动关闭，对于复选类操作，可设置 `StaysOpenOnClick="True"` 保持菜单展开：

xaml:

```xaml
<MenuItem Header="显示网格线" IsCheckable="True" StaysOpenOnClick="True"/>
```

------

## 五、常见问题与避坑指南

### 1. 右键点击没反应，菜单不弹出

- **常见原因**：目标控件背景为 `null`（默认值），不接收鼠标事件；
- **解决方案**：给控件设置 `Background="Transparent"`，即可接收鼠标点击。

### 2. 命令绑定无反应，点击没效果

- **常见原因**：ContextMenu 不在主视觉树，DataContext 为空，绑定路径错误；
- **解决方案**：使用 `PlacementTarget.DataContext` 中转绑定，参考上文 MVVM 绑定写法。

### 3. 菜单项一直是灰的，不可点击

- **常见原因 1**：命令的 `CanExecute` 方法返回了 `false`，菜单项会自动禁用；
- **排查方法**：检查命令的权限校验、状态校验逻辑是否正确。
- **常见原因 2**：绑定路径错误，Command 实际为 null；
- **排查方法**：用 VS 输出窗口查看绑定错误，确认 DataContext 和属性名是否正确。

### 4. 菜单弹出位置偏移、超出屏幕

- WPF 内置了屏幕边界检测，空间不足时会自动反向弹出，一般不需要手动处理；
- 如果需要自定义偏移，可通过 `HorizontalOffset` / `VerticalOffset` 属性调整。

------

## 六、工业场景完整实例：设备列表右键操作菜单

整合以上知识点，实现一个工业常用的设备列表右键菜单：

- 包含启动、停止、查看详情、导出记录四个操作
- 根据设备运行状态动态显隐启动 / 停止按钮
- MVVM 命令绑定，传递设备对象参数
- 带状态色图标

### XAML 代码

xaml:

```xaml
<Window x:Class="ContextMenuDemo.DeviceListWindow"
        Title="设备监控列表" Height="450" Width="600"
        xmlns:local="clr-namespace:ContextMenuDemo">
    <Window.DataContext>
        <local:DeviceMonitorViewModel/>
    </Window.DataContext>

    <Grid Margin="10">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="260"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- 左侧设备列表 -->
        <ListBox ItemsSource="{Binding DeviceList}"
                 SelectedItem="{Binding SelectedDevice}"
                 BorderBrush="#DDD" BorderThickness="1">
            <ListBox.ItemContainerStyle>
                <Style TargetType="ListBoxItem">
                    <Setter Property="Padding" Value="8 4"/>
                    <Setter Property="ContextMenu">
                        <Setter.Value>
                            <ContextMenu>
                                <!-- 启动设备：停机时显示 -->
                                <MenuItem Header="启动设备"
                                          Command="{Binding PlacementTarget.DataContext.StartDeviceCommand, 
                                                    RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                          CommandParameter="{Binding}">
                                    <MenuItem.Icon>
                                        <Ellipse Width="12" Height="12" Fill="#52C41A" VerticalAlignment="Center"/>
                                    </MenuItem.Icon>
                                    <MenuItem.Style>
                                        <Style TargetType="MenuItem">
                                            <Setter Property="Visibility" Value="Visible"/>
                                            <Style.Triggers>
                                                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                                    <Setter Property="Visibility" Value="Collapsed"/>
                                                </DataTrigger>
                                            </Style.Triggers>
                                        </Style>
                                    </MenuItem.Style>
                                </MenuItem>

                                <!-- 停止设备：运行时显示 -->
                                <MenuItem Header="停止设备"
                                          Command="{Binding PlacementTarget.DataContext.StopDeviceCommand, 
                                                    RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                          CommandParameter="{Binding}">
                                    <MenuItem.Icon>
                                        <Ellipse Width="12" Height="12" Fill="#FAAD14" VerticalAlignment="Center"/>
                                    </MenuItem.Icon>
                                    <MenuItem.Style>
                                        <Style TargetType="MenuItem">
                                            <Setter Property="Visibility" Value="Collapsed"/>
                                            <Style.Triggers>
                                                <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                                    <Setter Property="Visibility" Value="Visible"/>
                                                </DataTrigger>
                                            </Style.Triggers>
                                        </Style>
                                    </MenuItem.Style>
                                </MenuItem>

                                <Separator/>

                                <MenuItem Header="查看详情"
                                          Command="{Binding PlacementTarget.DataContext.ViewDetailCommand, 
                                                    RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                          CommandParameter="{Binding}"/>
                                <MenuItem Header="导出运行记录"
                                          Command="{Binding PlacementTarget.DataContext.ExportRecordCommand, 
                                                    RelativeSource={RelativeSource AncestorType=ContextMenu}}"
                                          CommandParameter="{Binding}"/>
                            </ContextMenu>
                        </Setter.Value>
                    </Setter>
                </Style>
            </ListBox.ItemContainerStyle>

            <ListBox.ItemTemplate>
                <DataTemplate>
                    <DockPanel>
                        <Ellipse DockPanel.Dock="Left" Width="8" Height="8" 
                                 VerticalAlignment="Center" Margin="0 0 8 0">
                            <Ellipse.Style>
                                <Style TargetType="Ellipse">
                                    <Setter Property="Fill" Value="#BFBFBF"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding IsRunning}" Value="True">
                                            <Setter Property="Fill" Value="#52C41A"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Ellipse.Style>
                        </Ellipse>
                        <TextBlock Text="{Binding DeviceName}"/>
                    </DockPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>

        <!-- 右侧设备详情 -->
        <Border Grid.Column="1" Margin="10 0 0 0"
                Background="#F8F9FA" Padding="20"
                BorderBrush="#DDD" BorderThickness="1">
            <StackPanel DataContext="{Binding SelectedDevice}">
                <TextBlock FontSize="18" FontWeight="Bold" Text="{Binding DeviceName}"/>
                <TextBlock Margin="0 8" Text="{Binding DeviceCode, StringFormat=设备编码：{0}}"/>
                <TextBlock Text="{Binding IsRunning, StringFormat=运行状态：{0}}"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

### ViewModel 核心代码

csharp:

```c#
public class DeviceMonitorViewModel : INotifyPropertyChanged
{
    public ObservableCollection<DeviceInfo> DeviceList { get; set; }
    public DeviceInfo SelectedDevice { get; set; }
    
    public ICommand StartDeviceCommand { get; }
    public ICommand StopDeviceCommand { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand ExportRecordCommand { get; }

    public DeviceMonitorViewModel()
    {
        // 初始化数据
        DeviceList = new ObservableCollection<DeviceInfo>
        {
            new DeviceInfo { DeviceName = "喷涂机器人A01", DeviceCode = "D001", IsRunning = true },
            new DeviceInfo { DeviceName = "固化炉B01", DeviceCode = "D002", IsRunning = false },
            new DeviceInfo { DeviceName = "上料机C01", DeviceCode = "D003", IsRunning = true }
        };

        // 初始化命令
        StartDeviceCommand = new RelayCommand(ExecuteStartDevice);
        StopDeviceCommand = new RelayCommand(ExecuteStopDevice);
        ViewDetailCommand = new RelayCommand(ExecuteViewDetail);
        ExportRecordCommand = new RelayCommand(ExecuteExportRecord);
    }

    private void ExecuteStartDevice(object parameter)
    {
        if (parameter is DeviceInfo device)
        {
            device.IsRunning = true;
        }
    }

    private void ExecuteStopDevice(object parameter)
    {
        if (parameter is DeviceInfo device)
        {
            device.IsRunning = false;
        }
    }

    // 其他方法略...
}
```

------

## 最佳实践总结

1. **简单场景用静态菜单**：固定功能、少量菜单项直接写在 XAML 中，简洁直观；
2. **MVVM 架构必用 PlacementTarget 中转**：记住「ContextMenu 不在视觉树」这个核心点，绑定命令一律通过 `PlacementTarget.DataContext` 取值；
3. **工业场景优先右键快捷操作**：设备启停、详情查看等高频功能放到右键菜单，减少操作路径，提升工控效率；
4. **状态驱动菜单项显隐**：不要在代码里手动增删菜单项，通过数据绑定 + 触发器动态控制，保持 MVVM 架构整洁；
5. **菜单项配图标 + 快捷键提示**：工业界面用颜色图标区分操作类型，比纯文字辨识度更高，降低操作失误率。