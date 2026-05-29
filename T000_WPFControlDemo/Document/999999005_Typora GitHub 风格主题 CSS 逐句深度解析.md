# 999999005_Typora GitHub 风格主题 CSS 逐句深度解析

**源码：**

```css
:root {
    --side-bar-bg-color: #fafafa;
    --control-text-color: #777;
}

@include-when-export url(https://fonts.googleapis.com/css?family=Open+Sans:400italic,700italic,700,400&subset=latin,latin-ext);

/* open-sans-regular - latin-ext_latin */
@font-face {
    font-family: 'Open Sans';
    font-style: normal;
    font-weight: normal;
    src: local('Open Sans Regular'), local('OpenSans-Regular'), url('./github/open-sans-v17-latin-ext_latin-regular.woff2') format('woff2');
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD, U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;
  }
  /* open-sans-italic - latin-ext_latin */
  @font-face {
    font-family: 'Open Sans';
    font-style: italic;
    font-weight: normal;
    src: local('Open Sans Italic'), local('OpenSans-Italic'), url('./github/open-sans-v17-latin-ext_latin-italic.woff2') format('woff2');
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD, U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;
  }
  /* open-sans-700 - latin-ext_latin */
  @font-face {
    font-family: 'Open Sans';
    font-style: normal;
    font-weight: bold;
    src: local('Open Sans Bold'), local('OpenSans-Bold'), url('./github/open-sans-v17-latin-ext_latin-700.woff2') format('woff2'); 
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD, U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;
  }
  /* open-sans-700italic - latin-ext_latin */
  @font-face {
    font-family: 'Open Sans';
    font-style: italic;
    font-weight: bold;
    src: local('Open Sans Bold Italic'), local('OpenSans-BoldItalic'), url('./github/open-sans-v17-latin-ext_latin-700italic.woff2') format('woff2');
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD, U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;
  }

html {
    font-size: 16px;
    -webkit-font-smoothing: antialiased;
}

body {
    font-family: "Open Sans","Clear Sans", "Helvetica Neue", Helvetica, Arial, 'Segoe UI Emoji', 'SF Pro', sans-serif;
    color: rgb(51, 51, 51);
    line-height: 1.6;
}

#write {
    max-width: 860px;
  	margin: 0 auto;
  	padding: 30px;
    padding-bottom: 100px;
}

@media only screen and (min-width: 1400px) {
	#write {
		max-width: 1024px;
	}
}

@media only screen and (min-width: 1800px) {
	#write {
		max-width: 1200px;
	}
}

#write > ul:first-child,
#write > ol:first-child{
    margin-top: 30px;
}

a {
    color: #4183C4;
}
h1,
h2,
h3,
h4,
h5,
h6 {
    position: relative;
    margin-top: 1rem;
    margin-bottom: 1rem;
    font-weight: bold;
    line-height: 1.4;
    cursor: text;
}
h1:hover a.anchor,
h2:hover a.anchor,
h3:hover a.anchor,
h4:hover a.anchor,
h5:hover a.anchor,
h6:hover a.anchor {
    text-decoration: none;
}
h1 tt,
h1 code {
    font-size: inherit;
}
h2 tt,
h2 code {
    font-size: inherit;
}
h3 tt,
h3 code {
    font-size: inherit;
}
h4 tt,
h4 code {
    font-size: inherit;
}
h5 tt,
h5 code {
    font-size: inherit;
}
h6 tt,
h6 code {
    font-size: inherit;
}
h1 {
    font-size: 2.25em;
    line-height: 1.2;
    border-bottom: 1px solid #eee;
}
h2 {
    font-size: 1.75em;
    line-height: 1.225;
    border-bottom: 1px solid #eee;
}

/*@media print {
    .typora-export h1,
    .typora-export h2 {
        border-bottom: none;
        padding-bottom: initial;
    }

    .typora-export h1::after,
    .typora-export h2::after {
        content: "";
        display: block;
        height: 100px;
        margin-top: -96px;
        border-top: 1px solid #eee;
    }
}*/

h3 {
    font-size: 1.5em;
    line-height: 1.43;
}
h4 {
    font-size: 1.25em;
}
h5 {
    font-size: 1em;
}
h6 {
   font-size: 1em;
    color: #777;
}
p,
blockquote,
ul,
ol,
dl,
table{
    margin: 0.8em 0;
}
li>ol,
li>ul {
    margin: 0 0;
}
hr {
    height: 2px;
    padding: 0;
    margin: 16px 0;
    background-color: #e7e7e7;
    border: 0 none;
    overflow: hidden;
    box-sizing: content-box;
}

li p.first {
    display: inline-block;
}
ul,
ol {
    padding-left: 30px;
}
ul:first-child,
ol:first-child {
    margin-top: 0;
}
ul:last-child,
ol:last-child {
    margin-bottom: 0;
}
blockquote {
    border-left: 4px solid #dfe2e5;
    padding: 0 15px;
    color: #777777;
}
blockquote blockquote {
    padding-right: 0;
}
table {
    padding: 0;
    word-break: initial;
}
table tr {
    border: 1px solid #dfe2e5;
    margin: 0;
    padding: 0;
}
table tr:nth-child(2n),
thead {
    background-color: #f8f8f8;
}
table th {
    font-weight: bold;
    border: 1px solid #dfe2e5;
    border-bottom: 0;
    margin: 0;
    padding: 6px 13px;
}
table td {
    border: 1px solid #dfe2e5;
    margin: 0;
    padding: 6px 13px;
}
table th:first-child,
table td:first-child {
    margin-top: 0;
}
table th:last-child,
table td:last-child {
    margin-bottom: 0;
}

.CodeMirror-lines {
    padding-left: 4px;
}

.code-tooltip {
    box-shadow: 0 1px 1px 0 rgba(0,28,36,.3);
    border-top: 1px solid #eef2f2;
}

.md-fences,
code,
tt {
    border: 1px solid #e7eaed;
    background-color: #f8f8f8;
    border-radius: 3px;
    padding: 0;
    padding: 2px 4px 0px 4px;
    font-size: 0.9em;
}

code {
    background-color: #f3f4f4;
    padding: 0 2px 0 2px;
}

.md-fences {
    margin-bottom: 15px;
    margin-top: 15px;
    padding-top: 8px;
    padding-bottom: 6px;
}


.md-task-list-item > input {
  margin-left: -1.3em;
}

@media print {
    html {
        font-size: 13px;
    }
    pre {
        page-break-inside: avoid;
        word-wrap: break-word;
    }
}

.md-fences {
	background-color: #f8f8f8;
}
#write pre.md-meta-block {
	padding: 1rem;
    font-size: 85%;
    line-height: 1.45;
    background-color: #f7f7f7;
    border: 0;
    border-radius: 3px;
    color: #777777;
    margin-top: 0 !important;
}

.mathjax-block>.code-tooltip {
	bottom: .375rem;
}

.md-mathjax-midline {
    background: #fafafa;
}

#write>h3.md-focus:before{
	left: -1.5625rem;
	top: .375rem;
}
#write>h4.md-focus:before{
	left: -1.5625rem;
	top: .285714286rem;
}
#write>h5.md-focus:before{
	left: -1.5625rem;
	top: .285714286rem;
}
#write>h6.md-focus:before{
	left: -1.5625rem;
	top: .285714286rem;
}
.md-image>.md-meta {
    /*border: 1px solid #ddd;*/
    border-radius: 3px;
    padding: 2px 0px 0px 4px;
    font-size: 0.9em;
    color: inherit;
}

.md-tag {
    color: #a7a7a7;
    opacity: 1;
}

.md-toc { 
    margin-top:20px;
    padding-bottom:20px;
}

.sidebar-tabs {
    border-bottom: none;
}

#typora-quick-open {
    border: 1px solid #ddd;
    background-color: #f8f8f8;
}

#typora-quick-open-item {
    background-color: #FAFAFA;
    border-color: #FEFEFE #e5e5e5 #e5e5e5 #eee;
    border-style: solid;
    border-width: 1px;
}

/** focus mode */
.on-focus-mode blockquote {
    border-left-color: rgba(85, 85, 85, 0.12);
}

header, .context-menu, .megamenu-content, footer{
    font-family: "Segoe UI", "Arial", sans-serif;
}

.file-node-content:hover .file-node-icon,
.file-node-content:hover .file-node-open-state{
    visibility: visible;
}

.mac-seamless-mode #typora-sidebar {
    background-color: #fafafa;
    background-color: var(--side-bar-bg-color);
}

.mac-os #write{
    caret-color: AccentColor;
}

.md-lang {
    color: #b4654d;
}

/*.html-for-mac {
    --item-hover-bg-color: #E6F0FE;
}*/

#md-notification .btn {
    border: 0;
}

.dropdown-menu .divider {
    border-color: #e5e5e5;
    opacity: 0.4;
}

.ty-preferences .window-content {
    background-color: #fafafa;
}

.ty-preferences .nav-group-item.active {
    color: white;
    background: #999;
}

.menu-item-container a.menu-style-btn {
    background-color: #f5f8fa;
    background-image: linear-gradient( 180deg , hsla(0, 0%, 100%, 0.8), hsla(0, 0%, 100%, 0)); 
}
```



这是**Typora 最经典的 GitHub 风格主题**，完全复刻 GitHub Markdown 的渲染效果，也是绝大多数 Typora 用户的默认主题。下面分模块逐句解析每一行代码的功能、作用和设计意图。

------

## 一、全局 CSS 变量定义

css:

```css
:root {
    --side-bar-bg-color: #fafafa;
    --control-text-color: #777;
}
```

### 逐句解析：

1. **`:root`**

   

   - 含义：CSS 伪类，匹配文档的根元素（HTML 中的`<html>`标签）
   - 功能：用于定义**全局 CSS 变量**，所有子元素都可以通过`var(--变量名)`引用
   - 设计意图：将全局通用的颜色、尺寸集中管理，便于后期修改主题

   

2. **`--side-bar-bg-color: #fafafa;`**

   

   - 含义：定义侧边栏背景色变量，值为极浅灰色
   - 作用：Typora 左侧文件树、大纲侧边栏的背景色
   - 设计意图：与编辑区域的纯白色背景形成轻微区分，不刺眼

   

3. **`--control-text-color: #777;`**

   

   - 含义：定义控件文本颜色变量，值为中灰色
   - 作用：按钮、菜单、标签等非主要文本的颜色
   - 设计意图：降低次要信息的视觉权重，突出主要内容

   

------

## 二、导出时字体引入（Typora 特有指令）

css:

```css
@include-when-export url(https://fonts.googleapis.com/css?family=Open+Sans:400italic,700italic,700,400&subset=latin,latin-ext);
```

### 逐句解析：

1. **`@include-when-export`**

   

   - 含义：**Typora 特有的 CSS 指令**，只有在导出为 PDF/HTML 时才会执行后面的代码
   - 功能：导出时从 Google Fonts 加载 Open Sans 字体
   - 设计意图：确保导出的文档在任何设备上都能正确显示字体，不受本地字体影响

   

2. **`url(https://fonts.googleapis.com/css?family=Open+Sans:400italic,700italic,700,400&subset=latin,latin-ext)`**

   

   - 含义：Google Fonts 的 Open Sans 字体加载地址

   - 参数说明：

     - `400italic`：400 字重的斜体
     - `700italic`：700 字重的斜体
     - `700`：700 字重的正常体（粗体）
     - `400`：400 字重的正常体（常规）
     - `subset=latin,latin-ext`：只加载拉丁字符集，减小文件体积

     

   

------

## 三、本地字体引入（@font-face）

这部分定义了 4 种字重和样式的 Open Sans 字体，优先使用本地安装的字体，本地没有时使用导出时加载的在线字体。

### 3.1 常规体（400 字重）

css:

```css
@font-face {
    font-family: 'Open Sans';
    font-style: normal;
    font-weight: normal;
    src: local('Open Sans Regular'), local('OpenSans-Regular'), url('./github/open-sans-v17-latin-ext_latin-regular.woff2') format('woff2');
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD, U+0100-024F, U+0259, U+1E00-1EFF, U+2020, U+20A0-20AB, U+20AD-20CF, U+2113, U+2C60-2C7F, U+A720-A7FF;
}
```

### 逐句解析：

1. **`@font-face`**

   

   - 含义：CSS 规则，用于定义自定义字体
   - 功能：将外部字体文件嵌入到 CSS 中，供页面使用

   

2. **`font-family: 'Open Sans';`**

   

   - 含义：定义字体的名称，后续通过这个名称引用字体

   

3. **`font-style: normal;`**

   

   - 含义：字体样式，`normal`表示正常体，`italic`表示斜体

   

4. **`font-weight: normal;`**

   

   - 含义：字体粗细，`normal`等价于 400，`bold`等价于 700

   

5. **`src: local('Open Sans Regular'), local('OpenSans-Regular'), url('./github/open-sans-v17-latin-ext_latin-regular.woff2') format('woff2');`**

   

   - 含义：字体源，按优先级依次尝试加载

   - 优先级：

     1. 本地系统中名为`Open Sans Regular`的字体
     2. 本地系统中名为`OpenSans-Regular`的字体
     3. 本地相对路径下的`woff2`字体文件

     

   - 设计意图：优先使用本地字体，提高加载速度；本地没有时使用内置字体文件

   

6. **`unicode-range: ...;`**

   

   - 含义：指定该字体支持的 Unicode 字符范围
   - 功能：只加载需要的字符，减小字体文件体积，提高加载速度
   - 这里包含了所有常用的拉丁字符、符号和扩展拉丁字符

   

### 3.2 斜体、粗体、粗斜体

后面三个`@font-face`分别定义了：

- 斜体（`font-style: italic; font-weight: normal;`）
- 粗体（`font-style: normal; font-weight: bold;`）
- 粗斜体（`font-style: italic; font-weight: bold;`）
- 结构和参数与常规体完全相同，只是字体样式和字重不同

------

## 四、基础 HTML/Body 样式

css:

```css
html {
    font-size: 16px;
    -webkit-font-smoothing: antialiased;
}

body {
    font-family: "Open Sans","Clear Sans", "Helvetica Neue", Helvetica, Arial, 'Segoe UI Emoji', 'SF Pro', sans-serif;
    color: rgb(51, 51, 51);
    line-height: 1.6;
}
```

### 逐句解析：

1. **`html { font-size: 16px; }`**

   

   - 含义：设置根元素的字体大小为 16 像素
   - 作用：所有使用`em`/`rem`单位的元素都会基于这个值计算
   - 设计意图：16px 是网页标准的基础字体大小，阅读体验最佳

   

2. **`-webkit-font-smoothing: antialiased;`**

   

   - 含义：WebKit 内核浏览器的字体抗锯齿属性
   - 作用：让字体显示更平滑，特别是在 macOS 系统上
   - 设计意图：提高文本的可读性

   

3. **`body { font-family: "Open Sans", ..., sans-serif; }`**

   

   - 含义：设置全局字体栈，按优先级依次使用
   - 优先级：Open Sans → Clear Sans → Helvetica Neue → Helvetica → Arial → Segoe UI Emoji → SF Pro → 系统默认无衬线字体
   - 设计意图：确保在任何系统上都能显示合适的字体，同时支持 emoji 表情

   

4. **`color: rgb(51, 51, 51);`**

   

   - 含义：设置全局文本颜色为深灰色（#333333）
   - 设计意图：比纯黑色更柔和，长时间阅读不易疲劳

   

5. **`line-height: 1.6;`**

   

   - 含义：设置行高为字体大小的 1.6 倍
   - 设计意图：行高适中，阅读时不会太拥挤也不会太松散

   

------

## 五、编辑区域容器样式（Typora 核心）

css:

```css
#write {
    max-width: 860px;
  	margin: 0 auto;
  	padding: 30px;
    padding-bottom: 100px;
}
```

### 逐句解析：

1. **`#write`**

   

   - 含义：Typora 编辑区域的根容器 ID，所有 Markdown 内容都渲染在这个容器内
   - 这是 Typora 最重要的选择器，几乎所有内容样式都基于它

   

2. **`max-width: 860px;`**

   

   - 含义：编辑区域的最大宽度为 860 像素
   - 设计意图：限制每行文本的长度，最佳阅读行宽是 60-80 个字符

   

3. **`margin: 0 auto;`**

   

   - 含义：上下边距为 0，左右边距自动
   - 作用：让编辑区域在窗口中水平居中

   

4. **`padding: 30px; padding-bottom: 100px;`**

   

   - 含义：内边距上下左右 30 像素，底部额外增加到 100 像素
   - 设计意图：底部增加内边距，避免最后一行内容紧贴窗口底部

   

------

## 六、响应式媒体查询

css:

```css
@media only screen and (min-width: 1400px) {
	#write {
		max-width: 1024px;
	}
}

@media only screen and (min-width: 1800px) {
	#write {
		max-width: 1200px;
	}
}
```

### 逐句解析：

1. **`@media only screen and (min-width: 1400px)`**

   

   - 含义：媒体查询，当屏幕宽度大于等于 1400 像素时生效
   - 作用：将编辑区域最大宽度增加到 1024 像素

   

2. **`@media only screen and (min-width: 1800px)`**

   

   - 含义：当屏幕宽度大于等于 1800 像素时生效
   - 作用：将编辑区域最大宽度增加到 1200 像素
   - 设计意图：在大屏幕上充分利用空间，显示更多内容

   

------

## 七、基础内容样式

### 7.1 第一个列表的上边距

css:

```css
#write > ul:first-child,
#write > ol:first-child{
    margin-top: 30px;
}
```

- 含义：如果编辑区域的第一个元素是无序列表或有序列表，给它增加 30 像素的上边距
- 设计意图：避免列表紧贴编辑区域顶部

### 7.2 链接样式

css:

```css
a {
    color: #4183C4;
}
```

- 含义：设置链接颜色为 GitHub 蓝色
- 设计意图：与 GitHub 的链接颜色完全一致，保持风格统一

### 7.3 标题样式（h1-h6）

css:

```css
h1,
h2,
h3,
h4,
h5,
h6 {
    position: relative;
    margin-top: 1rem;
    margin-bottom: 1rem;
    font-weight: bold;
    line-height: 1.4;
    cursor: text;
}
```

- 通用标题样式：

  - `position: relative`：为标题锚点定位做准备
  - `margin-top: 1rem; margin-bottom: 1rem`：上下边距 1 个字体大小
  - `font-weight: bold`：所有标题都是粗体
  - `line-height: 1.4`：标题行高比正文小
  - `cursor: text`：鼠标悬停在标题上时显示文本光标

  

#### 标题锚点悬停效果

css:

```css
h1:hover a.anchor,
h2:hover a.anchor,
h3:hover a.anchor,
h4:hover a.anchor,
h5:hover a.anchor,
h6:hover a.anchor {
    text-decoration: none;
}
```

- 含义：鼠标悬停在标题上时，标题锚点链接不显示下划线
- 设计意图：保持界面整洁

#### 标题内代码样式

css:

```css
h1 tt,
h1 code {
    font-size: inherit;
}
```

- 含义：标题内的代码字体大小与标题相同
- 设计意图：避免标题内的代码大小不一致

#### 各级标题单独样式

css:

```css
h1 {
    font-size: 2.25em;
    line-height: 1.2;
    border-bottom: 1px solid #eee;
}

h2 {
    font-size: 1.75em;
    line-height: 1.225;
    border-bottom: 1px solid #eee;
}

h3 {
    font-size: 1.5em;
    line-height: 1.43;
}

h4 {
    font-size: 1.25em;
}

h5 {
    font-size: 1em;
}

h6 {
   font-size: 1em;
    color: #777;
}
```

- 设计特点：

  - h1 和 h2 有底部边框，与 GitHub 完全一致
  - 字体大小逐级递减，层次清晰
  - h6 颜色为灰色，视觉权重最低

  

### 7.4 段落、引用、列表、水平线

css:

```css
p,
blockquote,
ul,
ol,
dl,
table{
    margin: 0.8em 0;
}
```

- 含义：段落、引用、列表、定义列表、表格的上下边距都是 0.8em
- 设计意图：统一内容块之间的间距

css:

```css
li>ol,
li>ul {
    margin: 0 0;
}
```

- 含义：嵌套列表的上下边距为 0
- 设计意图：避免嵌套列表间距过大

css:

```css
hr {
    height: 2px;
    padding: 0;
    margin: 16px 0;
    background-color: #e7e7e7;
    border: 0 none;
    overflow: hidden;
    box-sizing: content-box;
}
```

- 含义：水平线样式

  - 高度 2 像素，浅灰色背景
  - 无边框，上下边距 16 像素

  

- 设计意图：与 GitHub 的水平线样式完全一致

### 7.5 引用样式

css:

```css
blockquote {
    border-left: 4px solid #dfe2e5;
    padding: 0 15px;
    color: #777777;
}

blockquote blockquote {
    padding-right: 0;
}
```

- 含义：引用样式

  - 左侧 4 像素的灰色边框
  - 左右内边距 15 像素
  - 文本颜色为灰色

  

- 设计意图：清晰区分引用内容和正文

### 7.6 表格样式

css:

```css
table {
    padding: 0;
    word-break: initial;
}

table tr {
    border: 1px solid #dfe2e5;
    margin: 0;
    padding: 0;
}

table tr:nth-child(2n),
thead {
    background-color: #f8f8f8;
}

table th {
    font-weight: bold;
    border: 1px solid #dfe2e5;
    border-bottom: 0;
    margin: 0;
    padding: 6px 13px;
}

table td {
    border: 1px solid #dfe2e5;
    margin: 0;
    padding: 6px 13px;
}
```

- 设计特点：

  - 所有单元格都有边框
  - 表头和偶数行有浅灰色背景（斑马纹）
  - 表头文字加粗
  - 单元格内边距 6px 13px

  

- 完全复刻 GitHub 的表格样式

------

## 八、代码与代码块样式

css:

```css
.CodeMirror-lines {
    padding-left: 4px;
}

.code-tooltip {
    box-shadow: 0 1px 1px 0 rgba(0,28,36,.3);
    border-top: 1px solid #eef2f2;
}

.md-fences,
code,
tt {
    border: 1px solid #e7eaed;
    background-color: #f8f8f8;
    border-radius: 3px;
    padding: 0;
    padding: 2px 4px 0px 4px;
    font-size: 0.9em;
}

code {
    background-color: #f3f4f4;
    padding: 0 2px 0 2px;
}

.md-fences {
    margin-bottom: 15px;
    margin-top: 15px;
    padding-top: 8px;
    padding-bottom: 6px;
}
```

### 逐句解析：

1. **`.CodeMirror-lines`**：代码编辑器的行容器，左边距 4 像素

2. **`.code-tooltip`**：代码提示工具的样式，添加阴影和顶部边框

3. **`.md-fences, code, tt`**：代码块、行内代码、打字机文本的通用样式

   - 浅灰色背景，1 像素灰色边框，3 像素圆角
   - 字体大小为正文的 0.9 倍

   

4. **`code`**：行内代码的单独样式，背景色稍深，内边距更小

5. **`.md-fences`**：代码块的单独样式，上下边距 15 像素，上下内边距 8px/6px

------

## 九、任务列表样式

css:

```css
.md-task-list-item > input {
  margin-left: -1.3em;
}
```

- 含义：任务列表的复选框向左偏移 1.3em
- 设计意图：让复选框与列表文本对齐

------

## 十、打印样式

css:

```css
@media print {
    html {
        font-size: 13px;
    }
    pre {
        page-break-inside: avoid;
        word-wrap: break-word;
    }
}
```

- 含义：打印时的样式

  - 字体大小改为 13px，节省纸张
  - 代码块内不分页，避免代码被截断
  - 代码块内自动换行

  

------

## 十一、Typora 特有功能样式

### 11.1 元数据块

css:

```css
#write pre.md-meta-block {
	padding: 1rem;
    font-size: 85%;
    line-height: 1.45;
    background-color: #f7f7f7;
    border: 0;
    border-radius: 3px;
    color: #777777;
    margin-top: 0 !important;
}
```

- 含义：Markdown 顶部的 YAML 元数据块样式
- 设计意图：清晰区分元数据和正文内容

### 11.2 数学公式

css:

```css
.mathjax-block>.code-tooltip {
	bottom: .375rem;
}

.md-mathjax-midline {
    background: #fafafa;
}
```

- 含义：MathJax 数学公式的样式

### 11.3 专注模式

css:

```css
.on-focus-mode blockquote {
    border-left-color: rgba(85, 85, 85, 0.12);
}
```

- 含义：专注模式下引用的边框颜色变浅
- 设计意图：降低非当前行内容的视觉权重

------

## 十二、Typora UI 元素样式

css:

```css
header, .context-menu, .megamenu-content, footer{
    font-family: "Segoe UI", "Arial", sans-serif;
}

.sidebar-tabs {
    border-bottom: none;
}

#typora-quick-open {
    border: 1px solid #ddd;
    background-color: #f8f8f8;
}

.ty-preferences .window-content {
    background-color: #fafafa;
}

.ty-preferences .nav-group-item.active {
    color: white;
    background: #999;
}
```

- 这些样式定义了 Typora 本身的 UI 元素外观，包括：

  - 菜单、右键菜单、页脚的字体
  - 侧边栏标签
  - 快速打开窗口
  - 偏好设置窗口

  

------

## 总结

这个 GitHub 风格主题的核心设计原则是：

1. **简洁清晰**：使用浅色调、清晰的分隔线、适中的间距
2. **高可读性**：合适的字体大小、行高和颜色对比度
3. **完全复刻 GitHub**：所有样式都与 GitHub Markdown 渲染效果一致
4. **响应式设计**：适配不同屏幕尺寸
5. **导出友好**：确保导出的 PDF/HTML 与编辑时的效果一致