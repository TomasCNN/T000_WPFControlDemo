# 005010007_WPF ContextMenu 工业场景实战实例合集

以下是 **5 个工业场景下 ContextMenu 的完整可运行代码合集**，全部遵循 MVVM 架构，适配 MES/SCADA/ 工控上位机等工业软件规范，复制即可直接落地使用。

------

## 前置：通用 MVVM 基础类（所有场景共用）

先创建通用命令类，是所有 ViewModel 的基础依赖。

csharp:

```c#
using System;
using System.Windows.Input;

namespace WpfIndustrialDemo.Common
{
    /// <summary>
    /// 无参数 RelayCommand
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
    }

    /// <summary>
    /// 带参数泛型 RelayCommand
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T typedParam)
                return _canExecute == null || _canExecute(typedParam);
            return _canExecute == null;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParam)
                _execute(typedParam);
        }
    }
}
```

------

## 场景一：MES 报警列表行右键菜单

### 业务场景

报警列表右键单条记录，执行「确认报警、查看详情、导出记录」，已确认报警自动禁用「确认」按钮。

### 1. 数据模型 `AlarmModel.cs`

csharp:

```c#
using System;
using System.ComponentModel;

namespace WpfIndustrialDemo.Models
{
    public class AlarmModel : INotifyPropertyChanged
    {
        private bool _isConfirmed;
        public string AlarmId { get; set; }
        public string EquipmentName { get; set; }
        public string AlarmContent { get; set; }
        public DateTime AlarmTime { get; set; }

        public bool IsConfirmed
        {
            get => _isConfirmed;
            set
            {
                _isConfirmed = value;
                OnPropertyChanged(nameof(IsConfirmed));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

### 2. 视图模型 `AlarmViewModel.cs`

csharp:

```c#
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using WpfIndustrialDemo.Common;
using WpfIndustrialDemo.Models;

namespace WpfIndustrialDemo.ViewModels
{
    public class AlarmViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AlarmModel> AlarmList { get; set; }
        public ICommand ConfirmAlarmCommand { get; }
        public ICommand ShowAlarmDetailCommand { get; }
        public ICommand ExportAlarmCommand { get; }

        public AlarmViewModel()
        {
            AlarmList = new ObservableCollection<AlarmModel>();
            ConfirmAlarmCommand = new RelayCommand<AlarmModel>(OnConfirmAlarm, CanConfirm);
            ShowAlarmDetailCommand = new RelayCommand<AlarmModel>(OnShowDetail);
            ExportAlarmCommand = new RelayCommand<AlarmModel>(OnExport);
            LoadMockAlarms();
        }

        private bool CanConfirm(AlarmModel alarm)
            => alarm != null && !alarm.IsConfirmed;

        private void OnConfirmAlarm(AlarmModel alarm)
        {
            if (MessageBox.Show($"确认处理报警 [{alarm.AlarmId}]？", "报警确认",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            alarm.IsConfirmed = true;
            // 实际项目：调用后端/PLC接口执行报警确认
        }

        private void OnShowDetail(AlarmModel alarm)
        {
            MessageBox.Show(
                $"报警编号：{alarm.AlarmId}\n设备：{alarm.EquipmentName}\n内容：{alarm.AlarmContent}\n时间：{alarm.AlarmTime:yyyy-MM-dd HH:mm:ss}",
                "报警详情", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnExport(AlarmModel alarm)
        {
            MessageBox.Show($"已导出报警 {alarm.AlarmId} 记录", "导出完成",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadMockAlarms()
        {
            AlarmList.Add(new AlarmModel
            {
                AlarmId = "ALM-20260601-001",
                EquipmentName = "CNC-01 数控车床",
                AlarmContent = "主轴温度超限（85℃）",
                AlarmTime = DateTime.Now.AddMinutes(-15),
                IsConfirmed = false
            });
            AlarmList.Add(new AlarmModel
            {
                AlarmId = "ALM-20260601-002",
                EquipmentName = "ROB-03 焊接机器人",
                AlarmContent = "伺服驱动器过载报警",
                AlarmTime = DateTime.Now.AddMinutes(-42),
                IsConfirmed = true
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
```

### 3. XAML 界面

xaml:

```xaml
<Window x:Class="WpfIndustrialDemo.AlarmWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:WpfIndustrialDemo.ViewModels"
        Title="报警管理" Height="450" Width="800">
    <Window.DataContext>
        <vm:AlarmViewModel/>
    </Window.DataContext>

    <Grid Margin="10">
        <DataGrid x:Name="AlarmGrid"
                  ItemsSource="{Binding AlarmList}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  GridLinesVisibility="Horizontal">
            <DataGrid.RowStyle>
                <Style TargetType="DataGridRow">
                    <!-- 核心：主ViewModel存入Tag，用于ContextMenu桥接 -->
                    <Setter Property="Tag" Value="{Binding DataContext, ElementName=AlarmGrid}"/>
                    <Setter Property="ContextMenu">
                        <Setter.Value>
                            <ContextMenu>
                                <MenuItem Header="确认报警"
                                          Command="{Binding PlacementTarget.Tag.ConfirmAlarmCommand, RelativeSource={RelativeSource Self}}"
                                          CommandParameter="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}">
                                    <MenuItem.Style>
                                        <Style TargetType="MenuItem" BasedOn="{StaticResource {x:Type MenuItem}}">
                                            <Style.Triggers>
                                                <!-- 已确认报警自动禁用 -->
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
                    <Style.Triggers>
                        <DataTrigger Binding="{Binding IsConfirmed}" Value="False">
                            <Setter Property="Background" Value="#FFF0F0"/>
                        </DataTrigger>
                    </Style.Triggers>
                </Style>
            </DataGrid.RowStyle>

            <DataGrid.Columns>
                <DataGridTextColumn Header="报警编号" Binding="{Binding AlarmId}" Width="180"/>
                <DataGridTextColumn Header="设备名称" Binding="{Binding EquipmentName}" Width="180"/>
                <DataGridTextColumn Header="报警内容" Binding="{Binding AlarmContent}" Width="*"/>
                <DataGridTextColumn Header="报警时间" Binding="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}" Width="180"/>
                <DataGridCheckBoxColumn Header="已确认" Binding="{Binding IsConfirmed, Mode=OneWay}" Width="60"/>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</Window>
```

------

## 场景二：SCADA 设备图标右键控制

### 业务场景

组态画面设备图标右键，执行「启动、停止、故障复位」，菜单项可用性随设备状态自动联动。

### 1. 设备模型 `EquipmentModel.cs`

csharp:

```c#
using System.ComponentModel;
using System.Windows.Input;

namespace WpfIndustrialDemo.Models
{
    public enum EquipmentStatus
    {
        Running, Stop, Fault, Manual
    }

    public class EquipmentModel : INotifyPropertyChanged
    {
        private EquipmentStatus _status;
        public string EqCode { get; set; }
        public string EqName { get; set; }

        public EquipmentStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

### 2. 视图模型 `EquipmentViewModel.cs`

csharp:

```c#
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using WpfIndustrialDemo.Common;
using WpfIndustrialDemo.Models;

namespace WpfIndustrialDemo.ViewModels
{
    public class EquipmentViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EquipmentModel> EquipmentList { get; set; }
        public ICommand StartEqCommand { get; }
        public ICommand StopEqCommand { get; }
        public ICommand ResetEqCommand { get; }

        public EquipmentViewModel()
        {
            EquipmentList = new ObservableCollection<EquipmentModel>();
            StartEqCommand = new RelayCommand<EquipmentModel>(OnStart, CanStart);
            StopEqCommand = new RelayCommand<EquipmentModel>(OnStop, CanStop);
            ResetEqCommand = new RelayCommand<EquipmentModel>(OnReset, CanReset);
            LoadMockEquipments();
        }

        private bool CanStart(EquipmentModel eq)
            => eq != null && eq.Status == EquipmentStatus.Stop;

        private bool CanStop(EquipmentModel eq)
            => eq != null && (eq.Status == EquipmentStatus.Running || eq.Status == EquipmentStatus.Manual);

        private bool CanReset(EquipmentModel eq)
            => eq != null && eq.Status == EquipmentStatus.Fault;

        private void OnStart(EquipmentModel eq)
        {
            if (MessageBox.Show($"确认启动 [{eq.EqName}]？", "设备启动",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            eq.Status = EquipmentStatus.Running;
            // 实际项目：下发PLC启动指令
        }

        private void OnStop(EquipmentModel eq)
        {
            if (MessageBox.Show($"确认停止 [{eq.EqName}]？", "设备停止",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            eq.Status = EquipmentStatus.Stop;
        }

        private void OnReset(EquipmentModel eq)
        {
            if (MessageBox.Show($"确认复位 [{eq.EqName}] 故障？", "故障复位",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            eq.Status = EquipmentStatus.Stop;
        }

        private void LoadMockEquipments()
        {
            EquipmentList.Add(new EquipmentModel { EqCode = "CNC-01", EqName = "数控车床1号", Status = EquipmentStatus.Running });
            EquipmentList.Add(new EquipmentModel { EqCode = "CNC-02", EqName = "数控车床2号", Status = EquipmentStatus.Stop });
            EquipmentList.Add(new EquipmentModel { EqCode = "ROB-01", EqName = "焊接机器人", Status = EquipmentStatus.Fault });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
```

### 3. XAML 组态画面

xaml:

```xaml
<Window x:Class="WpfIndustrialDemo.EquipmentWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:WpfIndustrialDemo.ViewModels"
        Title="设备监控组态" Height="450" Width="800" Background="#1e1e1e">
    <Window.DataContext>
        <vm:EquipmentViewModel/>
    </Window.DataContext>

    <Grid Margin="20">
        <ItemsControl ItemsSource="{Binding EquipmentList}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel Orientation="Horizontal"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Width="140" Height="90"
                            BorderThickness="1.5" CornerRadius="6"
                            Margin="15" Background="#2d2d30"
                            Tag="{Binding DataContext, RelativeSource={RelativeSource AncestorType=Window}}">
                        <Border.Style>
                            <Style TargetType="Border">
                                <Setter Property="BorderBrush" Value="#555"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding Status}" Value="Running">
                                        <Setter Property="BorderBrush" Value="#00cc66"/>
                                    </DataTrigger>
                                    <DataTrigger Binding="{Binding Status}" Value="Fault">
                                        <Setter Property="BorderBrush" Value="#ff3333"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Border.Style>

                        <!-- 右键菜单 -->
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
                            </ContextMenu>
                        </Border.ContextMenu>

                        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                            <TextBlock Text="{Binding EqName}" Foreground="White" FontSize="13" HorizontalAlignment="Center"/>
                            <TextBlock Text="{Binding Status}" FontSize="11" HorizontalAlignment="Center" Margin="0,6,0,0">
                                <TextBlock.Style>
                                    <Style TargetType="TextBlock">
                                        <Setter Property="Foreground" Value="#999"/>
                                        <Style.Triggers>
                                            <DataTrigger Binding="{Binding Status}" Value="Running">
                                                <Setter Property="Foreground" Value="#00cc66"/>
                                            </DataTrigger>
                                            <DataTrigger Binding="{Binding Status}" Value="Fault">
                                                <Setter Property="Foreground" Value="#ff3333"/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </TextBlock.Style>
                            </TextBlock>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</Window>
```

------

## 场景三：角色权限动态生成菜单

### 业务场景

不同角色（操作工 / 班长 / 管理员）看到不同菜单项，无权限操作完全不可见。

### 1. 菜单节点模型 `MenuItemNode.cs`

csharp:

```c#
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace WpfIndustrialDemo.Models
{
    public class MenuItemNode : INotifyPropertyChanged
    {
        public string MenuCode { get; set; }
        public string MenuHeader { get; set; }
        public ICommand Command { get; set; }
        public bool IsSeparator { get; set; }
        public ObservableCollection<MenuItemNode> Children { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

### 2. 权限服务 `PermissionService.cs`

csharp:

```c#
namespace WpfIndustrialDemo.Common
{
    public static class PermissionService
    {
        // 当前登录角色：Operator/Leader/Admin
        public static string CurrentRole { get; set; } = "Leader";

        public static bool HasPermission(string menuCode)
        {
            return menuCode switch
            {
                "View" => true,
                "Edit" => CurrentRole is "Leader" or "Admin",
                "Delete" => CurrentRole == "Admin",
                "Export" => CurrentRole is "Leader" or "Admin",
                _ => false
            };
        }
    }
}
```

### 3. 模板选择器 `MenuItemTemplateSelector.cs`

csharp:

```c#
using System.Windows;
using System.Windows.Controls;
using WpfIndustrialDemo.Models;

namespace WpfIndustrialDemo.Controls
{
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MenuItemNode { IsSeparator: true })
            {
                var template = new DataTemplate();
                template.VisualTree = new FrameworkElementFactory(typeof(Separator));
                return template;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
```

### 4. 视图模型 `PermissionMenuViewModel.cs`

csharp:

```c#
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using WpfIndustrialDemo.Common;
using WpfIndustrialDemo.Models;

namespace WpfIndustrialDemo.ViewModels
{
    public class PermissionMenuViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<MenuItemNode> _workOrderMenus;
        public ObservableCollection<MenuItemNode> WorkOrderMenus
        {
            get => _workOrderMenus;
            set { _workOrderMenus = value; OnPropertyChanged(nameof(WorkOrderMenus)); }
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
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

### 5. XAML 使用

xaml:

```xaml
<Window x:Class="WpfIndustrialDemo.PermissionWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:WpfIndustrialDemo.ViewModels"
        xmlns:ctrl="clr-namespace:WpfIndustrialDemo.Controls"
        Title="权限动态菜单" Height="300" Width="400">
    <Window.DataContext>
        <vm:PermissionMenuViewModel/>
    </Window.DataContext>

    <Grid>
        <Border x:Name="Card" Width="200" Height="100"
                Background="White" BorderBrush="#ddd" BorderThickness="1"
                CornerRadius="4" HorizontalAlignment="Center" VerticalAlignment="Center">
            <Border.ContextMenu>
                <ContextMenu ItemsSource="{Binding PlacementTarget.DataContext.WorkOrderMenus, RelativeSource={RelativeSource Self}}">
                    <ContextMenu.ItemContainerStyle>
                        <Style TargetType="MenuItem">
                            <Setter Property="Header" Value="{Binding MenuHeader}"/>
                            <Setter Property="Command" Value="{Binding Command}"/>
                            <Setter Property="ItemsSource" Value="{Binding Children}"/>
                        </Style>
                    </ContextMenu.ItemContainerStyle>
                    <ContextMenu.ItemTemplateSelector>
                        <ctrl:MenuItemTemplateSelector/>
                    </ContextMenu.ItemTemplateSelector>
                </ContextMenu>
            </Border.ContextMenu>
            <TextBlock Text="工单卡片（右键测试）" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
    </Grid>
</Window>
```

------

## 场景四：工业触屏长按弹出菜单

### 业务场景

车间触控一体机无鼠标右键，通过长按控件弹出菜单，支持滑出取消、防误触。

### 1. 长按附加属性 `TouchLongPress.cs`

csharp:

```c#
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfIndustrialDemo.Controls
{
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

        private const int PressDurationMs = 1000; // 工业场景默认1秒触发
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
            element.MouseLeave += Element_MouseLeave;
        }

        private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartTimer(sender as UIElement);
            e.Handled = true;
        }

        private static void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopTimer();
            e.Handled = true;
        }

        private static void Element_MouseLeave(object sender, MouseEventArgs e) => StopTimer();

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
            StopTimer(); // 滑出控件自动取消
            e.Handled = true;
        }

        private static void StartTimer(UIElement element)
        {
            StopTimer();
            _currentElement = element;
            _pressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(PressDurationMs) };
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
                command.Execute(_currentElement);
        }
    }
}
```

### 2. 视图模型 `TouchMenuViewModel.cs`

csharp:

```c#
using System.ComponentModel;
using System.Windows;
using WpfIndustrialDemo.Common;

namespace WpfIndustrialDemo.ViewModels
{
    public class TouchMenuViewModel : INotifyPropertyChanged
    {
        public ICommand ShowStationMenuCommand { get; }

        public TouchMenuViewModel()
        {
            ShowStationMenuCommand = new RelayCommand<UIElement>(OnShowMenu);
        }

        private void OnShowMenu(UIElement element)
        {
            if (element?.ContextMenu == null) return;
            element.ContextMenu.PlacementTarget = elcement;
            element.ContextMenu.Placement = PlacementMode.Center;
            element.ContextMenu.IsOpen = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
```

### 3. XAML 触屏界面

xaml:

```xaml
<Window x:Class="WpfIndustrialDemo.TouchWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:WpfIndustrialDemo.ViewModels"
        xmlns:ctrl="clr-namespace:WpfIndustrialDemo.Controls"
        Title="工位触控界面" Height="400" Width="600" Background="#1e1e1e">
    <Window.DataContext>
        <vm:TouchMenuViewModel/>
    </Window.DataContext>

    <Grid>
        <Border Width="180" Height="120"
                Background="#2d2d30"
                BorderBrush="#00aaff" BorderThickness="2"
                CornerRadius="8"
                HorizontalAlignment="Center" VerticalAlignment="Center"
                ctrl:TouchLongPress.LongPressCommand="{Binding ShowStationMenuCommand}">
            <Border.ContextMenu>
                <ContextMenu FontSize="16">
                    <MenuItem Header="开始作业"/>
                    <MenuItem Header="暂停作业"/>
                    <MenuItem Header="完工上报"/>
                    <Separator/>
                    <MenuItem Header="呼叫班长"/>
                </ContextMenu>
            </Border.ContextMenu>

            <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
                <TextBlock Text="压合工位01" Foreground="White" FontSize="18" HorizontalAlignment="Center"/>
                <TextBlock Text="长按弹出菜单" Foreground="#888" FontSize="12" Margin="0,8,0,0" HorizontalAlignment="Center"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

------

## 场景五：工业深色主题 ContextMenu 全局样式

### 业务场景

工业上位机深色护眼主题，全局统一样式，直接引入资源字典即可生效。

新建 `IndustrialContextMenu.xaml` 资源字典：

xaml:

```xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- ContextMenu 整体容器 -->
    <Style TargetType="{x:Type ContextMenu}">
        <Setter Property="Background" Value="#1e1e1e"/>
        <Setter Property="Foreground" Value="#d4d4d4"/>
        <Setter Property="BorderBrush" Value="#3e3e42"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Padding" Value="2"/>
        <Setter Property="HasDropShadow" Value="True"/>
        <Setter Property="SnapsToDevicePixels" Value="True"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type ContextMenu}">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="4"
                            Padding="{TemplateBinding Padding}">
                        <ScrollViewer CanContentScroll="True">
                            <ItemsPresenter KeyboardNavigation.DirectionalNavigation="Cycle"/>
                        </ScrollViewer>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 菜单项样式 -->
    <Style TargetType="{x:Type MenuItem}">
        <Setter Property="Foreground" Value="#d4d4d4"/>
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Padding" Value="16,7"/>
        <Setter Property="Margin" Value="2"/>
        <Setter Property="FontSize" Value="12"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type MenuItem}">
                    <Border x:Name="Bd"
                            Background="{TemplateBinding Background}"
                            Padding="{TemplateBinding Padding}"
                            CornerRadius="3"
                            SnapsToDevicePixels="True">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto" MinWidth="22"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto" MinWidth="14"/>
                            </Grid.ColumnDefinitions>
                            <ContentPresenter x:Name="Icon" ContentSource="Icon" Margin="0,0,8,0" VerticalAlignment="Center"/>
                            <ContentPresenter Grid.Column="1" ContentSource="Header" RecognizesAccessKey="True" VerticalAlignment="Center"/>
                            <ContentPresenter Grid.Column="2" ContentSource="InputGestureText" Margin="16,0,0,0" VerticalAlignment="Center"/>
                            <Path Grid.Column="3" x:Name="Arrow" Data="M 0 0 L 4 4 L 0 8 Z" Fill="#888" Margin="8,0,2,0" VerticalAlignment="Center" Visibility="Collapsed"/>
                        </Grid>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsHighlighted" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="#094771"/>
                            <Setter Property="Foreground" Value="White"/>
                            <Setter TargetName="Arrow" Property="Fill" Value="White"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter Property="Foreground" Value="#666"/>
                            <Setter TargetName="Arrow" Property="Fill" Value="#555"/>
                        </Trigger>
                        <Trigger Property="Role" Value="SubmenuHeader">
                            <Setter TargetName="Arrow" Property="Visibility" Value="Visible"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- 分隔线样式 -->
    <Style TargetType="{x:Type Separator}">
        <Setter Property="Margin" Value="4,3"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type Separator}">
                    <Border Background="#3e3e42" Height="1"/>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

**全局引用方式**：在 `App.xaml` 中引入

xaml:

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/IndustrialContextMenu.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

------

## 工业场景通用注意事项

1. **绑定必用 `PlacementTarget` 桥接**：ContextMenu 位于独立 Popup 视觉树，所有命令 / 数据必须通过 `PlacementTarget` 中转绑定。
2. **高危操作二次确认**：设备启停、删除、复位等操作必须增加弹窗确认，防止现场误触。
3. **状态联动用 `CanExecute`**：菜单项可用性通过命令的 `CanExecute` 控制，WPF 自动刷新，代码更简洁。
4. **触屏防误触**：长按触发必须支持滑动取消、提前抬起取消，适配车间复杂操作环境。
5. **权限最小化原则**：无权限菜单直接不生成，而非隐藏 / 禁用，符合工业信息安全规范。