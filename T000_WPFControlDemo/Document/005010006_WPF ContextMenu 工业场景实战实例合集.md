# 005010006_WPF ContextMenu 工业场景实战实例合集

以下实例均来自 MES、SCADA、工控上位机等真实工业软件场景，覆盖**数据交互、设备控制、权限管控、触屏适配、组态画面**五大工业核心需求，全部遵循 MVVM 架构，可直接落地复用。

------

## 一、生产报警数据行右键菜单（MES 高频场景）

### 场景背景

报警列表是工业系统的核心模块，操作员右键单条报警，执行「确认报警、查看详情、标记处理、导出记录」等操作，是最常用的交互方式。

### 核心痛点

- ContextMenu 独立视觉树，无法直接绑定 ViewModel 命令
- 需要将当前行的报警实体作为命令参数传递
- 已确认的报警需禁用「确认」菜单项

### 完整实现

#### 1. 报警实体与 ViewModel

csharp:

```c#
// 报警数据模型
public class AlarmModel : INotifyPropertyChanged
{
    public string AlarmId { get; set; }
    public string EquipmentName { get; set; }
    public string AlarmContent { get; set; }
    public DateTime AlarmTime { get; set; }
    public bool IsConfirmed { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}

// 主ViewModel
public class AlarmViewModel : INotifyPropertyChanged
{
    public ObservableCollection<AlarmModel> AlarmList { get; set; } = new();
    
    // 右键命令
    public ICommand ConfirmAlarmCommand { get; }
    public ICommand ShowAlarmDetailCommand { get; }
    public ICommand ExportAlarmCommand { get; }

    public AlarmViewModel()
    {
        ConfirmAlarmCommand = new RelayCommand<AlarmModel>(OnConfirmAlarm);
        ShowAlarmDetailCommand = new RelayCommand<AlarmModel>(OnShowDetail);
        ExportAlarmCommand = new RelayCommand<AlarmModel>(OnExport);
        
        // 模拟加载报警数据
        LoadAlarmData();
    }

    private void OnConfirmAlarm(AlarmModel alarm)
    {
        if (alarm == null || alarm.IsConfirmed) return;
        alarm.IsConfirmed = true;
        // 实际项目中调用下位机/接口执行确认逻辑
    }

    private void OnShowDetail(AlarmModel alarm) 
        => MessageBox.Show($"报警详情：{alarm.AlarmContent}\n设备：{alarm.EquipmentName}");
    
    private void OnExport(AlarmModel alarm) 
        => MessageBox.Show($"已导出报警 {alarm.AlarmId} 记录");

    private void LoadAlarmData()
    {
        AlarmList.Add(new AlarmModel
        {
            AlarmId = "ALM-2026001",
            EquipmentName = "CNC-01",
            AlarmContent = "主轴温度过高",
            AlarmTime = DateTime.Now.AddMinutes(-10),
            IsConfirmed = false
        });
        AlarmList.Add(new AlarmModel
        {
            AlarmId = "ALM-2026002",
            EquipmentName = "Robot-03",
            AlarmContent = "伺服驱动器报警",
            AlarmTime = DateTime.Now.AddMinutes(-30),
            IsConfirmed = true
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

#### 2. XAML 界面实现

xaml:

```xaml
<DataGrid x:Name="AlarmGrid" 
          ItemsSource="{Binding AlarmList}"
          AutoGenerateColumns="False"
          IsReadOnly="True"
          SelectionMode="Single">
    <DataGrid.RowStyle>
        <Style TargetType="DataGridRow">
            <!-- 将主ViewModel存入Tag，用于菜单命令绑定 -->
            <Setter Property="Tag" Value="{Binding DataContext, ElementName=AlarmGrid}"/>
            <Setter Property="ContextMenu">
                <Setter.Value>
                    <ContextMenu>
                        <!-- 核心：通过PlacementTarget桥接主视觉树的DataContext -->
                        <MenuItem Header="确认报警"
                                  Command="{Binding PlacementTarget.Tag.ConfirmAlarmCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}">
                            <MenuItem.Style>
                                <Style TargetType="MenuItem">
                                    <Style.Triggers>
                                        <!-- 已确认的报警禁用该菜单项 -->
                                        <DataTrigger Binding="{Binding PlacementTarget.DataContext.IsConfirmed, RelativeSource={RelativeSource Self}}" Value="True">
                                            <Setter Property="IsEnabled" Value="False"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </MenuItem.Style>
                        </MenuItem>
                        <MenuItem Header="查看详情"
                                  Command="{Binding PlacementTarget.Tag.ShowAlarmDetailCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"/>
                        <Separator/>
                        <MenuItem Header="导出记录"
                                  Command="{Binding PlacementTarget.Tag.ExportAlarmCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"/>
                    </ContextMenu>
                </Setter.Value>
            </Setter>
        </Style>
    </DataGrid.RowStyle>
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="报警编号" Binding="{Binding AlarmId}"/>
        <DataGridTextColumn Header="设备名称" Binding="{Binding EquipmentName}"/>
        <DataGridTextColumn Header="报警内容" Binding="{Binding AlarmContent}"/>
        <DataGridTextColumn Header="报警时间" Binding="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}"/>
        <DataGridCheckBoxColumn Header="已确认" Binding="{Binding IsConfirmed}"/>
    </DataGrid.Columns>
</DataGrid>
```

### 工业适配关键点

1. **权限与状态联动**：通过 DataTrigger 根据业务状态动态禁用菜单项，符合工业操作安全规范
2. **命令参数透传**：`PlacementTarget.DataContext` 直接获取当前行数据，无需额外查找
3. **操作闭环**：命令执行后实时更新界面状态，数据双向绑定保证状态同步

------

## 二、设备监控图标右键控制（SCADA 组态场景）

### 场景背景

组态画面中，每个设备图标对应一台现场设备，右键弹出控制菜单，执行「启动、停止、复位、查看参数、手动模式」等操作，菜单项随设备实时状态动态可用。

### 核心痛点

- 菜单项可用性与设备运行状态强绑定（运行中不能重复启动，停止中不能停止）
- 设备状态实时刷新，菜单需同步更新
- 高危操作需二次确认

### 完整实现

#### 1. 设备模型与 ViewModel

csharp:

```c#
public enum EquipmentStatus
{
    Running, Stop, Fault, Manual
}

public class EquipmentModel : INotifyPropertyChanged
{
    public string EqCode { get; set; }
    public string EqName { get; set; }
    private EquipmentStatus _status;
    public EquipmentStatus Status
    {
        get => _status;
        set { _status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public class EquipmentViewModel : INotifyPropertyChanged
{
    public ObservableCollection<EquipmentModel> EquipmentList { get; set; } = new();
    public ICommand StartEqCommand { get; }
    public ICommand StopEqCommand { get; }
    public ICommand ResetEqCommand { get; }

    public EquipmentViewModel()
    {
        StartEqCommand = new RelayCommand<EquipmentModel>(OnStart, CanStart);
        StopEqCommand = new RelayCommand<EquipmentModel>(OnStop, CanStop);
        ResetEqCommand = new RelayCommand<EquipmentModel>(OnReset, CanReset);
        
        // 模拟设备数据
        EquipmentList.Add(new EquipmentModel { EqCode = "CNC-01", EqName = "数控车床1号", Status = EquipmentStatus.Running });
        EquipmentList.Add(new EquipmentModel { EqCode = "CNC-02", EqName = "数控车床2号", Status = EquipmentStatus.Stop });
        EquipmentList.Add(new EquipmentModel { EqCode = "ROB-01", EqName = "焊接机器人", Status = EquipmentStatus.Fault });
    }

    private bool CanStart(EquipmentModel eq) => eq.Status == EquipmentStatus.Stop;
    private bool CanStop(EquipmentModel eq) => eq.Status == EquipmentStatus.Running || eq.Status == EquipmentStatus.Manual;
    private bool CanReset(EquipmentModel eq) => eq.Status == EquipmentStatus.Fault;

    private void OnStart(EquipmentModel eq)
    {
        if (MessageBox.Show($"确认启动设备 {eq.EqName}？", "操作确认", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        eq.Status = EquipmentStatus.Running;
        // 实际项目下发PLC启动指令
    }

    private void OnStop(EquipmentModel eq)
    {
        eq.Status = EquipmentStatus.Stop;
        // 实际项目下发PLC停止指令
    }

    private void OnReset(EquipmentModel eq)
    {
        eq.Status = EquipmentStatus.Stop;
        // 实际项目下发故障复位指令
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

#### 2. 组态画面 XAML

xaml:

```xaml
<ItemsControl ItemsSource="{Binding EquipmentList}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Width="120" Height="80" 
                    BorderThickness="1" CornerRadius="4"
                    Margin="10" Tag="{Binding DataContext, RelativeSource={RelativeSource AncestorType=Window}}">
                <Border.Style>
                    <Style TargetType="Border">
                        <Setter Property="Background" Value="#2d2d30"/>
                        <Setter Property="BorderBrush" Value="#3e3e42"/>
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding Status}" Value="Running">
                                <Setter Property="BorderBrush" Value="#00ff00"/>
                            </DataTrigger>
                            <DataTrigger Binding="{Binding Status}" Value="Fault">
                                <Setter Property="BorderBrush" Value="#ff0000"/>
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </Border.Style>
                
                <Border.ContextMenu>
                    <ContextMenu>
                        <MenuItem Header="启动设备"
                                  Command="{Binding PlacementTarget.Tag.StartEqCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"/>
                        <MenuItem Header="停止设备"
                                  Command="{Binding PlacementTarget.Tag.StopEqCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"/>
                        <MenuItem Header="故障复位"
                                  Command="{Binding PlacementTarget.Tag.ResetEqCommand, RelativeSource={RelativeSource Self}}"
                                  CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"/>
                        <Separator/>
                        <MenuItem Header="参数详情"/>
                    </ContextMenu>
                </Border.ContextMenu>

                <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                    <TextBlock Text="{Binding EqName}" Foreground="White" FontSize="12" HorizontalAlignment="Center"/>
                    <TextBlock Text="{Binding Status}" Foreground="Gray" FontSize="10" HorizontalAlignment="Center" Margin="0,5,0,0"/>
                </StackPanel>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### 工业适配关键点

1. **状态驱动可用性**：命令的 `CanExecute` 方法联动设备状态，WPF 自动更新菜单项启用状态，无需手动控制
2. **高危操作二次确认**：启动、复位等高危操作增加弹窗确认，防止误触
3. **视觉状态同步**：设备状态变化时，边框颜色与菜单可用性同步更新，符合组态画面交互习惯

------

## 三、权限动态生成右键菜单（工业系统安全管控）

### 场景背景

工业系统严格遵循角色权限管控，不同操作员（操作工、班长、管理员）看到的右键菜单项不同，需根据登录角色动态过滤菜单。

### 核心痛点

- 菜单项不固定，需运行时根据权限动态生成
- 支持多级菜单、分隔线、禁用状态
- 遵循 MVVM，不允许在后台代码硬拼菜单

### 完整实现

#### 1. 菜单模型与权限逻辑

csharp:

```c#
// 菜单项模型
public class MenuItemNode : INotifyPropertyChanged
{
    public string MenuCode { get; set; }
    public string MenuHeader { get; set; }
    public ICommand Command { get; set; }
    public object CommandParameter { get; set; }
    public bool IsSeparator { get; set; }
    public ObservableCollection<MenuItemNode> Children { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;
}

// 权限服务
public static class PermissionService
{
    // 模拟当前登录角色
    public static string CurrentRole { get; set; } = "Operator"; // Operator/Leader/Admin

    public static bool HasPermission(string menuCode)
    {
        return menuCode switch
        {
            "View" => true, // 所有角色都可查看
            "Edit" => CurrentRole is "Leader" or "Admin",
            "Delete" => CurrentRole == "Admin",
            "Export" => CurrentRole is "Leader" or "Admin",
            _ => false
        };
    }
}
```

#### 2. ViewModel 动态构建菜单

csharp:

```c#
public class PermissionMenuViewModel : INotifyPropertyChanged
{
    private ObservableCollection<MenuItemNode> _workOrderMenus;
    public ObservableCollection<MenuItemNode> WorkOrderMenus
    {
        get => _workOrderMenus;
        set { _workOrderMenus = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorkOrderMenus))); }
    }

    public ICommand ViewCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ExportCommand { get; }

    public PermissionMenuViewModel()
    {
        ViewCommand = new RelayCommand(() => MessageBox.Show("查看工单"));
        EditCommand = new RelayCommand(() => MessageBox.Show("编辑工单"));
        DeleteCommand = new RelayCommand(() => MessageBox.Show("删除工单"));
        ExportCommand = new RelayCommand(() => MessageBox.Show("导出工单"));
        
        BuildMenuByPermission();
    }

    // 根据权限构建菜单树
    private void BuildMenuByPermission()
    {
        WorkOrderMenus = new ObservableCollection<MenuItemNode>();
        
        if (PermissionService.HasPermission("View"))
            WorkOrderMenus.Add(new MenuItemNode { MenuCode = "View", MenuHeader = "查看详情", Command = ViewCommand });
        
        if (PermissionService.HasPermission("Edit"))
            WorkOrderMenus.Add(new MenuItemNode { MenuCode = "Edit", MenuHeader = "编辑工单", Command = EditCommand });
        
        if (PermissionService.HasPermission("Export") || PermissionService.HasPermission("Delete"))
            WorkOrderMenus.Add(new MenuItemNode { IsSeparator = true });
        
        if (PermissionService.HasPermission("Export"))
            WorkOrderMenus.Add(new MenuItemNode { MenuCode = "Export", MenuHeader = "导出数据", Command = ExportCommand });
        
        if (PermissionService.HasPermission("Delete"))
            WorkOrderMenus.Add(new MenuItemNode { MenuCode = "Delete", MenuHeader = "删除工单", Command = DeleteCommand });
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

#### 3. XAML 动态绑定菜单

xaml:

```xaml
<ContentControl x:Name="MenuHost" Content="工单右键测试" Background="LightGray" Width="200" Height="100" HorizontalAlignment="Center">
    <ContentControl.ContextMenu>
        <ContextMenu ItemsSource="{Binding PlacementTarget.DataContext.WorkOrderMenus, RelativeSource={RelativeSource Self}}">
            <ContextMenu.ItemContainerStyle>
                <Style TargetType="MenuItem">
                    <Setter Property="Header" Value="{Binding MenuHeader}"/>
                    <Setter Property="Command" Value="{Binding Command}"/>
                    <Setter Property="ItemsSource" Value="{Binding Children}"/>
                </Style>
            </ContextMenu.ItemContainerStyle>
            <ContextMenu.ItemTemplateSelector>
                <local:MenuItemTemplateSelector/>
            </ContextMenu.ItemTemplateSelector>
        </ContextMenu>
    </ContentControl.ContextMenu>
</ContentControl>
```

#### 4. 菜单模板选择器（处理分隔线）

csharp:

```c#
public class MenuItemTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is MenuItemNode node && node.IsSeparator)
        {
            return new DataTemplate(typeof(Separator));
        }
        return base.SelectTemplate(item, container);
    }
}
```

### 工业适配关键点

1. **权限最小化原则**：菜单按需生成，无权限的操作完全不可见，符合工业信息安全规范
2. **数据驱动菜单**：菜单结构由数据模型控制，修改权限逻辑无需改动 UI 代码
3. **支持多级嵌套**：通过 `Children` 属性支持无限级子菜单，适配复杂业务操作

------

## 四、工业触屏长按弹出菜单（工位触控场景）

### 场景背景

车间工位的触控一体机没有鼠标右键，需通过**长按控件**弹出上下文菜单，是工业触屏设备的标准交互方式。

### 核心痛点

- 自定义长按时长（通常 800ms~1200ms）
- 手指抬起、滑动时取消触发，防止误触
- 长按过程提供视觉反馈

### 完整实现

#### 1. 长按附加属性（复用性封装）

csharp:

```c#
public static class TouchLongPress
{
    public static readonly DependencyProperty LongPressCommandProperty =
        DependencyProperty.RegisterAttached(
            "LongPressCommand",
            typeof(ICommand),
            typeof(TouchLongPress),
            new PropertyMetadata(null, OnLongPressCommandChanged));

    public static ICommand GetLongPressCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(LongPressCommandProperty);

    public static void SetLongPressCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(LongPressCommandProperty, value);

    private static DispatcherTimer _pressTimer;
    private static UIElement _currentElement;

    private static void OnLongPressCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;
        
        element.TouchDown += Element_TouchDown;
        element.TouchUp += Element_TouchUp;
        element.TouchLeave += Element_TouchLeave;
        element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
        element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
    }

    private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        StartTimer(sender as UIElement);
    }

    private static void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        StopTimer();
    }

    private static void Element_TouchDown(object sender, TouchEventArgs e)
    {
        StartTimer(sender as UIElement);
        e.Handled = true;
    }

    private static void Element_TouchUp(object sender, TouchEventArgs e)
    {
        StopTimer();
        e.Handled = true;
    }

    private static void Element_TouchLeave(object sender, TouchEventArgs e)
    {
        StopTimer(); // 手指滑出控件范围取消
        e.Handled = true;
    }

    private static void StartTimer(UIElement element)
    {
        StopTimer();
        _currentElement = element;
        _pressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        _pressTimer.Tick += PressTimer_Tick;
        _pressTimer.Start();
    }

    private static void StopTimer()
    {
        if (_pressTimer != null)
        {
            _pressTimer.Stop();
            _pressTimer = null;
        }
        _currentElement = null;
    }

    private static void PressTimer_Tick(object sender, EventArgs e)
    {
        StopTimer();
        if (_currentElement == null) return;
        
        var command = GetLongPressCommand(_currentElement);
        if (command != null && command.CanExecute(_currentElement))
        {
            command.Execute(_currentElement);
        }
    }
}
```

#### 2. ViewModel 弹出菜单逻辑

csharp:

```c#
public class TouchMenuViewModel : INotifyPropertyChanged
{
    public ICommand ShowStationMenuCommand { get; }

    public TouchMenuViewModel()
    {
        ShowStationMenuCommand = new RelayCommand<UIElement>(OnShowMenu);
    }

    private void OnShowMenu(UIElement element)
    {
        if (element == null) return;
        var menu = element.ContextMenu;
        if (menu == null) return;
        
        menu.PlacementTarget = element;
        menu.Placement = PlacementMode.Center;
        menu.IsOpen = true;
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

#### 3. XAML 触屏界面

xaml:

```xaml
<Border Width="150" Height="100" Background="#3a3a3d" CornerRadius="6"
        HorizontalAlignment="Center" VerticalAlignment="Center"
        local:TouchLongPress.LongPressCommand="{Binding ShowStationMenuCommand}">
    <Border.ContextMenu>
        <ContextMenu StaysOpen="False">
            <MenuItem Header="开始作业"/>
            <MenuItem Header="暂停作业"/>
            <MenuItem Header="完工上报"/>
            <Separator/>
            <MenuItem Header="工位详情"/>
        </ContextMenu>
    </Border.ContextMenu>
    <TextBlock Text="长按弹出菜单" Foreground="White" 
               HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>
```

### 工业适配关键点

1. **防误触机制**：手指滑出控件、提前抬起均取消触发，适配车间复杂操作环境
2. **双输入兼容**：同时支持触摸长按和鼠标左键长按，调试和现场操作都兼容
3. **可配置时长**：通过修改 `Interval` 可调整长按触发时间，适配不同工位操作习惯

------

## 五、工业深色主题自定义菜单样式（工控 UI 标准）

### 场景背景

工业上位机软件普遍使用深色主题，降低屏幕亮度、减少视觉疲劳，默认白色 ContextMenu 与整体风格违和，需自定义深色工业风样式。

### 完整样式代码

xaml:

```xaml
<Window.Resources>
    <!-- ContextMenu 整体容器样式 -->
    <Style TargetType="ContextMenu">
        <Setter Property="Background" Value="#1e1e1e"/>
        <Setter Property="Foreground" Value="#d4d4d4"/>
        <Setter Property="BorderBrush" Value="#3e3e42"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="2"/>
        <Setter Property="HasDropShadow" Value="True"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="ContextMenu">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="4"
                            Padding="{TemplateBinding Padding}"
                            Effect="{DynamicResource {x:Static SystemParameters.DropShadowKey}}">
                        <ScrollViewer x:Name="ScrollViewer" CanContentScroll="True">
                            <ItemsPresenter/>
                        </ScrollViewer>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- MenuItem 菜单项样式 -->
    <Style TargetType="MenuItem">
        <Setter Property="Foreground" Value="#d4d4d4"/>
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Padding" Value="16,6"/>
        <Setter Property="Margin" Value="2"/>
        <Setter Property="FontSize" Value="12"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="MenuItem">
                    <Border x:Name="Bd"
                            Background="{TemplateBinding Background}"
                            Padding="{TemplateBinding Padding}"
                            CornerRadius="3"
                            SnapsToDevicePixels="True">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto" MinWidth="20"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <ContentPresenter x:Name="Icon" ContentSource="Icon" Margin="0,0,8,0" VerticalAlignment="Center"/>
                            <ContentPresenter Grid.Column="1" ContentSource="Header" RecognizesAccessKey="True" VerticalAlignment="Center"/>
                            <ContentPresenter Grid.Column="2" ContentSource="InputGestureText" Margin="8,0,0,0" VerticalAlignment="Center"/>
                        </Grid>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsHighlighted" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="#094771"/>
                            <Setter Property="Foreground" Value="White"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter Property="Foreground" Value="#666666"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 分隔线样式 -->
    <Style TargetType="Separator">
        <Setter Property="Margin" Value="4,2"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Separator">
                    <Border Background="#3e3e42" Height="1"/>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</Window.Resources>
```

### 工业适配关键点

1. **低对比度护眼**：深灰背景 + 浅灰文字，避免高亮度刺眼，适合车间长时间作业
2. **悬停高亮清晰**：选中态用工业蓝高亮，辨识度高，操作不易出错
3. **圆角与边框**：轻微圆角 + 细边框，兼顾工业硬朗风格与现代 UI 质感

------

## 六、配方下发多级右键菜单（工艺管理场景）

### 场景背景

生产工位需快速切换产品配方，右键设备图标展开多级配方菜单，选择后直接下发到设备，无需打开单独窗口。

### 核心实现

xaml:

```xaml
<Border Width="120" Height="80" Background="#2d2d30" BorderBrush="#00ff00" BorderThickness="1">
    <Border.ContextMenu>
        <ContextMenu>
            <MenuItem Header="切换配方">
                <MenuItem Header="产品A系列">
                    <MenuItem Header="A01-标准配方" Command="{Binding PlacementTarget.DataContext.LoadRecipeCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}}" CommandParameter="A01"/>
                    <MenuItem Header="A02-高精度配方" Command="{Binding PlacementTarget.DataContext.LoadRecipeCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}}" CommandParameter="A02"/>
                </MenuItem>
                <MenuItem Header="产品B系列">
                    <MenuItem Header="B01-快速配方" Command="{Binding PlacementTarget.DataContext.LoadRecipeCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}}" CommandParameter="B01"/>
                    <MenuItem Header="B02-加厚配方" Command="{Binding PlacementTarget.DataContext.LoadRecipeCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}}" CommandParameter="B02"/>
                </MenuItem>
            </MenuItem>
            <MenuItem Header="保存当前配方"/>
            <Separator/>
            <MenuItem Header="配方参数设置"/>
        </ContextMenu>
    </Border.ContextMenu>
    <TextBlock Text="压合工位" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center"/>
</Border>
```

------

## 工业场景通用避坑总结

1. **绑定必用 PlacementTarget 桥接**：ContextMenu 不在主视觉树，所有命令、数据绑定必须通过 `PlacementTarget` 中转
2. **高危操作必加二次确认**：设备启停、参数修改、删除等操作必须弹窗确认，防止误触
3. **状态联动用 CanExecute**：菜单项可用性通过命令的 `CanExecute` 控制，WPF 自动刷新，代码更简洁
4. **触屏场景防误触**：长按触发必须支持滑动取消、提前抬起取消，适配车间操作环境
5. **权限控制用数据驱动**：菜单由权限动态生成，不要通过隐藏 / 显示控件的方式控制，避免权限绕过