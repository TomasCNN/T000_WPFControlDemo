# 003008002_VirtualizingStackPanel 类性能优化方案

## 一、最核心、最有效、必须开的 4 个优化（立刻见效）

### 1. 开启 **Recycling 虚拟化模式**（最重要！）

xaml:

```xaml
VirtualizingStackPanel.VirtualizationMode="Recycling"
```

- **Standard**：滚动时销毁、重建 UI → 卡、闪烁
- **Recycling**：滚动时**复用 UI**，不销毁不重建 → **极致流畅**

### 2. 开启 **滚动虚拟化**

xaml:

```xaml
VirtualizingStackPanel.IsVirtualizing="True"
```

​	默认是 True，但有时候被样式覆盖，必须显式写。

### 3. 关闭 **自动滚动到选中项**

xaml:

```xaml
ScrollViewer.CanContentScroll="True"
```

​	必须为`True`，才能按像素 / 项虚拟化，否则会强制加载全部 UI。

### 4. 关闭 **自动排序、自动重排**

xaml:

```xaml
ItemsControl.IsItemClickEnabled="False"
```

减少布局触发次数。

------

## 二、完整最优模板（直接复制到你的 DataGrid/ListBox）

xaml:

```xaml
<ListBox 
    VirtualizingStackPanel.IsVirtualizing="True"
    VirtualizingStackPanel.VirtualizationMode="Recycling"
    ScrollViewer.CanContentScroll="True"
    VirtualizingStackPanel.CleanUpVirtualizedItem="VirtualizingStackPanel_CleanUpVirtualizedItem">

    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel />
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

**DataGrid 版本：**

xaml:

```xaml
<DataGrid
    VirtualizingStackPanel.IsVirtualizing="True"
    VirtualizingStackPanel.VirtualizationMode="Recycling"
    ScrollViewer.IsDeferredScrollingEnabled="True"
    ScrollViewer.CanContentScroll="True"/>
```

------

## 三、进阶性能优化（上位机高并发数据必备）

### 5. 开启 **延迟滚动（Deferred Scrolling）**

xaml:

```xaml
ScrollViewer.IsDeferredScrollingEnabled="True"
```

​	拖动滚动条时**不实时渲染**，松开才刷新 → 超级顺滑

### 6. 关闭 **自动列宽、自动高度**

xaml:

```xaml
ColumnWidth="*" 或固定值
RowHeight="30"
```

​	禁止自动计算，否则滚动时反复布局，CPU 爆炸。

### 7. 关闭 **UI 自动刷新**

​	列表更新时用：

​	csharp:

```c#
ObservableCollection.CollectionChanged += ...
```

​	**不要频繁 Clear + Add**，会触发大量布局。

​	正确做法：

csharp:

```c#
list.SuspendUpdates();
list.Clear();
list.AddRange(大量数据);
list.ResumeUpdates();
```

### 8. 绑定使用 **x:Shared="False"**

​	模板缓存，避免重复创建：

​	xaml:

```xaml
<DataTemplate x:Shared="False">
```

### 9. Item 不要用复杂布局

- 少用 **嵌套 Grid、Canvas、Border**

- 少用 **圆角、阴影、渐变**

- 少用 **多绑定、多转换器**

  

  这些都会让 UI 渲染变慢。

### 10. 启用 **GPU 渲染**

xaml:

```xaml
RenderOptions.ProcessRenderMode="Hardware"
```

------

## 四、最关键的后台优化（90% 的人忽略）

### 11. 后台数据加载 **异步化**

不要在 UI 线程加载 1 万条数据！

csharp:

```c#
await Task.Run(() => {
    // 加载数据
});
```

### 12. 使用 **ICollectionView** 分页 / 延迟加载

csharp:

```c#
ListCollectionView
```

只加载可视区域数据，不加载全部。

------

## 五、最终极优化（百万数据不卡）

### 13. 使用 **ItemsRepeater**（.NET 8+ / WinUI3）

比 VirtualizingStackPanel 快 3~5 倍。

### 14. 使用 **ListView 虚拟化 + 分页**

真正工业上位机标准方案：

- 只加载当前页
- 滚动到底部再加载下一页

------

## 六、一句话总结（你只要记住这个）

### **VirtualizingStackPanel 性能 = Recycling 复用 + CanContentScroll + 固定大小 + 异步加载**

只要开启这 4 个，**10 万条数据秒开、丝滑滚动**。