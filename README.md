# 三维虚拟展厅 · 荒野之光

一个用 Unity 6 制作的第一人称虚拟展厅。玩家在展厅中自由行走，走近任意一幅画时左下角出现「进入」按钮，点击后进入该画作的导览面板，读完退出继续漫游。

三幅画按「十九世纪美国风景画」的主题串成一组 —— 从科尔到他的学生丘奇，光色从暖金斜阳、冷白雨后，走到暮色余烬。

| 画作 | 作者 | 光色 |
|---|---|---|
| 《尼亚加拉瀑布远景》1830 | 托马斯·科尔 | 暖金，壮阔 |
| 《牛轭湖》1836 | 托马斯·科尔 | 冷白，敬畏 |
| 《荒野黄昏》1860 | 弗雷德里克·埃德温·丘奇 | 暮色，静默 |

## 技术栈

| 项目 | 版本 / 说明 |
|---|---|
| Unity | 6000.0.84f1（Unity 6） |
| 渲染管线 | Universal Render Pipeline 17.0.4 |
| 输入 | Input System 1.20.0 |
| UI | uGUI 2.0.0 + TextMesh Pro |
| 导航 | AI Navigation 2.0.14 |

## 如何运行

1. 安装 **Unity 6（6000.0.84f1）**，用 Unity Hub 打开本工程目录。
2. 首次打开会重新导入资源并生成 `Library/`，需要几分钟。
3. 打开 `Assets/Scenes/SampleScene.unity`，点 Play。

> 若要从登录页开始体验，先打开 `loginScence`，并在
> **File → Build Settings** 里把它拖到场景列表最上面（当前列表里只有 `SampleScene` 和 `frameScene1`）。

## 操作

| 按键 | 作用 |
|---|---|
| `W A S D` | 移动 |
| 鼠标移动 | 转视角 |
| `Space` | 跳跃 |
| `E` | 跳舞 / 停止 |
| `Esc` | 释放鼠标（界面模式下自动释放） |

## 目录结构

```
Assets/
├── Scenes/            三个场景：登录、展厅、画作页
├── script/            全部 C# 脚本
├── Model/             展厅、角色、机器人等模型（glb / fbx）
├── Animator/          角色动画控制器
├── photo/             画作导览背景图
├── UI/                界面素材
├── Settings/          URP 渲染配置
├── Universal Animation Library[Standard]/   第三方动画库
└── TextMesh Pro/      字体资源
```

## 核心脚本

| 脚本 | 作用 |
|---|---|
| `player.cs` | 第一人称移动、跳跃、转身平滑、动画状态驱动 |
| `Camera move.cs` | 第三人称跟随相机：轨道角、防穿墙球形射线、界面模式开关 |
| `ArtTrigger.cs` | 画作触发区：走近显示按钮，点按钮打开对应导览面板 |
| `SceneSwitch.cs` | 场景切换与面板开关 |
| `lonUI.cs` | 登录页淡入淡出转场 |
| `CameraSimple.cs` | 教学用的最简相机版本（与 `Camera move.cs` 二选一） |

### 走来走去的那套是怎么接上的

```
collider1 ─┐
collider2 ─┼─ ArtTrigger（每个触发区记住自己对应哪块面板）
collider3 ─┘        │
                    ├─ 进入 → 显示共用的「进入」按钮
                    └─ 点击 → EnterCurrent() 打开脚下这幅画的面板
                              ClosePanel()  关闭并恢复漫游
```

按钮只有一块，它不记「我是哪幅画」，而是在点击的那一刻去读静态变量 `current`（玩家站在哪个触发区）。所以三个触发区、三块面板、一块按钮就够了。

## 说明

- `photo/` 下三张导览背景图为 AI 生成，仅用于界面背景，与画作本身无关。
- 画作的年代、尺寸与馆藏信息按公开资料填写；若用于正式展陈请再核对一次。
- `Library/`、`Temp/`、`Logs/` 等 Unity 自动生成目录已通过 `.gitignore` 排除，克隆后会自动重建。
