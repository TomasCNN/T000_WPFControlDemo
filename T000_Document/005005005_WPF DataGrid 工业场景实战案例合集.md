# 005005005_WPF `DataGrid` 工业场景实战案例合集

以下案例全部贴合工业上位机、设备监控、生产管理等真实业务场景，覆盖基础编辑、样式定制、性能优化、行详情、模板列、批量操作、数据验证等核心功能，每个案例明确标注对应 `DataGrid` 核心特性，可直接复用至项目中。

------

## 案例 1：生产配方参数编辑表（基础多类型列 + MVVM）

### 场景说明

配方管理界面，编辑配方的参数名称、数值、是否启用、参数类型，支持下拉选择参数分类，是工业配方系统的基础可编辑表格场景。

### 对应核心特性

- 手动定义列（`AutoGenerateColumns="False"`）
- 多列类型：`DataGridTextColumn` / `DataGridCheckBoxColumn` / `DataGridComboBoxColumn`
- 禁止用户直接增删行，走业务按钮控制
- 单列只读控制，关键字段不可修改

### 1. 数据模型

csharp:

```c#
public class RecipeParam : INotifyPropertyChanged
{
    private string _paramCode;
    public string ParamCode
    {
        get => _paramCode;
        set { _paramCode = value; OnPropertyChanged(); }
    }

    private string _paramName;
    public string ParamName
    {
        get => _paramName;
        set { _paramName = value; OnPropertyChanged(); }
    }

    private double _paramValue;
    public double ParamValue
    {
        get => _paramValue;
        set { _paramValue = value; OnPropertyChanged(); }
    }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    private string _paramType;
    public string ParamType
    {
        get => _paramType;
        set { _paramType = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 2. ViewModel

csharp:

```c#
public class RecipeViewModel : INotifyPropertyChanged
{
    public ObservableCollection<RecipeParam> ParamList { get; set; }
    public List<string> ParamTypeOptions { get; } = new List<string> { "温度", "压力", "时间", "速度" };

    public RecipeViewModel()
    {
        ParamList = new ObservableCollection<RecipeParam>
        {
            new RecipeParam { ParamCode = "T001", ParamName = "预热温度", ParamValue = 85.5, IsEnabled = true, ParamType = "温度" },
            new RecipeParam { ParamCode = "T002", ParamName = "固化温度", ParamValue = 120.0, IsEnabled = true, ParamType = "温度" },
            new RecipeParam { ParamCode = "S001", ParamName = "输送速度", ParamValue = 0.5, IsEnabled = false, ParamType = "速度" },
            new RecipeParam { ParamCode = "P001", ParamName = "喷涂压力", ParamValue = 0.32, IsEnabled = true, ParamType = "压力" },
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 3. XAML 界面

xaml:

```xaml
<Window.DataContext>
    <local:RecipeViewModel/>
</Window.DataContext>

<Grid Margin="10">
    <DataGrid ItemsSource="{Binding ParamList}"
              AutoGenerateColumns="False"
              CanUserAddRows="False"
              CanUserDeleteRows="False"
              CanUserReorderColumns="False"
              SelectionMode="Single"
              SelectionUnit="FullRow"
              GridLinesVisibility="All"
              HeadersVisibility="Column"
              BorderBrush="#DDD" BorderThickness="1">
        
        <DataGrid.Columns>
            <!-- 文本列：参数编码，只读 -->
            <DataGridTextColumn Header="参数编码" Binding="{Binding ParamCode}" Width="100" IsReadOnly="True"/>
            
            <!-- 文本列：参数名称，只读 -->
            <DataGridTextColumn Header="参数名称" Binding="{Binding ParamName}" Width="120" IsReadOnly="True"/>
            
            <!-- 文本列：参数值，可编辑 -->
            <DataGridTextColumn Header="参数值" Binding="{Binding ParamValue}" Width="100"/>
            
            <!-- 复选框列：是否启用 -->
            <DataGridCheckBoxColumn Header="是否启用" Binding="{Binding IsEnabled}" Width="80"/>
            
            <!-- 下拉列：参数类型 -->
            <DataGridComboBoxColumn Header="参数类型" 
                                    SelectedItemBinding="{Binding ParamType}"
                                    ItemsSource="{Binding ParamTypeOptions, Source={x:Static local:RecipeViewModel.Instance}}"
                                    Width="100"/>
        </DataGrid.Columns>
    </DataGrid>
</Grid>
```

------

## 案例 2：设备参数配置表（样式定制 + 交替行 + 选中高亮）

### 场景说明

设备参数配置界面，关键参数字段只读，奇偶行交替背景，选中行高亮，统一单元格内边距与对齐方式，符合工业软件视觉规范。

### 对应核心特性

- `AlternatingRowBackground` 交替行背景
- `RowStyle` 行容器样式定制
- `CellStyle` 单元格统一样式
- `GridLinesVisibility` 网格线控制
- 单列 `IsReadOnly` 权限控制

xaml:

```xaml
<DataGrid ItemsSource="{Binding DeviceParamList}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          AlternatingRowBackground="#F8F9FA"
          GridLinesVisibility="Horizontal"
          RowBackground="White"
          CanUserReorderColumns="False"
          BorderBrush="#DDD" BorderThickness="1">
    
    <!-- 行容器样式 -->
    <DataGrid.RowStyle>
        <Style TargetType="DataGridRow">
            <Setter Property="Height" Value="30"/>
            <Setter Property="Background" Value="White"/>
            <Setter Property="Foreground" Value="#333"/>
            <Style.Triggers>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="#E6F4FF"/>
                    <Setter Property="Foreground" Value="#333"/>
                </Trigger>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#F0F7FF"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </DataGrid.RowStyle>

    <!-- 单元格统一样式 -->
    <DataGrid.CellStyle>
        <Style TargetType="DataGridCell">
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="Padding" Value="6 2"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="FocusVisualStyle" Value="{x:Null}"/>
        </Style>
    </DataGrid.CellStyle>

    <DataGrid.Columns>
        <DataGridTextColumn Header="参数编码" Binding="{Binding ParamCode}" Width="100" IsReadOnly="True"/>
        <DataGridTextColumn Header="参数名称" Binding="{Binding ParamName}" Width="150" IsReadOnly="True"/>
        <DataGridTextColumn Header="当前值" Binding="{Binding CurrentValue}" Width="100"/>
        <DataGridTextColumn Header="单位" Binding="{Binding Unit}" Width="60" IsReadOnly="True"/>
        <DataGridTextColumn Header="量程范围" Binding="{Binding Range}" Width="120" IsReadOnly="True"/>
        <DataGridTextColumn Header="备注" Binding="{Binding Remark}" Width="*"/>
    </DataGrid.Columns>
</DataGrid>
```

------

## 案例 3：万级历史生产记录高性能表（双级虚拟化 + 性能优化）

### 场景说明

上万条历史生产记录查询，要求滚动流畅、内存占用低，支持横向滚动时固定左侧关键列，是工业历史数据查询的标准性能优化方案。

### 对应核心特性

- `EnableRowVirtualization` 行虚拟化
- `EnableColumnVirtualization` 列虚拟化
- `VirtualizingStackPanel.VirtualizationMode="Recycling"` 容器回收
- `ScrollViewer.IsDeferredScrollingEnabled` 延迟滚动
- `FrozenColumnCount` 冻结左侧关键列

xaml:

```xaml
<DataGrid ItemsSource="{Binding HistoryRecordList}"
          AutoGenerateColumns="False"
          EnableRowVirtualization="True"
          EnableColumnVirtualization="True"
          VirtualizingStackPanel.VirtualizationMode="Recycling"
          VirtualizingStackPanel.IsVirtualizing="True"
          ScrollViewer.IsDeferredScrollingEnabled="True"
          FrozenColumnCount="2"
          AlternatingRowBackground="#F8F9FA"
          CanUserReorderColumns="False"
          CanUserSortColumns="True"
          Height="500"
          BorderBrush="#DDD" BorderThickness="1">
    
    <DataGrid.Columns>
        <!-- 前2列冻结，横向滚动时固定显示 -->
        <DataGridTextColumn Header="记录时间" Binding="{Binding RecordTime, StringFormat=yyyy-MM-dd HH:mm:ss}" Width="150" IsReadOnly="True"/>
        <DataGridTextColumn Header="设备编号" Binding="{Binding DeviceCode}" Width="100" IsReadOnly="True"/>
        
        <DataGridTextColumn Header="温度(℃)" Binding="{Binding Temperature, StringFormat=F1}" Width="80" IsReadOnly="True"/>
        <DataGridTextColumn Header="压力(MPa)" Binding="{Binding Pressure, StringFormat=F2}" Width="90" IsReadOnly="True"/>
        <DataGridTextColumn Header="转速(rpm)" Binding="{Binding Speed}" Width="90" IsReadOnly="True"/>
        <DataGridTextColumn Header="运行状态" Binding="{Binding Status}" Width="80" IsReadOnly="True"/>
        <DataGridTextColumn Header="班次" Binding="{Binding Shift}" Width="60" IsReadOnly="True"/>
        <DataGridTextColumn Header="操作员" Binding="{Binding Operator}" Width="80" IsReadOnly="True"/>
        <DataGridTextColumn Header="备注" Binding="{Binding Remark}" Width="200" IsReadOnly="True"/>
    </DataGrid.Columns>
</DataGrid>
```

### 性能说明

- 开启行虚拟化 + 回收模式后，10 万条数据内存占用仅为全量渲染的 5%~10%；
- 列虚拟化在列数 > 20 时效果显著，仅生成可见列的单元格；
- 延迟滚动开启后，拖动滚动条时仅显示提示，松开后再渲染，大幅提升大数据量下的拖动流畅度；
- 冻结列保证横向滚动时，设备编号、时间等关键字段始终可见，符合工业操作习惯。

------

## 案例 4：设备台账行详情展开（分级展示 + 行详情）

### 场景说明

设备台账列表，选中某行后展开显示设备的详细参数、安装信息、维护记录，适合信息量大、不需要一次性全部展示的台账场景。

### 对应核心特性

- `RowDetailsTemplate` 行详情模板
- `RowDetailsVisibilityMode="VisibleWhenSelected"` 选中时展开
- `AreRowDetailsFrozen="True"` 详情随横向滚动固定

xaml:

```xaml
<DataGrid ItemsSource="{Binding DeviceList}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          RowDetailsVisibilityMode="VisibleWhenSelected"
          AreRowDetailsFrozen="True"
          AlternatingRowBackground="#F8F9FA"
          BorderBrush="#DDD" BorderThickness="1">
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="设备编号" Binding="{Binding DeviceCode}" Width="100" IsReadOnly="True"/>
        <DataGridTextColumn Header="设备名称" Binding="{Binding DeviceName}" Width="150" IsReadOnly="True"/>
        <DataGridTextColumn Header="设备类型" Binding="{Binding DeviceType}" Width="100" IsReadOnly="True"/>
        <DataGridTextColumn Header="运行状态" Binding="{Binding StatusText}" Width="80" IsReadOnly="True"/>
    </DataGrid.Columns>

    <!-- 行详情模板 -->
    <DataGrid.RowDetailsTemplate>
        <DataTemplate>
            <Border Background="#F5F7FA" Padding="15" BorderBrush="#DDD" BorderThickness="1 0 1 1">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    
                    <StackPanel>
                        <TextBlock FontWeight="Bold" Text="基础信息"/>
                        <Separator Margin="0 5 0 8"/>
                        <TextBlock Text="{Binding DeviceDesc}" TextWrapping="Wrap"/>
                        <TextBlock Margin="0 5" Text="{Binding InstallTime, StringFormat=安装时间：{0:yyyy-MM-dd}}"/>
                        <TextBlock Text="{Binding Location, StringFormat=安装位置：{0}}"/>
                    </StackPanel>

                    <StackPanel Grid.Column="1">
                        <TextBlock FontWeight="Bold" Text="维护信息"/>
                        <Separator Margin="0 5 0 8"/>
                        <TextBlock Text="{Binding LastMaintainTime, StringFormat=上次维护：{0:yyyy-MM-dd}}"/>
                        <TextBlock Margin="0 5" Text="{Binding Maintainer, StringFormat=维护人员：{0}}"/>
                        <TextBlock Text="{Binding NextMaintainTime, StringFormat=下次维护：{0:yyyy-MM-dd}}"/>
                    </StackPanel>

                    <StackPanel Grid.Column="2">
                        <TextBlock FontWeight="Bold" Text="性能参数"/>
                        <Separator Margin="0 5 0 8"/>
                        <TextBlock Text="{Binding RatedPower, StringFormat=额定功率：{0} kW}"/>
                        <TextBlock Margin="0 5" Text="{Binding RatedVoltage, StringFormat=额定电压：{0} V}"/>
                        <TextBlock Text="{Binding IpGrade, StringFormat=防护等级：{0}}"/>
                    </StackPanel>
                </Grid>
            </Border>
        </DataTemplate>
    </DataGrid.RowDetailsTemplate>
</DataGrid>
```

------

## 案例 5：报警列表状态指示 + 行内操作按钮（模板列自定义）

### 场景说明

报警列表，用颜色圆点直观展示报警等级，每行带确认、详情操作按钮，是工业报警系统的标准交互模式。

### 对应核心特性

- `DataGridTemplateColumn` 自定义模板列
- 数据触发器实现状态颜色联动
- 行内按钮绑定数据上下文
- 单元格级自定义内容

xaml:

```xaml
<DataGrid ItemsSource="{Binding AlarmList}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          SelectionMode="Extended"
          AlternatingRowBackground="#F8F9FA"
          GridLinesVisibility="Horizontal"
          BorderBrush="#DDD" BorderThickness="1">
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="报警时间" Binding="{Binding AlarmTime, StringFormat=yyyy-MM-dd HH:mm:ss}" Width="150" IsReadOnly="True"/>
        <DataGridTextColumn Header="设备名称" Binding="{Binding DeviceName}" Width="120" IsReadOnly="True"/>
        
        <!-- 自定义状态列：颜色指示 -->
        <DataGridTemplateColumn Header="等级" Width="80">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                        <Ellipse Width="8" Height="8" VerticalAlignment="Center" Margin="0 0 6 0">
                            <Ellipse.Style>
                                <Style TargetType="Ellipse">
                                    <Setter Property="Fill" Value="#FAAD14"/>
                                    <Style.Triggers>
                                        <DataTrigger Binding="{Binding Level}" Value="严重">
                                            <Setter Property="Fill" Value="#F5222D"/>
                                        </DataTrigger>
                                        <DataTrigger Binding="{Binding Level}" Value="提示">
                                            <Setter Property="Fill" Value="#1890FF"/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Ellipse.Style>
                        </Ellipse>
                        <TextBlock Text="{Binding Level}"/>
                    </StackPanel>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>

        <DataGridTextColumn Header="报警内容" Binding="{Binding Message}" Width="*" IsReadOnly="True"/>
        
        <!-- 操作列：行内按钮 -->
        <DataGridTemplateColumn Header="操作" Width="140">
            <DataGridTemplateColumn.CellTemplate>
                <DataTemplate>
                    <StackPanel Orientation="Horizontal">
                        <Button Content="确认" Click="ConfirmAlarm_Click" Margin="0 0 8 0" Padding="8 2"
                                IsEnabled="{Binding IsConfirmed, Converter={StaticResource BoolInverseConverter}}"/>
                        <Button Content="详情" Click="ViewAlarmDetail_Click" Padding="8 2"/>
                    </StackPanel>
                </DataTemplate>
            </DataGridTemplateColumn.CellTemplate>
        </DataGridTemplateColumn>
    </DataGrid.Columns>
</DataGrid>
```

### 后台按钮事件

csharp:

```c#
private void ConfirmAlarm_Click(object sender, RoutedEventArgs e)
{
    // 获取当前行绑定的报警数据
    var button = sender as Button;
    var alarm = button?.DataContext as AlarmRecord;
    if (alarm != null)
    {
        alarm.IsConfirmed = true;
        // 执行业务确认逻辑
    }
}

private void ViewAlarmDetail_Click(object sender, RoutedEventArgs e)
{
    var alarm = (sender as Button)?.DataContext as AlarmRecord;
    // 打开详情窗口
}
```

------

## 案例 6：批量确认报警表（多选 + 批量操作 + 冻结列）

### 场景说明

报警列表支持 Ctrl 点选、Shift 连选，顶部工具栏批量执行确认、导出操作，冻结左侧关键列，是工业报警系统的批量处理标准方案。

### 对应核心特性

- `SelectionMode="Extended"` 扩展多选
- `FrozenColumnCount` 冻结列
- `SelectedItems` 选中集合（继承自 `MultiSelector`，内置批量更新优化）
- 全选 / 全不选批量操作

xaml:

```xaml
<Grid Margin="10">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- 顶部操作栏 -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0 0 0 8">
        <Button Content="全选" Click="SelectAll_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="取消全选" Click="UnselectAll_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="批量确认" Click="BatchConfirm_Click" Margin="0 0 8 0" Padding="12 4"/>
        <Button Content="批量导出" Click="BatchExport_Click" Padding="12 4"/>
        <TextBlock Margin="20 0 0 0" VerticalAlignment="Center" Foreground="#666">
            已选中 <Run x:Name="SelectedCountText">0</Run> 条
        </TextBlock>
    </StackPanel>

    <!-- 报警表格 -->
    <DataGrid Grid.Row="1"
              x:Name="AlarmGrid"
              ItemsSource="{Binding AlarmList}"
              AutoGenerateColumns="False"
              CanUserAddRows="False"
              SelectionMode="Extended"
              SelectionUnit="FullRow"
              FrozenColumnCount="1"
              AlternatingRowBackground="#F8F9FA"
              SelectionChanged="AlarmGrid_SelectionChanged"
              BorderBrush="#DDD" BorderThickness="1">
        
        <DataGrid.Columns>
            <DataGridTextColumn Header="报警时间" Binding="{Binding AlarmTime, StringFormat=MM-dd HH:mm:ss}" Width="120" IsReadOnly="True"/>
            <DataGridTextColumn Header="设备名称" Binding="{Binding DeviceName}" Width="120" IsReadOnly="True"/>
            <DataGridTextColumn Header="等级" Binding="{Binding Level}" Width="60" IsReadOnly="True"/>
            <DataGridTextColumn Header="报警内容" Binding="{Binding Message}" Width="*" IsReadOnly="True"/>
            <DataGridTextColumn Header="确认状态" Binding="{Binding ConfirmStatus}" Width="80" IsReadOnly="True"/>
        </DataGrid.Columns>
    </DataGrid>
</Grid>
```

### 后台交互逻辑

csharp:

```c#
// 选中数量统计
private void AlarmGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    SelectedCountText.Text = AlarmGrid.SelectedItems.Count.ToString();
}

// 全选（内部自动使用 MultiSelector 批量更新机制，性能优异）
private void SelectAll_Click(object sender, RoutedEventArgs e)
{
    AlarmGrid.SelectAll();
}

// 取消全选
private void UnselectAll_Click(object sender, RoutedEventArgs e)
{
    AlarmGrid.UnselectAll();
}

// 批量确认
private void BatchConfirm_Click(object sender, RoutedEventArgs e)
{
    var selectedAlarms = AlarmGrid.SelectedItems.Cast<AlarmRecord>().ToList();
    if (selectedAlarms.Count == 0)
    {
        MessageBox.Show("请先选择要确认的报警记录");
        return;
    }

    foreach (var alarm in selectedAlarms)
    {
        alarm.IsConfirmed = true;
        alarm.ConfirmStatus = "已确认";
    }

    MessageBox.Show($"成功确认 {selectedAlarms.Count} 条报警");
}
```

> 💡 性能说明：`SelectAll` 方法内部自动使用 `MultiSelector` 的批量更新机制，上千条数据全选仅触发一次 `SelectionChanged` 事件，性能远高于循环逐条设置选中。

------

## 案例 7：参数录入数据验证（编辑校验 + 行验证）

### 场景说明

工艺参数录入表格，输入数值超量程时提示错误，行级校验必填项完整性，防止非法参数下发到设备。

### 对应核心特性

- `CellEditEnding` 单元格编辑校验
- `RowValidationRules` 行级验证规则
- 数据模型实现 `IDataErrorInfo` 接口

### 1. 带验证的数据模型

csharp:

```c#
public class ProcessParam : INotifyPropertyChanged, IDataErrorInfo
{
    private double _tempValue;
    public double TempValue
    {
        get => _tempValue;
        set { _tempValue = value; OnPropertyChanged(); }
    }

    private double _pressureValue;
    public double PressureValue
    {
        get => _pressureValue;
        set { _pressureValue = value; OnPropertyChanged(); }
    }

    // 单字段验证
    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(TempValue):
                    if (TempValue < 0 || TempValue > 200)
                        return "温度范围 0~200℃";
                    break;
                case nameof(PressureValue):
                    if (PressureValue < 0 || PressureValue > 1.6)
                        return "压力范围 0~1.6 MPa";
                    break;
            }
            return string.Empty;
        }
    }

    public string Error => string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 2. XAML 启用验证

xaml:

```xaml
<DataGrid ItemsSource="{Binding ParamList}"
          AutoGenerateColumns="False"
          CanUserAddRows="False"
          GridLinesVisibility="All"
          BorderBrush="#DDD" BorderThickness="1">
    
    <!-- 行级错误提示模板 -->
    <DataGrid.RowValidationErrorTemplate>
        <ControlTemplate>
            <Grid ToolTip="{Binding Path=(Validation.Errors)[0].ErrorContent}">
                <Ellipse Width="14" Height="14" Fill="Red" HorizontalAlignment="Right" VerticalAlignment="Center"/>
                <TextBlock Text="!" Foreground="White" FontWeight="Bold" HorizontalAlignment="Right" VerticalAlignment="Center" FontSize="11" Margin="0 0 4 0"/>
            </Grid>
        </ControlTemplate>
    </DataGrid.RowValidationErrorTemplate>

    <DataGrid.Columns>
        <DataGridTextColumn Header="温度(℃)" Binding="{Binding TempValue, ValidatesOnDataErrors=True}" Width="100"/>
        <DataGridTextColumn Header="压力(MPa)" Binding="{Binding PressureValue, ValidatesOnDataErrors=True}" Width="100"/>
    </DataGrid.Columns>
</DataGrid>
```

------

## 工业场景最佳实践总结

1. **生产环境标准配置**：关闭自动生成列、关闭用户直接增删行、关闭列重排，通过业务按钮控制数据变更，保证操作规范性与数据安全性。
2. **大数据量必开虚拟化**：500 行以上开启行虚拟化，20 列以上开启列虚拟化，配合回收模式大幅降低内存占用与 GC 压力。
3. **宽表格冻结关键列**：设备编号、时间等关键字段设置冻结，横向滚动时始终可见，提升操作效率。
4. **验证逻辑下沉到模型**：数据校验实现 `IDataErrorInfo` 接口，不要大量写在 `CellEditEnding` 事件中，逻辑更清晰、可复用性更强。
5. **永远操作数据层**：不要遍历 `DataGridRow` / `DataGridCell` 获取或修改数据，直接操作绑定的业务集合，虚拟化下操作 UI 容器必然失效。
6. **批量操作用内置方法**：全选、取消全选使用控件内置方法，自动复用 `MultiSelector` 批量更新优化，性能远高于循环逐条设置。