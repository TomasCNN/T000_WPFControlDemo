# 004006001_WPF CheckBox 最全深度解析（基类定义 + 功能 + 使用 + 实例）

- 这是**工业上位机、业务系统最常用控件之一**，我会从**官方类定义、继承链、核心属性、事件、绑定、样式、多场景实例**一次性讲透。

  ------

  ## 一、先看核心：CheckBox 官方类定义（.NET 8）

  csharp:

  ```c#
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  
  namespace System.Windows.Controls
  {
      /// <summary>
      /// 复选框控件，支持选中、未选中、不确定三种状态
      /// </summary>
      public class CheckBox : ToggleButton
      {
          // 静态构造函数（注册依赖属性、元数据覆盖）
          static CheckBox();
          
          // 构造函数
          public CheckBox();
  
          // 唯一新增：支持 Content 内容居中对齐（复选框+文字居中）
          public static readonly DependencyProperty ContentVerticalAlignmentProperty;
          public VerticalAlignment ContentVerticalAlignment { get; set; }
      }
  }
  ```

  ------

  ## 二、CheckBox 完整继承链（超级重要）

  CheckBox **自己几乎没有代码**，99% 功能来自父类！

  plaintext:

  ```tex
  System.Object
   └─ DispatcherObject
      └─ DependencyObject
         └─ Visual
            └─ UIElement
               └─ FrameworkElement
                  └─ Control
                     └─ ContentControl  (支持 Content 内容)
                        └─ ButtonBase   (按钮基类)
                           └─ ToggleButton (切换按钮：三态)
                              └─ CheckBox (复选框，最终类)
  ```

  ### 一句话总结

  **CheckBox = ToggleButton + 复选框样式 + Content 居中对齐**

  它**没有自己的核心逻辑**，所有选中、绑定、命令、三态功能全部来自 `ToggleButton`。

  ------

  ## 三、CheckBox 核心功能（来自 ToggleButton）

  ### 1. 三种状态（CheckBox 灵魂）

  1. **Unchecked**（未选中）= false
  2. **Checked**（已选中）= true
  3. **Indeterminate**（不确定 / 半选）= null

  ### 2. 核心属性

  | 属性                         | 作用                          |
  | :--------------------------- | :---------------------------- |
  | **IsChecked**                | 选中状态：true /false/null    |
  | **IsThreeState**             | 是否允许三态（默认 false）    |
  | **Content**                  | 复选框右侧文字                |
  | **IsEnabled**                | 是否可用                      |
  | **IsHitTestVisible**         | 是否可点击                    |
  | **ContentVerticalAlignment** | 文字垂直对齐（CheckBox 独有） |
  | **Foreground / Background**  | 文字颜色、背景                |
  | **FontSize / FontWeight**    | 字体                          |

  ### 3. 核心事件

  | 事件               | 触发时机         |
  | :----------------- | :--------------- |
  | **Checked**        | 选中时           |
  | **Unchecked**      | 取消选中时       |
  | **Indeterminate**  | 变为不确定状态时 |
  | **Click**          | 点击时           |
  | **CheckedChanged** | 状态变化时       |

  ------

  ## 四、CheckBox 标准使用方法（4 种最常用场景）

  ### 场景 1：基础复选框（最简单）

  xaml:

  ```xaml
  <CheckBox Content="记住密码" IsChecked="True"/>
  ```

  ### 场景 2：三态复选框（半选）

  xaml:

  ```xaml
  <CheckBox Content="全选子项" IsThreeState="True" IsChecked="{x:Null}"/>
  ```

  ### 场景 3：MVVM 数据绑定（工业 / 正式项目必用）

  xaml:

  ```xaml
  <CheckBox Content="启动自动保存" 
            IsChecked="{Binding IsAutoSaveEnabled, Mode=TwoWay}"/>
  ```

  ### 场景 4：禁用复选框

  xaml:

  ```xaml
  <CheckBox Content="运行中不可修改" IsEnabled="False"/>
  ```

  ------

  ##  五、事件使用实例（C# 后台响应状态变化）

  ### XAML

  xaml:

  ```xaml
  <CheckBox x:Name="chkEnableAlarm" 
            Content="启用报警提醒"
            Checked="ChkEnableAlarm_Checked"
            Unchecked="ChkEnableAlarm_Unchecked"/>
  ```

  ## C# 后台

  csharp

  ```c#
  private void ChkEnableAlarm_Checked(object sender, RoutedEventArgs e)
  {
      // 选中：开启报警
      AlarmSystem.IsEnabled = true;
  }
  
  private void ChkEnableAlarm_Unchecked(object sender, RoutedEventArgs e)
  {
      // 取消：关闭报警
      AlarmSystem.IsEnabled = false;
  }
  ```

  ------

  ## 六、高级实例：全选 / 反选 / 半选（树形菜单、列表必备）

  这是**工业上位机、权限管理、配置页面**最经典的 CheckBox 用法。

  ## 效果

  - 子项全部选中 → 全选框 **勾选**
  - 子项部分选中 → 全选框 **半选（null）**
  - 子项全不选 → 全选框 **不勾选**

  ### XAML

  xaml:

  ```xaml
  <StackPanel>
      <!-- 全选框 -->
      <CheckBox x:Name="chkAll" Content="全选参数配置" 
                IsThreeState="True"
                Checked="ChkAll_Checked"
                Unchecked="ChkAll_Unchecked"
                Indeterminate="ChkAll_Indeterminate"/>
  
      <Separator Margin="0,5"/>
  
      <!-- 子项 -->
      <CheckBox x:Name="chkItem1" Content="参数A：温度报警" 
                Checked="UpdateAllCheckState" Unchecked="UpdateAllCheckState"/>
      <CheckBox x:Name="chkItem2" Content="参数B：压力上限" 
                Checked="UpdateAllCheckState" Unchecked="UpdateAllCheckState"/>
      <CheckBox x:Name="chkItem3" Content="参数C：运行日志" 
                Checked="UpdateAllCheckState" Unchecked="UpdateAllCheckState"/>
  </StackPanel>
  ```

  ### C# 逻辑（自动计算全选 / 半选 / 取消）

  csharp:

  ```c#
  private void ChkAll_Checked(object sender, RoutedEventArgs e)
  {
      chkItem1.IsChecked = true;
      chkItem2.IsChecked = true;
      chkItem3.IsChecked = true;
  }
  
  private void ChkAll_Unchecked(object sender, RoutedEventArgs e)
  {
      chkItem1.IsChecked = false;
      chkItem2.IsChecked = false;
      chkItem3.IsChecked = false;
  }
  
  private void ChkAll_Indeterminate(object sender, RoutedEventArgs e)
  {
      // 半选状态点击 → 设为全选
      chkAll.IsChecked = true;
  }
  
  // 子项变化 → 自动更新全选框状态
  private void UpdateAllCheckState(object sender, RoutedEventArgs e)
  {
      bool allChecked = chkItem1.IsChecked == true &&
                        chkItem2.IsChecked == true &&
                        chkItem3.IsChecked == true;
  
      bool allUnchecked = chkItem1.IsChecked == false &&
                          chkItem2.IsChecked == false &&
                          chkItem3.IsChecked == false;
  
      if (allChecked)
          chkAll.IsChecked = true;
      else if (allUnchecked)
          chkAll.IsChecked = false;
      else
          chkAll.IsChecked = null; // 半选
  }
  ```

  ------

  ## 七、CheckBox 样式修改（自定义外观，工业界面常用）

  你可以完全自定义复选框的勾选、未勾选、半选样式。

  ### 示例：自定义绿色勾选框

  xaml:

  ```xaml
  <CheckBox Content="已启用">
      <CheckBox.Style>
          <Style TargetType="CheckBox">
              <Setter Property="Foreground" Value="White"/>
              <Setter Property="FontWeight" Value="Bold"/>
              <Style.Triggers>
                  <Trigger Property="IsChecked" Value="True">
                      <Setter Property="Background" Value="#2ECC71"/>
                  </Trigger>
                  <Trigger Property="IsChecked" Value="{x:Null}">
                      <Setter Property="Background" Value="#F39C12"/>
                  </Trigger>
              </Style.Triggers>
          </Style>
      </CheckBox.Style>
  </CheckBox>
  ```

  ------

  ## 八、CheckBox 高频面试 / 开发要点（必背）

  1. **CheckBox 继承自 ToggleButton**，不是 Button！
  2. **三态必须设置 IsThreeState="True"**
  3. **IsChecked 可绑定 bool 或 bool?**（可空布尔）
  4. **全选功能必须用 Indeterminate（null）** 表示半选
  5. **MVVM 绑定必须用 Mode=TwoWay**（默认可省略）
  6. **Content 可以放任意元素**（图片、图标、复杂布局）
  7. 点击复选框**文字也能触发选中**，比 WinForm 更友好

  ------

  ## 九、完整总结（最精简）

  - **基类**：`ToggleButton` → 提供三态切换
  - **核心**：`IsChecked`（true/false/null）
  - **事件**：`Checked`、`Unchecked`、`Indeterminate`
  - **用途**：配置项、权限选择、全选 / 反选、状态开关
  - **项目必备**：三态全选、MVVM 双向绑定、自定义样式
  - **定位**：WPF 最基础、最常用、最稳定的选择控件

  